using GTANetworkServer;
using System.Collections.Generic;
using System;
using GTANetworkShared;

namespace RPGResource.Citizen
{
    public class Taxi : Script
    {
        public Taxi()
        {

            API.onResourceStart += onResourceStart;

        }

        public ColShape Infotaxi;

        public void onResourceStart()
        {
            API.createMarker(1, new Vector3(895.6638, -179.3419, 73.70026), new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 255, 255, 255, 0);
            Blip Taxi = API.createBlip(new Vector3(895.6638, -179.3419, 74.70034));
            API.setBlipSprite(Taxi, 56);
            API.setBlipColor(Taxi, 5);
            API.setBlipName(Taxi, "Downtown Cab Co.");
            Infotaxi = API.createCylinderColShape(new Vector3(895.6638, -179.3419, 73.70026), 2f, 3f);

            Infotaxi.onEntityEnterColShape += (shape, entity) =>
            {
                Client player;
                if ((player = API.getPlayerFromHandle(entity)) != null)
                {
                    if (API.getEntityData(player, "IS_COP") == true)
                    {
                        API.sendChatMessageToPlayer(player, "~r~You are a cop you can't be a taxi driver");
                    }
                    else
                    {

                        API.sendChatMessageToPlayer(player, "~b~type /job to become a taxi driver");
                    }

                }


            };

        }
        Client sen;
        double senderxcoords, senderycoords;


        public void useTaxis(Client sender)
        {
            API.sendChatMessageToPlayer(sender, "~r~This task has already been taken");
            sen = sender;
           senderxcoords = API.getEntityPosition(sender.handle).X;
            senderycoords = API.getEntityPosition(sender.handle).Y;
            List<Client> taxiPlayers = new List<Client>();
            foreach (var driver in API.getAllPlayers())
            {
                if (API.getEntityData(driver, "TAXI") && API.getEntityData(driver, "TASK") != 1.623482)
                {
                    API.sendPictureNotificationToPlayer(driver, sender.name + " is requesting a taxi, would you like to take it?", "CHAR_TAXI", 0, 1, "Downtown Cab Co.", "Job");

                }
            }


        }
        public void accepted(Client driver, double d)
        {
            int i = 0;
            foreach (var driver2 in API.getAllPlayers())
            {
                if (API.getEntityData(driver2, "TASK") == d)
                {
                  
                    i = 1;
                }

            }

            if (i == 0)
            {

                API.sendChatMessageToPlayer(driver, "~g~You have accepted the task, a waypoint has been set to the client");
                API.triggerClientEvent(driver, "markonmap", senderxcoords, senderycoords);
                API.setEntityData(driver, "TASK", d);
                API.sendPictureNotificationToPlayer(sen, driver.name + " is coming to pick you up, please be patient and stay in your place", "CHAR_TAXI", 0, 1, "Downtown Cab Co.", "Message");

            }

        }
        public bool isincircle(NetHandle lit)
        {
            return Infotaxi.containsEntity(lit);

        }

    }

}