using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GTANetworkServer;
using GTANetworkShared;
using System.Threading;

public class FreeroamScript : Script
{
    public FreeroamScript()
    {
        API.onResourceStart += OnResourceStartHandler;
        API.onResourceStop += OnResourceStopHandler;
        API.onPlayerFinishedDownload += OnPlayerFinishedDownloadHandler;
        API.onPlayerDisconnected += OnPlayerDisconnectedHandler;
        API.onClientEventTrigger += OnClientEvent;
    }

    private static Random Rnd = new Random();

    #region IPLs
    private string[] YachtIPL = {
        "hei_yacht_heist",
        "hei_yacht_heist_enginrm",
        "hei_yacht_heist_Lounge",
        "hei_yacht_heist_Bar",
        "hei_yacht_heist_Bedrm",
        "hei_yacht_heist_DistantLights",
        "hei_yacht_heist_LODLights",
        "hei_yacht_heist_Bridge"
    };

    private string[] CarrierIPL = {
        "hei_carrier",
        "hei_Carrier_int1",
        "hei_Carrier_int2",
        "hei_Carrier_int3",
        "hei_Carrier_int4",
        "hei_Carrier_int5",
        "hei_Carrier_int6",
        "hei_carrier_DistantLights",
        "hei_carrier_LODLights",
    };
    #endregion

    public List<Players> PlayerList { get; set; }

    public Dictionary<string, string> AnimationList = new Dictionary<string, string>
    {
        {"finger", "mp_player_intfinger mp_player_int_finger"},
        {"guitar", "anim@mp_player_intcelebrationmale@air_guitar air_guitar"},
        {"shagging", "anim@mp_player_intcelebrationmale@air_shagging air_shagging"},
        {"synth", "anim@mp_player_intcelebrationmale@air_synth air_synth"},
        {"kiss", "anim@mp_player_intcelebrationmale@blow_kiss blow_kiss"},
        {"bro", "anim@mp_player_intcelebrationmale@bro_love bro_love"},
        {"chicken", "anim@mp_player_intcelebrationmale@chicken_taunt chicken_taunt"},
        {"chin", "anim@mp_player_intcelebrationmale@chin_brush chin_brush"},
        {"dj", "anim@mp_player_intcelebrationmale@dj dj"},
        {"dock", "anim@mp_player_intcelebrationmale@dock dock"},
        {"facepalm", "anim@mp_player_intcelebrationmale@face_palm face_palm"},
        {"fingerkiss", "anim@mp_player_intcelebrationmale@finger_kiss finger_kiss"},
        {"freakout", "anim@mp_player_intcelebrationmale@freakout freakout"},
        {"jazzhands", "anim@mp_player_intcelebrationmale@jazz_hands jazz_hands"},
        {"knuckle", "anim@mp_player_intcelebrationmale@knuckle_crunch knuckle_crunch"},
        {"nose", "anim@mp_player_intcelebrationmale@nose_pick nose_pick"},
        {"no", "anim@mp_player_intcelebrationmale@no_way no_way"},
        {"peace", "anim@mp_player_intcelebrationmale@peace peace"},
        {"photo", "anim@mp_player_intcelebrationmale@photography photography"},
        {"rock", "anim@mp_player_intcelebrationmale@rock rock"},
        {"salute", "anim@mp_player_intcelebrationmale@salute salute"},
        {"shush", "anim@mp_player_intcelebrationmale@shush shush"},
        {"slowclap", "anim@mp_player_intcelebrationmale@slow_clap slow_clap"},
        {"surrender", "anim@mp_player_intcelebrationmale@surrender surrender"},
        {"thumbs", "anim@mp_player_intcelebrationmale@thumbs_up thumbs_up"},
        {"taunt", "anim@mp_player_intcelebrationmale@thumb_on_ears thumb_on_ears"},
        {"vsign", "anim@mp_player_intcelebrationmale@v_sign v_sign"},
        {"wank", "anim@mp_player_intcelebrationmale@wank wank"},
        {"wave", "anim@mp_player_intcelebrationmale@wave wave"},
        {"loco", "anim@mp_player_intcelebrationmale@you_loco you_loco"},
        {"handsup", "missminuteman_1ig_2 handsup_base"},
    };

