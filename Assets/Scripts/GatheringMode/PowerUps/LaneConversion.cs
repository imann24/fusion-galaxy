/*
 * A powerup that converts elements to the correct lane type
 * Can be upgraded to effect two lanes and then all of them
 * Converts all the current elements in the specified lanes to the correct time
 */

using UnityEngine;
public class LaneConversion : PowerUp {
	public const string NAME = "LaneConversion";
	public static readonly string [] DESCRIPTIONS = new string[]{
		"Tap or drag this ability into a specific lane to convert the elements contained to the lane’s elemental type.",//power 1, level 1
		"Tap or drag this ability into a specific lane to convert the elements contained, and an adjacent lane, to each lane’s elemental type.",//power 1, level 2
		"Tap or drag this ability to convert the elements contained within each lane to the lane’s elemental type.",//power 1, level 3
	};

	//event call
	public delegate void LaneConversionActivatedAction();
	public static event LaneConversionActivatedAction OnLaneConversionActivated;

	public new float duration{ get; private set; } //overriding float to make sure it's not nullable

	//constructor for the powerup
	public LaneConversion (float duration): base (NAME, DESCRIPTIONS, duration) {
		this.duration = duration;
	}
	
	//sets all the elements onscreen to a type and continues to generate elements of just that type for the duration
    #region implemented abstract members of PowerUp
    public override void usePowerUp (int lane)
    {
		if (controller == null) {
			controller = GlobalVars.GATHERING_CONTROLLER;
		}
		if (OnLaneConversionActivated != null) {
			OnLaneConversionActivated();
		}

		if (level == 1) {
			//makes all the currently spawned elements in the lane into the proper type
			controller.allElementsInLaneToOneType(lane, lane);

			//makes all the elements spawn for a specific amount of time into the proper type
			controller.generateInCorrectLane(lane, duration);
		} else if (level == 2) { //one additional lane spawns correctly
			//makes the elements in the lane spawn correctly 
			controller.allElementsInLaneToOneType(lane, lane);

			//makes all the elements spawn for a specific amount of time into the proper type
			controller.generateInCorrectLane(lane, duration);
			int secondLane = getSecondLane(lane);

			//makes the elements in the second lane spawn correctly 
			controller.allElementsInLaneToOneType(secondLane, secondLane);
			
			//makes all the elements spawn for a specific amount of time into the proper type
			controller.generateInCorrectLane(secondLane, duration);
		} else if (level == 3) { //performs a total lane conversion
			controller.allElementsToLaneType();

			//keeps all the elements spawning in the correct lanes for a certain amount of time
			for (int i = 0; i < GlobalVars.NUMBER_OF_LANES; i++) {
				controller.generateInCorrectLane(i, duration);
			}
		}

    }
    #endregion
}