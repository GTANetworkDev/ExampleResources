using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GTANetworkAPI;

public class HunterScript : Script
{
    public HunterScript()
    {
        Event.OnResourceStart += startGM;
        Event.OnResourceStop += stopGm;
        Event.OnUpdate += update;
        Event.OnPlayerDeath += dead;
        //Event.OnPlayerRespawn += respawn;
        Event.OnPlayerConnected += (player, cancel) => respawn(player);
        Event.OnPlayerDisconnected += playerleft;
        Event.OnPlayerConnected += (player, cancel) => 
        {
            if (API.GetAllPlayers().Count == 1 && !roundstarted)
            {
                roundstart();
            }
        };
    }

    private Vector3 _quadSpawn = new Vector3(-1564.36f, 4499.71f, 21.37f);
    private NetHandle _quad;

    private List<Vector3> _animalSpawnpoints = new List<Vector3>
    {
        new Vector3(-1488.76f, 4720.44f, 46.22f),
        new Vector3(-1317.07f, 4642.76f, 108.31f),
        new Vector3(-1291.08f, 4496.54f, 14.7f),
        new Vector3(-1166.29f, 4409.41f, 8.87f),
        new Vector3(-1522.33f, 4418.05f, 12.2f),
        new Vector3(-1597.47f, 4483.42f, 17.4f),
    };

    private List<Vector3> _hunterSpawnpoints = new List<Vector3>
    {
        new Vector3(-1592.35f, 4486.51f, 17.57f),
        new Vector3(-1572.83f, 4471.25f, 13.92f),
        new Vector3(-1559.22f, 4467.11f, 18.74f),
        new Vector3(-1515.67f, 4482.07f, 17.56f),
        new Vector3(-1538.81f, 4443.99f, 11.53f),
    };

    private List<PedHash> _skinList = new List<PedHash>
    {
        PedHash.Hunter,
        PedHash.Hillbilly01AMM,
        PedHash.Hillbilly02AMM,
        PedHash.Cletus,
        PedHash.OldMan1aCutscene,
    };

    private List<Vector3> _checkpointPoses = new List<Vector3>
    {
        new Vector3(-1567.13f, 4541.15f, 17.26f),
        new Vector3(-1582.11f, 4654.32f, 46.93f),
        new Vector3(-1607.65f, 4369.2f, 2.45f),
        new Vector3(-1336.23f, 4404.12f, 31.91f),
        new Vector3(-1289.52f, 4498.14f, 14.8f),
        new Vector3(-1268.58f, 4690.92f, 83.74f),
    };

    private List<NetHandle> _checkpoints = new List<NetHandle>();
    private List<NetHandle> _checkpointBlips = new List<NetHandle>();

    public bool roundstarted;
    public Client animal;
    private long roundStart;
    private long lastIdleCheck;
    private Vector3 lastIdlePosition;
    private Client hawk;

    private const int TEAM_ANIMAL = 2;
    private const int TEAM_HUNTER = 1;

    private Random r = new Random();
    public void startGM()
    {
        roundstart();
    }


    public void stopGm()
    {
        var players = API.GetAllPlayers();

        foreach (var player in players)
        {
            var pBlip = API.Exported.playerblips.getPlayerBlip(player);

            API.SetBlipTransparency(pBlip, 255);
            API.SetBlipSprite(pBlip, 1);
            API.SetBlipColor(pBlip, 0);
            API.SetPlayerTeam(player, 0);
            API.SetEntityInvincible(player, false);
        }
    }

    public void playerleft(Client player, byte type, string reason)
    {
        if (player == animal)
        {
            API.SendChatMessageToAll("The animal has left! Restarting...");
            animal = null;
            roundstarted = false;
            roundstart();
        }
    }

    public void roundstart()
    {
        var players = API.GetAllPlayers();

        if (players.Count == 0) return;

        API.DeleteEntity(breadcrumb);
        API.DeleteEntity(_quad);

        foreach (var c in _checkpoints)
        {
            API.DeleteEntity(c);
        }

        foreach (var c in _checkpointBlips)
        {
            API.DeleteEntity(c);
        }

        _checkpoints.Clear();
        _checkpointBlips.Clear();

        for(int i = 0; i < _checkpointPoses.Count; i++)
        {
            _checkpoints.Add(API.CreateMarker(1, _checkpointPoses[i], new Vector3(), new Vector3(),
                /*new Vector3(10f, 10f, 3f)*/
                10f // TODO: Fix scales?
                , new Color(200, 255, 255, 0)));
            var b = API.CreateBlip(_checkpointPoses[i]);
            API.SetBlipColor(b, 66);
            _checkpointBlips.Add(b);
        }

        _quad = API.CreateVehicle(VehicleHash.Blazer, _quadSpawn, 0, 0, 0);

        animal = players[r.Next(players.Count)];
        var aBlip = API.Exported.playerblips.getPlayerBlip(animal);
        API.SetPlayerSkin(animal, PedHash.Deer);
        API.SetPlayerTeam(animal, TEAM_ANIMAL);
        API.SetBlipTransparency(aBlip, 0);
        API.SetEntityInvincible(animal, false);
        var spawnp = _animalSpawnpoints[r.Next(_animalSpawnpoints.Count)];
        API.SetEntityPosition(animal.Handle, spawnp);
        API.SetBlipSprite(aBlip, 141);
        API.SetBlipColor(aBlip, 1);

        API.SendChatMessageToPlayer(animal, "You are the animal! Collect all the checkpoints to win!");		

        roundStart = Environment.TickCount;
        lastIdleCheck = Environment.TickCount;
        lastBreadcrumb = Environment.TickCount;
        roundstarted = true;
        lastIdlePosition = spawnp;

        if (players.Count > 3)
        {
            do
            {
                hawk = players[r.Next(players.Count)];
            } while (hawk == animal);
        }
        foreach(var p in players)
        {
            if (p == animal)
                continue;

            Spawn(p, p == hawk);
        }
    }

