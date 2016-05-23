#define DEBUG
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
//timer script for the collect screen
public class CollectionTimer : MonoBehaviour {
	//instace variable
	public static CollectionTimer instance;

	//event reference
	public delegate void StartGameAction();
	public delegate void EndGameAction();

	public static event StartGameAction OnStartGame;
	public static event EndGameAction OnEndGame;

	public Text timeTextmesh;
	private int timeRemaining;
	private int timerOffset = 1;
	private int timeAdded;
	private float timeElapsed = 0;
	private float baseCreationFrequency;
	GenerationScript generationScript;
	public GameObject timesUpBackground;
	// TUNING VARIABLES
	public float baseElementMovementSpeed;
	public float maxElementMovementSpeed = 2.5f;
	public float maxCreationFrequency = 600f;

	//speed change variables
	private float rateOfSpeedChange;
	private float rateOfFrequencyChange;

	// A bool to decide whether or not to countdown.
	private bool isCountingDown = true;
	private bool clockIsActive = true;
	private bool gamesHasBegun = false;

	private IEnumerator clockPausedCoroutine;
	void Awake () {
		instance = this;
		//global script reference
		GlobalVars.GATHERING_TIMER = this;
		GlobalVars.GATHERING_PLAYTIME = GlobalVars.COLLECT_TIME;
	}

	void Start () {
		//sets the starting time
		timeRemaining = GlobalVars.COLLECT_TIME+timerOffset;
		timeTextmesh.text  ="" + (timeRemaining - timerOffset);
		GlobalVars.PAUSED = true;
		//reference to game controller script
		generationScript = transform.GetComponent<GenerationScript>();

		
#if DEBUG
		//Debug.Log(generationScript.elementMovementSpeed);
		         #endif


		//sets the base speed and creation frequency
	//	baseElementMovementSpeed = generationScript.elementMovementSpeed;
	//	baseCreationFrequency = generationScript.creationFrequency;
	//	rateOfSpeedChange = (maxElementMovementSpeed - baseElementMovementSpeed)/ (float)GlobalVars.COLLECT_TIME;
	//	rateOfFrequencyChange = (maxCreationFrequency - baseCreationFrequency)/ (float)GlobalVars.COLLECT_TIME;
	}


	public void setBaseMovementSpeed (float baseElementMovementSpeed) {
		this.baseElementMovementSpeed = baseElementMovementSpeed;
		rateOfSpeedChange = (maxElementMovementSpeed - baseElementMovementSpeed)/ (float)GlobalVars.COLLECT_TIME;
	}

	public void setBaseCreationFrequency (float baseCreationFrequency) {
		this.baseCreationFrequency = baseCreationFrequency;
		rateOfFrequencyChange = (maxCreationFrequency - baseCreationFrequency)/ (float)GlobalVars.COLLECT_TIME;
	}
	void Update () {
		//counts the amount of time elapsed ---- isCountingDown boolean is a check for switchBucketScript, if buckets swapping it's set to false
		if (clockIsActive && isCountingDown && !GlobalVars.PAUSED && timeRemaining > 0) {
			if (!gamesHasBegun && OnStartGame != null) {
#if DEBUG
				Debug.Log("Game has started");
#endif
				OnStartGame();
				gamesHasBegun = true;
			}
			timeElapsed += Time.deltaTime;
		}
		//sets the timer each time the number of seconds remaining changes
		//---- isCountingDown boolean is a check for switchBucketScript, if buckets swapping it's set to false
		if (clockIsActive && isCountingDown && !GlobalVars.PAUSED && timeRemaining > GlobalVars.COLLECT_TIME+timerOffset - timeElapsed + timeAdded) {
			timeRemaining--;
			timeTextmesh.text = "" + timeRemaining;
			generationScript.increaseElementMovementSpeed(rateOfSpeedChange);
			generationScript.increaseCreationFrequency(rateOfFrequencyChange);
		}

		//stops the generation of new elements and destroys the onscreen ones
		if (timeRemaining == 0) {
			timeRemaining = -1;
			this.GetComponent<switchBucketScript>().enabled = false;
			GameObject finalElement = GenerationScript.findHighestSpawnedElement();

			//calls the end game event
			if (OnEndGame != null) {
				OnEndGame();
			}

			//ends the game immediately if it's a powerup
			if (finalElement == null) {
				endCollectMode();
			} else {
				GlobalVars.GATHERING_CONTROLLER.SetSpawning(false);
				//adds a game ending script to the final element
				finalElement.AddComponent<EndGatheringOnDestroy>().setTimerInstance(this);
			}

		}

		// Constantly increase the speed and frequency based on the amount of timeElapsed.
		generationScript.setElementMovementSpeed (baseElementMovementSpeed + (baseElementMovementSpeed * timeElapsed/(float)(GlobalVars.COLLECT_TIME+timeAdded)));
		generationScript.setCreationFrequency (baseCreationFrequency + (baseCreationFrequency * timeElapsed/(float)(GlobalVars.COLLECT_TIME+timeAdded)));
	}

	// Called when the time remaining runs out. 
	public void endCollectMode () {
		GlobalVars.PAUSED = true;
		timesUpBackground.SetActive (true);
		//transform.GetComponent<GenerationScript>().enabled = false;
		timesUpBackground.GetComponent<TimeUpScreen> ().TimesUp ();
	}
	// The game becomes paused and the score screen is shown.

	// A method that is given an int: adds time to the clock (used for powerups)
	public void addTime (int timeBonus) {
		//adds time to the counter
		timeRemaining += timeBonus;
		timeAdded += timeBonus;

		//upates the tracker of playtime to send to analytics
		GlobalVars.GATHERING_PLAYTIME += timeBonus;

		//updates the clock (depracated)
		timeTextmesh.text = "" + timeRemaining;
	}
	// The int given adds time to the amount remaining, one of the power ups - more play time.

	// A method to get the amount of time remaining.
	public int GetTimeRemaining(){
		return timeRemaining;
	}
	// Returns the timeRemaining variable, time remaining in a round of the gathering game.

	// A method given a boolean.
	public void SetIsCountingDown(bool answer){
		isCountingDown = answer;
	}
	// That sets whether or not the timer should countdown.

	public bool getIsCountingDown(){
		return isCountingDown;
	}

	//stops the clock (but keeps the game running for a set amount of seconds)
	public void pauseClock (float seconds) {
		clockIsActive = false;

		//stops the countdown to resume the clock if currently active
		if (clockPausedCoroutine != null) {
			StopCoroutine(clockPausedCoroutine);
		}

		//updates reference to clock paused, so that coroutine can be stopped if another method stops the clock while it's active
		clockPausedCoroutine = clockPaused(seconds);

		//starts the coroutine to resume the clock
		StartCoroutine(clockPausedCoroutine);
	}

	//resumes the clock after specified amount of time
	IEnumerator clockPaused (float seconds) {
		yield return new WaitForSeconds(seconds);
		clockIsActive = true;
	}
}

