#define DEBUG
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//attached to every powerup that is spawned, used to assign its type and activate its power
/// <summary>
/// Activate power up.
/// Turns on the powerup when the user releases it in a lane
/// affects the lane its dragged into if it is lane specifc
/// </summary>
public class ActivatePowerUp : MonoBehaviour {
	//event call when the powerup is used
	public delegate void PowerUpAction(string powerUpName, int powerUpLevel);
	public static event PowerUpAction OnPowerUpUsed;

	//tuning variables
	public float powerupLifeTime = 10.0f;
	public float powerupTappedTime = 0.10f;

	//visual stuff
	public SpriteRenderer mySprite;
	GameObject background;
	BackgroundVisuals backgroundScript;

	//powerup specific colors
	private Color addTimeColor;
	private Color invincibleColor;
	private Color elementMultiplyColor;
	private Color slowFallColor;
	private Color totalMultiplierColor;
	private Color invincibleConversionColor;
	private Color laneConversionColor;
	private Color totalConversionColor;
	private Color totalLaneConversionColor;
	private Color collectAllColor;
	private Color bucketShieldColor = new Color (72f/255f, 184f/255f, 201f/255f);


	//the animator that play the dissolve animation when the powerup is pressed
	Animator powerUpTappedAnim;

	//array of PowerUp instances
	private static PowerUp[] PowerUps;
	private static List<PowerUp> UnlockedPowerUps = new List<PowerUp>();

	//uses a dictionary to index the unlocked sprites to the list of all sprites
	private static Dictionary<int, int> UnlockedPowerupIndex = new Dictionary<int,int>();

	//the power that is chosen
	int whichPower;
	PowerUp myPowerUp;

	//arrays of visual stuffs
	static Color[] powerColor = new Color[10];

	//AddFuel/TimeBonus PowerUp vars
	private static int timeBonusAddTime = 5;

	//Invincible PowerUp vars
	private static float spawnRateModifierInvincible = 2.0f;
	private static float durationInvincible = 10;

	//ElementMultiply PowerUp vars
	private static int multiplierElementMultiply = 2;

	//SlowFall PowerUp vars
	private static float fallSpeedModifierSlowFall = 0.5f;
	private static int timeBonusSlowFall = 3;
	private static float durationSlowFall = 10;

	//TotalMultiply PowerUp vars
	private static float durationTotalMultiply;

	//for bucket shield
	private static int bucketShieldHitPoints = 2;

	//duration for tap to collect
	private static float durationTapToCollect = 5.0f;

	//LaneConversion PowerUp vars
	private static float durationLaneConversion = 15;

	//TotalConversion PowerUp vars
	private static float durationTotalConversion = 5;

	//prevents double tapping
	bool iHaveBeenUsed = false;

	// An animator for the powerup flash
	private Animator backgroundFlash;

	// An animator for the powerup text
	public Animator powerUpTextAnimator;

	// An array of the lanes animators for lane conversion
	public Animator[] laneAnimators = new Animator[4];

	// An animator for the collectAll animation that covers the screen
	public Animator collectionAnimator;
	// A gameobject for the controller.
	private GameObject controller;
	// Powerup has been manually set.
	private bool powerUpSet = false;

	// Use this for initialization
	void Start () {
		// Find the Controller in the scene
		controller = GameObject.Find ("Controller");
		//sets the reference to play the background animation
		backgroundFlash = GatheringPowerUpAnimationController.instance.animator;

		background = GameObject.Find("SceneComponents/background/backgroundImage");
		backgroundScript = background.GetComponent<BackgroundVisuals> ();

		//initializes the colos for the poweups, if they're currently none
		InitializeColors ();
		Debug.Log("Colour 1 should be: " + powerColor[0]);
		SetPowerUp ();

		powerUpTappedAnim = this.transform.GetChild(0).GetComponent<Animator> ();


	}


	// destroys the powerup if it goes off the screen
	void OnBecameInvisible(){
		Destroy (gameObject);
	}

