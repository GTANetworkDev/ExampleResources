API.onServerEventTrigger.connect(function (eventName, args) {
    if (eventName=="markonmap") {

        API.setWaypoint(args[0], args[1]);
       
    }
});