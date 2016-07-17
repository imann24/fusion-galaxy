// slow down fall for elements
// NOTE: the fallSpeedModifier is multipled by the level2 and level3 modifers based on the level of the powerup
public class SlowFall : PowerUp {
	public const string NAME = "SlowFall";
	public static readonly string [] DESCRIPTIONS = new string[]{
		"Tap or drag this ability into a specific lane to slow down the fall of elements contained.",//power 2, level 1
		"Tap or drag this ability into a specific lane to significantly slow down the fall of elements contained.",//power 2, level 2
		"Tap or drag this ability to significantly slow down the fall all elements.",//power 2, level 3
	};

	//event call
	public delegate void SlowFallActivatedAction();
	public static event SlowFallActivatedAction OnSlowFallActivated;

	//controls the effects of the slow fall
	public new float duration{ get; private set;} //an overriding float to avoid passing modifyFallRate a nullable float? 
	private float baseFallSpeedModifier;
	private float level2Multiplier = 0.5f; //determines the factor the slow down rate increases by for level 2
	private float level3Multiplier = 0.5f; //determines the factor the slow down rate increases by for level 3
	private int timeBonus;


	//creates the powerup
	// NOTE: baseFallSpeedModifier is twice as slow for level 2 and 3 of the power up (see multipliers above)
	public SlowFall (float baseFallSpeedModifier, int timeBonus, float duration): base (NAME, DESCRIPTIONS, duration) {

		this.duration = duration;
		this.baseFallSpeedModifier = baseFallSpeedModifier;
		this.timeBonus = timeBonus;
	}

	#region implemented abstract members of PowerUp
	public override void usePowerUp (int lane) {
		if (OnSlowFallActivated != null) {
			OnSlowFallActivated();
		}

		//adds time to the clock
		timer.addTime(timeBonus);

		//slows elements down based on the level of the powerup
		if (level == 1) {
			controller.modifyFallRate(baseFallSpeedModifier, duration, lane);
		} else if (level == 2) {
			controller.modifyFallRate(baseFallSpeedModifier * level2Multiplier, duration, lane);
		} else if (level == 3) {
			controller.modifyFallRate(baseFallSpeedModifier * level3Multiplier, duration);
		}
	}
	#endregion
}
