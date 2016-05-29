using UnityEngine;
using System.Collections;
//
public class Invincible : PowerUp {
	private float spawnRateModifier = 1f;
	private float upgradeTimeMultiplier = 1.5f; //increased duration when the powerup is upgraded
	private float levelThreeIncreasedSpawnRate;
	//defines the increased spawn rate and the duration
	public Invincible (float duration, float spawnRateModifier) :base("Invincible", duration) {
		this.spawnRateModifier = spawnRateModifier;
	}

	//defines the duration and leaves spawn rate unaffected
	public Invincible (float duration) :base("Invincible", duration){}

	//makes invincible and changes spawn rate if set
	#region implemented abstract members of PowerUp
	public override void usePowerUp (int lane) {
		//casts nullable float to float
		float duration = (float) this.duration;

		if (level == 1) {
			//increased spawn rate
			controller.increaseSpawnRate(duration, spawnRateModifier);

			//makes the specified lane invincible
			allZones[lane].makeInvunerable(duration);
		} else if (level == 2) {
			int secondLane = getSecondLane(lane);
			controller.increaseSpawnRate(duration, spawnRateModifier);
			//sets the lanes to invincible
			allZones[lane].makeInvunerable(duration);
			allZones[secondLane].makeInvunerable(duration);
		} else if (level == 3) {
			//increases spawn rate for all lanes
			controller.increaseSpawnRate(duration, spawnRateModifier * levelThreeIncreasedSpawnRate);

			//makes all lanes invincible
			foreach (ZoneCollisionDetection zone in allZones) {
				zone.makeInvunerable(duration);
			}
		}
	}
	#endregion
}
