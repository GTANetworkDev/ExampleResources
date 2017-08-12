using GTANetworkServer;
using GTANetworkShared;
using System;
using System.Collections.Generic;

public class Graffiti : Script
{
	public Graffiti()
	{
		API.onClientEventTrigger += onClientEventTrigger;
	}
	public void onClientEventTrigger(Client sender, string ev, object[] args)
	{
		if (ev == "GRAFFITI_RESPONSE")
		{
			bool success = (bool)args[0];

			if (success)
			{
				var rot = sender.rotation;
				var pos = (Vector3) args[1];
				var text = (string) args[2];
				var color = (int) args[3];
				var rcRight = (Vector3) args[4];			

				var dir = rcRight - pos;

				var angle = (float) Math.Atan2(dir.Y, dir.X);


				var stickOutDir = sender.position - pos;
				stickOutDir.Normalize();

				pos = pos + stickOutDir * 0.05f;
				pos = pos + new Vector3(0, 0, 1);
				angle = (angle * 57.2958f);

				API.exported.billboard.createOrganizationName(text, 0, color, 11, pos, new Vector3(0, 0, angle), new Vector3(6, 6, 1));
			}
			else
			{
				sender.sendChatMessage("You cannot draw graffiti here!");
			}
		}
	}

	[Command("Graffiti", GreedyArg = true)]
	public void Graffiticmd(Client sender, int color, string text)
	{
		sender.triggerEvent("REQUEST_GRAFFITI", text, color);
	}
}