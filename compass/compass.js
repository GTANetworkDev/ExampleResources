var x;
var y;
var text;

API.onUpdate.connect(function (sender) {

    x = API.getGameplayCamDir().X;
    y = API.getGameplayCamDir().Y;

    if (0.3 < x && 0.3 < y) {

        text="NE";

    }

    else if (x<-0.3 && 0.3 < y) {

        text="NW";
    }

    else if (0.3 < x && y < -0.3) {

        text="SE";
    }

    else if (x < -0.3 && y < -0.3) {

        text="SW";
    }

    else if (-0.3 < x && x < 0.3 && y < -0.3) {

        text="S";
    }

    else if (x < -0.3 && -0.3 < y && y < 0.3) {

        text="W";
    }

    else if (0.3 < x && -0.3 < y && y < 0.3) {

        text="E";
    }

    else if (-0.3 < x && x < 0.3 && y > 0.3) {

        text="N";
    }
    
    API.drawText(text, 350, 1000, 1, 255, 255, 255, 255, 2, 1, false, true, 0);
});


