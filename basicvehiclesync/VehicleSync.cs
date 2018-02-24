using GTANetworkAPI;
using Newtonsoft.Json.Linq;

//Disapproved by god himself

//Just use the API functions, you have nothing else to worry about

//Things to note
//More things like vehicle mods will be added in the next version

/* API FUNCTIONS:
public static void SetVehicleWindowState(Vehicle veh, WindowID window, WindowState state)
public static WindowState GetVehicleWindowState(Vehicle veh, WindowID window)
public static void SetVehicleWheelState(Vehicle veh, WheelID wheel, WheelState state)
public static WheelState GetVehicleWheelState(Vehicle veh, WheelID wheel)
public static void SetVehicleDirt(Vehicle veh, float dirt)
public static float GetVehicleDirt(Vehicle veh)
public static void SetDoorState(Vehicle veh, DoorID door, DoorState state)
public static DoorState GetDoorState(Vehicle veh, DoorID door)
public static void SetEngineState(Vehicle veh, bool status)
public static bool GetEngineState(Vehicle veh)
public static void SetLockStatus(Vehicle veh, bool status)
public static bool GetLockState(Vehicle veh)
*/

namespace VehicleSync
{
    //Enums for ease of use
    public enum WindowID
    {
        WindowFrontRight,
        WindowFrontLeft,
        WindowRearRight,
        WindowRearLeft
    }

    public enum WindowState
    {
        WindowFixed,
        WindowDown,
        WindowBroken
    }

    public enum DoorID
    {
        DoorFrontLeft,
        DoorFrontRight,
        DoorRearLeft,
        DoorRearRight,
        DoorHood,
        DoorTrunk
    }

    public enum DoorState
    {
        DoorClosed,
        DoorOpen,
        DoorBroken,
    }

    public enum WheelID
    {
        Wheel0,
        Wheel1,
        Wheel2,
        Wheel3,
        Wheel4,
        Wheel5,
        Wheel6,
        Wheel7,
        Wheel8,
        Wheel9
    }

    public enum WheelState
    {
        WheelFixed,
        WheelBurst,
        WheelOnRim,
    }

    public class VehicleStreaming : Script
    {
        //This is the data object which will be synced to vehicles
        public class VehicleSyncData
        {
            //Used to bypass some streaming bugs
            public Vector3 Position { get; set; } = new Vector3();
            public Vector3 Rotation { get; set; } = new Vector3();

            //Basics
            public float Dirt { get; set; } = 0.0f;
            public bool Locked { get; set; } = true;
            public bool Engine { get; set; } = false;

            //(Not synced)
            public float BodyHealth { get; set; } = 1000.0f;
            public float EngineHealth { get; set; } = 1000.0f;

            //Doors 0-7 (0 = closed, 1 = open, 2 = broken) (This uses enums so don't worry about it)
            public int[] Door { get; set; } = new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 };

            //Windows (0 = up, 1 = down, 2 = smashed) (This uses enums so don't worry about it)
            public int[] Window { get; set; } = new int[4] { 0, 0, 0, 0 };

            //Wheels 0-7, 45/47 (0 = fixed, 1 = flat, 2 = missing) (This uses enums so don't worry about it)
            public int[] Wheel { get; set; } = new int[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        }

