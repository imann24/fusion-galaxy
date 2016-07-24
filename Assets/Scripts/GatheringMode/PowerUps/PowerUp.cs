using UnityEngine;

public abstract class PowerUp {
	const char bronze = 'b';
	const char silver = 's';
	const char gold = 'g';
	
	//event call
	public delegate void PowerUpPromotionAction();
	public static event PowerUpPromotionAction OnPowerUpPromotion;
	public delegate void UnlockPowerUpAction(PowerUp powerUp);
	public static event UnlockPowerUpAction OnUnlockPowerUp;

	//name is accessible but not changeable outside the subclass
	public string name {get; protected set;}

	//description is accessible but not changeable outside the subclass
	public string[] descriptions {get; protected set;}

	//duration is accessible but not changeable outside the subclass
	public float? duration {get; protected set;}

	//level is accessible but not changeable outside the subclass
	public int level {get; protected set;}
	public const int MAX_LEVEL = 3;

	//script references
	protected ZoneCollisionDetection [] allZones {
		get {
			return GlobalVars.GATHERING_ZONES;
		}
	}
	protected GenerationScript controller {
		get {
			return GlobalVars.GATHERING_CONTROLLER;
		}
	}
	protected static CollectionTimer timer {
		get {
			return GlobalVars.GATHERING_TIMER;
		}
	}

	public Sprite GetSprite () {
		Sprite mySprite;
		if (GlobalVars.POWERUP_SPRITES_BY_NAME.TryGetValue(GetSpriteKey(), out mySprite)) {
			return mySprite;
		} else {
			Debug.LogFormat("Does not contain sprite key {0}", GetSpriteKey());
			return null;
		}
	}

	public string GetDescription () {
		int zeroIndexed = 1;
		if (IntUtil.InRange(level-zeroIndexed, descriptions.Length)) {
			return descriptions[level-zeroIndexed];
    	} else {
			return null;
		}
	}

	string GetSpriteKey () {
		return string.Format("{0}-{1}", name.ToLower().Replace(" ", string.Empty), RankSuffix());
	}

	char RankSuffix () {
		switch (level) {
		case 1:
			return bronze;
		case 2:
			return silver;
		case 3:
			return gold;
		default:
			return bronze;
		}
	}

	//super class constructor for powerups, should be called by all subclass constructors
	public PowerUp (string name, string[] descriptions, float? duration) {
		this.name = name;
		this.descriptions = descriptions;
		this.duration = duration;
		level = GetPowerUpLevel(this);
	}

	//uses the powerup on a certain lane
	public abstract void usePowerUp (int lane);	

	//increases powerup by a level
	public static void PromotePowerUp (PowerUp powerUp) {
		if (powerUp.level < PowerUp.MAX_LEVEL) {
			Utility.IncreasePlayerPrefValue(powerUp.name + GlobalVars.POWERUP_LEVEL_STRING);
			powerUp.level++;

			//event for powerup promotion
			if (OnPowerUpPromotion != null) {
				OnPowerUpPromotion();
			}
		}
	}

	//overloaded version that takes a string
	public static void PromotePowerUp (string powerUpName) {
		if (GetPowerUpLevel(powerUpName) < PowerUp.MAX_LEVEL) {
			Utility.IncreasePlayerPrefValue(powerUpName + GlobalVars.POWERUP_LEVEL_STRING);
			
			//event for powerup promotion
			if (OnPowerUpPromotion != null) {
				OnPowerUpPromotion();
			}
		}
	}

	//returns the powerups current level
	public static int GetPowerUpLevel (PowerUp powerUp) {
		return PlayerPrefs.GetInt(powerUp.name + GlobalVars.POWERUP_LEVEL_STRING, 1);
	}

	//overloaded version that takes a string
	public static int GetPowerUpLevel (string powerUpName) {
		return PlayerPrefs.GetInt(powerUpName + GlobalVars.POWERUP_LEVEL_STRING, 1);
	}

