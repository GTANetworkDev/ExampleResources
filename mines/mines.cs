using GTANetworkAPI;

public class MinesScript : Script
{
    [ServerEvent(Event.ResourceStart)]
    public void MyResourceStart()
    {
        NAPI.Util.ConsoleOutput("Mines resource started!");
    }

    [Command("mine")]
    public void PlaceMine(Client sender, float mineRange = 10f)
    {
        var playerPos = NAPI.Entity.GetEntityPosition(sender);
        var playerDimension = NAPI.Entity.GetEntityDimension(sender);
        var minePropHash = NAPI.Util.GetHashKey("prop_bomb_01");
        var mineProp = NAPI.Object.CreateObject(minePropHash, playerPos - new Vector3(0, 0, 1f), new Vector3(), dimension: playerDimension);
        var colShape = NAPI.ColShape.CreateSphereColShape(playerPos, mineRange, playerDimension);
        var isMineArmed = false;

        colShape.OnEntityEnterColShape += (s, ent) =>
        {
            if (!isMineArmed) return;

            NAPI.Explosion.CreateOwnedExplosion(sender, ExplosionType.HiOctane, playerPos, 1f, playerDimension);
            NAPI.Entity.DeleteEntity(mineProp);
            NAPI.ColShape.DeleteColShape(colShape);
        };

        colShape.OnEntityExitColShape += (s, ent) =>
        {
            if (isMineArmed) return;

            isMineArmed = true;
            NAPI.Notification.SendNotificationToPlayer(sender, "Mine has been ~r~armed~w~!", true);
        };
    }
}