    public void Spawn(Client player, bool hawk = false)
    {
        var pBlip = API.Exported.playerblips.getPlayerBlip(player);
        var spawnpoint = _hunterSpawnpoints[r.Next(_hunterSpawnpoints.Count)];
        API.SpawnPlayer(player, spawnpoint, 0f);

        if (!hawk)
        {
            var skin = _skinList[r.Next(_skinList.Count)];		
            API.SetPlayerSkin(player, skin);
            API.GivePlayerWeapon(player, WeaponHash.PumpShotgun, 9999);
            API.GivePlayerWeapon(player, WeaponHash.SniperRifle, 9999);
            API.SetBlipTransparency(pBlip, 0);
            API.SetBlipSprite(pBlip, 1);		
        }
        else
        {
            API.SetPlayerSkin(player, PedHash.ChickenHawk);
            API.SetBlipTransparency(pBlip, 255);
            API.SetBlipSprite(pBlip, 422);
        }
        API.SetBlipColor(pBlip, 0);
        //API.SetEntityPosition(player.handle, );
        if (animal != null) API.SendChatMessageToPlayer(player, "~r~" + animal.Name + "~w~ is the animal! ~r~Hunt~w~ it!");		
        API.SetPlayerTeam(player, TEAM_HUNTER);
        API.SetEntityInvincible(player, false);
    }

    public async void dead(Client player, NetHandle reason, uint weapon, CancelEventArgs cancel)
    {
        cancel.Cancel = true;

        if (player == animal)
        {
            var killer = API.GetPlayerFromHandle(reason);
            roundstarted = false;			
            API.SendChatMessageToAll("The animal has been killed" + (killer == null ? "!" : " by " + killer.Name + "!") + " The hunters win!");
            API.SendChatMessageToAll("Starting next round in 15 seconds...");
            animal = null;
            roundstarted = false;
            await Task.Delay(15000);
            roundstart();
        }
        else
        {
            await Task.Run(async () =>
            {
                await Task.Delay(3000);
                respawn(player);
            });
        }
    }

    public void respawn (Client player)
    {
        if (roundstarted && player != animal)		
            Spawn(player, player == hawk);
    }

    public async void update()
    {
        if (!roundstarted) return;
        if (animal != null)
        {
            var pBlip = API.Exported.playerblips.getPlayerBlip(animal);

            for(int i = 0; i < _checkpoints.Count; i++)
            {
                var pos = API.GetEntityPosition(_checkpoints[i]);
                if (API.GetEntityPosition(animal.Handle).DistanceToSquared(pos) < 100f)
                {
                    API.DeleteEntity(_checkpoints[i]);
                    API.DeleteEntity(_checkpointBlips[i]);
                    _checkpointBlips.RemoveAt(i);
                    _checkpoints.RemoveAt(i);
                    API.SendChatMessageToAll("The animal has picked up a checkpoint!");

                    if (_checkpoints.Count == 0)
                    {
                        roundstarted = false;
                        API.SendChatMessageToAll("The animal has collected all checkpoints! " + animal.Name + " has won!");
                        API.SendChatMessageToAll("Starting next round in 15 seconds...");
                        animal = null;
                        roundstarted = false;
                        //API.Sleep(15000);
                        await Task.Delay(15000);
                        roundstart();
                        break;
                    }

                    API.SetBlipTransparency(pBlip, 255);
                    //API.Sleep(5000);
                    await Task.Delay(5000);
                    API.SetBlipTransparency(pBlip, 0);					

                    break;
                }
            }
        
            if (Environment.TickCount - lastIdleCheck > 20000) // 20 secs
            {
                lastIdleCheck = Environment.TickCount;

                if (API.GetEntityPosition(animal.Handle).DistanceToSquared(lastIdlePosition) < 5f)
                {
                    API.SetBlipTransparency(pBlip, 255);
                    //API.Sleep(1000);
                    await Task.Delay(1000);
                    API.SetBlipTransparency(pBlip, 0);
                }
                else
                {
                    API.SetBlipTransparency(pBlip, 0);
                }

                if (!breadcrumbLock) // breadcrumbs are very messy since i was debugging the blips not disappearing
                {
                    breadcrumbLock = true;
                    breadcrumb = API.CreateBlip(lastIdlePosition);
                    API.SetBlipSprite(breadcrumb, 161);
                    API.SetBlipColor(breadcrumb, 1);
                    API.SetBlipTransparency(breadcrumb, 200);

                    lastBreadcrumb = Environment.TickCount;					
                }
                if (animal != null)
                    lastIdlePosition = API.GetEntityPosition(animal.Handle);
            }

            if (Environment.TickCount - lastBreadcrumb > 15000 && breadcrumbLock)
            {
                API.DeleteEntity(breadcrumb);
                breadcrumbLock = false;
            }
        }
    }

    private bool breadcrumbLock;
    private NetHandle breadcrumb;
    private long lastBreadcrumb;
}
