using System;
using System.IO;
using System.Dynamic;
using System.Reflection;
using System.Globalization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using GTANetworkServer;
using GTANetworkShared;

class ModelDimensions
{
    public int Model;
    public Vector3 Max, Min;
}

class TrunkInfo
{    
    public int Model;
    public Vector3 Offset, Size;
}

public class TrunkPlacer : Script
{
    public TrunkPlacer()
    {
        API.onClientEventTrigger += ServerEvent;
        API.onResourceStart += resStart;
        API.onResourceStop += resStop;

    }

    private const string TRUNK_KEY = "VEHICLE_TRUNK_CONTENTS"; // List<NetHandle>

    private Dictionary<int, ModelDimensions> _cachedDims = new Dictionary<int, ModelDimensions>();
    private Dictionary<int, List<Action<ModelDimensions>>> _callbacks = new Dictionary<int, List<Action<ModelDimensions>>>();
    private Dictionary<int, TrunkInfo> _trunks = new Dictionary<int, TrunkInfo>();

    private void resStart()
    {
        var config = API.loadConfig("trunks.xml");

        foreach (var element in config.getElementsByType("TrunkInfo"))
        {
            var model = element.getElementData<int>("model");

            var offset = new Vector3(element.getElementData<float>("offsetX"), element.getElementData<float>("offsetY"),
                element.getElementData<float>("offsetZ"));
            var size = new Vector3(element.getElementData<float>("sizeX"), element.getElementData<float>("sizeY"),
                element.getElementData<float>("sizeZ"));

            _trunks.Add(model, new TrunkInfo()
            {
                Model = model,
                Offset = offset,
                Size = size,
            });
        }
    }

    private void resStop()
    {
        foreach (var veh in API.getAllVehicles())
        {
            API.resetEntityData(veh, TRUNK_KEY);
        }
    }

    private TrunkInfo GetTrunkInfo(int model)
    {
        if (_trunks.ContainsKey(model))
            return _trunks[model];
        return null;
    }

    private Vector3 getModelSize(Vector3 max, Vector3 min)
    {
        return new Vector3((float) Math.Abs(max.X - min.X),
                           (float) Math.Abs(max.Y - min.Y),
                           (float) Math.Abs(max.Z - min.Z));
    }

    private void ServerEvent(Client sender, string ev, object[] args)
    {
        if (ev == "QUERY_MODEL_RESPONSE")
        {
            int model = (int) args[0];
            Vector3 max = (Vector3) args[1];
            Vector3 min = (Vector3) args[2];

            var mdims = new ModelDimensions()
            {
                Model = model,
                Max = max,
                Min = min
            };

            if (_callbacks.ContainsKey(model) && _callbacks[model] != null && _callbacks[model].Count > 0)
            {
                foreach (var callback in _callbacks[model])
                    callback(mdims);
            }

            _callbacks.Remove(model);

            if (!_cachedDims.ContainsKey(model))
                _cachedDims.Add(model, mdims);
        }
    }

    private ModelDimensions RequestModelDimensions(int model)
    {
        if (_cachedDims.ContainsKey(model))
        {
            return _cachedDims[model];
        }

        var players = API.getAllPlayers();

        if (players.Count == 0)
            return null;

        ModelDimensions dims = null;

        Action<ModelDimensions> callback = new Action<ModelDimensions>((m) =>
        {
            dims = m;
        });

        if (!_callbacks.ContainsKey(model))
            _callbacks.Add(model, new List<Action<ModelDimensions>>());
        _callbacks[model].Add(callback);

        players[0].triggerEvent("QUERY_MODEL_SIZE", model);


        DateTime start = DateTime.Now;
        while (dims == null && DateTime.Now.Subtract(start).TotalMilliseconds < 1000)
            API.sleep(0);

        return dims;
    }

    private bool canModelFitInsideTrunk(int objectModel, int vehicleModel)
    {
        TrunkInfo info = GetTrunkInfo(vehicleModel);

        if (info == null)
            return false;

        var m = RequestModelDimensions(objectModel);
        if (m == null) return false;
        var objectSize = m.Max - m.Min;

        if (Math.Abs(objectSize.X) < info.Size.X &&
            Math.Abs(objectSize.Y) < info.Size.Y &&
            Math.Abs(objectSize.Z) < info.Size.Z)
            return true;
        else return false;
    }

    // TODO: Don't assume everything is square
    private bool isThereEnoughPlaceInTrunk(int objectModel, NetHandle vehicle)
    {
        int vehModel = API.getEntityModel(vehicle);
        if (!canModelFitInsideTrunk(objectModel, vehModel))
            return false;

        if (!API.hasEntityData(vehicle, TRUNK_KEY))
            return true;

        TrunkInfo info = GetTrunkInfo(vehModel);
        var m = RequestModelDimensions(objectModel);

        List<NetHandle> contents = API.getEntityData(vehicle, TRUNK_KEY);
        float area = info.Size.X * info.Size.Y;

        foreach (var obj in contents)
        {
            var om = RequestModelDimensions(API.getEntityModel(obj));
            var osize = om.Max - om.Min;

            area -= (float) Math.Abs(osize.X * osize.Y);
        }

        var msize = m.Max - m.Min;
        return area > Math.Abs(msize.X * msize.Y);
    }

