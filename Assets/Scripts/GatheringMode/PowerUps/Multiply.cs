/*
 * A powerup that multiplies the score in specified lanes
 * Can be upgraded to effect more lanes
 * The current score in that lane is multiplied
 * And the score of the elements that fall into the bucket are also multiplied for a certain amount of time
 */

using UnityEngine;
using System.Collections;

public class Multiply: PowerUp {
	public const string NAME = "Multiply";
	public static readonly string [] DESCRIPTIONS = new string[]{
		"Tap or drag this ability into a specific lane to receive a score multiplier for the elements collected of that lane’s elemental type.",//power 4, level 1
		"Tap or drag this ability into a specific lane to receive a score multiplier for the elements collected of that lane’s elemental type, and an adjacent lane’s elemental type.",//power 4, level 2
		"Tap or drag this ability to receive a score multiplier for elements collected of all elemental types.",//power 4, level 3
	};

	//event
	public delegate void ElementMultipyActivatedAction();
	public static event ElementMultipyActivatedAction OnElementMultiplyActived;

	//the score modifiers
	private int multiplier = 2;

	//uses the default multiplier
	public Multiply (float duration):base (NAME, DESCRIPTIONS, duration) {}

	//assigns the multiplier and a version that has no expiration
	public Multiply (int mutiplier):base(NAME, DESCRIPTIONS, null) {
		this.multiplier = multiplier;
	}

	//assigns the target zone and the score modifier
	public Multiply (int multiplier, float duration):base(NAME, DESCRIPTIONS, duration) {
		this.multiplier = multiplier;
	}

	//uses the power up
	// Called on a specific lane
	public override void usePowerUp (int lane) {

		/*
		 * Uses the three level upgrade system 
		 * At the first level it multiplies the score in one lane
		 * Multiplies the score in two lanes at the second
		 * Multiplies the score in all four lanes in the third
		 */

		if (level == 1) {
			allZones[lane].modifyPoints(multiplier, duration);
			allZones[lane].multiplyCurrentScore(multiplier);

			//increases the base modifier (so that multipliers stack)
			if (duration == null) {
				allZones[lane].modifyBasePoints(multiplier);
			}
		} else if (level == 2) {
			//targets two lanes
			int secondLane = getSecondLane(lane);
			allZones[lane].modifyPoints(multiplier, duration);
			allZones[lane].multiplyCurrentScore(multiplier);
			allZones[secondLane].modifyPoints(multiplier, duration);
			allZones[secondLane].multiplyCurrentScore(multiplier);
			//increases the base modifier (so that multipliers stack)
			if (duration == null) {
				allZones[lane].modifyBasePoints(multiplier);
				allZones[secondLane].modifyBasePoints(multiplier);
			}
		} else if (level == 3) {
			foreach (ZoneCollisionDetection zone in allZones) {
				zone.modifyPoints(multiplier, duration);
				zone.multiplyCurrentScore(multiplier);
				if (duration == null) { 
					zone.modifyBasePoints(multiplier);
				}
			}
		}		
	}
}
