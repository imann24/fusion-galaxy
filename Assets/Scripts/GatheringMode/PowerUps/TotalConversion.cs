using UnityEngine;
public class TotalConversion : PowerUp {
	public const string NAME = "TotalConversion";
	public static readonly string[] DESCRIPTIONS = new string[]{
		"Tap or drag this ability into a specific lane to convert all spawned elements to the lane’s elemental type for a small amount of time.",//power 8, level 1
		"Tap or drag this ability into a specific lane to convert all spawned elements to the lane’s elemental type for a decent amount of time.",//power 8, level 2
		"Tap or drag this ability into a specific lane to convert all spawned elements to the lane’s elemental type and gain invincibility for a decent amount of time.",//power 8, level 3
	};

	public new float duration{ get; private set; } //overriding float to make sure it's not nullable

	private float timeMultiplier = 1.5f; // time increase for higher levels 

	//if you want it to be chosen randomely each time the usePowerUp function is called
	public TotalConversion (float duration): base (NAME, DESCRIPTIONS, duration) {
		this.duration = duration;
	}
	
	//sets all the elements onscreen to a type and continues to generate elements of just that type for the duration
    #region implemented abstract members of PowerUp
	public override void usePowerUp (int lane)
    {
		float duration;
		if (level > 1) { //increases duration if higher level
			duration = this.duration * timeMultiplier;
		} else {
			duration = this.duration;
		}

		// converts all elmeents to one type
		controller.allOnScreenElementsToOneType(lane);
	
		//spawns only one type for for set amount of time
		controller.generateOnlyOneType(lane, duration);

		//adds invincibility if level 3 
		if (level == 3) {
			foreach (ZoneCollisionDetection zone in allZones) {
				zone.makeInvunerable(duration);
			}
		}

    }
    #endregion
}