    public List<VehicleHash> BannedVehicles = new List<VehicleHash>
    {
        VehicleHash.CargoPlane,
    };

    public List<Vector3> SpawnPositions = new List<Vector3>
    {
        new Vector3(-237.172, -650.3887, 33.30411),
        new Vector3(-276.9281, -642.3959, 33.20348),
        new Vector3(-284.7394, -679.6924, 33.27827),
        new Vector3(-219.5132, -697.4506, 33.67715),
        new Vector3(-172.2065, -666.7617, 40.48457),
        new Vector3(-344.8585, -691.2588, 32.73247),
    };

    public class Players
    {
        public Players(string sc)
        {
            socialClubName = sc;
        }

        public bool Fresh { get; set; }
        public string socialClubName { get; set; }
        public int Skin { get; set; }
        public Vector3 LastPosition { get; set; }
        public Vehicle LastVehicle { get; set; }
        public List<NetHandle> VehicleHistory { get; set; }
    }

    public class Vehicle
    {
        public int Model { get; set; }
        public int PrimaryColor { get; set; }
        public int SecondaryColor { get; set; }
    }

    private void OnResourceStartHandler()
    {
        PlayerList = new List<Players>();

        foreach (string element in YachtIPL)
        {
            API.requestIpl(element);
        }
        foreach (string element in CarrierIPL)
        {
            API.requestIpl(element);
        }
    }

    private void OnResourceStopHandler()
    {
        foreach (string element in YachtIPL)
        {
            API.removeIpl(element);
        }
        foreach (string element in CarrierIPL)
        {
            API.removeIpl(element);
        }
    }

    public Players Handle(Client player)
    {
        if (PlayerList.FirstOrDefault(op => op.socialClubName == player.socialClubName) == null)
        {
            PlayerList.Add(new Players(player.socialClubName) { Fresh = true, VehicleHistory = new List<NetHandle>(), LastVehicle = new Vehicle() });
        }

        Players Player = PlayerList.First(op => op.socialClubName == player.socialClubName);
        return Player;
    }

    private void OnPlayerFinishedDownloadHandler(Client player)
    {
        if (Handle(player).Fresh)
        {
            var vals = Enum.GetValues(typeof(PedHash)).OfType<PedHash>();
            var randomSkin = vals.ElementAt(Rnd.Next(vals.Count()));
            player.setSkin(randomSkin);

            player.position = SpawnPositions[Rnd.Next(SpawnPositions.Count)];
            return;
        }

        player.position = Handle(player).LastPosition;
        player.setSkin((PedHash)Handle(player).Skin);

        if (Handle(player).LastVehicle.Model == 0) return;
        var veh = API.createVehicle((VehicleHash)Handle(player).LastVehicle.Model, player.position, player.rotation, 0, 0);
        player.setIntoVehicle(veh, -1);
        Handle(player).VehicleHistory.Add(veh.handle);
        player.vehicle.primaryColor = Handle(player).LastVehicle.PrimaryColor;
        player.vehicle.secondaryColor = Handle(player).LastVehicle.SecondaryColor;
    }

    private void OnPlayerDisconnectedHandler(Client player, string reason)
    {
        Handle(player).LastPosition = player.position;
        Handle(player).Skin = player.model;
        if (player.isInVehicle)
        {
            Handle(player).LastVehicle.Model = player.vehicle.model;
            Handle(player).LastVehicle.PrimaryColor = player.vehicle.primaryColor;
            Handle(player).LastVehicle.SecondaryColor = player.vehicle.secondaryColor;
        }
        else { Handle(player).LastVehicle.Model = 0; }
        foreach (var veh in Handle(player).VehicleHistory)
        {
            if (veh == null) continue;
            API.deleteEntity(veh);
        }
        Handle(player).VehicleHistory.Clear();
        Handle(player).Fresh = false;
    }


