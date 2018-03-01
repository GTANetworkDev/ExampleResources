//This script will make vehicles have smoother acceleration.
//To disable the script call an event and set "GlobalDisable" to true.
//To disable individual sections, set "DisableAntiReverse" or "DisableSmoothThrottle" to true.

//This script includes an anti-reverse brake system with automatic brake lights.

//When you brake, while holding brake you will come to a complete stop and won't reverse until you
//release the brake button and press it again.

mp.events.add("SmoothThrottle_PlayerEnterVehicle", (entity, seat) =>
{
	BrakeSystem = true;
});

mp.events.add("SmoothThrottle_PlayerExitVehicle", (entity) =>
{
	BrakeSystem = false;
});

mp.events.add("SmoothThrottle_SetSmoothThrottle", (turnedOn) =>
{
	DisableSmoothThrottle = !turnedOn;
});

mp.events.add("SmoothThrottle_SetAntiReverse", (turnedOn) =>
{
	DisableAntiReverse = !turnedOn;
});

mp.events.add("SmoothThrottle_SetGlobal", (turnedOn) =>
{
	GlobalDisable = !turnedOn;
});

let GlobalDisable = false;
let DisableAntiReverse = false;
let DisableSmoothThrottle = false;

let BrakeSystem = false;
let vehicleStopped = false;
let vehicleStoppedOnOwn = false;
let constantStart = 0.25; //starts at 0.25 and increases to 1
let constantStep = 0.065; //You can change this for a faster throttle response (Will cause more skidding)

let deltaAmount = constantStart; 
let prevTime = mp.game.invoke('0x5AFB8ED811F05E4D');
let diffToggle = false;

mp.events.add("render", () =>
{
	if(GlobalDisable)
		return;
	
	if(BrakeSystem)
	{
		if(mp.players.local.vehicle != null)
		{
			if(!mp.players.local.vehicle.isSeatFree(-1)) //only do this if the vehicle has a driver (doesn't have to be the player who is rendering this)
			{
				//Optimize function calls to variables (probably doesn't make a difference)
				let vehClass = mp.players.local.vehicle.getClass();
				let isControl71Pressed = mp.game.controls.isControlPressed(0, 71); //accelerate
				let isControl72Pressed = mp.game.controls.isControlPressed(0, 72); //brake
				let isControl76Pressed = mp.game.controls.isControlPressed(0, 76); //handbrake
				let speed = mp.players.local.vehicle.getSpeed();
				
				//Only do it to car classes
				if(!DisableSmoothThrottle && ((vehClass >= 0 && vehClass <= 12) || vehClass === 18 || vehClass === 19 || vehClass === 20))
				{
					if(isControl71Pressed || isControl72Pressed)
					{
						if(isControl76Pressed)
						{
							deltaAmount = 1.0; //If people are buffering their throttle up
						}
						
						mp.players.local.vehicle.setEngineTorqueMultiplier(deltaAmount);
						
						//Calculate tick time and step every 250ms
						if(mp.game.invoke('0x5AFB8ED811F05E4D') - prevTime > 250)
						{
							prevTime = mp.game.invoke('0x5AFB8ED811F05E4D');
							deltaAmount += constantStep * speed; //Curve
							if(deltaAmount > 1.0)
							{
								deltaAmount = 1.0;
							}
						}
					}
					else
					{
						deltaAmount = constantStart; //Reset when they let go of throttle
						//mp.game.controls.setControlNormal(0, 71, amount);
					}
				}
				
				//THIS IS THE BRAKE LIGHT SYSTEM WITH ANTI-REVERSE
				if(DisableAntiReverse)
					return;
				
				if(speed < 1)
				{
					vehicleStopped = true;
				}
				else
				{
					vehicleStopped = false;
					vehicleStoppedOnOwn = false;
					diffToggle = false;
				}
				
				if((!isControl72Pressed && mp.game.controls.isControlEnabled(0, 72)) && !isControl76Pressed && vehicleStopped)
				{
					vehicleStoppedOnOwn = true;
					mp.players.local.vehicle.setBrakeLights(true);
				}
				
				if(vehicleStopped && !vehicleStoppedOnOwn && !mp.players.local.vehicle.isInBurnout() && !diffToggle)
				{
					mp.players.local.vehicle.setBrakeLights(true);
					mp.game.controls.disableControlAction(0, 72, true);
				}
				
				if((isControl71Pressed && !isControl72Pressed) || isControl76Pressed)
				{
					mp.players.local.vehicle.setBrakeLights(false);
				}
				
				if(mp.game.controls.isDisabledControlJustReleased(0, 72) && vehicleStopped)
				{
					mp.game.controls.enableControlAction(0, 72, true);
					diffToggle = true;
				}
			}
		}
	}
});