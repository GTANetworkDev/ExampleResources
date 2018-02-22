using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;

public struct RespawnablePickup
{
    public int Hash;
    public int Amount;
    public int PickupTime;
    public int RespawnTime;
    public Vector3 Position;
}

public class Deathmatch : Script
{
    public static List<Vector3> Spawns = new List<Vector3>
    {
        new Vector3(1482.36, 3587.45, 35.39),
        new Vector3(1613.67, 3560.03, 35.42),
        new Vector3(1533.44, 3581.24, 38.73),
        new Vector3(1576.09, 3607.35, 38.73),
        new Vector3(1596.88, 3590.43, 42.12)
    };

    public static List<WeaponHash> Weapons = new List<WeaponHash> { WeaponHash.MicroSMG, WeaponHash.PumpShotgun, WeaponHash.CarbineRifle };
    public static Dictionary<Client, int> Killstreaks = new Dictionary<Client, int>();
    public static Random Rand = new Random();
    public static int KillTarget;

    public Deathmatch()
    {
        NAPI.Server.SetAutoSpawnOnConnect(false);
        NAPI.Server.SetAutoRespawnAfterDeath(false);
    }

    [ServerEvent(Event.MapChange)]
    public void OnMapChange(string mapName, XmlGroup map)
    {
        Console.WriteLine("OnMapChange");
        Spawns.Clear();
        Weapons.Clear();
        Killstreaks.Clear();
        var spawnpoints = map.getElementsByType("spawnpoint");
        foreach (var point in spawnpoints)
        {
            Spawns.Add(new Vector3(point.getElementData<float>("posX"), point.getElementData<float>("posY"), point.getElementData<float>("posZ")));
        }

        var availableGuns = map.getElementsByType("weapon");
        foreach (var point in availableGuns)
        {
            Weapons.Add(NAPI.Util.WeaponNameToModel(point.getElementData<string>("model")));
        }

        NAPI.World.ResetIplList();

        var neededInteriors = map.getElementsByType("ipl");
        foreach (var point in neededInteriors)
        {
            NAPI.World.RequestIpl(point.getElementData<string>("name"));
        }

        foreach (var player in NAPI.Pools.GetAllPlayers())
        {
            var pBlip = NAPI.Exported.playerblips.getPlayerBlip(player);
            NAPI.Blip.SetBlipSprite(pBlip, 1);
            NAPI.Blip.SetBlipColor(pBlip, 0);

            Spawn(player);
        }
    }

    [ServerEvent(Event.ResourceStart)]
    public void OnResourceStart()
    {
        foreach (var player in NAPI.Pools.GetAllPlayers())
        {
            Spawn(player);
            NAPI.Data.SetEntityData(player.Handle, "dm_score", 0);
            NAPI.Data.SetEntityData(player.Handle, "dm_deaths", 0);
            NAPI.Data.SetEntityData(player.Handle, "dm_kills", 0);
            NAPI.Data.SetEntityData(player.Handle, "dm_kdr", 0);
        }

        KillTarget = NAPI.Resource.GetSetting<int>(this, "victory_kills");
    }

    [ServerEvent(Event.ResourceStop)]
    public void OnResourceStop()
    {
        foreach (var player in NAPI.Pools.GetAllPlayers())
        {
            var pBlip = NAPI.Exported.playerblips.getPlayerBlip(player);

            NAPI.Blip.SetBlipSprite(pBlip, 1);
            NAPI.Blip.SetBlipColor(pBlip, 0);

            NAPI.Data.ResetEntityData(player.Handle, "dm_score");
            NAPI.Data.ResetEntityData(player.Handle, "dm_deaths");
            NAPI.Data.ResetEntityData(player.Handle, "dm_kills");
            NAPI.Data.ResetEntityData(player.Handle, "dm_kdr");
        }
    }

    [ServerEvent(Event.PlayerConnected)]
    public void OnPlayerConnected(Client player)
    {
        NAPI.Data.SetEntityData(player.Handle, "dm_score", 0);
        NAPI.Data.SetEntityData(player.Handle, "dm_deaths", 0);
        NAPI.Data.SetEntityData(player.Handle, "dm_kills", 0);
        NAPI.Data.SetEntityData(player.Handle, "dm_kdr", 0);
        Spawn(player);
    }

    public void Spawn(Client player)
    {
        var randSpawn = Spawns[Rand.Next(Spawns.Count)];
        NAPI.Player.SpawnPlayer(player, randSpawn);

        NAPI.Player.RemoveAllPlayerWeapons(player);
        Weapons.ForEach(gun => NAPI.Player.GivePlayerWeapon(player, gun, 500));

        NAPI.Player.SetPlayerHealth(player, 100);
    }