    public void OnClientEvent(Client sender, string name, object[] args)
    {
        if (name == "CREATE_VEHICLE")
        {
            int model = (int)args[0];

            if (!Enum.IsDefined(typeof(VehicleHash), model))
                return;

            var rot = API.getEntityRotation(sender.handle);
            var veh = API.createVehicle((VehicleHash)model, sender.position, new Vector3(0, 0, rot.Z), 0, 0);

            Handle(sender).VehicleHistory.Add(veh);
            if (Handle(sender).VehicleHistory.Count > 2)
            {
                API.deleteEntity(Handle(sender).VehicleHistory[0]);
                Handle(sender).VehicleHistory.RemoveAt(0);
            }

            API.setPlayerIntoVehicle(sender, veh, -1);
        }
    }

    [Command("me", GreedyArg = true)]
    public void MeCommand(Client sender, string text)
    {
        API.sendChatMessageToAll("~#C2A2DA~", sender.name + " " + text);
    }

    [Command("spec")]
    public void SpectatorCommand(Client sender, Client target)
    {
        API.setPlayerToSpectatePlayer(sender, target);
    }

    [Command("unspec")]
    public void StopSpectatingCommand(Client sender)
    {
        API.unspectatePlayer(sender);
    }

    [Command("loadipl")]
    public void LoadIplCommand(Client sender, string ipl)
    {
        API.requestIpl(ipl);
        API.consoleOutput("LOADED IPL " + ipl);
        API.sendChatMessageToPlayer(sender, "Loaded IPL ~b~" + ipl + "~w~.");
    }

    [Command("removeipl")]
    public void RemoveIplCommand(Client sender, string ipl)
    {
        API.removeIpl(ipl);
        API.consoleOutput("REMOVED IPL " + ipl);
        API.sendChatMessageToPlayer(sender, "Removed IPL ~b~" + ipl + "~w~.");
    }

    [Command("blackout")]
    public void BlackoutCommand(Client sender, bool blackout)
    {
        API.sendNativeToAllPlayers(0x1268615ACE24D504, blackout);
    }

    [Command("dimension")]
    public void ChangeDimension(Client sender, int dimension)
    {
        API.setEntityDimension(sender.handle, dimension);
    }

    [Command("mod")]
    public void SetCarModificationCommand(Client sender, int modIndex, int modVar)
    {
        if (!sender.vehicle.IsNull)
        {
            API.setVehicleMod(sender.vehicle, modIndex, modVar);
            API.sendChatMessageToPlayer(sender, "Mod applied successfully!");
        }
        else
        {
            API.sendChatMessageToPlayer(sender, "~r~ERROR: ~w~You're not in a vehicle!");
        }
    }

    [Command("clothes")]
    public void SetPedClothesCommand(Client sender, int slot, int drawable, int texture)
    {
        API.setPlayerClothes(sender, slot, drawable, texture);
        API.sendChatMessageToPlayer(sender, "Clothes applied successfully!");
    }

    [Command("props")]
    public void SetPedAccessoriesCommand(Client sender, int slot, int drawable, int texture)
    {
        API.setPlayerAccessory(sender, slot, drawable, texture);
        API.sendChatMessageToPlayer(sender, "Props applied successfully!");
    }

    [Command("colors")]
    public void GameVehicleColorsCommand(Client sender, int primaryColor, int secondaryColor)
    {
        if (!sender.vehicle.IsNull)
        {
            API.setVehiclePrimaryColor(sender.vehicle, primaryColor);
            API.setVehicleSecondaryColor(sender.vehicle, secondaryColor);
            API.sendChatMessageToPlayer(sender, "Colors applied successfully!");
        }
        else
        {
            API.sendChatMessageToPlayer(sender, "~r~ERROR: ~w~You're not in a vehicle!");
        }
    }

