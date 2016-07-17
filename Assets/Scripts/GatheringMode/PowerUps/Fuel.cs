public class Fuel : PowerUp {
	public const string NAME = "Fuel";
	public static readonly string [] DESCRIPTIONS = new string[]{
		"Your fuel does not deplete for a certain duration.",//power 3, level 1
		"You gain additional fuel and it does not deplete for a certain duration.",//power 3, level 2
		"You gain a significant amount of additional fuel and it does not deplete for a certain duration.",//power 3, level 3
	};

	//event call
	public delegate void FuelAddedAction();
	public static event FuelAddedAction OnFuelAdded;
	
	//how time is to be added
	private int timeBonus;

	private float levelThreeTimeMultiplier = 1.5f;

	//constructor to make a new bonus time powerup
	public Fuel (int timeBonus) : base (NAME, DESCRIPTIONS, null) {
		this.timeBonus = timeBonus;
	}

	public Fuel (float duration, int timeBonus) : base (NAME, DESCRIPTIONS, duration) {
		this.timeBonus = timeBonus;
		this.duration = duration;
	}

	//adds time in seconds to the clock
	public override void usePowerUp (int lane) {
		if (OnFuelAdded != null) {
			OnFuelAdded();
		}
		//allows for the original use of the powerup, just to add time, not to pause the timer
		if (duration == null) {
			timer.addTime(timeBonus);
		} else { //if the timer is using the levelled system
			//paused the clock
			timer.pauseClock((float)duration);
		}

		//implementation of the level system
		if (level == 1) { //level 1 functionality is already implemented in else block above
			return;
		} else if (level == 2) { //pauses the timer and adds time
			timer.addTime(timeBonus);
		} else if (level == 3) {
			//time is multiplied by the level three modifier and then cast to an int
			timer.addTime((int)(timeBonus * levelThreeTimeMultiplier));
		}
	}

	//overload method so that it can be called without a number (because lane does not effect powerup)
	public void usePowerUp () {
		usePowerUp(0);
	}
}
