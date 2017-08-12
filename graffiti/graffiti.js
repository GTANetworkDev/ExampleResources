API.onServerEventTrigger.connect(function(ev, args) {
	if (ev === "REQUEST_GRAFFITI") {
		var player = API.getLocalPlayer();

		var pos = API.getEntityPosition(player);
		var rot = API.getEntityRotation(player);

		var dir = new Vector3(Math.cos((rot.Z + 90) * 0.0174533) * 2, Math.sin((rot.Z + 90) * 0.0174533) * 2, 0);
		var dirRight = new Vector3(Math.cos((rot.Z) * 0.0174533) * 2, Math.sin((rot.Z) * 0.0174533) * 2, 0);

		var end = new Vector3(pos.X + dir.X, pos.Y + dir.Y, pos.Z);
		var startRight = new Vector3(pos.X + dirRight.X, pos.Y + dirRight.Y, pos.Z);
		var endRight = new Vector3(end.X + dirRight.X, end.Y + dirRight.Y, end.Z);

		
		var raycast = API.createRaycast(pos, end, 1, player);
		var raycastRight = API.createRaycast(startRight, endRight, 1, player);

		if (raycast.didHitAnything && !raycast.didHitEntity && raycastRight.didHitAnything && !raycastRight.didHitEntity) {
			API.triggerServerEvent("GRAFFITI_RESPONSE", true, raycast.hitCoords, args[0], args[1], raycastRight.hitCoords);
		} else {
			API.triggerServerEvent("GRAFFITI_RESPONSE", false);
		}

	}
});