    private Dictionary<Client, NetHandle> cars = new Dictionary<Client, NetHandle>();
    private Dictionary<Client, NetHandle> shields = new Dictionary<Client, NetHandle>();

    [Command("detach")]
    public void detachtest(Client sender)
    {
        if (cars.ContainsKey(sender))
        {
            API.deleteEntity(cars[sender]);
            cars.Remove(sender);
        }

        if (labels.ContainsKey(sender))
        {
            API.deleteEntity(labels[sender]);
            labels.Remove(sender);
        }

        if (shields.ContainsKey(sender))
        {
            API.deleteEntity(shields[sender]);
            shields.Remove(sender);
        }
    }

    [Command("attachveh")]
    public void attachtest2(Client sender, VehicleHash veh)
    {
        if (cars.ContainsKey(sender))
        {
            API.deleteEntity(cars[sender]);
            cars.Remove(sender);
        }

        var prop = API.createVehicle(veh, API.getEntityPosition(sender.handle), new Vector3(), 0, 0);
        API.attachEntityToEntity(prop, sender.handle, null,
                    new Vector3(), new Vector3());

        cars.Add(sender, prop);
    }

    private Dictionary<Client, NetHandle> labels = new Dictionary<Client, NetHandle>();

    [Command("attachlabel")]
    public void attachtest3(Client sender, string message)
    {
        if (labels.ContainsKey(sender))
        {
            API.deleteEntity(labels[sender]);
            labels.Remove(sender);
        }

        var prop = API.createTextLabel(message, API.getEntityPosition(sender.handle), 50f, 0.4f, true);

        API.attachEntityToEntity(prop, sender.handle, null,
                    new Vector3(0, 0, 1f), new Vector3());

        labels.Add(sender, prop);
    }

    [Command("attachmarker")]
    public void attachtest4(Client sender)
    {
        var prop = API.createMarker(0, API.getEntityPosition(sender.handle), new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 255, 255, 255, 255);
        API.attachEntityToEntity(prop, sender.handle, null,
                    new Vector3(), new Vector3());
    }

    [Command("shield")]
    public void attachtest5(Client sender)
    {
        if (shields.ContainsKey(sender))
        {
            API.deleteEntity(shields[sender]);
            shields.Remove(sender);
        }

        var prop = API.createObject(API.getHashKey("prop_riot_shield"), API.getEntityPosition(sender.handle), new Vector3());
        API.attachEntityToEntity(prop, sender.handle, "SKEL_L_Hand",
            new Vector3(0, 0, 0), new Vector3(0f, 0f, 0f));

        shields.Add(sender, prop);
    }

    [Command("attachrpg")]
    public void attachtest1(Client sender)
    {
        var prop = API.createObject(API.getHashKey("w_lr_rpg"), API.getEntityPosition(sender.handle), new Vector3());
        API.attachEntityToEntity(prop, sender.handle, "SKEL_SPINE3",
            new Vector3(-0.13f, -0.231f, 0.07f), new Vector3(0f, 200f, 10f));
    }

    [Command("colorsrgb")]
    public void CustomVehicleColorsCommand(Client sender, int primaryRed, int primaryGreen, int primaryBlue, int secondaryRed, int secondaryGreen, int secondaryBlue)
    {
        if (!sender.vehicle.IsNull)
        {
            API.setVehicleCustomPrimaryColor(sender.vehicle, primaryRed, primaryGreen, primaryBlue);
            API.setVehicleCustomSecondaryColor(sender.vehicle, secondaryRed, secondaryGreen, secondaryBlue);
            API.sendChatMessageToPlayer(sender, "Colors applied successfully!");
        }
        else
        {
            API.sendChatMessageToPlayer(sender, "~r~ERROR: ~w~You're not in a vehicle!");
        }
    }

