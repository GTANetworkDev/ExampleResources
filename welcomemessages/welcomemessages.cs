using GTANetworkAPI;

public class WelcomeMsgs : Script
{
    public WelcomeMsgs()
    {
        Event.OnPlayerConnected += onPlayerConnect;
        Event.OnPlayerDisconnected += onPlayerDisconnect;
    }

    public void onPlayerConnect(Client player, CancelEventArgs e)
    {
        API.SendNotificationToAll("~b~~h~" + player.Name + "~h~ ~w~joined.");
        API.SendChatMessageToAll("~b~~h~" + player.Name + "~h~~w~ has joined the server.");
    }

    public void onPlayerDisconnect(Client player, byte type, string reason)
    {
        API.SendNotificationToAll("~b~~h~" + player.Name + "~h~ ~w~quit.");
        switch (type)
        {
            case 1:
                API.SendChatMessageToAll("~b~~h~" + player.Name + "~h~~w~ has quit the server.");
                break;
            case 2:
                API.SendChatMessageToAll("~b~~h~" + player.Name + "~h~~w~ has timed out.");
                break;
            case 3:
                API.SendChatMessageToAll("~b~~h~" + player.Name + "~h~~w~ has been kicked for ~r~" + reason);
                break;
        }
    }
}
