using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkServer;
using GTANetworkShared;
using RPGResource.Cops;

namespace RPGResource.Global
{
    public class GlobalCommands : Script
    {
        [Flags]
        public enum AnimationFlags
        {
            Loop = 1 << 0,
            StopOnLastFrame = 1 << 1,
            OnlyAnimateUpperBody = 1 << 4,
            AllowPlayerControl = 1 << 5,
            Cancellable = 1 << 7
        }

        [Command("stats", Group = "Global Commands")]
        public void GetStatistics(Client sender)
        {
            API.sendChatMessageToPlayer(sender, "_____STATS_____");

            API.sendChatMessageToPlayer(sender, string.Format("~h~Name:~h~ {0} ~h~Class:~h~ {1} ~h~Level:~h~ {2}", sender.name,
                API.getEntityData(sender, "IS_COP") == true ? "Cop" : "Citizen",
                (int)API.getEntityData(sender, "Level")));

            if (API.getEntityData(sender, "WantedLevel") > 0)
            {
                var crimes = (List<int>)API.getEntityData(sender, "Crimes");

                string crimeList = string.Join(", ", crimes.Select(i => WantedLevelDataProvider.Crimes.Get(i).Name));

                if (API.getEntityData(sender, "IS_COP") != true)
                {
                    API.sendChatMessageToPlayer(sender,
                        string.Format("~h~Wanted Level:~h~ ~b~{0}~w~~h~Crimes~h~: {1}",
                            Util.Repeat("* ", API.getEntityData(sender, "WantedLevel")),
                            crimeList
                            ));

                    // TODO: Skills
                }
                else
                {
                    // TODO: Cop ranks, experience
                }
            }
        }

        [Command("loc")]
        public void ShowLocationCommand(Client sender)
        {
            var pos = API.getEntityPosition(sender.handle);

            API.sendChatMessageToPlayer(sender, pos.X + ", " + pos.Y + ", " + pos.Z);
        }

        [Command("me")]
        public void emotes(Client sender, string text)
        {
            string name = "~p~" + sender.name +" " + text;
            API.sendChatMessageToAll(name);

        }

        [Command("pm")]
        public void message(Client sender, Client reciever, string text)
        {
            API.sendChatMessageToPlayer(reciever, "~r~"+ sender.name + " tells you ", text);

        }

        [Command("shout")]
        public void shout(Client sender, string text)
        {

            API.sendChatMessageToAll("~h~~b~" + sender.name + " shouts " + text);

        }

        [Command("give")]
        public void givemoney (Client sender,Client player,int amount) 
         {

            if (API.getEntityData(sender, "Money") > amount && amount>0)
            {

                API.setEntityData(player, "Money", amount+API.getEntityData(player, "Money"));
                API.sendChatMessageToPlayer(player, "~g~"+sender.name+" Sent you "+amount);
                API.setEntityData(sender, "Money", API.getEntityData(sender, "Money")-amount);
            }

            else API.sendChatMessageToPlayer(sender, "~r~You don't have enough money");

         }

        [Command("help")]
        public void help (Client sender)
        {

            API.sendChatMessageToPlayer(sender, "/me /pm /kill /shout /give /stop /surrender /time /stats");

        }

        [Command ("surrender")]
        public void surrender (Client sender)
        {

            API.playPlayerAnimation(sender, (int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "mp_am_hold_up", "handsup_base");
            sender.removeAllWeapons();
        }

        [Command("stop")]
        public void stopanimation (Client sender)
        {
            API.stopPlayerAnimation(sender);

        }

        [Command("time")]
        public void GetTimeCommand(Client sender)
        {
            sender.sendChatMessage("~b~Time: " + API.getTime());
        }
    }
  
}