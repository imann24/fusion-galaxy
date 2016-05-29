public class TapToCollect : PowerUp {
	//event call
	public delegate void TapToCollectActivatedAction();
	public static event TapToCollectActivatedAction OnTapToCollectActivated;
	//multipliers to extend the time that the powerup lasts for
	private float level2Multiplier = 1.25f;
	private float level3Multiplier = 2.0f;

	//creates the powerup
	public TapToCollect (float duration):base ("TapToCollect", duration){}

	#region implemented abstract members of PowerUp

	//actiavtes tap to collect
	public override void usePowerUp (int lane) {
		if (OnTapToCollectActivated != null) {
			OnTapToCollectActivated();
		}
		float seconds = (float) duration;

		if (level == 1) {} //currently no additional action needed if on level 1

		//multipliers for higher levels
		else if (level == 2) { 
			seconds *= level2Multiplier;
		} else if (level == 3) {
			seconds *= level3Multiplier;
		}

		//activates the powerup for the specified amount of time
		controller.activateTapToCollect(seconds);
	}

	//overloaded method so you can call it without a lane (because lane does not effect it
	public void usePowerUp () {
		usePowerUp();
	}
	#endregion



}
