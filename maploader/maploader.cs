using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using GTANetworkAPI;


public class MapLoader : Script
{
    public MapLoader()
    {
        Event.OnResourceStart += OnResourceStart;
        Event.OnResourceStop += OnResourceStop;
    }

    public List<NetHandle> CreatedEntities = new List<NetHandle>();

    public void OnResourceStop()
    {
        foreach (var handle in CreatedEntities)
        {
            API.DeleteEntity(handle);
        }
    }

    public void OnResourceStart()
    {
        if (!Directory.Exists("maps"))
            Directory.CreateDirectory("maps");

        var files = Directory.GetFiles("maps", "*.xml");
        int mapsLoaded = 0;
        API.ConsoleOutput("Loading maps...");
        foreach (var path in files)
        {
            mapsLoaded++;
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var ser = new XmlSerializer(typeof(Map));
                var myMap = (Map)ser.Deserialize(stream);


                foreach (var prop in myMap.Objects)
                {
                    if (prop.Type == ObjectTypes.Prop)
                    {
                        if (prop.Quaternion != null)
                        {
                            CreatedEntities.Add(API.CreateObject(prop.Hash, prop.Position, prop.Quaternion));
                        }
                        else
                        {
                            CreatedEntities.Add(API.CreateObject(prop.Hash, prop.Position, prop.Rotation));
                        }
                    }
                    else if (prop.Type == ObjectTypes.Vehicle)
                    {
                        CreatedEntities.Add(API.CreateVehicle(prop.Hash, prop.Position, prop.Rotation.Z, 0, 0));
                    }
                }
            }
        }

        API.ConsoleOutput("Loaded " + mapsLoaded + " maps!");
    }

}

public class MapObject
{
    public ObjectTypes Type;
    public Vector3 Position;
    public Vector3 Rotation;
    public int Hash;
    public bool Dynamic;

    public Quaternion Quaternion;

    // Ped stuff
    public string Action;
    public string Relationship;
    public string Weapon;

    // Vehicle stuff
    public bool SirensActive;

    [XmlAttribute("Id")]
    public string Id;
}

public class PedDrawables
{
    public int[] Drawables;
    public int[] Textures;
}

public enum ObjectTypes
{
    Prop,
    Vehicle,
    Ped,
    Marker,
}

public class Map
{
    public List<MapObject> Objects = new List<MapObject>();
    public List<MapObject> RemoveFromWorld = new List<MapObject>();
    public List<object> Markers = new List<object>();
}
