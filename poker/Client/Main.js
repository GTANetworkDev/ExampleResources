"use strict";
let inGame = false;
let browser;
let browserReady;
let turnMenu;
let gamePlayers = {};
let currentPot = 0;
let ourMoney = 0;
let maxBet = 0;
let ourBet = 0;
let currentTurn = null;
let lobbyId = 0;
let myturn = false;
API.onResourceStart.connect(() => {
    API.startCoroutine(browserCreationCoroutine);
    createMenu();
});
API.onServerEventTrigger.connect((ev, args) => {
    if (ev === "ENTER_GAME") {
        lobbyId = args[0];
        enterGame();
    }
    else if (ev === "LEAVE_GAME") {
        endGame();
    }
    else if (ev === "ADD_PLAYER") {
        gamePlayers[args[0]] = {};
        updateTimerbars();
    }
    else if (ev === "SET_PLAYERS") {
        let count = args[0];
        for (let i = 1; i <= count; i++) {
            gamePlayers[args[i]] = {};
        }
        updateTimerbars();
    }
    else if (ev === "PLAYER_LEAVE_GAME") {
        delete gamePlayers[args[0]];
        updateTimerbars();
    }
    else if (ev === "SET_POT") {
        currentPot = args[0];
        updateTimerbars();
    }
    else if (ev === "UPDATE_LAST_PLAYER_ACTION") {
        gamePlayers[args[0]].lastAction = args[1];
        gamePlayers[args[0]].hasCards = false;
        updateTimerbars();
    }
    else if (ev === "SET_HAND_CARDS") {
        browser.call("setHandCards", args[0], args[1]);
    }
    else if (ev === "SET_TABLE_CARDS") {
        browser.call("clearTableCards");
        let count = args[0];
        for (let j = 0; j < count; j++) {
            browser.call("addTableCard", args[j + 1]);
        }
    }
    else if (ev === "YOUR_TURN") {
        rebuildMenu();
        myturn = true;
        currentTurn = null;
        turnMenu.Visible = true;
        resource.InstructionalButtons.instance.clear();
        updateTimerbars();
    }
    else if (ev === "SOMEONES_TURN") {
        currentTurn = args[0];
        resource.InstructionalButtons.instance.clear();
        resource.InstructionalButtons.instance.addButtonInt("Waiting for " + args[0], 50);
        myturn = false;
        updateTimerbars();
    }
    else if (ev === "SET_MAX_BET") {
        maxBet = args[0];
    }
    else if (ev === "NEW_ROUND") {
        for (let player in gamePlayers) {
            gamePlayers[player].hasCards = false;
            gamePlayers[player].lastAction = null;
        }
        ourBet = 0;
        currentPot = 0;
        currentTurn = null;
        browser.call("clearTableCards");
        updateTimerbars();
        resource.InstructionalButtons.instance.clear();
    }
    else if (ev === "UPDATE_MONEY") {
        ourMoney = args[0];
        ourBet = args[1];
        updateTimerbars();
    }
    else if (ev === "SET_PLAYER_CARDS") {
        gamePlayers[args[0]].hasCards = true;
        gamePlayers[args[0]].cardOne = args[1];
        gamePlayers[args[0]].cardTwo = args[2];
        updateTimerbars();
    }
    else if (ev === "SHARD_CUSTOM") {
        API.showShard(args[0], args[1]);
    }
    else if (ev === "COLOR_SHARD_CUSTOM") {
        API.showColorShard(args[0], args[1], args[2], args[3], args[4]);
    }
});
API.onUpdate.connect(() => {
    if (myturn && !turnMenu.Visible)
        turnMenu.Visible = true;
    API.drawMenu(turnMenu);
});
API.onResourceStop.connect(() => {
    if (browserReady) {
        API.destroyCefBrowser(browser);
        browserReady = false;
    }
    //resource.InstructionalButtons.instance.clear();
    API.setHudVisible(true);
});
function createMenu() {
    turnMenu = API.createMenu("", "YOUR TURN", 0, 0, 6, false);
    turnMenu.Visible = false;
    turnMenu.ResetKey(menuControl.Back);
    API.setMenuBannerRectangle(turnMenu, 0, 0, 0, 0);
    turnMenu.OnItemSelect.connect((sender, selectedItem, index) => {
        myturn = false;
        switch (index) {
            case 0:
                // Raise
                API.triggerServerEvent("TURN_RESPONSE", lobbyId, 0, parseInt(selectedItem.IndexToItem(selectedItem.Index).toString()));
            case 1:
                // Call/check
                API.triggerServerEvent("TURN_RESPONSE", lobbyId, 1);
            case 2:
                // Fold
                API.triggerServerEvent("TURN_RESPONSE", lobbyId, 2);
        }
        turnMenu.Visible = false;
    });
}
function rebuildMenu() {
    turnMenu.Clear();
    var list = new List(String);
    list.Add((maxBet + maxBet * 0.5).toString());
    list.Add((maxBet + maxBet * 1).toString());
    list.Add((maxBet + maxBet * 1.5).toString());
    list.Add((maxBet + maxBet * 2).toString());
    list.Add((maxBet + maxBet * 2.5).toString());
    list.Add(ourMoney.toString());
    let raiseItem = API.createListItem("Raise", "", list, 0);
    turnMenu.AddItem(raiseItem);
    if (ourMoney <= maxBet)
        raiseItem.Enabled = false;
    let callItem;
    if (maxBet > ourBet) {
        callItem = API.createMenuItem("Call", "");
        callItem.SetRightLabel("$" + Math.min(maxBet, ourMoney));
    }
    else {
        callItem = API.createMenuItem("Check", "");
        callItem.SetRightLabel("$0");
    }
    turnMenu.AddItem(callItem);
    turnMenu.AddItem(API.createMenuItem("Fold", ""));
}
function enterGame() {
    inGame = true;
    API.setHudVisible(false);
    API.setCefBrowserHeadless(browser, false);
    browser.call("setHandCards", "a:diams", "a:spades");
    browser.call("clearTableCards");
    browser.call("clearTimers");
    resource.InstructionalButtons.instance.clear();
    resource.InstructionalButtons.instance.addButtonInt("Waiting for players", 50);
    gamePlayers = {};
}
function endGame() {
    inGame = false;
    API.setHudVisible(true);
    resource.InstructionalButtons.instance.clear();
    if (browserReady)
        API.setCefBrowserHeadless(browser, true);
}
function updateTimerbars() {
    if (!browserReady)
        return;
    browser.call("clearTimers");
    for (let player in gamePlayers) {
        if (gamePlayers[player].hasCards) {
            browser.call("addTimerCards", player, gamePlayers[player].cardOne, gamePlayers[player].cardTwo);
        }
        else {
            if (gamePlayers[player].lastAction)
                browser.call("addTimerText", player, gamePlayers[player].lastAction, player === currentTurn);
            else
                browser.call("addTimerText", player, "READY", player === currentTurn);
        }
    }
    browser.call("addTimerText", "CASH", "$" + ourMoney); // COMMENT THIS LINE IF YOU ALREADY HAVE A CASH HUD
    browser.call("addTimerText", "BET", "$" + maxBet);
    browser.call("addTimerText", "POT", "$" + currentPot);
}
function* browserCreationCoroutine() {
    var res = API.getScreenResolution();
    browser = API.createCefBrowser(res.Width, res.Height, true);
    API.setCefBrowserPosition(browser, 0, 0);
    API.setCefBrowserHeadless(browser, true);
    while (!API.isCefBrowserInitialized(browser)) {
        yield 500;
    }
    browserReady = true;
    API.loadPageCefBrowser(browser, "Client/cef/ui.html", false);
    while (API.isCefBrowserLoading(browser)) {
        yield 500;
    }
    yield 100;
    browser.call("setHandCards", "5:diams", "8:clubs");
    browser.call("clearTableCards");
    browser.call("addTableCard", "5:clubs");
    browser.call("addTableCard", "5:spades");
    browser.call("addTableCard", "5:hearts");
    browser.call("addTableCard", "7:diams");
    browser.call("addTableCard", "k:spades");
    browser.call("clearTimers");
    browser.call("addTimerCards", "TYCHO", "q:hearts", "q:spades");
    browser.call("addTimerText", "MAX", "$100 blind");
    browser.call("addTimerText", "STRONG BAD", "called $200");
    browser.call("addTimerText", "BROCK", "raised $200");
    browser.call("addTimerText", "POT", "$24000");
}
