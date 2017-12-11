using System;
using GTANetworkAPI;

namespace WipRagempResource.admin
{
    public class AdminScript : Script
    {
        public AdminScript()
        {
            Event.OnPlayerConnected += Event_OnPlayerConnected;
            Event.OnPlayerDisconnected += EventOnOnPlayerDisconnected;
        }

        #region Events
        
        private void EventOnOnPlayerDisconnected(Client player, byte type, string reason)
        {
            API.LogoutPlayer(player);
        }

        private void Event_OnPlayerConnected(Client player, CancelEventArgs cancel)
        {
            var log = API.LoginPlayer(player, "");
            if (log == LoginResult.LoginSuccessful || log == LoginResult.LoginSuccessfulNoPassword)
            {
                API.SendChatMessageToPlayer(player, "Logged in as ~b~" + API.GetPlayerAclGroup(player) + "~w~.");
            }
            else if (log == LoginResult.WrongPassword)
            {
                API.SendChatMessageToPlayer(player, "Please log in with ~b~/login [password]");
            }
        }

        #endregion

        #region Commands

        [Command(SensitiveInfo = true, ACLRequired = true)]
        public void Login(Client sender, string password)
        {
            var logResult = API.LoginPlayer(sender, password);
            switch (logResult)
            {
                case LoginResult.NoAccountFound:
                    API.SendChatMessageToPlayer(sender, "~r~ERROR:~w~ No account found with your name.");
                    break;

                case LoginResult.LoginSuccessfulNoPassword:
                case LoginResult.LoginSuccessful:
                    API.SendChatMessageToPlayer(sender, "~g~Login successful!~w~ Logged in as ~b~" + API.GetPlayerAclGroup(sender) + "~w~.");
                    break;
                case LoginResult.WrongPassword:
                    API.SendChatMessageToPlayer(sender, "~r~ERROR:~w~ Wrong password!");
                    break;
                case LoginResult.AlreadyLoggedIn:
                    API.SendChatMessageToPlayer(sender, "~r~ERROR:~w~ You're already logged in!");
                    break;
                case LoginResult.ACLDisabled:
                    API.SendChatMessageToPlayer(sender, "~r~ERROR:~w~ ACL has been disabled on this server.");
                    break;
            }
        }

        [Command(ACLRequired = true)]
        public void SetTime(Client sender, int hours, int minutes)
        {
            API.SetTime(hours, minutes, 0);
        }

        [Command(ACLRequired = true)]
        public void SetWeather(Client sender, int newWeather)
        {
            API.SetWeather(newWeather);
        }

        [Command(ACLRequired = true)]
        public void Logout(Client sender)
        {
            API.LogoutPlayer(sender);
        }

        [Command(ACLRequired = true)]
        public void Start(Client sender, string resource)
        {
            if (!API.DoesResourceExist(resource))
            {
                API.SendChatMessageToPlayer(sender, "~r~No such resource found: \"" + resource + "\"");
            }
            else if (API.IsResourceRunning(resource))
            {
                API.SendChatMessageToPlayer(sender, "~r~Resource \"" + resource + "\" is already running!");
            }
            else
            {
                API.StartResource(resource);
                API.SendChatMessageToPlayer(sender, "~g~Started resource \"" + resource + "\"");
            }
        }

        [Command(ACLRequired = true)]
        public void Stop(Client sender, string resource)
        {
            if (!API.DoesResourceExist(resource))
            {
                API.SendChatMessageToPlayer(sender, "~r~No such resource found: \"" + resource + "\"");
            }
            else if (!API.IsResourceRunning(resource))
            {
                API.SendChatMessageToPlayer(sender, "~r~Resource \"" + resource + "\" is not running!");
            }
            else
            {
                API.StopResource(resource);
                API.SendChatMessageToPlayer(sender, "~g~Stopped resource \"" + resource + "\"");
            }
        }

        [Command(ACLRequired = true)]
        public void Restart(Client sender, string resource)
        {
            if (API.DoesResourceExist(resource))
            {
                API.StopResource(resource);
                API.StartResource(resource);
                API.SendChatMessageToPlayer(sender, "~g~Restarted resource \"" + resource + "\"");
            }
            else
            {
                API.SendChatMessageToPlayer(sender, "~r~No such resource found: \"" + resource + "\"");
            }
        }

        [Command(GreedyArg = true, ACLRequired = true)]
        public void Kick(Client sender, Client target, string reason)
        {
            API.KickPlayer(target, reason);
        }

        [Command(ACLRequired = true)]
        public void Kill(Client sender, Client target)
        {
            API.SetPlayerHealth(target, -1);
        }
        #endregion
    }
}