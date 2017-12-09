namespace WipRagempResource.mines
{
    using GTANetworkAPI;

    public class MinesTest : Script
    {
        public MinesTest()
        {
            Event.OnResourceStart += myResourceStart;
        }

        public void myResourceStart()
        {
            API.ConsoleOutput("Starting mines!");
        }

        [Command("mine")]
        public void PlaceMine(Client sender, float MineRange = 10f)
        {
            var pos = API.GetEntityPosition(sender);
            var playerDimension = API.GetEntityDimension(sender);

            var prop = API.CreateObject(API.GetHashKey("prop_bomb_01"), pos - new Vector3(0, 0, 1f), new Vector3(), playerDimension);
            var shape = API.CreateSphereColShape(pos, MineRange);
            shape.Dimension = playerDimension;

            bool mineArmed = false;

            shape.OnEntityEnterColShape += (s, ent) =>
            {
                if (!mineArmed) return;
                API.CreateOwnedExplosion(sender, ExplosionType.HiOctane, pos, 1f, playerDimension);
                API.DeleteEntity(prop);
                API.DeleteColShape(shape);
            };

            shape.OnEntityExitColShape += (s, ent) =>
            {
                if (ent == sender.Handle && !mineArmed)
                {
                    mineArmed = true;
                    API.SendNotificationToPlayer(sender, "Mine has been ~r~armed~w~!", true);
                }
            };
        }
    }
}