    private bool placeEntityInTrunkCentered(NetHandle target, NetHandle entity)
    {
        var info = GetTrunkInfo(API.getEntityModel(target));

        if (info == null) return false;

        if (!canModelFitInsideTrunk(API.getEntityModel(entity), API.getEntityModel(target)))
            return false;

        var m = RequestModelDimensions(API.getEntityModel(entity));

        // Place in the center
        API.attachEntityToEntity(entity, target, null, new Vector3(
            info.Offset.X,
            info.Offset.Y,
            info.Offset.Z - m.Min.Z
            ), new Vector3(0, 0, 0));

        if (!API.hasEntityData(target, TRUNK_KEY))
            API.setEntityData(target, TRUNK_KEY, new List<NetHandle>());

        List<NetHandle> contents = API.getEntityData(target, TRUNK_KEY);

        contents.Add(entity);

        API.setEntityData(target, TRUNK_KEY, contents);

        return true;
    }

    private bool repackEntitiesInTrunk(NetHandle target)
    {
        var info = GetTrunkInfo(API.getEntityModel(target));
        if (info == null) return false;
        if (!API.hasEntityData(target, TRUNK_KEY))
            return false;
        List<NetHandle> contents = API.getEntityData(target, TRUNK_KEY);
        if (contents.Count <= 1) return true;
        float maxWidth = info.Size.X;
        float maxHeight = info.Size.Y;        

        float currentHeight = 0;        
        float currentOffsetY = 0;
        float currentOffsetX = 0;
        // Order by height
        foreach (var obj in contents.OrderByDescending(o =>
            {
                var model = API.getEntityModel(o);
                var m = RequestModelDimensions(model);
                var size = getModelSize(m.Max, m.Min);
                return size.Y;
            }))
        {
            var objModel = RequestModelDimensions(API.getEntityModel(obj));
            var objSize = getModelSize(objModel.Max, objModel.Min);

            if (objSize.X > maxWidth - currentOffsetX) // Too large to fit, place next level
            {
                currentOffsetY += currentHeight;                
                currentOffsetX = 0;
                currentHeight = objSize.Y;

                if (currentOffsetY + objSize.Y > maxHeight)
                    return false;
            }

            if (objSize.Y > currentHeight)
                currentHeight = objSize.Y;

            API.attachEntityToEntity(obj, target, null, new Vector3(
                info.Offset.X - info.Size.X/2f + objModel.Max.X + currentOffsetX,
                info.Offset.Y + info.Size.Y/2f - objModel.Max.Y - currentOffsetY,
                info.Offset.Z - objModel.Min.Z
                ), new Vector3(0, 0, 0));

            currentOffsetX += objSize.X;
        }

        return true;
    }

    // EXPORTED

    public bool canPlaceStuff()
    {
        return API.getAllPlayers().Count > 0;
    }

    public void refreshTrunkEntities(NetHandle veh)
    {
        if (!API.hasEntityData(veh, TRUNK_KEY))
            return;
        List<NetHandle> contents = API.getEntityData(veh, TRUNK_KEY);
        for (int i = contents.Count - 1; i >= 0; i--)
        {
            if (!API.doesEntityExist(contents[i]))
                contents.RemoveAt(i);
        }

        API.setEntityData(veh, TRUNK_KEY, contents);
    }

    // DEBUG
    [Command]
    public void canfit(Client sender, string objname)
    {
        if (!sender.isInVehicle) return;

        int modelHash = API.getHashKey(objname);

        sender.sendChatMessage("Object " + objname + " can fit: " + canModelFitInsideTrunk(modelHash, API.getEntityModel(sender.vehicle)));
    }

    [Command]
    public void clearTrunk(Client sender)
    {
        if (!sender.isInVehicle) return;
        API.resetEntityData(sender.vehicle, TRUNK_KEY);
    }

    [Command]
    public void place(Client sender, string objname)
    {
        if (!sender.isInVehicle) return;

        int modelHash = API.getHashKey(objname);

        var obj = API.createObject(modelHash, sender.position, new Vector3());
        if (!placeEntityInTrunkCentered(sender.vehicle, obj) ||
            !repackEntitiesInTrunk(sender.vehicle))
            {
                sender.sendChatMessage("Placement failed");
                API.deleteEntity(obj);
            }

        refreshTrunkEntities(sender.vehicle);
    }

    [Command]
    public void extra(Client sender, int n, bool state = false)
    {
        if (!sender.isInVehicle) return;
        API.setVehicleExtra(sender.vehicle, n, state);
    }

    [Command]
    public void placecar(Client sender, VehicleHash model)
    {
        if (!sender.isInVehicle) return;

        var obj = API.createVehicle(model, sender.position, new Vector3(), 0, 0);
        API.setEntityCollisionless(obj, true);
        if (!placeEntityInTrunkCentered(sender.vehicle, obj) ||
            !repackEntitiesInTrunk(sender.vehicle))
            {
                sender.sendChatMessage("Placement failed");
                API.deleteEntity(obj);
            }
        refreshTrunkEntities(sender.vehicle);
    }
}