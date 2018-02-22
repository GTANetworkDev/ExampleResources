//var mainScaleform = API.requestScaleform("yacht_name_stern");

var billboards = {};
var billboardTypes = [
    "player_name_",
    "yacht_name",
    "yacht_name_stern",
    "organisation_name",
    "mugshot_board_01",
    "blimp_text",
    "yacht_gamername"
];

var player_name_count = 0;

API.onServerEventTrigger.connect(function(ev, args) {
    if (ev === "REMOVE_BILLBOARD") {
        if (billboards[args[0]]) {
            //API.sendChatMessage("Destroying billboard #" + args[0]);
            //billboards[args[0]].scaleform.Dispose();
            delete billboards[args[0]];
        }        
    } else if (ev === "CREATE_BILLBOARD") {
        var key = args[0];

        if (!billboards[key]) {
            billboards[key] = {};
        }

        billboards[key].pos = args[1];
        billboards[key].rot = args[2];
        billboards[key].scale = args[3];
        billboards[key].type = args[4];

        var argc = args[5];

        for (var i = 0; i < argc; i++) {
            billboards[key][args[6 + i*2]] = args[7 + i*2];
        }

        var scaleformName = billboardTypes[billboards[key].type];
        if (billboards[key].type == 0) // player_name
        {
            scaleformName = billboardTypes[0] + toStrPad(player_name_count + 1);
            player_name_count = (player_name_count + 1) % 15;
        }

        //API.sendChatMessage("Loading " + scaleformName);

        do
        {
            billboards[key].scaleform = API.requestScaleform(scaleformName);
            API.sleep(100);
        } while(!billboards[key].scaleform.IsLoaded);

        switch (billboards[key].type) {
            case 0: // player_name_01
                billboards[key].scaleform.CallFunction("SET_PLAYER_NAME", billboards[key].text);
                break;
            case 1: // yacht_name
            case 2: // yacht_name_stern
                billboards[key].scaleform.CallFunction("SET_YACHT_NAME", billboards[key].text, billboards[key].isWhite, billboards[key].subtitle);
                break;
            case 3: // organisation_name
                billboards[key].scaleform.CallFunction("SET_ORGANISATION_NAME", billboards[key].text, billboards[key].style, billboards[key].color, billboards[key].font);
                break; 
            case 4: // mugshot_board_01
                billboards[key].scaleform.CallFunction("SET_BOARD", billboards[key].name, billboards[key].subtitle, billboards[key].subtitle2, billboards[key].title, 0, billboards[key].rank, 0);
                break;
            case 5: // blimp_text
                billboards[key].scaleform.CallFunction("SET_COLOUR", billboards[key].color);
                billboards[key].scaleform.CallFunction("SET_MESSAGE", billboards[key].text);
                break;
            case 6: // yacht_gamername
                billboards[key].scaleform.CallFunction("SET_MISSION_INFO", billboards[key].text, billboards[key].subtitle, 0, billboards[key].percentage, "", billboards[key].isVerified, billboards[key].players, billboards[key].RP, billboards[key].money, billboards[key].time);
                break;
        }

        //API.sendChatMessage("Creating billboard #" + key + " of type " + billboardTypes[billboards[key].type] + " text:" + billboards[key].text);
    }
});

API.onUpdate.connect(function() {
    for (key in billboards) {        
        render3d(billboards[key]);
    }
});

function render3d(billboard) {    
    //API.displaySubtitle("Drawing billboard @ " + billboard.pos.X + ", " + billboard.pos.Y + ", " + billboard.pos.Z + " @ " + billboard.scale.X + ", " + billboard.scale.Y + ", " + billboard.scale.Z);
    var rot = API.getEntityRotation(API.getLocalPlayer());
    API.callNative("_DRAW_SCALEFORM_MOVIE_3D_NON_ADDITIVE", billboard.scaleform.Handle,
        API.f(billboard.pos.X), API.f(billboard.pos.Y), API.f(billboard.pos.Z),
        API.f(billboard.rot.X), API.f(billboard.rot.Y), API.f(360 - billboard.rot.Z),
        API.f(2), API.f(2), API.f(1),
        API.f(billboard.scale.X), API.f(billboard.scale.Y), API.f(billboard.scale.Z), // scale
        2);
}

function toStrPad(num) {
    if (num < 10)
        return "0" + num;
    return "" + num;
}