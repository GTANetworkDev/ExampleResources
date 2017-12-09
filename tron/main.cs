using System;
using GTANetworkAPI;


public class Tron : Script
{
    public Tron()
    {
        Event.OnUpdate += update;
    }

    public void update()
    {
        var players = API.GetAllPlayers();

        foreach(var player in players)
        {
            if (!API.IsPlayerInAnyVehicle(player) ||
                API.GetEntityModel(API.GetPlayerVehicle(player)) != 3889340782) // Shotaro
                continue;

            Vector3 _lastPos;
            if ((_lastPos = API.GetEntityData(player, "TRON_LAST_PLACED_POS")) == null)
            {
                API.SetEntityData(player, "TRON_LAST_PLACED_POS", API.GetEntityPosition(player));
                continue;
            }

            Vector3 currentPos = API.GetEntityPosition(player);

            if (_lastPos.DistanceToSquared(currentPos) > 25f)
            {
                var dir = currentPos - _lastPos;
                dir.Normalize();
                var radAtan = -Math.Atan2(dir.X, dir.Y);
                var heading = (float)(radAtan * 180f / Math.PI);
                heading += 90f;

                API.SetEntityData(player, "TRON_LAST_PLACED_POS", _lastPos + dir*4f);

                API.CreateObject(API.GetHashKey("prop_const_fence01b_cr"), _lastPos + dir*2f - new Vector3(0, 0, 2f), new Vector3(0, 0, heading));
            }
        }
    }
}
