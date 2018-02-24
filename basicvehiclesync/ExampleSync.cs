using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

//You do not need this file, this is just an example on how to use it

namespace VehicleSync
{
    public class ExampleSync : Script
    {
        Vehicle veh = null;
        [Command("veh")]
        public void VehCommand(Client player)
        {
            veh = NAPI.Vehicle.CreateVehicle(VehicleHash.Tempesta, player.Position, player.Rotation, 25, 25, "test", 255, false, false, player.Dimension);
            NAPI.Player.GivePlayerWeapon(player, WeaponHash.Pistol50, 500);
        }

        [Command("deluxo")]
        public void DVehCommand(Client player)
        {
            Vehicle v = NAPI.Vehicle.CreateVehicle(VehicleHash.DELUXO, player.Position, player.Rotation, 25, 25, "test", 255, false, false, player.Dimension);
            NAPI.Player.SetPlayerIntoVehicle(player, v, -1);
            VehicleStreaming.SetVehicleWheelState(v, 0, 0);
        }

        [Command("lock")]
        public void LockCommand(Client player)
        {
            VehicleStreaming.SetLockStatus(veh, !VehicleStreaming.GetLockState(veh));
        }

        [Command("window")]
        public void WindowCommand(Client player, int window)
        {
            WindowState windowState = VehicleStreaming.GetVehicleWindowState(veh, (WindowID)window);
            if (windowState == WindowState.WindowDown)
                VehicleStreaming.SetVehicleWindowState(veh, (WindowID)window, WindowState.WindowFixed);
            else if (windowState == WindowState.WindowFixed)
                VehicleStreaming.SetVehicleWindowState(veh, (WindowID)window, WindowState.WindowDown);
            else
                player.SendChatMessage("~y~The window is broken!");
            player.SendChatMessage("Window toggled.");
        }

        [Command("door")]
        public void DoorCommand(Client player, int door)
        {
            DoorState currentState = VehicleStreaming.GetDoorState(veh, (DoorID)door);
            if (currentState == DoorState.DoorClosed)
                VehicleSync.VehicleStreaming.SetDoorState(veh, (DoorID)door, DoorState.DoorOpen);
            else if (currentState == DoorState.DoorOpen)
                VehicleSync.VehicleStreaming.SetDoorState(veh, (DoorID)door, DoorState.DoorClosed);
            else
                player.SendChatMessage("~y~The door is broken!");
            player.SendChatMessage("Door toggled.");
        }

        [Command("doorall")]
        public void DoorAllCommand(Client player)
        {
            for (int i = 0; i < 8; i++)
            {
                DoorState currentState = VehicleStreaming.GetDoorState(veh, (DoorID)i);
                if (currentState == DoorState.DoorClosed)
                    VehicleSync.VehicleStreaming.SetDoorState(veh, (DoorID)i, DoorState.DoorOpen);
                else if (currentState == DoorState.DoorOpen)
                    VehicleSync.VehicleStreaming.SetDoorState(veh, (DoorID)i, DoorState.DoorClosed);
            }
            player.SendChatMessage("All doors toggled.");
        }

        [Command("engine")]
        public void EngineCommand(Client player)
        {
            VehicleStreaming.SetEngineState(veh, !VehicleStreaming.GetEngineState(veh));
            player.SendChatMessage("Engine toggled.");
        }
    }
}
