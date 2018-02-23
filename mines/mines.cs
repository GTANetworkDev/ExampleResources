namespace WipRagempResource.mines
{
    using GTANetworkAPI;

    public class MinesTest : Script
    {
        public MinesTest()
        {

        }

        [ServerEvent(Event.ResourceStart)]
        public void MyResourceStart()
        {
            NAPI.Util.ConsoleOutput("Starting mines!");
        }

        [Command("mine")]
        public void PlaceMine(Client sender, float MineRange = 10f)
        {
            var pos = NAPI.Entity.GetEntityPosition(sender);
            var playerDimension = NAPI.Entity.GetEntityDimension(sender);
            var prop = NAPI.Object.CreateObject((int) NAPI.Util.GetHashKey("prop_bomb_01"), pos - new Vector3(0, 0, 1f), (Quaternion) new Vector3(), playerDimension);
            var shape = NAPI.ColShape.CreateSphereColShape(pos, MineRange);
            shape.Dimension = playerDimension;

            bool mineArmed = false;

            shape.OnEntityEnterColShape += (s, ent) =>
            {
                if (!mineArmed) return;
                NAPI.Explosion.CreateOwnedExplosion(sender, ExplosionType.HiOctane, pos, 1f, playerDimension);
                NAPI.Entity.DeleteEntity(prop);
                NAPI.ColShape.DeleteColShape(shape);
            };

            shape.OnEntityExitColShape += (s, ent) =>
            {
                if (ent == sender.Handle && !mineArmed)
                {
                    mineArmed = true;
                    NAPI.Notification.SendNotificationToPlayer(sender, "Mine has been ~r~armed~w~!", true);
                }
            };
        }
    }
}