        //API functions for people to use
        public static void SetVehicleWindowState(Vehicle veh, WindowID window, WindowState state)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData)) //If data doesn't exist create a new one. This is the process for all API functions
                data = new VehicleSyncData();

            data.Window[(int)window] = (int)state;
            UpdateVehicleSyncData(veh, data);
            NAPI.ClientEvent.TriggerClientEventInDimension(veh.Dimension, "VehStream_SetVehicleWindowStatus_Single", veh.Handle, (int)window, (int)state);
        }

        public static WindowState GetVehicleWindowState(Vehicle veh, WindowID window)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
            {
                data = new VehicleSyncData();
                UpdateVehicleSyncData(veh, data);
            }
            return (WindowState)data.Window[(int)window];
        }

        public static void SetVehicleWheelState(Vehicle veh, WheelID wheel, WheelState state)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.Wheel[(int)wheel] = (int)state;
            UpdateVehicleSyncData(veh, data);
            NAPI.ClientEvent.TriggerClientEventInDimension(veh.Dimension, "VehStream_SetVehicleWheelStatus_Single", veh.Handle, (int)wheel, (int)state);
        }

        public static WheelState GetVehicleWheelState(Vehicle veh, WheelID wheel)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
            {
                data = new VehicleSyncData();
                UpdateVehicleSyncData(veh, data);
            }
            return (WheelState)data.Wheel[(int)wheel];
        }

        public static void SetVehicleDirt(Vehicle veh, float dirt)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.Dirt = dirt;
            UpdateVehicleSyncData(veh, data);
            NAPI.ClientEvent.TriggerClientEventInDimension(veh.Dimension, "VehStream_SetVehicleDirtLevel", veh.Handle, dirt);
        }

        public static float GetVehicleDirt(Vehicle veh)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
            {
                data = new VehicleSyncData();
                UpdateVehicleSyncData(veh, data);
            }
            return data.Dirt;
        }

        public static void SetDoorState(Vehicle veh, DoorID door, DoorState state)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.Door[(int)door] = (int)state;
            UpdateVehicleSyncData(veh, data);
            NAPI.ClientEvent.TriggerClientEventInDimension(veh.Dimension, "VehStream_SetVehicleDoorStatus_Single", veh, (int)door, (int)state);
        }

        public static DoorState GetDoorState(Vehicle veh, DoorID door)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
            {
                data = new VehicleSyncData();
                UpdateVehicleSyncData(veh, data);
            }
            return (DoorState)data.Door[(int)door];
        }

        public static void SetEngineState(Vehicle veh, bool status)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.Engine = status;
            UpdateVehicleSyncData(veh, data);
            NAPI.ClientEvent.TriggerClientEventInDimension(veh.Dimension, "VehStream_SetEngineStatus", veh, status);
        }

        public static bool GetEngineState(Vehicle veh)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
            {
                data = new VehicleSyncData();
                UpdateVehicleSyncData(veh, data);
            }
            return data.Engine;
        }

        public static void SetLockStatus(Vehicle veh, bool status)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.Locked = status;
            UpdateVehicleSyncData(veh, data);
            NAPI.ClientEvent.TriggerClientEventInDimension(veh.Dimension, "VehStream_SetLockStatus", veh, status);
        }

        public static bool GetLockState(Vehicle veh)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
            {
                data = new VehicleSyncData();
                UpdateVehicleSyncData(veh, data);
            }
            return data.Locked;
        }

        //Used internally only but publicly available in case any of you need it
        public static VehicleSyncData GetVehicleSyncData(Vehicle veh)
        {
            if (veh != null)
            {
                if (NAPI.Entity.DoesEntityExist(veh))
                {
                    if (NAPI.Data.HasEntitySharedData(veh.Handle, "VehicleSyncData"))
                    {
                        //API converts class objects to JObject so we have to change it back
                        JObject obj = NAPI.Data.GetEntitySharedData(veh.Handle, "VehicleSyncData");
                        return obj.ToObject<VehicleSyncData>();
                    }
                }
            }

            return default(VehicleSyncData); //null
        }

        //Used internally only but publicly available in case any of you need it
        public static bool UpdateVehicleSyncData(Vehicle veh, VehicleSyncData data)
        {
            if (veh != null)
            {
                if (NAPI.Entity.DoesEntityExist(veh))
                {
                    if (data != null)
                    {
                        data.Position = veh.Position;
                        data.Rotation = veh.Rotation;
                        NAPI.Data.SetEntitySharedData(veh, "VehicleSyncData", data);
                        return true;
                    }
                }
            }
            return false;
        }

        //Called from the client to sync dirt level
        [RemoteEvent("VehStream_SetDirtLevel")]
        public void VehStreamSetDirtLevel(Client player, Vehicle veh, float dirt)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.Dirt = dirt;

            UpdateVehicleSyncData(veh, data);

            //Re-distribute the goods
            NAPI.ClientEvent.TriggerClientEventInDimension(veh.Dimension, "VehStream_SetVehicleDirtLevel", veh.Handle, dirt);
        }

        //Called from the client to sync door data
        [RemoteEvent("VehStream_SetDoorData")]
        public void VehStreamSetDoorData(Client player, Vehicle veh, int door1state, int door2state, int door3state, int door4state, int door5state, int door6state, int door7state, int door8state)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.Door[0] = door1state;
            data.Door[1] = door2state;
            data.Door[2] = door3state;
            data.Door[3] = door4state;
            data.Door[4] = door5state;
            data.Door[5] = door6state;
            data.Door[6] = door7state;
            data.Door[7] = door8state;

            UpdateVehicleSyncData(veh, data);

            //Re-distribute the goods
            NAPI.ClientEvent.TriggerClientEventInDimension(veh.Dimension, "VehStream_SetVehicleDoorStatus", veh.Handle, door1state, door2state, door3state, door4state, door5state, door6state, door7state, door8state);
        }

        //Called from the client to sync window data
        [RemoteEvent("VehStream_SetWindowData")]
        public void VehStreamSetWindowData(Client player, Vehicle veh, int window1state, int window2state, int window3state, int window4state)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.Window[0] = window1state;
            data.Window[1] = window2state;
            data.Window[2] = window3state;
            data.Window[3] = window4state;

            UpdateVehicleSyncData(veh, data);

            //Re-distribute the goods
            NAPI.ClientEvent.TriggerClientEventInDimension(veh.Dimension, "VehStream_SetVehicleWindowStatus", veh.Handle, window1state, window2state, window3state, window4state);
        }

        //Called from the client to sync wheel data
        [RemoteEvent("VehStream_SetWheelData")]
        public void VehStreamSetWheelData(Client player, Vehicle veh, int wheel1state, int wheel2state, int wheel3state, int wheel4state, int wheel5state, int wheel6state, int wheel7state, int wheel8state, int wheel9state, int wheel10state)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.Wheel[0] = wheel1state;
            data.Wheel[1] = wheel2state;
            data.Wheel[2] = wheel3state;
            data.Wheel[3] = wheel4state;
            data.Wheel[4] = wheel5state;
            data.Wheel[5] = wheel6state;
            data.Wheel[6] = wheel7state;
            data.Wheel[7] = wheel8state;
            data.Wheel[8] = wheel9state;
            data.Wheel[9] = wheel10state;
            UpdateVehicleSyncData(veh, data);

            //Re-distribute the goods
            NAPI.ClientEvent.TriggerClientEventInDimension(veh.Dimension, "VehStream_SetVehicleWheelStatus", veh.Handle, wheel1state, wheel2state, wheel3state, wheel4state, wheel5state, wheel6state, wheel7state, wheel8state, wheel9state, wheel10state);
        }

        //Other events
        [ServerEvent(Event.PlayerEnterVehicleAttempt)]
        public void VehStreamEnterAttempt(Client player, Vehicle veh, sbyte seat)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            UpdateVehicleSyncData(veh, data);
            NAPI.ClientEvent.TriggerClientEvent(player, "VehStream_PlayerEnterVehicleAttempt", veh, seat);
        }

        [ServerEvent(Event.PlayerExitVehicleAttempt)]
        public void VehStreamExitAttempt(Client player, Vehicle veh)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.Position = veh.Position;
            data.Rotation = veh.Rotation;

            UpdateVehicleSyncData(veh, data);
            NAPI.ClientEvent.TriggerClientEvent(player, "VehStream_PlayerExitVehicleAttempt", veh);
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void VehStreamExit(Client player, Vehicle veh)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.Position = veh.Position;
            data.Rotation = veh.Rotation;

            UpdateVehicleSyncData(veh, data);
            NAPI.ClientEvent.TriggerClientEvent(player, "VehStream_PlayerExitVehicle", veh);
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void VehStreamEnter(Client player, Vehicle veh, sbyte seat)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            UpdateVehicleSyncData(veh, data);
            NAPI.ClientEvent.TriggerClientEvent(player, "VehStream_PlayerEnterVehicle", veh, seat);
        }

        [ServerEvent(Event.VehicleDamage)]
        public void VehDamage(Vehicle veh, float bodyHealthLoss, float engineHealthLoss)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.BodyHealth -= bodyHealthLoss;
            data.EngineHealth -= engineHealthLoss;

            UpdateVehicleSyncData(veh, data);

            if (NAPI.Vehicle.GetVehicleDriver(veh) != default(Client)) //Doesn't work?
                NAPI.ClientEvent.TriggerClientEvent(NAPI.Vehicle.GetVehicleDriver(veh), "VehStream_PlayerExitVehicleAttempt", veh);
        }

        [ServerEvent(Event.VehicleDoorBreak)]
        public void VehDoorBreak(Vehicle veh, int index)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.Door[index] = 2;

            UpdateVehicleSyncData(veh, data);

            NAPI.ClientEvent.TriggerClientEventInDimension(veh.Dimension, "VehStream_SetVehicleDoorStatus", veh.Handle, data.Door[0], data.Door[1], data.Door[2], data.Door[3], data.Door[4], data.Door[5], data.Door[6], data.Door[7]);
        }
    }
}