	// sets the type of the powerup based off random selection from the unlocked powerups
	void SetPowerUp(){
		if(powerUpSet){
			return;
		}

		// If no powerups are unlocked, sets the powerup to lane conversion (the first powerup)
		if (UnlockedPowerUps.Count == 0) {
			SetPowerUp(0);
			UnlockedPowerUps.Add(PowerUps[0]);
			UnlockedPowerupIndex.Add(0, 0);
			Debug.Log("No powerups are unlocked but game is trying to spawn one");
			return;
		}

		StartCoroutine(destroyPowerUpOnTimer(powerupLifeTime));
		//defines which of the powers the spawned PowerUp will be
		whichPower = Random.Range (0,UnlockedPowerUps.Count);
		myPowerUp = UnlockedPowerUps[whichPower];

		//spawn the power-up and add a life timer
		mySprite.sprite = GlobalVars.POWERUP_SPRITES[UnlockedPowerupIndex[whichPower]];
	}

	// Sets the powerup to a specified type (regardless of which are unlocked)
	public void SetPowerUp(int index){
		powerUpSet = true;
		//defines which of the powers the spawned PowerUp will be
		whichPower = index;
		myPowerUp = PowerUps[whichPower];
		//spawn the power-up and add a life timer
		mySprite.sprite = GlobalVars.POWERUP_SPRITES[whichPower];
	}

	#region POWERUP_USED
	//this is when the power-up actually gets activated
	public void OnMouseUp(){
		if(PlayerPrefs.GetInt(GlobalVars.GATHERING_TUTORIAL_WATCHED_POWER_UP) == 0 && this.gameObject.name == "PowerUpTap"){
			GlobalVars.POWER_UP_TUTORIAL_TAP = true;
		}if(PlayerPrefs.GetInt(GlobalVars.GATHERING_TUTORIAL_WATCHED_POWER_UP) == 0 && this.gameObject.name == "PowerUpSwipe"){
			if(this.gameObject.transform.position.x < -3.4f){
				return;
			}
		}
		if (!iHaveBeenUsed) {
			if(myPowerUp.name == "Invincible"){
				powerUpTextAnimator.SetTrigger("invincibleText");
			}
			else if(myPowerUp.name == "CollectAll"){
				collectionAnimator.SetTrigger("collectAll");
			}
			//turn on animator which had been turned off due to render order issues
			powerUpTappedAnim.enabled = true;
			// Play the animation
			backgroundFlash.SetTrigger("powerUpFlash");
			//this power-up is now considered used
			iHaveBeenUsed = true;
			//stop its flashing colors and change the background to its specific power-up color
			BroadcastMessage("stopColors");
			background.GetComponent<SpriteRenderer> ().color = powerColor[UnlockedPowerupIndex[whichPower]];
			//how much before the end of its duration should the visual effects begin to wear off
			float beginPowerTaperOff = 0.2f; 
			//conversion from nullable float
			if (myPowerUp.duration != null){
				Debug.Log("Duration: " + ((float)myPowerUp.duration - beginPowerTaperOff));
				background.BroadcastMessage("beginVisualCoroutine",(float)myPowerUp.duration-beginPowerTaperOff);
			}
			//destroys the collider so the object can no longer receieve clicks
			Destroy(transform.GetComponent<BoxCollider>());

			if(PlayerPrefs.GetInt(GlobalVars.GATHERING_TUTORIAL_WATCHED_POWER_UP) == 0 && this.gameObject.name == "PowerUpSwipe"){
				myPowerUp.usePowerUp(1);
				controller.GetComponent<PlayTutorial>().StartElementsMove();
				controller.GetComponent<PlayTutorial>().ResetRingAndArrow();
			}else{
				myPowerUp.usePowerUp (GlobalVars.GATHERING_CONTROLLER.whichLane(transform.position.x)); 
			}
			//scale adjustment for spritesheet
			this.transform.localScale = new Vector3(2,2,1);
			//begin its tapped animation
			powerUpTappedAnim.SetInteger ("whichPower", UnlockedPowerupIndex[whichPower]);
			if (OnPowerUpUsed != null) {
				OnPowerUpUsed(myPowerUp.name, myPowerUp.level);
			}
		}
	}

	#endregion