    [ServerEvent(Event.PlayerDeath)]
    public void OnPlayerDeath(Client player, NetHandle entitykiller, uint weapon)
    {
        Client killer = null;

        if (!entitykiller.IsNull)
        {
            foreach (var ply in NAPI.Pools.GetAllPlayers())
            {
                if (ply.Handle != entitykiller) continue;
                killer = ply;
                break;
            }
        }

        NAPI.Data.SetEntityData(player.Handle, "dm_score", NAPI.Data.GetEntitySharedData(player.Handle, "dm_score") - 1);
        NAPI.Data.SetEntityData(player.Handle, "dm_deaths", NAPI.Data.GetEntitySharedData(player.Handle, "dm_deaths") + 1);
        NAPI.Data.SetEntityData(player.Handle, "dm_kdr", NAPI.Data.GetEntitySharedData(player.Handle, "dm_kills") / (float)NAPI.Data.GetEntitySharedData(player.Handle, "dm_deaths"));

        if (killer != null)
        {
            NAPI.Data.SetEntityData(killer.Handle, "dm_kills", NAPI.Data.GetEntitySharedData(killer.Handle, "dm_kills") + 1);
            NAPI.Data.SetEntityData(killer.Handle, "dm_score", NAPI.Data.GetEntitySharedData(killer.Handle, "dm_score") + 1);
            if (NAPI.Data.GetEntitySharedData(killer.Handle, "dm_deaths") != 0)
            {
                NAPI.Data.SetEntityData(killer.Handle, "dm_kdr", NAPI.Data.GetEntitySharedData(killer.Handle, "dm_kills") / (float)NAPI.Data.GetEntitySharedData(killer.Handle, "dm_deaths"));
            }

            if (NAPI.Data.GetEntitySharedData(killer.Handle, "dm_kills") >= KillTarget)
            {
                NAPI.Chat.SendChatMessageToAll($"~b~~h~{killer.Name}~h~~w~ has won the round with ~h~{KillTarget}~h~ kills and {NAPI.Data.GetEntitySharedData(player.Handle, "dm_deaths")} deaths!");
                //API.Exported.mapcycler.endRound();
            }

            if (Killstreaks.ContainsKey(killer))
            {
                Killstreaks[killer]++;
                if (Killstreaks[killer] >= 3)
                {
                    var kBlip = NAPI.Exported.playerblips.getPlayerBlip(killer);
                    NAPI.Blip.SetBlipSprite(kBlip, 303);
                    NAPI.Blip.SetBlipColor(kBlip, 1);

                    NAPI.Chat.SendChatMessageToAll($"~b~{killer.Name}~w~ is on a killstreak! ~r~{Killstreaks[killer]}~w~ kills and counting!");
                    switch (Killstreaks[killer])
                    {
                        case 4:
                            NAPI.Player.SetPlayerHealth(killer, Math.Min(100, NAPI.Player.GetPlayerHealth(killer) + 25));
                            NAPI.Chat.SendChatMessageToPlayer(killer, "~g~Health bonus!");
                            break;

                        case 6:
                            NAPI.Player.SetPlayerHealth(killer, Math.Min(100, NAPI.Player.GetPlayerHealth(killer) + 50));
                            NAPI.Chat.SendChatMessageToPlayer(killer, "~g~Health bonus!");
                            break;

                        case 8:
                            NAPI.Player.SetPlayerHealth(killer, Math.Min(100, NAPI.Player.GetPlayerHealth(killer) + 75));
                            NAPI.Player.SetPlayerArmor(killer, Math.Min(100, NAPI.Player.GetPlayerArmor(killer) + 25));
                            NAPI.Chat.SendChatMessageToPlayer(killer, "~g~Health and armor bonus!");
                            break;

                        case 12:
                            NAPI.Player.SetPlayerHealth(killer, Math.Min(100, NAPI.Player.GetPlayerHealth(killer) + 75));
                            NAPI.Player.SetPlayerArmor(killer, Math.Min(100, NAPI.Player.GetPlayerArmor(killer) + 50));
                            NAPI.Chat.SendChatMessageToPlayer(killer, "~g~Health and armor bonus!");
                            break;

                        default:
                            if (Killstreaks[killer] >= 16 && Killstreaks[killer] % 4 == 0)
                            {
                                NAPI.Player.SetPlayerHealth(killer, Math.Min(100, NAPI.Player.GetPlayerHealth(killer) + 75));
                                NAPI.Player.SetPlayerArmor(killer, Math.Min(100, NAPI.Player.GetPlayerArmor(killer) + 75));
                                NAPI.Chat.SendChatMessageToPlayer(killer, "~g~Health and armor bonus!");
                            }
                            break;
                    }
                }
            }
            else
            {
                Killstreaks.Add(killer, 1);
            }
        }

        var pBlip = NAPI.Exported.playerblips.getPlayerBlip(player);
        if (Killstreaks.ContainsKey(player))
        {
            if (Killstreaks[player] >= 3 && killer != null)
            {
                NAPI.Chat.SendChatMessageToAll($"~b~{killer.Name}~w~ ruined ~r~{player.Name}~w~'s killstreak!");
                NAPI.Blip.SetBlipColor(pBlip, 0);
                NAPI.Blip.SetBlipSprite(pBlip, 1);
            }
            Killstreaks[player] = 0;
        }
        else
        {
            Killstreaks.Add(player, 0);
        }

        NAPI.Blip.SetBlipSprite(pBlip, 274);
        Spawn(player);
    }
}