    [Command("anim", "~y~USAGE: ~w~/anim [animation]\n" +
                     "~y~USAGE: ~w~/anim help for animation list.\n" +
                     "~y~USAGE: ~w~/anim stop to stop current animation.")]
    public void SetPlayerAnim(Client sender, string animation)
    {
        if (animation == "help")
        {
            string helpText = AnimationList.Aggregate(new StringBuilder(),
                            (sb, kvp) => sb.Append(kvp.Key + " "), sb => sb.ToString());
            API.sendChatMessageToPlayer(sender, "~b~Available animations:");
            var split = helpText.Split();
            for (int i = 0; i < split.Length; i += 5)
            {
                string output = "";
                if (split.Length > i)
                    output += split[i] + " ";
                if (split.Length > i + 1)
                    output += split[i + 1] + " ";
                if (split.Length > i + 2)
                    output += split[i + 2] + " ";
                if (split.Length > i + 3)
                    output += split[i + 3] + " ";
                if (split.Length > i + 4)
                    output += split[i + 4] + " ";
                if (!string.IsNullOrWhiteSpace(output))
                    API.sendChatMessageToPlayer(sender, "~b~>> ~w~" + output);
            }
        }
        else if (animation == "stop")
        {
            API.stopPlayerAnimation(sender);
        }
        else if (!AnimationList.ContainsKey(animation))
        {
            API.sendChatMessageToPlayer(sender, "~r~ERROR: ~w~Animation not found!");
        }
        else
        {
            var flag = 0;
            if (animation == "handsup") flag = 1;

            API.playPlayerAnimation(sender, flag, AnimationList[animation].Split()[0], AnimationList[animation].Split()[1]);
        }
    }

    private string getRandomNumberPlate(Client client = null)
    {
        if (client != null)
        {
            string strClientName = client.name;
            if (strClientName.Length <= 8)
                return strClientName.ToUpper();
        }
        string strCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string strRet = "";
        for (int i = 0; i < 8; i++)
        {
            strRet += strCharacters[Rnd.Next(strCharacters.Length)];
        }
        return strRet;
    }

    [Command("car", Alias = "v")]
    public void SpawnCarCommand(Client sender, VehicleHash model)
    {
        if(API.isPlayerInAnyVehicle(sender))
        {
            sender.sendChatMessage("~r~Error: Please exit your current vehicle first.");
            return;
        }
        if (BannedVehicles.Contains(model))
        {
            sender.sendChatMessage("The vehicle ~r~" + model.ToString() + "~s~ is ~r~banned~s~!");
            return;
        }

        var veh = API.createVehicle(model, sender.position, new Vector3(0, 0, sender.rotation.Z), 0, 0);
        veh.primaryColor = Rnd.Next(158);
        veh.secondaryColor = Rnd.Next(158);
        veh.numberPlate = getRandomNumberPlate(sender);
        veh.numberPlateStyle = Rnd.Next(6);

        Handle(sender).VehicleHistory.Add(veh);
        if (Handle(sender).VehicleHistory.Count > 2)
        {
            API.deleteEntity(Handle(sender).VehicleHistory[0]);
            Handle(sender).VehicleHistory.RemoveAt(0);
        }

        API.setPlayerIntoVehicle(sender, veh, -1);
    }

    [Command("repair", Alias = "r")]
    public void RepairCarCommand(Client sender)
    {
        var veh = sender.vehicle;
        if (veh == null)
            return;
        veh.repair();
    }

    [Command("clearvehicles", Alias = "vc")]
    public void ClearVehiclesCommand(Client sender)
    {
        foreach (var veh in Handle(sender).VehicleHistory)
            API.deleteEntity(veh);
        Handle(sender).VehicleHistory.Clear();
    }

    [Command("skin")]
    public void ChangeSkinCommand(Client sender, PedHash model)
    {
        API.setPlayerSkin(sender, model);
        API.sendNativeToPlayer(sender, Hash.SET_PED_DEFAULT_COMPONENT_VARIATION, sender.handle);
    }

    [Command("pic")]
    public void SpawnPickupCommand(Client sender, PickupHash pickup)
    {
        API.createPickup(pickup, new Vector3(sender.position.X + 10, sender.position.Y, sender.position.Z), new Vector3(), 100, 0);
    }

