/*
 * GenerationScript is the main controller of the gathering mode
 * It handles spawning the elements and launching the tutorials
 * This is where the tuning variables for speed and spawn rate are kept that effect difficulty
 * Also used to enact many of the powerups effects
 */

/// <summary>
/// The MAC_BOOK tag is used to make builds the proper resolution for a MacBookPro 2011 to display in portrait
/// (the model of the former dev)
/// </summary>
//#define MAC_BOOK_PRO_2011

/// <summary>
/// DEBUG is a preprocessor directive used in many scripts to print debugging statements and perform other debugging actions
/// Commenting it out will also comment out all the code wrapped in #if DEBUG statements
/// It can then be uncommented again when debugging is needed again
/// </summary>
//#define DEBUG

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenerationScript : MonoBehaviour {
	//events 
	public delegate void SpawnPowerUpAction();
	public static SpawnPowerUpAction OnSpawnPowerUp;

	private GameObject spawnedElement;
	//tuning variables
	public float creationFrequency {get; private set;}
	public float elementMovementSpeed {get; private set;}
	public float initialCreationFrequency {get; private set;}
	public float initialElementMovementSpeed {get; private set;}
	public bool spawning = true;

	//multipliers to modify the spawn rate and fall speed
	public float spawnRateModifier {get; private set;}
	public float [] fallSpeedModifiers {get; private set;}//public so that moveelementdown script can access it
	public float defaultSpawnModifier;
	public float defaultFallSpeedModifier;

	public int powerUpsPerRound; //the most powerUps a player can see in a round
	public int powerUpCounter;
	private bool spawnOneTypeOnly;
	private bool [] forceCorrectSpawnInLane = new bool[GlobalVars.NUMBER_OF_LANES]; 
	private int oneTypeSpawnIndex; 
	private int basePenality = GlobalVars.WRONG_ZONE_PENALTY;

	//for tap to collect mode
	public static bool TAP_TO_COLLECT = false;

	public GameObject element1;
	public GameObject element2;
	public GameObject element3;
	public GameObject element4;
	public GameObject powerUp;
	public GameObject collectionPot1, collectionPot2, collectionPot3, collectionPot4;
	public GameObject[] elements = new GameObject[GlobalVars.NUMBER_OF_LANES+1];
	// Has to change for randomized bucket positions
	public Vector3 lane1Spawn;
	public Vector3 lane2Spawn;
	public Vector3 lane3Spawn;
	public Vector3 lane4Spawn;
	public Vector3[] lanes = new Vector3[GlobalVars.NUMBER_OF_LANES];
	public GameObject elementText1;
	public GameObject elementText2;
	public GameObject elementText3;
	public GameObject elementText4;
	private GameObject[] scoreTexts = new GameObject[GlobalVars.NUMBER_OF_LANES];
	// The timer is set to start at 2 seconds so that when the countdown finishes it launches right away.
	// Other wise it would have to reach 100f/(creationFrequency * spawnRateModifier) which is 2 seconds of waiting.
	private float timer = 2;
	//stores the sprites of the current elements
	public Sprite [] elementSprites = new Sprite[GlobalVars.NUMBER_OF_LANES];
	
	private int randomSpawnedElement;
	private int randomSpawnedLane;
	private int duplicationRandomSpawnedElement;
	private int duplicationRandomSpawnedLane;
	private Queue<int> spawnedElements = new Queue<int>();
	private Queue<int> lanesUsed = new Queue<int> ();
	private int duplicateElementCounter;
	private int miniTutorialElementSpawnPosition = 3;

	//references to coroutines
	private IEnumerator deactivateIncreasedSpawnRateCoroutine;
	private IEnumerator deactivateModifiedFallRateCoroutine;
	private IEnumerator stopGeneratingOnlyOneTypeCoroutine;
	private IEnumerator deactivateTapToCollectCoroutine;
	private IEnumerator [] stopGeneratingInCorrectLaneCoroutine = new IEnumerator[GlobalVars.NUMBER_OF_LANES];
	// END TEMPORARY FIX
	private string[] arrayOfPlayerPrefs;
	// An animator array so we can play animations from generation script
	public Animator[] animators = new Animator[4];
	// A gameobject to hold the first element spawned in.
	private GameObject tutorialElement;
	// A bool to check if an element has been spawned or not -- for use with firstElement
	private bool hasTutorialElementSpawned;
	// A bool to check if the element has been given to the playTutorial script.
	private bool isElementGiven;
	// A bool to check if the second swipe tutorial element has been given to the playTutorial script.
	private bool isSecondElementGiven;
	// A bool to let us know if the first element has been spawned and that the next element should be a powerup.
	private bool timeToPowerUp;
	// Three arrays to be used with the power up tutorial.
	GameObject[] tutorialElements;
	Vector3[] powerUpTutorialPosition;
	int [] elementNumbers;

	void Awake () {
		#if UNITY_STANDALONE
			//sets the camera aspect for standalone builds
			Camera.main.aspect = 3.0f/4.0f;
			#if MAC_BOOK_PRO_2011
				Screen.SetResolution(675,900,false);
			#endif
		#endif

		//resets the score variables
		for (int i = 0; i < GlobalVars.SCORES.Length; i++){ 
			GlobalVars.SCORES[i] = 0;
		}
		GlobalVars.MISSED = 0;
		GlobalVars.POWERUP_USE_COUNT = 0;
		GlobalVars.POWERUP_SPAWN_COUNT = 0;
		GlobalVars.GATHERING_PLAY_COUNT++;
		ActivatePowerUp.OnPowerUpUsed += GlobalVars.INCREASE_POWER_UP_USE_COUNT;

		//sets the global script reference to this script
		GlobalVars.GATHERING_CONTROLLER = this;
		GlobalVars.InitializePowerUpSprites();


#if DEBUG
		Utility.SetPlayerPrefIntAsBool(GlobalVars.GATHERING_TUTORIAL_WATCHED_SWIPE, true);
		Utility.SetPlayerPrefIntAsBool(GlobalVars.GATHERING_TUTORIAL_WATCHED_POWER_UP, true);
		//ActivatePowerUp.UnlockAllPowerups ();

		TAP_TO_COLLECT = true;
#endif

	}
	
	// Use this for initialization
	void Start () {
		// ----- CHEATS ----- \\
		//ActivatePowerUp.UnlockAllPowerups ();
		//Cheats.UnlockAllElements();
		// ----- CHEATS ----- \\
		animators[0] = animators[0].GetComponent<Animator>();
		animators [1] = animators [1].GetComponent<Animator> ();
		animators [2] = animators [2].GetComponent<Animator> ();
		animators [3] = animators [3].GetComponent<Animator> ();

		animators [0].SetTrigger ("finishedLoading");

		arrayOfPlayerPrefs = new string[4];
		arrayOfPlayerPrefs [0] = PlayerPrefs.GetString ("ELEMENT1", "fire");
		arrayOfPlayerPrefs [1] = PlayerPrefs.GetString ("ELEMENT2", "water");
		arrayOfPlayerPrefs [3] = PlayerPrefs.GetString ("ELEMENT3", "earth");
		arrayOfPlayerPrefs [2] = PlayerPrefs.GetString ("ELEMENT4", "air");

		//sets player prefs if they have not been set
		PlayerPrefs.SetString("ELEMENT1", PlayerPrefs.GetString ("ELEMENT1", "fire"));
		PlayerPrefs.SetString("ELEMENT2", PlayerPrefs.GetString ("ELEMENT2", "water"));
		PlayerPrefs.SetString("ELEMENT3", PlayerPrefs.GetString ("ELEMENT3", "earth"));
		PlayerPrefs.SetString("ELEMENT4", PlayerPrefs.GetString ("ELEMENT4", "air"));

		// Setting the player given sprites
		element1.transform.GetChild (0).GetComponent<SpriteRenderer> ().sprite = Resources.Load<Sprite> (GlobalVars.FILE_PATH + arrayOfPlayerPrefs [0]);
		collectionPot1.transform.GetChild (1).GetChild (0).GetComponent<SpriteRenderer> ().sprite = (elementSprites[0] = Resources.Load<Sprite> (GlobalVars.FILE_PATH + arrayOfPlayerPrefs [0]));

		element2.transform.GetChild (0).GetComponent<SpriteRenderer> ().sprite = Resources.Load<Sprite> (GlobalVars.FILE_PATH + arrayOfPlayerPrefs [1]);
		collectionPot2.transform.GetChild (1).GetChild (0).GetComponent<SpriteRenderer> ().sprite = (elementSprites[1] = Resources.Load<Sprite> (GlobalVars.FILE_PATH + arrayOfPlayerPrefs [1]));
		
		element3.transform.GetChild (0).GetComponent<SpriteRenderer> ().sprite = Resources.Load<Sprite> (GlobalVars.FILE_PATH + arrayOfPlayerPrefs [3]);
		collectionPot3.transform.GetChild (1).GetChild (0).GetComponent<SpriteRenderer> ().sprite = (elementSprites[3] = Resources.Load<Sprite> (GlobalVars.FILE_PATH + arrayOfPlayerPrefs [2]));
		
		element4.transform.GetChild (0).GetComponent<SpriteRenderer> ().sprite = Resources.Load<Sprite> (GlobalVars.FILE_PATH + arrayOfPlayerPrefs [2]);
		collectionPot4.transform.GetChild (1).GetChild (0).GetComponent<SpriteRenderer> ().sprite = (elementSprites[2] = Resources.Load<Sprite> (GlobalVars.FILE_PATH + arrayOfPlayerPrefs [3]));
		// End of setting player given sprites
	
		elements[0] = element1;
		elements[1] = element2;
		elements[2] = element3;
		elements[3] = element4;
		if (elements.Length > 4) {
			elements [4] = powerUp;
		}
		// At the start assign the lanes but create a method to change the lanes positions
		lanes [0] = LanePositions.GetLanePositions(9f, -1)[0];
		lanes [1] = LanePositions.GetLanePositions(9f, -1)[1];
		lanes [2] = LanePositions.GetLanePositions(9f, -1)[2];
		lanes [3] = LanePositions.GetLanePositions(9f, -1)[3];
		
		scoreTexts[0] = elementText1;
		scoreTexts[1] = elementText2;
		scoreTexts[2] = elementText3;
		scoreTexts[3] = elementText4;

		//reset powerUpCounter (number of powerUps seen in a given round)
		powerUpCounter = 0;
		powerUpsPerRound = 3;

		//difficulty setting
		defaultSpawnModifier = defaultFallSpeedModifier = 1;//0.6f + (0.9f * GlobalVars.NUMBER_ELEMENTS_UNLOCKED / ((float)GlobalVars.ELEMENTS.Count));
		creationFrequency = 60f + (140f * GlobalVars.NUMBER_ELEMENTS_UNLOCKED / ((float)GlobalVars.ELEMENTS.Count));
		elementMovementSpeed = 2 + (0.3f * GlobalVars.NUMBER_ELEMENTS_UNLOCKED / ((float)GlobalVars.ELEMENTS.Count));// 0.6f + (0.9f * GlobalVars.NUMBER_ELEMENTS_UNLOCKED / ((float)GlobalVars.ELEMENTS.Count));

		CollectionTimer.instance.setBaseCreationFrequency (creationFrequency);
		CollectionTimer.instance.setBaseMovementSpeed (elementMovementSpeed); 

		fallSpeedModifiers = new float [GlobalVars.NUMBER_OF_LANES];

		//sets the fallSpeedModifiers to default
		for (int i = 0; i < fallSpeedModifiers.Length; i++) {
			fallSpeedModifiers[i] = defaultFallSpeedModifier;
		}

		//sets the spawnRateModifiers to default values
		spawnRateModifier = defaultSpawnModifier;


		//saves the starting values of fall speed and spawn rate
		initialCreationFrequency = creationFrequency;
		initialElementMovementSpeed = elementMovementSpeed;
		#if DEBUG
			powerUp1 = new BucketShield(2);
			PowerUp.ResetPowerUpLevel(powerUp1);
		#endif

		// Bools for tutorial checks
		isElementGiven = false;
		isSecondElementGiven = false;
		hasTutorialElementSpawned = false;
		timeToPowerUp = false;
		GlobalVars.SWIPE_TUTORIAL_FIRST_SPAWNED = false;

		//generates the powerups
		ActivatePowerUp.GenerateAllPowerups ();
		ActivatePowerUp.GenerateUnlockedPowerups ();

		// Instantiating the arrays for the power up tutorial
		tutorialElements = new GameObject[6];
		powerUpTutorialPosition = new Vector3[6];
		elementNumbers = new int[6];

		CheckForPowerUpTutorial ();
	}

	#if DEBUG
		private PowerUp powerUp1;
	#endif

	// Update is called once per frame
	void Update () {

		#if DEBUG
			if (Input.GetKeyDown(KeyCode.Space)) {
				powerUp1.usePowerUp(0);
				PowerUp.PromotePowerUp(powerUp1);
				//allOnScreenElementsToOneType(0);
			}
		#endif

		if (!GlobalVars.PAUSED){
			timer += Time.deltaTime;
		}
		if (!GlobalVars.PAUSED && spawning && timer > 100f/(creationFrequency * spawnRateModifier)) {
			createNewUnit();
			timer = 0;
		}

		// Check if the first element spawned (tutorial element) has fallen below the required height.
		if(tutorialElement != null && tutorialElement.transform.position.y <= 6.5f && !isElementGiven && PlayerPrefs.GetInt(GlobalVars.GATHERING_TUTORIAL_WATCHED_SWIPE) == 0){
			Debug.Log("Spawn in tutorial element");
			isElementGiven = true;
			tutorialElement.GetComponent<Collider>().enabled = true;
			this.GetComponent<PlayTutorial>().StartTutorial(tutorialElement);
		}
		if(tutorialElement != null && tutorialElement.transform.position.y <= 6.5f && GlobalVars.SWIPE_TUTORIAL_SECOND_SPAWNED && !isSecondElementGiven && PlayerPrefs.GetInt(GlobalVars.GATHERING_TUTORIAL_WATCHED_SWIPE) == 0){
			isSecondElementGiven = true;
			tutorialElement.GetComponent<Collider>().enabled = true;
			this.GetComponent<PlayTutorial>().SpawnSecondElementSwipeTutorial(tutorialElement);
		}
	}
	// Generate 2 random numbers which relate to the lane and element both ranging from 1 - 4 -- doesn't allow generation of an element into its respective lane -- tries to limit the same element spawning repeatitly
	private void generateLaneAndElem(){
		randomSpawnedLane = Random.Range(0,4);
		if (spawnOneTypeOnly) { //disables random generation if the generation is set to one element only (from powerup)
			if (Random.Range(0, GlobalVars.NUMBER_OF_LANES+1) == GlobalVars.NUMBER_OF_LANES) { //retains the one in five chance that the element is a powerup
				randomSpawnedElement = GlobalVars.NUMBER_OF_LANES;

			} else { //otherwise, that element spawns as the set element
				randomSpawnedElement = oneTypeSpawnIndex;
			} 
			return;
		} else { //continues through the script to generate elements pseudorandomely otherwise

			//sets whether or not you can spawn powerups, based on whether you have any unlocked and whether the tutorials have ended
			int spawnMaxIndex = (hasTutorialElementSpawned || 
			                     Utility.PlayerPrefIntToBool(GlobalVars.GATHERING_TUTORIAL_WATCHED_SWIPE) ||
			                     Utility.PlayerPrefIntToBool(GlobalVars.GATHERING_TUTORIAL_WATCHED_POWER_UP)) &&
				ActivatePowerUp.PowerUpsUnlocked() ? GlobalVars.NUMBER_OF_LANES+1 : GlobalVars.NUMBER_OF_LANES;

			//determines the randomely spawned element
			randomSpawnedElement = Random.Range(0,spawnMaxIndex);
		}

		if(spawnedElements != null && spawnedElements.Count >= 5){
			spawnedElements.Dequeue();
		}
		if(lanesUsed != null && lanesUsed.Count >= 5){
			lanesUsed.Dequeue();
		}
		if(spawnedElements != null && spawnedElements.Contains(randomSpawnedElement)){
			foreach(int i in spawnedElements){
				if(i == randomSpawnedElement){
					duplicateElementCounter++;
					if(duplicateElementCounter >= 2){
						duplicationRandomSpawnedElement = randomSpawnedElement;
						duplicateElementCounter = 0;
						break;
					}
				}
			}
			if(randomSpawnedElement == duplicationRandomSpawnedElement){
				while(randomSpawnedElement == duplicationRandomSpawnedElement){
					randomSpawnedElement = Random.Range(0,GlobalVars.NUMBER_OF_LANES);				
				}
			}
			//spawnedElements.Clear();
		}
		if(lanesUsed != null && lanesUsed.Contains(randomSpawnedLane)){
			foreach(int i in lanesUsed){
				if(i == randomSpawnedLane){
					duplicateElementCounter++;
					if(duplicateElementCounter >= 2){
						duplicationRandomSpawnedLane = randomSpawnedLane;
						duplicateElementCounter = 0;
						break;
					}
				}
			}
			if(randomSpawnedLane == duplicationRandomSpawnedLane){
				while(randomSpawnedLane == duplicationRandomSpawnedLane){
					randomSpawnedLane = Random.Range(0,GlobalVars.NUMBER_OF_LANES);
				}
			}
			//lanesUsed.Clear();
		}
		if(randomSpawnedElement == randomSpawnedLane){
			while(randomSpawnedElement == randomSpawnedLane){
				randomSpawnedLane = Random.Range(0,GlobalVars.NUMBER_OF_LANES);
			}
		}
		spawnedElements.Enqueue (randomSpawnedElement);
		lanesUsed.Enqueue (randomSpawnedLane);

		//if a powerUp was spawned, increment the counter
		if (randomSpawnedElement == GlobalVars.NUMBER_OF_LANES) {
			if (powerUpCounter < powerUpsPerRound) {
				powerUpCounter++;
			} else {//spawn something that isn't a powerUp
				randomSpawnedElement = Random.Range (0, GlobalVars.NUMBER_OF_LANES);
			}
		}

	}
	// 2 random numbers have been generated to decide which element spawns and what lane it is spawned into.
	
	// Given the random element and location this method will Instantiate a prefab or that element into that lane.
	private void createNewUnit () {
		generateLaneAndElem ();
		animators [randomSpawnedLane].SetTrigger ("spawning");
		//checks whether its going to be another element and not a powerup
		if (forceCorrectSpawnInLane[randomSpawnedLane] && randomSpawnedElement != GlobalVars.NUMBER_OF_LANES) {
			//sets the element to be of the correct type (if it's not a powerup)
			randomSpawnedElement = randomSpawnedLane;
		}
		//if we're spawning a powerup
		if (randomSpawnedElement == GlobalVars.NUMBER_OF_LANES) {
			//calls the event
			if (OnSpawnPowerUp != null) {
				OnSpawnPowerUp();
			}
			//increases the count
			GlobalVars.POWERUP_SPAWN_COUNT++;
		}
		// INSTANTIATE
		if (PlayerPrefs.GetInt (GlobalVars.GATHERING_TUTORIAL_WATCHED_SWIPE) == 0 && !GlobalVars.SWIPE_TUTORIAL_FIRST_SPAWNED) {
			spawnedElement = (GameObject)Instantiate (elements [1], lanes [0], Quaternion.identity);
			spawnedElement.name = GlobalVars.SWIPE_TUTORIAL_ELEMENT_NAME;
			GlobalVars.SWIPE_TUTORIAL_FIRST_SPAWNED = true;
		} else {
#if DEBUG
//		randomSpawnedElement = GlobalVars.NUMBER_OF_LANES;
//		Debug.Log("setting the game to only spawn powerups");
#endif
			spawnedElement = (GameObject)Instantiate (elements [randomSpawnedElement], lanes [randomSpawnedLane], Quaternion.identity);
		}
		// Catching the first element to spawn and stops spawning so we can show a tutorial.
		if(!hasTutorialElementSpawned && PlayerPrefs.GetInt(GlobalVars.GATHERING_TUTORIAL_WATCHED_SWIPE) == 0){
			spawning = false;
			tutorialElement = spawnedElement;
			tutorialElement.GetComponent<Collider>().enabled = false;
			hasTutorialElementSpawned = true;
			timeToPowerUp = true;
		}
		if(!isSecondElementGiven && PlayerPrefs.GetInt(GlobalVars.GATHERING_TUTORIAL_WATCHED_SWIPE) == 0){
			spawning = false;
			tutorialElement = spawnedElement;
		}
		if(randomSpawnedElement == GlobalVars.NUMBER_OF_LANES){
			spawnedElement.SetActive(true);
		}
		spawnedElement.AddComponent<MoveElementDown>();
		spawnedElement.AddComponent<MoveElementUp> (); 
		spawnedElement.GetComponent<MoveElementUp> ().enabled = false;
		ZoneCollisionDetection.addToOnScreenElements (spawnedElement);
	}
	// The new element is spawned and a movement script attached, as well as giving the gameObject to the zoneCollisionDetection script.

	void SecondElementHasBeenSpawned(){
		spawning = false;
		tutorialElement = spawnedElement;
		tutorialElement.GetComponent<Collider> ().enabled = false;
	}

	// Is given the elements lane and the amount you want to increase the score.
	public void updateScore (int elementToIncrease, int increaseAmount) {
		
		//updates the text on the zone
		int score = int.Parse (scoreTexts [elementToIncrease].GetComponent<TextMesh> ().text);
		score += increaseAmount;
		if(score < 10){
			scoreTexts [elementToIncrease].GetComponent<TextMesh> ().text = "0" + score.ToString ();
		}else{
			scoreTexts [elementToIncrease].GetComponent<TextMesh> ().text = score.ToString ();
		}
		//increases the score of elements collected
		GlobalVars.SCORES[elementToIncrease] += increaseAmount;
	
	}
	// Updates the amount of elements you have caught in your bucket.
	
	// Called when an element enters a bucket it doesn't belong to.
	public void wipeScore(int elemObject){
		//updates the score text on the zone
		int score = int.Parse(scoreTexts[elemObject].GetComponent<TextMesh>().text);
		score = Mathf.Clamp(score -= GlobalVars.WRONG_ZONE_PENALTY, 0, int.MaxValue);
		if (score < 10) {
			scoreTexts [elemObject].GetComponent<TextMesh> ().text = "0" + score.ToString ();
		} else {
			scoreTexts [elemObject].GetComponent<TextMesh> ().text = score.ToString ();
		}
		//updates the player's score
		GlobalVars.SCORES[elemObject] = Mathf.Clamp(GlobalVars.SCORES[elemObject] - GlobalVars.WRONG_ZONE_PENALTY, 0, int.MaxValue);
	}
	// When the wrong element enters a bucket clear the amount in that bucket.

	//multiplies the current score 
	public void multiplyScore (int elementToIncrease, int multiplyAmount) {

		//updates the score text in the zone
		int score = int.Parse (scoreTexts [elementToIncrease].GetComponent<TextMesh> ().text);
		score *= multiplyAmount;
		if(score < 10){
			scoreTexts [elementToIncrease].GetComponent<TextMesh> ().text = "0" + score.ToString ();
		}else{
			scoreTexts [elementToIncrease].GetComponent<TextMesh> ().text = score.ToString ();
		}

		//updates the score itself
		GlobalVars.SCORES [elementToIncrease] *= multiplyAmount;
	}

	//changes the spawn rate for a set amount of time
	public void increaseSpawnRate (float duration, float spawnRateModifier) {
		//stops the currently running instance (if any) of the coroutine
		if (deactivateIncreasedSpawnRateCoroutine != null) {
			StopCoroutine(deactivateIncreasedSpawnRateCoroutine);
		}
		this.spawnRateModifier *= spawnRateModifier;
		deactivateIncreasedSpawnRateCoroutine = deactivateIncreasedSpawnRate(duration);
		StartCoroutine(deactivateIncreasedSpawnRateCoroutine);
	}

	//deactivates the increased spawn rate after a set amount of time
	IEnumerator deactivateIncreasedSpawnRate (float duration) {
		//resets spawn rate modifier
		yield return new WaitForSeconds(duration);
		spawnRateModifier = defaultSpawnModifier;
	}

	//changes the rate at which all elements fall for a certain amount of time
	public void modifyFallRate (float fallSpeedModifier, float duration) {
		//stops the currently running instance (if any) of the coroutine
		if (deactivateModifiedFallRateCoroutine != null) {
			StopCoroutine(deactivateModifiedFallRateCoroutine);
		}
		for (int i = 0; i < fallSpeedModifiers.Length; i++) {
			fallSpeedModifiers[i] *= fallSpeedModifier;
		}
		deactivateModifiedFallRateCoroutine = deactivateModifiedFallRate(duration);
		StartCoroutine(deactivateModifiedFallRateCoroutine);
	}

	//changes the rate at which all elements in lane fall
	public void modifyFallRate (float fallSpeedModifier, float duration, int targetLane) {
		//stops the currently running instance (if any) of the coroutine
		if (deactivateModifiedFallRateCoroutine != null) {
			StopCoroutine(deactivateModifiedFallRateCoroutine);
		}

		//changes fall speed modifier of set lane
		fallSpeedModifiers[targetLane] *= fallSpeedModifier;

		//starts the couroutine
		deactivateModifiedFallRateCoroutine = deactivateModifiedFallRate(duration, targetLane);
		StartCoroutine(deactivateModifiedFallRateCoroutine);
	}

	//deactivates the modified fall rate
	IEnumerator deactivateModifiedFallRate (float duration) {
		yield return new WaitForSeconds(duration);
		for (int i = 0; i < fallSpeedModifiers.Length; i++) {
			fallSpeedModifiers[i] = defaultFallSpeedModifier;
		}
	}
	
	//deactivates the modified fall rate for certain lanes
	IEnumerator deactivateModifiedFallRate (float duration, int targetLane) {
		yield return new WaitForSeconds(duration);
		fallSpeedModifiers[targetLane] = defaultFallSpeedModifier;
	}

	// Changes all of the onScreenElements into one type.
	public void allOnScreenElementsToOneType (int index) {
		foreach (GameObject element in ZoneCollisionDetection.ON_SCREEN_ELEMENTS) {
			if (element != null) {
				//skips over powerup elmeents
				if (element.tag == "PowerUp") {
					continue;
				}
				element.transform.GetChild (0).GetComponent<SpriteRenderer> ().sprite = elementSprites[index];
			//	print (element.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite.name);
				element.tag = elements[index].tag;
			//	print(elementSprites[index].name + " " + elements[index].tag);
			}
		}

		//overrides lane spawning if it was currently active
		disableCorrectLaneSpawning();
	}


	//converts all the elements in a select lane to the correct type
	public void allElementsInLaneToOneType (int elementIndex, int laneIndex) {
		foreach (GameObject element in ZoneCollisionDetection.ON_SCREEN_ELEMENTS) {
			if (whichLane(element) == laneIndex) {
				//skips over powerup elmeents
				if (element.tag == "PowerUp") {
					continue;
				}
				element.transform.GetChild (0).GetComponent<SpriteRenderer> ().sprite = elementSprites[elementIndex];
				element.tag = elements[elementIndex].tag;
			}
		}
	}


	//converts all the elements in all lanes to the correct type
	public void allElementsToLaneType () {
		foreach (GameObject element in ZoneCollisionDetection.ON_SCREEN_ELEMENTS) {
			int laneIndex;
			if ((laneIndex = whichLane(element)) != -1) {
				//skips over powerup elmeents
				if (element.tag == "PowerUp") {
					continue;
				}
				element.transform.GetChild (0).GetComponent<SpriteRenderer> ().sprite = elementSprites[laneIndex];
				element.tag = elements[laneIndex].tag;
			}
		}

		//disables one type spawning if currently active
		spawnOneTypeOnly = false;
	}
	// Only one type of element can be generated.
	public void generateOnlyOneType (int index, float duration) {
		//stops the currently running instance (if any) of the coroutine
		if (stopGeneratingOnlyOneTypeCoroutine != null) {
			StopCoroutine(stopGeneratingOnlyOneTypeCoroutine);
		}
		spawnOneTypeOnly = true;
		oneTypeSpawnIndex = index;
		stopGeneratingOnlyOneTypeCoroutine = stopGeneratingOnlyOneType(duration);
		StartCoroutine(stopGeneratingOnlyOneTypeCoroutine);
	}
	// Generate only one type of element for a set amount of time.
	IEnumerator stopGeneratingOnlyOneType (float duration) {
		yield return new WaitForSeconds(duration);
		spawnOneTypeOnly = false;
	}	

	//generates the correct type in the lane for a specified amount of time
	public void generateInCorrectLane (int laneIndex, float duration) {
		//stops the currently running instance (if any) of the coroutine
		if (stopGeneratingInCorrectLaneCoroutine[laneIndex] != null) {
			StopCoroutine(stopGeneratingInCorrectLaneCoroutine[laneIndex]);
		}
		stopGeneratingInCorrectLaneCoroutine[laneIndex] = stopGeneratingInCorrectLane(laneIndex, duration);
		forceCorrectSpawnInLane[laneIndex] = true;
		StartCoroutine(stopGeneratingInCorrectLaneCoroutine[laneIndex]);
	}

	//halts the generation of the correct element in the lane
	IEnumerator stopGeneratingInCorrectLane (int laneIndex, float duration) {
		yield return new WaitForSeconds(duration);
		forceCorrectSpawnInLane[laneIndex] = false;
	}

	//destroys all the onscreen elements and updates the score
	public void collectAllOnscreenElements () {
		int [] scoreIncreases = new int[GlobalVars.NUMBER_OF_LANES];
		List<GameObject> powerups = new List<GameObject>();
		//destroys all the gameobjects and counts how many of each there are
		foreach (GameObject element in ZoneCollisionDetection.ON_SCREEN_ELEMENTS) { 
			if (element != null && ZoneCollisionDetection.COLLECTION_TAGS.IndexOf(element.tag) != -1) {
				scoreIncreases[ZoneCollisionDetection.COLLECTION_TAGS.IndexOf(element.tag)]++;
			}
		}
		foreach (GameObject element in ZoneCollisionDetection.ON_SCREEN_ELEMENTS) { 
			//doesn't destroy powerups
			if (element != null && element.tag == GlobalVars.POWER_UP_TAG) {
				powerups.Add (element);
				continue;
			}
			Destroy(element);
		}

		//updates the scores
		for (int i = 0; i < GlobalVars.SCORES.Length; i++) {
			updateScore(i, scoreIncreases[i]*GlobalVars.GATHERING_ZONES[i].getCurrentPointValue());
		}

		ZoneCollisionDetection.ON_SCREEN_ELEMENTS = powerups;

	}

	//destroys all the on screen elements and updates the score within specific lanes
	public void collectAllElementsInLane (int [] lanes) {
		//scores to increase
		int [] scoreIncreases = new int[GlobalVars.NUMBER_OF_LANES];

		//keeps track of the elements collected
		List<GameObject> destroyedObjects = new List<GameObject>();

		//destroys all the gameobjects and counts how many of each there are
		foreach (GameObject element in ZoneCollisionDetection.ON_SCREEN_ELEMENTS) { 
			if (element != null && inLane(lanes, element) && ZoneCollisionDetection.COLLECTION_TAGS.IndexOf(element.tag) != -1) {
				scoreIncreases[ZoneCollisionDetection.COLLECTION_TAGS.IndexOf(element.tag)]++;
				destroyedObjects.Add(element);
			}
		}

		//destroys collected gameobjects
		foreach (GameObject element in destroyedObjects) { 
			//removes from list of elements on screen
			ZoneCollisionDetection.ON_SCREEN_ELEMENTS.Remove(element);

			//destroys gameobject
			Destroy(element);
		}

		//updates the scores
		for (int i = 0; i < GlobalVars.SCORES.Length; i++) {
			updateScore(i, scoreIncreases[i]*GlobalVars.GATHERING_ZONES[i].getCurrentPointValue());
		}
	}

	//sets whether or not new elements are spawning
	public void SetSpawning(bool answer){
		spawning = answer;
	}

	//returns the highest elemnt on screen
	public static GameObject findHighestSpawnedElement () {
		GameObject highestElement = null;
		float highestPos = float.MinValue;
		foreach (GameObject element in ZoneCollisionDetection.ON_SCREEN_ELEMENTS) {
			if (element != null) { //checks whether element in the list is null
				//disregards the element if it's a powerup
				if (element.tag == GlobalVars.POWER_UP_TAG) {
					continue;
				}
				//checks whether the element is higher than the current highest
				if (element.transform.position.y > highestPos) {
					highestElement = element;
				}
			}
		}
		//returns the highest element, or null if there are no elments onscreen
		return highestElement;
	}

	public bool inLane (int [] lanes, GameObject element) {
		foreach (int lane in lanes) {
			if (whichLane(element) == lane) {
				return true;
			}
		}

		return false;
	}

	//returns the lane number or -1 if the lane is not found
	public int whichLane (float xPos) {
		float tolerance = LanePositions.GetLaneTolerance (0);//0.25f;
		for (int i = 0; i < lanes.Length; i++) {
			if (tolerance >= Mathf.Abs(xPos - lanes[i].x)) {
				return i;
			}
		}
		return -1;
	}

	//overloaded method that takes a gameobject returns the lane number or -1 if not found/the gameobject is null
	public int whichLane (GameObject element) {
		float tolerance = 0.25f;
		if (element == null) {
			return -1;
		}
		for (int i =0; i < lanes.Length; i++) {
			if (tolerance >= Mathf.Abs(element.transform.position.x - lanes[i].x)) {
				return i; 
			}
		}
		return -1;
	}

	
	//activates the powerup for specified seconds
	public void activateTapToCollect (float seconds) {
		if (deactivateTapToCollectCoroutine != null) {
			StopCoroutine(deactivateTapToCollectCoroutine);
		}
		//sets tap to collect on 
		TAP_TO_COLLECT = true;

		//starts coroutine to deactivate 
		deactivateTapToCollectCoroutine = activateTapToCollectDuration(seconds);
		StartCoroutine(deactivateTapToCollectCoroutine);
	}

	//deactivates tap to collect
	IEnumerator activateTapToCollectDuration (float seconds) {
		yield return new WaitForSeconds(seconds);
		TAP_TO_COLLECT = false;
	}

	//disables all correct lane spawning 
	private void disableCorrectLaneSpawning () {
		for (int i = 0; i < GlobalVars.NUMBER_OF_LANES; i++) {
			forceCorrectSpawnInLane[i] = false;
		}
	}
	// A function to create the power up tutorial.
	public void CreatePowerUpTutorial(){
		// Spawning in the tutorial elements in the correct positions.
		powerUpTutorialPosition [0] = new Vector3 (LanePositions.GetLanePositions(0)[1].x, 10.92f, 0);
		powerUpTutorialPosition [1] = new Vector3 (LanePositions.GetLanePositions(0)[1].x, 13.52f, 0);
		powerUpTutorialPosition [2] = new Vector3 (LanePositions.GetLanePositions(0)[0].x, 16.06f, 0);
		powerUpTutorialPosition [3] = new Vector3 (LanePositions.GetLanePositions(0)[2].x, 11.89f, 0);
		powerUpTutorialPosition [4] = new Vector3 (LanePositions.GetLanePositions(0)[2].x, 14.43f, 0);
		powerUpTutorialPosition [5] = new Vector3 (LanePositions.GetLanePositions(0)[2].x, 16.73f, 0);
		// Assigning the tutorialElements their type. (fire, water, powerup etc...)
		elementNumbers [0] = 0; elementNumbers [1] = 1; elementNumbers [2] = 4; elementNumbers [3] = 4; elementNumbers [4] = 3; elementNumbers [5] = 0;
		// Loop through the powerUpTutorialPosition and spawn in the element, add MoveElementDown script and disable moving and colliders, set the powerup type.
		for(int i = 0; i < powerUpTutorialPosition.Length; i++){
			tutorialElements[i] = (GameObject) Instantiate(elements[elementNumbers[i]], powerUpTutorialPosition[i], Quaternion.identity);
			tutorialElements[i].AddComponent<MoveElementDown>();
			tutorialElements[i].GetComponent<MoveElementDown>().isAllowedToMove = false;
			tutorialElements[i].GetComponent<Collider>().enabled = false;
			if(tutorialElements[i].tag == GlobalVars.POWER_UP_TAG){
				tutorialElements[i].GetComponent<ActivatePowerUp>().SetPowerUp(0);
			}
			// Adding the elements to ON_SCREEN_ELEMENTS so that we can register powerups and collection.
			ZoneCollisionDetection.ON_SCREEN_ELEMENTS.Add(tutorialElements[i]);
		}
		tutorialElements [0].name = "TutorialElement1";
		tutorialElements [1].name = "TutorialElement2";
		tutorialElements [4].name = "TutorialElement4";
		tutorialElements [5].name = "TutorialElement5";
		for(int i = 0; i < 3; i++){
			tutorialElements[i].GetComponent<MoveElementDown>().isAllowedToMove = true;
		}
		tutorialElements [2].SetActive (true);
		tutorialElements [3].SetActive (true);

		this.GetComponent<PlayTutorial> ().SetTutorialElements ();
	}
	// Spawns certain elements in a certain position with certain conditions.

	// A getter method to return the tutorialElements array;
	public GameObject[] GetTutorialElements(){
		return tutorialElements;
	}

	//checks if the user has gone through the tutorial for how to use powerUps
	public void CheckForPowerUpTutorial(){
		if (ActivatePowerUp.PowerUpsUnlocked() && PlayerPrefs.GetInt(GlobalVars.GATHERING_TUTORIAL_WATCHED_POWER_UP) == 0 && PlayerPrefs.GetInt(GlobalVars.GATHERING_TUTORIAL_WATCHED_SWIPE) == 1 && !GlobalVars.PAUSED) {
			// Stops the game from starting normally.
			spawning = false;
			this.GetComponent<CollectionTimer>().SetIsCountingDown(false);
			CreatePowerUpTutorial();
		}
	}

	// A function to spawn in the second tutorial element.
	public void SpawnInSecondSwipeTutorialElement(){
		spawnedElement = (GameObject)Instantiate (elements [3], lanes [0], Quaternion.identity);
		GlobalVars.SWIPE_TUTORIAL_SECOND_SPAWNED = true;
		spawning = false;
		tutorialElement = spawnedElement;
		tutorialElement.GetComponent<Collider>().enabled = false;
		tutorialElement.name = GlobalVars.SWIPE_TUTORIAL_SECOND_ELEMENT_NAME;
		spawnedElement.AddComponent<MoveElementDown>();
		spawnedElement.AddComponent<MoveElementUp> (); 
		spawnedElement.GetComponent<MoveElementUp> ().enabled = false;
		ZoneCollisionDetection.addToOnScreenElements (spawnedElement);
		SecondElementHasBeenSpawned();
	}

	// Set the elementMovementSpeed to a new value.
	public void setElementMovementSpeed(float newSpeed){
		elementMovementSpeed = newSpeed;
	}
	// Increase the elementMovementSpeed by the entered amount.
	public void increaseElementMovementSpeed(float newSpeed){
		elementMovementSpeed += newSpeed;
	}
	// Set the creationFrequency to a new value.
	public void setCreationFrequency(float newFrequency){
		creationFrequency = newFrequency;
	}
	// Increase the creationFrequency by the entered amount.
	public void increaseCreationFrequency(float newFrequency){
		creationFrequency += newFrequency;
	}
}
