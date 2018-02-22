#define ENABLE_DATA_COLLECTION

using System;
using System.IO;
using System.Dynamic;
using System.Reflection;
using System.Globalization;
using System.Xml;
using System.Collections.Generic;
using GTANetworkServer;
using GTANetworkShared;

public class DataCollector : Script
{
    public DataCollector()
    {
        API.onClientEventTrigger += clientevent;
    }

    private XmlGroup _trunks;

    private void clientevent(Client sender, string evname, object[] args)
    {
        if (evname == "COLLECT_RESULTS")
        {
            SaveData(API.getEntityModel((NetHandle) args[0]), args[1] as Vector3, args[2] as Vector3);
            sender.sendChatMessage("Results saved!");
        }
    }

    private void SaveData(int model, Vector3 offset, Vector3 size)
    {
        if (_trunks == null)
            _trunks = API.loadConfig("trunks.xml");

        var bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        FieldInfo field = typeof(XmlGroup).GetField("_mapDocument", bindFlags);
        dynamic doc = field.GetValue(_trunks);

        var n = doc.CreateNode(XmlNodeType.Element, "TrunkInfo", null);

        {
            var attr = doc.CreateAttribute("model");
            attr.Value = model.ToString();
            n.Attributes.Append(attr);
        }

        {
            var attr = doc.CreateAttribute("offsetX");
            attr.Value = offset.X.ToString("F6", CultureInfo.InvariantCulture);
            n.Attributes.Append(attr);
        }

        {
            var attr = doc.CreateAttribute("offsetY");
            attr.Value = offset.Y.ToString("F6", CultureInfo.InvariantCulture);
            n.Attributes.Append(attr);
        }

        {
            var attr = doc.CreateAttribute("offsetZ");
            attr.Value = offset.Z.ToString("F6", CultureInfo.InvariantCulture);
            n.Attributes.Append(attr);
        }

        {
            var attr = doc.CreateAttribute("sizeX");
            attr.Value = size.X.ToString("F6", CultureInfo.InvariantCulture);
            n.Attributes.Append(attr);
        }

        {
            var attr = doc.CreateAttribute("sizeY");
            attr.Value = size.Y.ToString("F6", CultureInfo.InvariantCulture);
            n.Attributes.Append(attr);
        }

        {
            var attr = doc.CreateAttribute("sizeZ");
            attr.Value = size.Z.ToString("F6", CultureInfo.InvariantCulture);
            n.Attributes.Append(attr);
        }

        doc.FirstChild.AppendChild(n);

        doc.Save(API.getResourceFolder() + Path.DirectorySeparatorChar + "trunks.xml");
    }

#if ENABLE_DATA_COLLECTION
    [Command]
    public void StartCollect(Client sender)
    {
        if (!sender.isInVehicle) return;
        sender.triggerEvent("START_COLLECTING", sender.vehicle);
    }

    [Command]
    public void StopCollect(Client sender)
    {   
        sender.triggerEvent("STOP_COLLECTING");
    }

    [Command]
    public void SaveCollect(Client sender)
    {
        sender.triggerEvent("SAVE_COLLECT");
    }
#endif
}