	//resets the level to 1
	public static void ResetPowerUpLevel (PowerUp powerUp) {
		PlayerPrefs.SetInt(powerUp.name + GlobalVars.POWERUP_LEVEL_STRING, 1);
		powerUp.level = 1;
	}

	//overloaded version that takes a string
	public static void ResetPowerUpLevel (string powerUpName) {
		PlayerPrefs.SetInt(powerUpName + GlobalVars.POWERUP_LEVEL_STRING, 1);
	}

	//check to see if powerup is unlocked
	public static bool PowerUpUnlocked (string powerUpName) {
		int powerUpIndex = PowerUpIndex(powerUpName);
		if (powerUpIndex == 0) {
			return Element.AllTierElementsUnlocked(0) && Element.AllTierElementsUnlocked(1);
		} else {
			return Element.AllTierElementsUnlocked(powerUpIndex+1);
		}
	}

	//check to see if powerup is unlocked (overloaded to take a PowerUp)
	public static bool PowerUpUnlocked (PowerUp powerUp) {
		return PowerUpUnlocked(powerUp.name);
	}

	//gets the index of a powerup in the progression system
	public static int PowerUpIndex (PowerUp powerUp) {
		return GlobalVars.POWERUP_INDEXES[powerUp.name];
	}

	public static int PowerUpIndex (string powerUpName) {
		return GlobalVars.POWERUP_INDEXES[powerUpName];
	}

	//changes the stored level in the individual instance of the powerup
	public void overrideLevel (int level) {
		this.level = level;
	}

	//utility method 
	protected int getSecondLane (int lane) {
		int secondLane = 0;
		switch (lane) {
			case 0:
				secondLane = lane+1;
				break;
			case 1:
				secondLane = lane+1;
				break;
			case 2:
				secondLane = lane-1;
				break;
			case 3:
				secondLane = lane-1;
				break;
			default:  break;
		}
		return secondLane;
	}

	static void CallPowerUpUnlockedEvent (PowerUp powerUp) {
		if (OnUnlockPowerUp != null) {
			OnUnlockPowerUp(powerUp);
		}
	}

	public static void CheckForUnlocks () {
		foreach (string powerUpName in GlobalVars.POWERUP_INDEXES.Keys) {
			if (!DataController.GetPowerUpUnlocked(powerUpName) && 
			    PowerUpUnlocked(powerUpName)) {
				DataController.SetPowerUpUnlock(powerUpName, true);
				CallPowerUpUnlockedEvent(GetPowerUp(powerUpName));
			}
		}
	}

	public static PowerUp[] GetAll () {
		int DEFAULT_NUMBER = default(int);
		PowerUp[] powerUps = new PowerUp[GlobalVars.POWERUP_COUNT];
		
		//the array of all possible PowerUps the spawned PowerUp could be
		powerUps[0] = new LaneConversion(DEFAULT_NUMBER);
		powerUps[1] = new SlowFall(DEFAULT_NUMBER,DEFAULT_NUMBER,DEFAULT_NUMBER);
		powerUps[2] = new Fuel(DEFAULT_NUMBER) ;
		powerUps[3] = new Multiply(DEFAULT_NUMBER,DEFAULT_NUMBER);
		powerUps[4] = new BucketShield(DEFAULT_NUMBER);
		powerUps[5] = new TapToCollect(DEFAULT_NUMBER);
		powerUps[6] = new Invincible(DEFAULT_NUMBER, DEFAULT_NUMBER);
		powerUps[7] = new TotalConversion(DEFAULT_NUMBER);
		powerUps[8] = new CollectAll();

		return powerUps;

	}

	public static PowerUp GetPowerUp (string name) {
		PowerUp[] powerUps = GetAll ();
		for (int i = 0; i < powerUps.Length; i++) {
			if (powerUps[i].name == name) {
				return powerUps[i];
			}
		}
		return null;
	}
}
