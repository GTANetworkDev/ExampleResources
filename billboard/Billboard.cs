//#define DEBUG_COMMANDS // uncomment this line for commands

using GTANetworkServer;
using GTANetworkShared;
using System.Collections.Generic;

public class BillboardManager : Script
{
    public Dictionary<int, Billboard> Billboards;
    public float BillboardRange = 40f;
    private int _counter;

    public BillboardManager()
    {
        Billboards = new Dictionary<int, Billboard>();

        API.onResourceStart += () =>
        {
            BillboardRange = API.getSetting<float>("range");
        };
    }

    private Billboard createBasicBillboard(Vector3 pos, Vector3 rot, Vector3 scale)
    {
        int id = ++_counter;

        var bb = new Billboard();
        bb.Id = id;
        bb.Position = pos;
        bb.Rotation = rot;
        bb.Scale = scale;

        bb.Collision = API.createSphereColShape(pos, BillboardRange);

        bb.Collision.onEntityEnterColShape += (shape, entity) =>
        {
            var player = API.getPlayerFromHandle(entity);
            if (player == null) return;

            sendBillboard(player, bb);
        };

        bb.Collision.onEntityExitColShape += (shape, entity) =>
        {
            var player = API.getPlayerFromHandle(entity);
            if (player == null) return;

            player.triggerEvent("REMOVE_BILLBOARD", id);
        };

        Billboards.Add(id, bb);

        return bb;
    }

    private void sendBillboard(Client cl, Billboard bb)
    {
        object[] args = new object[6 + bb.Arguments.Count * 2];
        // id, pos, rot, scale, argc, args*2
        // 5 + args*2

        args[0] = bb.Id;
        args[1] = bb.Position;
        args[2] = bb.Rotation;
        args[3] = bb.Scale;
        args[4] = bb.Type;
        args[5] = bb.Arguments.Count;

        int counter = 6;
        foreach(var pair in bb.Arguments)
        {
            args[counter] = pair.Key;
            args[counter + 1] = pair.Value;

            counter += 2;
        }

        cl.triggerEvent("CREATE_BILLBOARD", args);
    }

    // EXPORTED METHODS

    public void setBillboardRange(float range)
    {
        BillboardRange = range;
    }

    // player_name
    public int createSimpleBillboard(string text, Vector3 pos, Vector3 rot, Vector3 scale)
    {
        var bb = createBasicBillboard(pos, rot, scale);

        bb.Type = 0;
        bb.Arguments.Add("text", text);

        return bb.Id;
    }

    public int createYachtNameBillboard(string text, string subtitle, bool isWhite, Vector3 pos, Vector3 rot, Vector3 scale)
    {
        var bb = createBasicBillboard(pos, rot, scale);

        bb.Type = 1;
        bb.Arguments.Add("text", text);
        bb.Arguments.Add("subtitle", subtitle);
        bb.Arguments.Add("isWhite", isWhite);

        return bb.Id;
    }

    public int createLargeYachtNameBillboard(string text, string subtitle, bool isWhite, Vector3 pos, Vector3 rot, Vector3 scale)
    {
        var bb = createBasicBillboard(pos, rot, scale);

        bb.Type = 2;
        bb.Arguments.Add("text", text);
        bb.Arguments.Add("subtitle", subtitle);
        bb.Arguments.Add("isWhite", isWhite);

        return bb.Id;
    }

    public int createOrganizationName(string text, int style, int color, int font, Vector3 pos, Vector3 rot, Vector3 scale)
    {
        var bb = createBasicBillboard(pos, rot, scale);

        bb.Type = 3;
        bb.Arguments.Add("text", text);
        bb.Arguments.Add("style", style);
        bb.Arguments.Add("color", color);
        bb.Arguments.Add("font", font);

        return bb.Id;
    }

    public int createMugshotBoard(string name, string subtitle, string subtitle2, string title, int rank, Vector3 pos, Vector3 rot, Vector3 scale)
    {
        var bb = createBasicBillboard(pos, rot, scale);

        bb.Type = 4;
        bb.Arguments.Add("name", name);
        bb.Arguments.Add("subtitle", subtitle);
        bb.Arguments.Add("subtitle2", subtitle2);
        bb.Arguments.Add("title", title);
        bb.Arguments.Add("rank", rank);

        return bb.Id;
    }

