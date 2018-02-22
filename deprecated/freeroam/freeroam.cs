using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GTANetworkAPI;

public class FreeroamScript : Script
{
    public FreeroamScript()
    {
        Event.OnResourceStart += OnResourceStartHandler;
        Event.OnResourceStop += OnResourceStopHandler;
        Event.OnPlayerConnected += OnPlayerConnectedHandler;
        Event.OnPlayerDisconnected += OnPlayerDisconnectedHandler;
    }

    private static Random Rnd = new Random();

    #region IPLs
    private readonly string[] YachtIPL = {
        "hei_yacht_heist",
        "hei_yacht_heist_enginrm",
        "hei_yacht_heist_Lounge",
        "hei_yacht_heist_Bar",
        "hei_yacht_heist_Bedrm",
        "hei_yacht_heist_DistantLights",
        "hei_yacht_heist_LODLights",
        "hei_yacht_heist_Bridge"
    };

    private readonly string[] CarrierIPL = {
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
            SocialClubName = sc;
        }

        public bool Fresh { get; set; }
        public string SocialClubName { get; set; }
        public uint Skin { get; set; }
        public Vector3 LastPosition { get; set; }
        public Vehicle LastVehicle { get; set; }
        public List<NetHandle> VehicleHistory { get; set; }
    }

    public class Vehicle
    {
        public uint Model { get; set; }
        public int PrimaryColor { get; set; }
        public int SecondaryColor { get; set; }
    }

    private void OnResourceStartHandler()
    {
        PlayerList = new List<Players>();

        foreach (string element in YachtIPL)
        {
            API.RequestIpl(element);
        }
        foreach (string element in CarrierIPL)
        {
            API.RequestIpl(element);
        }
    }

    private void OnResourceStopHandler()
    {
        foreach (string element in YachtIPL)
        {
            API.RemoveIpl(element);
        }
        foreach (string element in CarrierIPL)
        {
            API.RemoveIpl(element);
        }
    }

    public Players Handle(Client player)
    {
        if (PlayerList.FirstOrDefault(op => op.SocialClubName == player.SocialClubName) == null)
        {
            PlayerList.Add(new Players(player.SocialClubName) { Fresh = true, VehicleHistory = new List<NetHandle>(), LastVehicle = new Vehicle() });
        }

        Players Player = PlayerList.First(op => op.SocialClubName == player.SocialClubName);
        return Player;
    }

    private void OnPlayerConnectedHandler(Client player, CancelEventArgs args)
    {
        if (Handle(player).Fresh)
        {
            var vals = Enum.GetValues(typeof(PedHash)).OfType<PedHash>();
            var randomSkin = vals.ElementAt(Rnd.Next(vals.Count()));
            player.SetSkin(randomSkin);

            player.Position = SpawnPositions[Rnd.Next(SpawnPositions.Count)];
            return;
        }

        player.Position = Handle(player).LastPosition;
        player.SetSkin((PedHash)Handle(player).Skin);

        if (Handle(player).LastVehicle.Model == 0) return;
        var veh = API.CreateVehicle((VehicleHash)Handle(player).LastVehicle.Model, player.Position, player.Heading, 0, 0);
        player.SetIntoVehicle(veh, -1);
        Handle(player).VehicleHistory.Add(veh.Handle);
        player.Vehicle.PrimaryColor = Handle(player).LastVehicle.PrimaryColor;
        player.Vehicle.SecondaryColor = Handle(player).LastVehicle.SecondaryColor;
    }

    private void OnPlayerDisconnectedHandler(Client player, byte type, string reason)
    {
        Handle(player).LastPosition = player.Position;
        Handle(player).Skin = player.Model;
        if (player.IsInVehicle)
        {
            Handle(player).LastVehicle.Model = player.Vehicle.Model;
            Handle(player).LastVehicle.PrimaryColor = player.Vehicle.PrimaryColor;
            Handle(player).LastVehicle.SecondaryColor = player.Vehicle.SecondaryColor;
        }
        else { Handle(player).LastVehicle.Model = 0; }
        foreach (var veh in Handle(player).VehicleHistory)
        {
            API.DeleteEntity(veh);
        }
        Handle(player).VehicleHistory.Clear();
        Handle(player).Fresh = false;
    }

    [Command("me", GreedyArg = true)]
    public void MeCommand(Client sender, string text)
    {
        API.SendChatMessageToAll("!{#C2A2DA}" + sender.Name + " " + text);
    }

    [Command("spec")]
    public void SpectatorCommand(Client sender, Client target)
    {
        API.SetPlayerToSpectatePlayer(sender, target);
    }

    [Command("unspec")]
    public void StopSpectatingCommand(Client sender)
    {
        API.UnspectatePlayer(sender);
    }

    [Command("loadipl")]
    public void LoadIplCommand(Client sender, string ipl)
    {
        API.RequestIpl(ipl);
        API.ConsoleOutput("LOADED IPL " + ipl);
        API.SendChatMessageToPlayer(sender, "Loaded IPL ~b~" + ipl + "~w~.");
    }

    [Command("removeipl")]
    public void RemoveIplCommand(Client sender, string ipl)
    {
        API.RemoveIpl(ipl);
        API.ConsoleOutput("REMOVED IPL " + ipl);
        API.SendChatMessageToPlayer(sender, "Removed IPL ~b~" + ipl + "~w~.");
    }

    [Command("blackout")]
    public void BlackoutCommand(Client sender, bool blackout)
    {
        API.SendNativeToAllPlayers(0x1268615ACE24D504, blackout);
    }

    [Command("dimension")]
    public void ChangeDimension(Client sender, uint dimension)
    {
        if (dimension == 9999)
            dimension = API.GlobalDimension;
        API.SetEntityDimension(sender.Handle, dimension);
    }

    [Command("mod")]
    public void SetCarModificationCommand(Client sender, int modIndex, int modVar)
    {
        if (!sender.Vehicle.IsNull)
        {
            API.SetVehicleMod(sender.Vehicle, modIndex, modVar);
            API.SendChatMessageToPlayer(sender, "Mod applied successfully!");
        }
        else
        {
            API.SendChatMessageToPlayer(sender, "~r~ERROR: ~w~You're not in a vehicle!");
        }
    }

    [Command("clothes")]
    public void SetPedClothesCommand(Client sender, int slot, int drawable, int texture)
    {
        API.SetPlayerClothes(sender, slot, drawable, texture);
        API.SendChatMessageToPlayer(sender, "Clothes applied successfully!");
    }

    [Command("props")]
    public void SetPedAccessoriesCommand(Client sender, int slot, int drawable, int texture)
    {
        API.SetPlayerAccessory(sender, slot, drawable, texture);
        API.SendChatMessageToPlayer(sender, "Props applied successfully!");
    }

    [Command("colors")]
    public void GameVehicleColorsCommand(Client sender, int primaryColor, int secondaryColor)
    {
        if (!sender.Vehicle.IsNull)
        {
            API.SetVehiclePrimaryColor(sender.Vehicle, primaryColor);
            API.SetVehicleSecondaryColor(sender.Vehicle, secondaryColor);
            API.SendChatMessageToPlayer(sender, "Colors applied successfully!");
        }
        else
        {
            API.SendChatMessageToPlayer(sender, "~r~ERROR: ~w~You're not in a vehicle!");
        }
    }

    private Dictionary<Client, NetHandle> cars = new Dictionary<Client, NetHandle>();
    private Dictionary<Client, NetHandle> shields = new Dictionary<Client, NetHandle>();

    [Command("detach")]
    public void detachtest(Client sender)
    {
        if (cars.ContainsKey(sender))
        {
            API.DeleteEntity(cars[sender]);
            cars.Remove(sender);
        }

        if (labels.ContainsKey(sender))
        {
            API.DeleteEntity(labels[sender]);
            labels.Remove(sender);
        }

        if (shields.ContainsKey(sender))
        {
            API.DeleteEntity(shields[sender]);
            shields.Remove(sender);
        }
    }

    [Command("attachveh")]
    public void attachtest2(Client sender, VehicleHash veh)
    {
        if (cars.ContainsKey(sender))
        {
            API.DeleteEntity(cars[sender]);
            cars.Remove(sender);
        }

        var prop = API.CreateVehicle(veh, sender.Position, sender.Heading, 0, 0);
        API.AttachEntityToEntity(prop, sender.Handle, null, new Vector3(), new Vector3());

        cars.Add(sender, prop);
    }

    private Dictionary<Client, NetHandle> labels = new Dictionary<Client, NetHandle>();

    [Command("attachlabel")]
    public void attachtest3(Client sender, string message)
    {
        if (labels.ContainsKey(sender))
        {
            API.DeleteEntity(labels[sender]);
            labels.Remove(sender);
        }

        var prop = API.CreateTextLabel(message, API.GetEntityPosition(sender.Handle), 50f, 0.4f, 0, new Color(255));

        API.AttachEntityToEntity(prop, sender.Handle, null,
                    new Vector3(0, 0, 1f), new Vector3());

        labels.Add(sender, prop);
    }

    [Command("attachmarker")]
    public void attachtest4(Client sender)
    {
        var prop = API.CreateMarker(0, API.GetEntityPosition(sender.Handle), new Vector3(), new Vector3(), 1f, new Color(255, 255, 255));
        API.AttachEntityToEntity(prop, sender.Handle, null, new Vector3(), new Vector3());
    }

    [Command("shield")]
    public void attachtest5(Client sender)
    {
        if (shields.ContainsKey(sender))
        {
            API.DeleteEntity(shields[sender]);
            shields.Remove(sender);
        }

        var prop = API.CreateObject(API.GetHashKey("prop_riot_shield"), API.GetEntityPosition(sender.Handle), new Vector3());
        API.AttachEntityToEntity(prop, sender.Handle, "SKEL_L_Hand",
            new Vector3(0, 0, 0), new Vector3(0f, 0f, 0f));

        shields.Add(sender, prop);
    }

    [Command("attachrpg")]
    public void attachtest1(Client sender)
    {
        var prop = API.CreateObject(API.GetHashKey("w_lr_rpg"), API.GetEntityPosition(sender.Handle), new Vector3());
        API.AttachEntityToEntity(prop, sender.Handle, "SKEL_SPINE3",
            new Vector3(-0.13f, -0.231f, 0.07f), new Vector3(0f, 200f, 10f));
    }

    [Command("colorsrgb")]
    public void CustomVehicleColorsCommand(Client sender, int primaryRed, int primaryGreen, int primaryBlue, int secondaryRed, int secondaryGreen, int secondaryBlue)
    {
        if (!sender.Vehicle.IsNull)
        {
            API.SetVehicleCustomPrimaryColor(sender.Vehicle, primaryRed, primaryGreen, primaryBlue);
            API.SetVehicleCustomSecondaryColor(sender.Vehicle, secondaryRed, secondaryGreen, secondaryBlue);
            API.SendChatMessageToPlayer(sender, "Colors applied successfully!");
        }
        else
        {
            API.SendChatMessageToPlayer(sender, "~r~ERROR: ~w~You're not in a vehicle!");
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
            API.SendChatMessageToPlayer(sender, "~b~Available animations:");
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
                    API.SendChatMessageToPlayer(sender, "~b~>> ~w~" + output);
            }
        }
        else if (animation == "stop")
        {
            API.StopPlayerAnimation(sender);
        }
        else if (!AnimationList.ContainsKey(animation))
        {
            API.SendChatMessageToPlayer(sender, "~r~ERROR: ~w~Animation not found!");
        }
        else
        {
            var flag = 0;
            if (animation == "handsup") flag = 1;

            API.PlayPlayerAnimation(sender, flag, AnimationList[animation].Split()[0], AnimationList[animation].Split()[1]);
        }
    }

    private string getRandomNumberPlate(Client client = null)
    {
        if (client != null)
        {
            string strClientName = client.Name;
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
    public void SpawnCarCommand(Client sender, string modelName)
    {
        if (API.IsPlayerInAnyVehicle(sender))
        {
            sender.SendChatMessage("~r~Please exit your current vehicle first.");
            return;
        }

        var model = NAPI.Util.VehicleNameToModel(modelName);
        if (BannedVehicles.Contains(model))
        {
            sender.SendChatMessage("The vehicle ~r~" + model + "~s~ is ~r~banned~s~!");
            return;
        }

        var veh = API.CreateVehicle(model, sender.Position, 0f, 0, 0);
        veh.PrimaryColor = Rnd.Next(158);
        veh.SecondaryColor = Rnd.Next(158);
        veh.NumberPlate = getRandomNumberPlate(sender);
        veh.NumberPlateStyle = Rnd.Next(6);

        Handle(sender).VehicleHistory.Add(veh);
        if (Handle(sender).VehicleHistory.Count > 2)
        {
            API.DeleteEntity(Handle(sender).VehicleHistory[0]);
            Handle(sender).VehicleHistory.RemoveAt(0);
        }

        API.SetPlayerIntoVehicle(sender, veh, -1);
    }

    [Command("repair", Alias = "r")]
    public void RepairCarCommand(Client sender)
    {
        var veh = sender.Vehicle;
        if (veh == null)
            return;
        veh.Repair();
    }

    [Command("clearvehicles", Alias = "vc")]
    public void ClearVehiclesCommand(Client sender)
    {
        foreach (var veh in Handle(sender).VehicleHistory)
            API.DeleteEntity(veh);
        Handle(sender).VehicleHistory.Clear();
    }

    [Command("skin")]
    public void ChangeSkinCommand(Client sender, PedHash model)
    {
        API.SetPlayerSkin(sender, model);
        API.SendNativeToPlayer(sender, Hash.SET_PED_DEFAULT_COMPONENT_VARIATION, sender.Handle);
    }

    [Command("pic")]
    public void SpawnPickupCommand(Client sender, PickupHash pickup)
    {
        API.CreatePickup(pickup, new Vector3(sender.Position.X + 10, sender.Position.Y, sender.Position.Z), new Vector3(), 100, 0);
    }

    [Command("tp")]
    public void TeleportPlayerToPlayerCommand(Client sender, Client target)
    {
        var pos = API.GetEntityPosition(sender.Handle);

        API.CreateParticleEffectOnPosition("scr_rcbarry1", "scr_alien_teleport", pos, new Vector3(), 1f);

        API.SetEntityPosition(sender.Handle, API.GetEntityPosition(target.Handle));
    }

    [Command("weapon", Alias = "w,gun")]
    public void GiveWeaponCommand(Client sender, WeaponHash weapon)
    {
        API.GivePlayerWeapon(sender, weapon, 9999);
    }

    [Command("weaponcomponent", Alias = "wcomp,wc")]
    public void GiveWeaponComponentCmd(Client sender, WeaponComponent component)
    {
        API.SetPlayerWeaponComponent(sender, API.GetPlayerCurrentWeapon(sender), component);
    }


    [Command("weapontint", Alias = "wtint")]
    public void SetWeaponTintCmd(Client sender, WeaponTint tint)
    {
        API.SetPlayerWeaponTint(sender, API.GetPlayerCurrentWeapon(sender), tint);
    }

    [Command("help", Alias = "cmd,h")]
    public void HelpCommand(Client sender)
    {
        API.SendChatMessageToPlayer(sender, "~h~~g~SERVER~h~~w~: /tp /car /colors /gun /skin /clothes /props /anim /me /suicide");
        API.SendChatMessageToPlayer(sender, "~h~~g~SERVER~h~~w~: /attachveh /attachlabel /detach /ping /time /online /pm /loadipl");
    }

    [Command("online")]
    public void CommandOnline(Client player)
    {
        API.SendChatMessageToPlayer(player, "~b~Players Online: ~h~~w~" + API.GetAllPlayers().Count);
    }

    [Command("ping")]
    public void CommandPing(Client player)
    {
        API.SendChatMessageToPlayer(player, "~b~Your Ping: ~w~~h~" + player.Ping + " ms");
    }

    [Command("time")]
    public void CommandTime(Client player)
    {
        API.SendChatMessageToPlayer(player, "The time now is ~g~~h~" + DateTime.Now.ToString("HH:mm") + "~h~ (UTC)");
        API.SendNotificationToPlayer(player, "Current Time: ~g~~h~" + DateTime.Now.ToString("HH:mm") + "~h~ (UTC)");
    }

    [Command("suicide")]
    public void CommandKill(Client player)
    {
        API.SetPlayerHealth(player, -1);
    }

    [Command("pm", GreedyArg = true)]
    public void CommandPM(Client player, Client target, string message)
    {
        API.SendChatMessageToPlayer(player, "~y~PM Sent to ~h~" + target.Name + "~h~~w~", message);
        API.PlaySoundFrontEnd(player, "Click_Fail", "WEB_NAVIGATION_SOUNDS_PHONE");
        API.SendChatMessageToPlayer(target, "~y~PM From ~h~" + player.Name + "~h~~w~", message);
        API.PlaySoundFrontEnd(target, "Click", "DLC_HEIST_HACKING_SNAKE_SOUNDS");
    }

}
