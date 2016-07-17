using UnityEngine;

public abstract class PowerUp {
	const char bronze = 'b';
	const char silver = 's';
	const char gold = 'g';

	//event call
	public delegate void PowerUpPromotionAction();
	public static event PowerUpPromotionAction OnPowerUpPromotion;

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
	protected ZoneCollisionDetection [] allZones {get; set;}
	protected GenerationScript controller {get; set;}
	protected CollectionTimer timer {get; set;}

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
		if (IntUtil.InRange(level, descriptions.Length)) {
			int zeroIndexed = 1;
			return descriptions[level];
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
		allZones = GlobalVars.GATHERING_ZONES;
		controller = GlobalVars.GATHERING_CONTROLLER;
		timer = GlobalVars.GATHERING_TIMER;
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
		CheckIndexDictionary();
		return GlobalVars.POWERUP_INDEXES[powerUp.name];
	}

	public static int PowerUpIndex (string powerUpName) {
		CheckIndexDictionary();
		return GlobalVars.POWERUP_INDEXES[powerUpName];
	}

	public static void CheckIndexDictionary () {
		if (GlobalVars.POWERUP_INDEXES == null) {
			GlobalVars.InitializePowerupIndexes();
		}
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
}
