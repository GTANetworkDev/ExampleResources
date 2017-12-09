using System;
using GTANetworkAPI;

namespace WipRagempResource.playerblips
{
    public class PlayerBlips : Script
    {
        public PlayerBlips()
        {
            Event.OnPlayerConnected += PlayerJoin;
            Event.OnPlayerDisconnected += PlayerLeave;
            //Event.OnPlayerFinishedDownload += PlayerJavascriptDownloadComplete;
            Event.OnResourceStop += resourceStop;
            Event.OnResourceStart += () =>
            {
                foreach (var player in API.GetAllPlayers())
                {
                    PlayerJoin(player, null);
                }
            };
        }

        private void resourceStop()
        {
            foreach (var player in API.GetAllPlayers())
            {
                API.ResetEntitySharedData(player, "PLAYERBLIPS_HAS_BLIP_RECEIVED");
                API.ResetEntitySharedData(player, "PLAYERBLIPS_MAIN_BLIP");
            }
        }

        private void PlayerJoin(Client player, CancelEventArgs cancel)
        {
            var pBlip = API.CreateBlip(API.GetEntityPosition(player));
            API.AttachEntityToEntity(pBlip, player, null, new Vector3(), new Vector3());

            API.SetBlipName(pBlip, player.Name);
            API.SetBlipScale(pBlip, 0.8f);
            
            API.SetEntitySharedData(player, "PLAYERBLIPS_MAIN_BLIP", pBlip);

            PlayerJavascriptDownloadComplete(player);
        }

        private void PlayerJavascriptDownloadComplete(Client player)
        {
            if (API.GetEntitySharedData(player, "PLAYERBLIPS_HAS_BLIP_RECEIVED") != true)
            {
                API.TriggerClientEvent(player, "SET_PLAYER_BLIP", getPlayerBlip(player));
                API.SetEntitySharedData(player, "PLAYERBLIPS_HAS_BLIP_RECEIVED", true);
            }
        }

        private void PlayerLeave(Client player, byte type, string reason)
        {
            var ourBlip = API.GetEntitySharedData(player, "PLAYERBLIPS_MAIN_BLIP");

            if (ourBlip != null)
            {
                API.DeleteEntity(ourBlip);
            }
        }

        // EXPORTED METHODS

        public NetHandle getPlayerBlip(Client player)
        {
            if (!API.HasEntitySharedData(player, "PLAYERBLIPS_MAIN_BLIP")) return new NetHandle();

            var data = API.GetEntitySharedData(player, "PLAYERBLIPS_MAIN_BLIP");
            return (object)data == null ? new NetHandle() : data;
        }

        public void setPlayerBlip(Client player, NetHandle blip)
        {
            API.SetEntitySharedData(player, "PLAYERBLIPS_MAIN_BLIP", blip);
        }
    }

}