    [Command("tp")]
    public void TeleportPlayerToPlayerCommand(Client sender, Client target)
    {
        var pos = API.getEntityPosition(sender.handle);

        API.createParticleEffectOnPosition("scr_rcbarry1", "scr_alien_teleport", pos, new Vector3(), 1f);

        API.setEntityPosition(sender.handle, API.getEntityPosition(target.handle));
    }

    [Command("weapon", Alias = "w,gun")]
    public void GiveWeaponCommand(Client sender, WeaponHash weapon)
    {
        API.givePlayerWeapon(sender, weapon, 9999, true, true);
    }

    [Command("weaponcomponent", Alias = "wcomp,wc")]
    public void GiveWeaponComponentCmd(Client sender, WeaponComponent component)
    {
        API.givePlayerWeaponComponent(sender, API.getPlayerCurrentWeapon(sender), component);
    }


    [Command("weapontint", Alias = "wtint")]
    public void SetWeaponTintCmd(Client sender, WeaponTint tint)
    {
        API.setPlayerWeaponTint(sender, API.getPlayerCurrentWeapon(sender), tint);
    }

    [Command("help", Alias = "cmd,h")]
    public void HelpCommand(Client sender)
    {
        API.sendChatMessageToPlayer(sender, "~h~~g~SERVER~h~~w~: You can press ~h~~g~F1~h~~w~ to open the FREEROAM Menu.");
        API.sendChatMessageToPlayer(sender, "~h~~g~SERVER~h~~w~: /tp /car /colors /gun /skin /clothes /props /anim /me /suicide");
        API.sendChatMessageToPlayer(sender, "~h~~g~SERVER~h~~w~: /attachveh /attachlabel /detach /ping /time /online /pm /loadipl");
    }

    [Command("online")]
    public void CommandOnline(Client player)
    {
        API.sendChatMessageToPlayer(player, "~b~Players Online: ~h~~w~" + API.getAllPlayers().Count);
    }

    [Command("ping")]
    public void CommandPing(Client player)
    {
        API.sendChatMessageToPlayer(player, "~b~Your Ping: ~w~~h~" + player.ping + " ms");
    }

    [Command("time")]
    public void CommandTime(Client player)
    {
        API.sendChatMessageToPlayer(player, "The time now is ~g~~h~" + DateTime.Now.ToString("HH:mm") + "~h~ (UTC)");
        API.sendNotificationToPlayer(player, "Current Time: ~g~~h~" + DateTime.Now.ToString("HH:mm") + "~h~ (UTC)");
    }

    [Command("suicide")]
    public void CommandKill(Client player)
    {
        API.setPlayerHealth(player, -1);
    }

    [Command("pm", GreedyArg = true)]
    public void CommandPM(Client player, Client target, string message)
    {
        API.sendChatMessageToPlayer(player, "~y~PM Sent to ~h~" + target.name + "~h~~w~", message);
        API.playSoundFrontEnd(player, "Click_Fail", "WEB_NAVIGATION_SOUNDS_PHONE");
        API.sendChatMessageToPlayer(target, "~y~PM From ~h~" + player.name + "~h~~w~", message);
        API.playSoundFrontEnd(target, "Click", "DLC_HEIST_HACKING_SNAKE_SOUNDS");
        return;
    }

    [Command("save")]
    public void SaveCommand(Client player, string msg = "")
    {
        File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "positions.txt",
            "API.setEntityPosition(player, new Vector3(" + API.getEntityPosition(player.handle).X.ToString() + ", " + API.getEntityPosition(player.handle).Y.ToString() + ", " + API.getEntityPosition(player.handle).Z.ToString() + ")); //" + msg + Environment.NewLine +
            "API.setEntityRotation(player, new Vector3(0f, 0f, " + API.getEntityRotation(player.handle).Z.ToString() + "));" + Environment.NewLine + Environment.NewLine);
        API.sendChatMessageToPlayer(player, "Your position has been saved to positions.txt");
    }

}
