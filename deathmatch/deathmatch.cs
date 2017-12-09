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
    public static Random _rand = new Random();
    public static int KillTarget;

    public Deathmatch()
    {
        Event.OnPlayerConnected += OnPlayerConnected;
        Event.OnResourceStop += OnResourceStop;
        Event.OnResourceStart += OnResourceStart;
        Event.OnMapChange += OnMapChange;
        Event.OnPlayerDeath += OnPlayerDeath;
    }

    private void OnMapChange(string mapName, XmlGroup map)
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
            Weapons.Add(API.WeaponNameToModel(point.getElementData<string>("model")));
        }

        API.ResetIplList();

        var neededInteriors = map.getElementsByType("ipl");
        foreach (var point in neededInteriors)
        {
            API.RequestIpl(point.getElementData<string>("name"));
        }

        foreach (var player in API.GetAllPlayers())
        {
            var pBlip = API.Exported.playerblips.getPlayerBlip(player);
            API.SetBlipSprite(pBlip, 1);
            API.SetBlipColor(pBlip, 0);

            Spawn(player);
        }
    }

    private void OnResourceStart()
    {
        foreach (var player in API.GetAllPlayers())
        {
            Spawn(player);
            API.SetEntityData(player.Handle, "dm_score", 0);
            API.SetEntityData(player.Handle, "dm_deaths", 0);
            API.SetEntityData(player.Handle, "dm_kills", 0);
            API.SetEntityData(player.Handle, "dm_kdr", 0);
        }

        KillTarget = API.GetSetting<int>(this, "victory_kills");
    }

    private void OnResourceStop()
    {
        foreach (var player in API.GetAllPlayers())
        {
            var pBlip = API.Exported.playerblips.getPlayerBlip(player);

            API.SetBlipSprite(pBlip, 1);
            API.SetBlipColor(pBlip, 0);

            API.ResetEntityData(player.Handle, "dm_score");
            API.ResetEntityData(player.Handle, "dm_deaths");
            API.ResetEntityData(player.Handle, "dm_kills");
            API.ResetEntityData(player.Handle, "dm_kdr");
        }
    }

    public void OnPlayerConnected(Client player, CancelEventArgs e)
    {
        API.SetEntityData(player.Handle, "dm_score", 0);
        API.SetEntityData(player.Handle, "dm_deaths", 0);
        API.SetEntityData(player.Handle, "dm_kills", 0);
        API.SetEntityData(player.Handle, "dm_kdr", 0);
        e.Spawn = false;
        Spawn(player);
    }

    public void Spawn(Client player)
    {
        var randSpawn = Spawns[_rand.Next(Spawns.Count)];
        API.SpawnPlayer(player, randSpawn);

        API.RemoveAllPlayerWeapons(player);
        Weapons.ForEach(gun => API.GivePlayerWeapon(player, gun, 500));

        API.SetPlayerHealth(player, 100);
    }

    public void OnPlayerDeath(Client player, NetHandle entitykiller, uint weapon, CancelEventArgs e)
    {
        e.Cancel = true;
        Client killer = null;

        if (!entitykiller.IsNull)
        {
            foreach (var ply in API.GetAllPlayers())
            {
                if (ply.Handle != entitykiller) continue;
                killer = ply;
                break;
            }
        }

        API.SetEntityData(player.Handle, "dm_score", API.GetEntitySharedData(player.Handle, "dm_score") - 1);
        API.SetEntityData(player.Handle, "dm_deaths", API.GetEntitySharedData(player.Handle, "dm_deaths") + 1);
        API.SetEntityData(player.Handle, "dm_kdr", API.GetEntitySharedData(player.Handle, "dm_kills") / (float)API.GetEntitySharedData(player.Handle, "dm_deaths"));

        if (killer != null)
        {
            API.SetEntityData(killer.Handle, "dm_kills", API.GetEntitySharedData(killer.Handle, "dm_kills") + 1);
            API.SetEntityData(killer.Handle, "dm_score", API.GetEntitySharedData(killer.Handle, "dm_score") + 1);
            if (API.GetEntitySharedData(killer.Handle, "dm_deaths") != 0)
            {
                API.SetEntityData(killer.Handle, "dm_kdr", API.GetEntitySharedData(killer.Handle, "dm_kills") / (float)API.GetEntitySharedData(killer.Handle, "dm_deaths"));
            }

            if (API.GetEntitySharedData(killer.Handle, "dm_kills") >= KillTarget)
            {
                API.SendChatMessageToAll($"~b~~h~{killer.Name}~h~~w~ has won the round with ~h~{KillTarget}~h~ kills and {API.GetEntitySharedData(player.Handle, "dm_deaths")} deaths!");
                //API.Exported.mapcycler.endRound();
            }

            if (Killstreaks.ContainsKey(killer))
            {
                Killstreaks[killer]++;
                if (Killstreaks[killer] >= 3)
                {
                    var kBlip = API.Exported.playerblips.getPlayerBlip(killer);
                    API.SetBlipSprite(kBlip, 303);
                    API.SetBlipColor(kBlip, 1);

                    API.SendChatMessageToAll($"~b~{killer.Name}~w~ is on a killstreak! ~r~{Killstreaks[killer]}~w~ kills and counting!");
                    switch (Killstreaks[killer])
                    {
                        case 4:
                            API.SetPlayerHealth(killer, Math.Min(100, API.GetPlayerHealth(killer) + 25));
                            API.SendChatMessageToPlayer(killer, "~g~Health bonus!");
                            break;

                        case 6:
                            API.SetPlayerHealth(killer, Math.Min(100, API.GetPlayerHealth(killer) + 50));
                            API.SendChatMessageToPlayer(killer, "~g~Health bonus!");
                            break;

                        case 8:
                            API.SetPlayerHealth(killer, Math.Min(100, API.GetPlayerHealth(killer) + 75));
                            API.SetPlayerArmor(killer, Math.Min(100, API.GetPlayerArmor(killer) + 25));
                            API.SendChatMessageToPlayer(killer, "~g~Health and armor bonus!");
                            break;

                        case 12:
                            API.SetPlayerHealth(killer, Math.Min(100, API.GetPlayerHealth(killer) + 75));
                            API.SetPlayerArmor(killer, Math.Min(100, API.GetPlayerArmor(killer) + 50));
                            API.SendChatMessageToPlayer(killer, "~g~Health and armor bonus!");
                            break;

                        default:
                            if (Killstreaks[killer] >= 16 && Killstreaks[killer] % 4 == 0)
                            {
                                API.SetPlayerHealth(killer, Math.Min(100, API.GetPlayerHealth(killer) + 75));
                                API.SetPlayerArmor(killer, Math.Min(100, API.GetPlayerArmor(killer) + 75));
                                API.SendChatMessageToPlayer(killer, "~g~Health and armor bonus!");
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

        var pBlip = API.Exported.playerblips.getPlayerBlip(player);
        if (Killstreaks.ContainsKey(player))
        {
            if (Killstreaks[player] >= 3 && killer != null)
            {
                API.SendChatMessageToAll($"~b~{killer.Name}~w~ ruined ~r~{player.Name}~w~'s killstreak!");
                API.SetBlipColor(pBlip, 0);
                API.SetBlipSprite(pBlip, 1);
            }
            Killstreaks[player] = 0;
        }
        else
        {
            Killstreaks.Add(player, 0);
        }

        API.SetBlipSprite(pBlip, 274);
        Spawn(player);
    }
}