    public int createScrollingText(string text, int color, Vector3 pos, Vector3 rot, Vector3 scale)
    {
        var bb = createBasicBillboard(pos, rot, scale);

        bb.Type = 5;
        bb.Arguments.Add("text", text);
        bb.Arguments.Add("color", color);

        return bb.Id;
    }

    public int createMissionBillboard(string text, string subtitle, string percentage, bool isVerified, string players, int rp, int money, string time, Vector3 pos, Vector3 rot, Vector3 scale)
    {
        var bb = createBasicBillboard(pos, rot, scale);

        bb.Type = 6;
        bb.Arguments.Add("text", text);
        bb.Arguments.Add("subtitle", subtitle);
        bb.Arguments.Add("percentage", percentage);
        bb.Arguments.Add("isVerified", isVerified);
        bb.Arguments.Add("players", players);
        bb.Arguments.Add("RP", rp);
        bb.Arguments.Add("money", money);
        bb.Arguments.Add("time", time);

        return bb.Id;
    }

    public bool doesBillboardExist(int id)
    {
        return Billboards.ContainsKey(id);
    }

    public Billboard getBillboard(int id)
    {
        return Billboards[id];
    }

    public void setBillboardParams(int id, Vector3 pos, Vector3 rot, Vector3 scale)
    {
        Billboards[id].Position = pos;
        Billboards[id].Rotation = rot;
        Billboards[id].Scale = scale;
    }

    public void setBillboardArg(int id, string argname, object value)
    {
        if (Billboards[id].Arguments.ContainsKey(argname))
            Billboards[id].Arguments[argname] = value;
        else Billboards[id].Arguments.Add(argname, value);
    }

    public void refreshBillboard(int id)
    {
        foreach (var entity in Billboards[id].Collision.getAllEntities())
        {
            var player = API.getPlayerFromHandle(entity);
            if (player == null) return;

            sendBillboard(player, Billboards[id]);
        }
    }

    public void deleteBillboard(int id)
    {
        foreach (var entity in Billboards[id].Collision.getAllEntities())
        {
            var player = API.getPlayerFromHandle(entity);
            if (player == null) return;

            player.triggerEvent("REMOVE_BILLBOARD", id);
        }

        API.deleteColShape(Billboards[id].Collision);
        Billboards.Remove(id);
    }

#if DEBUG_COMMANDS
    // debug 
    [Command]
    public void createmissionbb(Client sender, string text, string subtitle, string percentage, bool verified, string players, int rp, int money, string time = null)
    {
        createMissionBillboard(text, subtitle, percentage, verified, players, rp, money, time, sender.position, new Vector3(), new Vector3(6, 6, 1));
    }

    [Command]
    public void createsbb(Client sender, string text)
    {
        createSimpleBillboard(text, sender.position + new Vector3(0, 0, 1), new Vector3(), new Vector3(3, 3, 3));
    }

    [Command(GreedyArg = true)]
    public void createsbb2(Client sender, string text)
    {
        createSimpleBillboard(text, sender.position + new Vector3(0, 0, 1), new Vector3(), new Vector3(3, 3, 3));
    }

    [Command]
    public void createybb(Client sender, string text, string subtitle = null)
    {
        createYachtNameBillboard(text, subtitle, true, sender.position + new Vector3(0, 0, 1), new Vector3(), new Vector3(3, 3, 3));
    }


    [Command]
    public void createOrg(Client sender, string text, int style, int color, int font)
    {
        createOrganizationName(text, style, color,font, sender.position + new Vector3(0, 0, 1), new Vector3(), new Vector3(6, 6, 3));
    }

    [Command]
    public void CreateBlimp(Client sender, string txt, int col)
    {
        createScrollingText(txt, col, sender.position + new Vector3(0, 0, 1), new Vector3(), new Vector3(6, 6, 3));
    }


    [Command]
    public void createmugshot(Client sender, string txt, string sub, string sub2, string tit, int rank)
    {
        createMugshotBoard(txt, sub, sub2, tit, rank, sender.position + new Vector3(0, 0, 1), new Vector3(), new Vector3(6, 6, 3));
    }

    [Command]
    public void newtest(Client sender)
    {
        createMissionBillboard("Paramedic", "FACTION", "", false, "", 0, 0, "", sender.position, new Vector3(), new Vector3(6, 6, 1));
    }
#endif
}

public class Billboard
{
    public int Id;
    public int Type;
    public Vector3 Position, Rotation, Scale;    
    public Dictionary<string, object> Arguments;
    public ColShape Collision;

    public Billboard()
    {
        Arguments = new Dictionary<string, object>();
    }
}