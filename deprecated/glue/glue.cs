using GTANetworkAPI;
using System.Linq;

namespace WipRagempResource.glue
{
    public class GlueScript : Script
    {
        [Command]
        public void Glue(Client sender)
        {
            if (API.IsEntityAttachedToAnything(sender.Handle))
            {
                API.DetachEntity(sender.Handle, false);
                API.SendChatMessageToPlayer(sender, "~g~Unglued!");
                return;
            }

            var vehicles = API.GetAllVehicles();
            var playerPos = API.GetEntityPosition(sender.Handle);

            if (vehicles.Count == 0)
            {
                API.SendChatMessageToPlayer(sender, "~r~ERROR: ~w~No nearby vehicles!");
                return;
            }

            var vOrd = vehicles.OrderBy(v => API.GetEntityPosition(v).DistanceToSquared(playerPos));
            var targetVehicle = vOrd.First();

            if (API.FetchNativeFromPlayer<bool>(sender, 0x17FFC1B2BA35A494, sender.Handle, targetVehicle))
            {
                var positionOffset = API.FetchNativeFromPlayer<Vector3>(sender, 0x2274BC1C4885E333, targetVehicle, playerPos.X, playerPos.Y, playerPos.Z);
                var rotOffset = API.GetEntityRotation(targetVehicle) - API.GetEntityRotation(sender.Handle);

                rotOffset = new Vector3(rotOffset.X, rotOffset.Y, rotOffset.Z * -1f);

                API.AttachEntityToEntity(sender.Handle, targetVehicle, null, positionOffset, rotOffset);

                API.SendChatMessageToPlayer(sender, "~g~Glued!");
            }
        }
    }
}