	//initializes colors for the powerups
	private void InitializeColors () {
		if (powerColor [0] != Color.gray) {
			//set power-up characteristic colors
			addTimeColor = new Color (0.0f / 255.0f, 255.0f / 255.0f, 149.0f / 255.0f, 255f);//new Color(1,1,1,1);
			invincibleColor = new Color (235.0f / 255.0f, 238.0f / 255.0f, 40.0f / 255.0f,255f);
			elementMultiplyColor = new Color (0.0f / 255.0f, 255.0f / 255.0f, 149.0f / 255.0f,255f);//new Color(1,1,1,1);
			slowFallColor = new Color (163.0f / 255.0f, 95.0f / 255.0f, 234.0f / 255.0f,255f);
			totalMultiplierColor = new Color (0.0f / 255.0f, 255.0f / 255.0f, 149.0f / 255.0f,255f);
			invincibleConversionColor = Color.gray;
			laneConversionColor = Color.gray;
			totalConversionColor = laneConversionColor;
			totalLaneConversionColor = laneConversionColor;
			collectAllColor = totalMultiplierColor;
		
			powerColor [0] = laneConversionColor;
			powerColor [1] = slowFallColor;
			powerColor [2] = addTimeColor;
			powerColor [3] = elementMultiplyColor;
			powerColor [4] = bucketShieldColor;
			powerColor [5] = invincibleConversionColor;
			powerColor [6] = laneConversionColor;
			powerColor [7] = totalConversionColor;
			powerColor [8] = totalLaneConversionColor;
			powerColor [9] = collectAllColor;
		}
	}
	//destroys the powerup after a set number of seconds
	IEnumerator destroyPowerUpOnTimer (float seconds) {
		yield return new WaitForSeconds(seconds);
		Destroy (this.gameObject);
	}

	//function to check whether there are any unlocked powerups
	public static bool PowerUpsUnlocked () {
		if (UnlockedPowerUps.Count > 0) {
			return true;
		} else {
			return false;
		}
	}

	//returns the number of powerups unlocked
	public static int PowerUpUnlockedCount () {
		return UnlockedPowerUps.Count;
	}

	// Generates references to each type of powerup object
	public static void GenerateAllPowerups () {
		if (PowerUps == null) {
			//initializes the array
			PowerUps = new PowerUp[GlobalVars.POWERUP_COUNT];
			
			//the array of all possible PowerUps the spawned PowerUp could be
			PowerUps[0] = new LaneConversion(durationLaneConversion);
			PowerUps[1] = new SlowFall(fallSpeedModifierSlowFall,timeBonusSlowFall,durationSlowFall);
			PowerUps[2] = new Fuel(timeBonusAddTime) ;
			PowerUps[3] = new Multiply(multiplierElementMultiply);
			PowerUps[4] = new BucketShield(bucketShieldHitPoints);
			PowerUps[5] = new TapToCollect(durationTapToCollect);
			PowerUps[6] = new Invincible(durationInvincible, spawnRateModifierInvincible);
			PowerUps[7] = new TotalConversion(durationTotalConversion);
			PowerUps[8] = new CollectAll();
		}
	}

	// Calculates which powerups are unlocked and adds their indexes to a dictionary to determine the index 
	public static void GenerateUnlockedPowerups () {
		Debug.Log("trying to unlock powerups");
		for (int i = 0; i < PowerUps.Length; i++) {
			if (PowerUp.PowerUpUnlocked(PowerUps[i]) && !UnlockedPowerUps.Contains(PowerUps[i])) {
				UnlockedPowerUps.Add(PowerUps[i]);
				UnlockedPowerupIndex.Add(UnlockedPowerUps.Count-1, i);
#if DEBUG
				Debug.Log(PowerUps[i].name + " is unlocked");
#endif
			}
		}
	}

	public static void UnlockAllPowerups () {
		UnlockedPowerupIndex.Clear ();
		GenerateAllPowerups ();
		for(int i = 0; i < PowerUps.Length; i++){

			UnlockedPowerupIndex.Add(i,i);
		}
		UnlockedPowerUps.Clear();
		UnlockedPowerUps.InsertRange(0, PowerUps);
	}

}
