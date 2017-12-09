var setPlayerBlip = (blip) => {
    blip.setAlpha(0);
};

mp.events.add('SET_PLAYER_BLIP', setPlayerBlip);