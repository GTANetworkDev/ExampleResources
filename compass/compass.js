API.onUpdate.connect(function (sender) {

    if (0.3 < API.getGameplayCamDir().X && 0.3 < API.getGameplayCamDir().Y) {

        API.drawText("NE", 350, 1000, 1, 255, 255, 255, 255, 2, 1, false, true, 0);

    }

    else if (API.getGameplayCamDir().X<-0.3 && 0.3 < API.getGameplayCamDir().Y) {

        API.drawText("NW", 350, 1000, 1, 255, 255, 255, 255, 2, 1, false, true, 0);

    }

    else if (0.3 < API.getGameplayCamDir().X && API.getGameplayCamDir().Y < -0.3) {

        API.drawText("SE", 350, 1000, 1, 255, 255, 255, 255, 2, 1, false, true, 0);

    }

    else if (API.getGameplayCamDir().X < -0.3 && API.getGameplayCamDir().Y < -0.3) {

        API.drawText("SW", 350, 1000, 1, 255, 255, 255, 255, 2, 1, false, true, 0);

    }

    else if (-0.3 < API.getGameplayCamDir().X && API.getGameplayCamDir().X < 0.3 && API.getGameplayCamDir().Y < -0.3) {

        API.drawText("S", 350, 1000, 1, 255, 255, 255, 255, 2, 1, false, true, 0);

    }

    else if (API.getGameplayCamDir().X < -0.3 && -0.3 < API.getGameplayCamDir().Y && API.getGameplayCamDir().Y < 0.3) {

        API.drawText("W", 350, 1000, 1, 255, 255, 255, 255, 2, 1, false, true, 0);

    }

    else if (0.3 < API.getGameplayCamDir().X && -0.3 < API.getGameplayCamDir().Y && API.getGameplayCamDir().Y < 0.3) {

        API.drawText("E", 350, 1000, 1, 255, 255, 255, 255, 2, 1, false, true, 0);

    }

    else if (-0.3 < API.getGameplayCamDir().X && API.getGameplayCamDir().X < 0.3 && API.getGameplayCamDir().Y > 0.3) {

        API.drawText("N", 350, 1000, 1, 255, 255, 255, 255, 2, 1, false, true, 0);

    }

});


