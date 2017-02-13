API.onUpdate.connect(function (sender) {

    var x = API.getGameplayCamDir().X;
    var y = API.getGameplayCamDir().Y;

    if (0.3 < x && 0.3 < y) {

        API.drawText("NE", 350, 1000, 1, 255, 255, 255, 255, 2, 1, false, true, 0);

    }

    else if (x<-0.3 && 0.3 < y) {

        API.drawText("NW", 350, 1000, 1, 255, 255, 255, 255, 2, 1, false, true, 0);

    }

    else if (0.3 < x && y < -0.3) {

        API.drawText("SE", 350, 1000, 1, 255, 255, 255, 255, 2, 1, false, true, 0);

    }

    else if (x < -0.3 && y < -0.3) {

        API.drawText("SW", 350, 1000, 1, 255, 255, 255, 255, 2, 1, false, true, 0);

    }

    else if (-0.3 < x && x < 0.3 && y < -0.3) {

        API.drawText("S", 350, 1000, 1, 255, 255, 255, 255, 2, 1, false, true, 0);

    }

    else if (x < -0.3 && -0.3 < y && y < 0.3) {

        API.drawText("W", 350, 1000, 1, 255, 255, 255, 255, 2, 1, false, true, 0);

    }

    else if (0.3 < x && -0.3 < y && y < 0.3) {

        API.drawText("E", 350, 1000, 1, 255, 255, 255, 255, 2, 1, false, true, 0);

    }

    else if (-0.3 < x && x < 0.3 && y > 0.3) {

        API.drawText("N", 350, 1000, 1, 255, 255, 255, 255, 2, 1, false, true, 0);

    }

});


