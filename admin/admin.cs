using System;
using GTANetworkAPI;

namespace Main
{
    public class AdminScript : Script
    {
        #region Events
        [ServerEvent(Event.PlayerConnected)]
        public void OnPlayerConnected(Client player)
        {
            var log = NAPI.ACL.LoginPlayer(player, "");
            if (log == LoginResult.LoginSuccessful || log == LoginResult.LoginSuccessfulNoPassword)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Logged in as ~b~" + NAPI.ACL.GetPlayerAclGroup(player) + "~w~.");
            }
            else if (log == LoginResult.WrongPassword)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Please log in with ~b~/login [password]");
            }
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnected(Client player, DisconnectionType type, string reason)
        {
            NAPI.ACL.LogoutPlayer(player);
        }
        #endregion

        #region Commands
        [Command(SensitiveInfo = true, ACLRequired = true)]
        public void Login(Client sender, string password)
        {
            var logResult = NAPI.ACL.LoginPlayer(sender, password);
            switch (logResult)
            {
                case LoginResult.NoAccountFound:
                    NAPI.Chat.SendChatMessageToPlayer(sender, "~r~ERROR:~w~ No account found with your name.");
                    break;

                case LoginResult.LoginSuccessfulNoPassword:
                case LoginResult.LoginSuccessful:
                    NAPI.Chat.SendChatMessageToPlayer(sender, "~g~Login successful!~w~ Logged in as ~b~" + NAPI.ACL.GetPlayerAclGroup(sender) + "~w~.");
                    break;
                case LoginResult.WrongPassword:
                    NAPI.Chat.SendChatMessageToPlayer(sender, "~r~ERROR:~w~ Wrong password!");
                    break;
                case LoginResult.AlreadyLoggedIn:
                    NAPI.Chat.SendChatMessageToPlayer(sender, "~r~ERROR:~w~ You're already logged in!");
                    break;
                case LoginResult.ACLDisabled:
                    NAPI.Chat.SendChatMessageToPlayer(sender, "~r~ERROR:~w~ ACL has been disabled on this server.");
                    break;
            }
        }

        [Command(ACLRequired = true)]
        public void SetTime(Client sender, int hours, int minutes)
        {
            NAPI.World.SetTime(hours, minutes, 0);
        }

        [Command(ACLRequired = true)]
        public void SetWeather(Client sender, int newWeather)
        {
            NAPI.World.SetWeather((Weather)newWeather);
        }

        [Command(ACLRequired = true)]
        public void Logout(Client sender)
        {
            NAPI.ACL.LogoutPlayer(sender);
        }

        [Command(ACLRequired = true)]
        public void Start(Client sender, string resource)
        {
            if (!NAPI.Resource.DoesResourceExist(resource))
            {
                NAPI.Chat.SendChatMessageToPlayer(sender, "~r~No such resource found: \"" + resource + "\"");
            }
            else if (NAPI.Resource.IsResourceRunning(resource))
            {
                NAPI.Chat.SendChatMessageToPlayer(sender, "~r~Resource \"" + resource + "\" is already running!");
            }
            else
            {
                NAPI.Resource.StartResource(resource);
                NAPI.Chat.SendChatMessageToPlayer(sender, "~g~Started resource \"" + resource + "\"");
            }
        }

        [Command(ACLRequired = true)]
        public void Stop(Client sender, string resource)
        {
            if (!NAPI.Resource.DoesResourceExist(resource))
            {
                NAPI.Chat.SendChatMessageToPlayer(sender, "~r~No such resource found: \"" + resource + "\"");
            }
            else if (!NAPI.Resource.IsResourceRunning(resource))
            {
                NAPI.Chat.SendChatMessageToPlayer(sender, "~r~Resource \"" + resource + "\" is not running!");
            }
            else
            {
                NAPI.Resource.StopResource(resource);
                NAPI.Chat.SendChatMessageToPlayer(sender, "~g~Stopped resource \"" + resource + "\"");
            }
        }

        [Command(ACLRequired = true)]
        public void Restart(Client sender, string resource)
        {
            if (NAPI.Resource.DoesResourceExist(resource))
            {
                NAPI.Resource.StopResource(resource);
                NAPI.Resource.StartResource(resource);
                NAPI.Chat.SendChatMessageToPlayer(sender, "~g~Restarted resource \"" + resource + "\"");
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(sender, "~r~No such resource found: \"" + resource + "\"");
            }
        }

        [Command(GreedyArg = true, ACLRequired = true)]
        public void Kick(Client sender, Client target, string reason)
        {
            NAPI.Player.KickPlayer(target, reason);
        }

        [Command(ACLRequired = true)]
        public void Kill(Client sender, Client target)
        {
            NAPI.Player.SetPlayerHealth(target, -1);
        }
        #endregion
    }
}