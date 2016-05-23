/*
 * A class that plays the tutorials for the gathering mode
 * Responds the falling elements and plays the correct tutorial at the proper moment
 * Only plays one tutorial max at the start of a gathering session
 * Times the tutorial completion and sends an event to MixPanel analytics
 */

/// <summary>
/// DEBUG is a preprocessor directive used in many scripts to print debugging statements and perform other debugging actions
/// Commenting it out will also comment out all the code wrapped in #if DEBUG statements
/// It can then be uncommented again when debugging is needed again
/// </summary>
//#define DEBUG

/// <summary>
/// The tutorials in this script are linked with other scripts in the gathering scene.
/// Generation Script - to catch the first and second spawned elements and make sure the power up tutorial spawns the correct elements.
/// The generation script also detects when the element has fallen the correct distance before starting the tutorials.
/// SwipingMovement script - to catch when the player is dragging the element and to tell us if they have traveled far enough/too far.
/// ZoneCollisionDetection script - to catch when a tutorial element has been collected and thus allowing the tutorial to move on.
/// </summary>
using UnityEngine;
using System.Collections;

public class PlayTutorial : MonoBehaviour {
	public delegate void SwipeTutorialComplete (float completionTime);
	public static event SwipeTutorialComplete onSwipeTutorialComplete;
	public delegate void PowerUpTutorialComplete (float completionTime);
	public static event PowerUpTutorialComplete onPowerUpTutorialComplete;

	// Used to instruct other scripts that the powerup is comlpete
	public delegate void TutorialCompleteAction ();
	public static event TutorialCompleteAction OnTutorialComplete;

	// Canvas element that gets turned on or off for the tutorials
	public GameObject tutorialPanel;
	// Canvas elements that get turned on or off for the tutorials.
	public GameObject timeTutorial, swappingTutorial, powerUpTutorial;
	// A gameobject to hold the tutorialElement i.e. the first element spawned in for swipe tutorial.
	private GameObject tutorialElement;
	// A vector3 to record the starting position of the elements.
	private Vector3 tutorialElementStartPosition;
	// The game objects for the ring and arrow for both tutorials.
	public GameObject ringAndArrow, ringAndArrow2, ringAndArrowForSwipeTutorial;
	// A game object to hold the resting position of the ringAndArrow
	public GameObject ringAndArrowRestingPosition;
	// A vector3 for setting where the rings and arrows game object sits on tutorial power up.
	private Vector3 correctElementPosition = new Vector3(0,0,0);
	// An array for the elements spawned in during a tutorial.
	private GameObject[] tutorialElements = new GameObject[6];
	// A boolean to tell us that the poweruptutorial has started and doesn't need called again.
	private bool hasPowerUpTutorialStarted, hasPowerUpTutorialTapStarted;

	//bool to tell the timer when to stop running
	private bool tutorialTimerActive;
	private float timeInTutorial;

	//whether a tutorial is running during the gathering mode
	public static bool TutorialActive;

	void Awake () {

	}
	// Use this for initialization
	void Start () {
		hasPowerUpTutorialStarted = false;
		hasPowerUpTutorialTapStarted = false;
		// Allows us to test the tutorial as if we were new players.
		//PlayerPrefs.SetInt (GlobalVars.GATHERING_TUTORIAL_WATCHED_SWIPE, 0);

#if DEBUG
		PlayerPrefs.SetInt (GlobalVars.GATHERING_TUTORIAL_WATCHED_POWER_UP, 0);
#endif

		//subscribes to the tutorialelementevent
		SwipingMovement.OnTutorialElementMouseAction += TutorialElementEventHandler;
	}

	void OnDestroy () {
		// Unsubscribes from the tutorial element event
		SwipingMovement.OnTutorialElementMouseAction -= TutorialElementEventHandler;
	}
	void Update(){
		// Check if the powerup tutorial elements have fallen enough to begin the poweruptutorial.
		if (tutorialElements [0] != null) {
			if (tutorialElements [0].transform.position.y <= 0.92f && PlayerPrefs.GetInt (GlobalVars.GATHERING_TUTORIAL_WATCHED_POWER_UP) == 0 && !hasPowerUpTutorialStarted) {
				// If the first element in the array is not element and falls to a position of 0.92f on the screen then the tutorial has started and the game is paused.
				hasPowerUpTutorialStarted = true;
				// Setting the first 3 power up tutorial elements to stop moving after they fall enough.
				for (int i = 0; i < 3; i++) {
					tutorialElements [i].GetComponent<MoveElementDown> ().isAllowedToMove = false;
				}
				// Making the player able to use the powerup.
				tutorialElements [2].GetComponent<Collider> ().enabled = true;
				StartPowerUpTutorial();
			}
		}
		// Check if the second set of elements for the power up tutorial have fallen enough to begin poweruptutorialtap.
		if(tutorialElements[3] != null){
			if(tutorialElements[3].transform.position.y <= 1.89f && PlayerPrefs.GetInt(GlobalVars.GATHERING_TUTORIAL_WATCHED_POWER_UP) == 0 && !hasPowerUpTutorialTapStarted){
				hasPowerUpTutorialTapStarted = true;
				// Setting the last 3 power up tutorial elements to stop moving after they fall enough.
				for (int i = 3; i < 6; i++) {
					tutorialElements [i].GetComponent<MoveElementDown> ().isAllowedToMove = false;
				}
				// Making the player be able to use the powerup.
				tutorialElements[3].GetComponent<Collider>().enabled = true;
				StartPowerUpTutorialTap();
			}
		}
		// Check if both swipe and tap are finished.
		if(GlobalVars.POWER_UP_TUTORIAL_TAP && GlobalVars.POWER_UP_TUTORIAL_SWIPE && PlayerPrefs.GetInt(GlobalVars.GATHERING_TUTORIAL_WATCHED_POWER_UP) == 0 && PlayerPrefs.GetInt(GlobalVars.GATHERING_TUTORIAL_WATCHED_SWIPE, 0) == 1){
			EndTimer();
			// This lets us know that the player has played the tutorial.
			PlayerPrefs.SetInt(GlobalVars.GATHERING_TUTORIAL_WATCHED_POWER_UP, 1);
			// Call event here
			if(onPowerUpTutorialComplete != null){
				onPowerUpTutorialComplete(timeInTutorial);
			}
			// Cycle through each on screen element and make it so they can move again.
			for(int i = 0; i < tutorialElements.Length; i++){
				//QWOP RENDERING
				if(tutorialElements[i] != null){
					this.tutorialElements[i].GetComponent<Collider>().enabled = true;
					this.tutorialElements[i].GetComponent<MoveElementDown>().enabled = true;
					this.tutorialElements[i].GetComponent<MoveElementDown>().isAllowedToMove = true;
				}
				// If a element in the array has been removed then check for this and allow the loop to contine.
				if(tutorialElements[i] == null){
					continue;
				}
			}
			// Turn off the tutorial canvas pieces
			tutorialPanel.SetActive(false);
			ringAndArrow.SetActive(false);
			// Essentially start the game, the tutorial is over and random spawning and the collection timer is turned on.
			this.GetComponent<GenerationScript> ().spawning = true;
			this.GetComponent<CollectionTimer> ().SetIsCountingDown(true);
		}
		// If the first powerup has moved into the correct lane.
		if (tutorialElements [2] != null && tutorialElements [1] != null && !GlobalVars.POWER_UP_TUTORIAL_SWIPE) {
			// Checking if the swipable powerup has been moved into the correct lane.
			if (tutorialElements [2].transform.position.x >= -3.4f) {
				GlobalVars.POWER_UP_TUTORIAL_SWIPE = true;
				// Destroying the collider means that we cannot interact with the power up.
				Destroy(this.tutorialElements [2].GetComponent<Collider> ());
				// We call OnMouseUp to activate the power up.
				this.tutorialElements [2].GetComponent<ActivatePowerUp> ().OnMouseUp ();
				// Looping through each of the tutorial elements to turn back on their ability to move.
				for(int i = 2; i < 6; i++){
					tutorialElements[i].GetComponent<MoveElementDown>().isAllowedToMove = true;
				}
			}
		}
	}
	// A function to start the swiping tutorial.
	public void StartTutorial(GameObject element){
		// Start counting - for mixpanel so we know how long people spend on our tutorials
		StartTimer ();
		// Begin making the arrows scale up and down.
		StartFlashingCoroutine ();
		// Setting the correct location of the ringAndArrow and blue hand.
		SettingTutorialArrowsPositionAndScale (ringAndArrowForSwipeTutorial, element);
	}
	// It gives the tutorial element a name and sets the location of the ringAndArrow GameObject. It also stops the timer and spawning.

	// A public funtion used to allow another element to spawn for part 2 of the swipe tutorial
	public void SpawnSecondElementSwipeTutorial(GameObject element){
		// Move the flashing arrows to the new bucket destination.
		this.gameObject.GetComponent<ChangeArrowScale> ().MoveSwipeTutorialArrows ();
		// Begin making the arrows scale up and down.
		StartFlashingCoroutine ();
		// Setting the correct location of the ringAndArrow and blue hand.
		SettingTutorialArrowsPositionAndScale (ringAndArrowForSwipeTutorial, element);
	}
	// A function that takes in the tutorialArrow and a game object.
	private void SettingTutorialArrowsPositionAndScale(GameObject tutorialArrowElement, GameObject element){
		// Making the tutorial a child so we can set it's position
		tutorialArrowElement.transform.parent = element.transform;
		// Hard code adjustments for arrow position, god have mercy on our souls.
		correctElementPosition.x = -1f;
		correctElementPosition.y = -0.02f;
		// Setting the tutorialsArrows position
		tutorialArrowElement.transform.localPosition = correctElementPosition;
		// Setting the tutorialArrows scale, which changed randomly when giving it new positions.
		tutorialArrowElement.transform.localScale = new Vector3 (1f, 1f, 1f);
		// Stop the clock from ticking down any further.
		this.GetComponent<CollectionTimer> ().SetIsCountingDown (false);
		// Setting the tutorial position to that of the element.
		this.tutorialElement = element;
		// Stopping the tutorial element from falling.
		this.tutorialElement.GetComponent<MoveElementDown> ().isAllowedToMove = false;
		tutorialElementStartPosition = this.tutorialElement.transform.position;
		// Setting tutorial parts active.
		tutorialPanel.SetActive (true);
		tutorialArrowElement.SetActive (true);
	}
	// Makes the tutorialArrowElement a child of the element and sets the position

	// A function to reset tutorial items to be used again.
	public void MovedFirstElementOver(){
		// Turn off the tutorial mask.
		tutorialPanel.SetActive(false);
		ringAndArrowForSwipeTutorial.SetActive(false);
		// Allow the tutorial element to start falling
		tutorialElement.GetComponent<MoveElementDown> ().isAllowedToMove = true;
	}
	// A public function to be called by swipingMovement script when its detected the user has done the correct thing.
	public void EndSwipeTutorial(){
		// If the tutorial element has been destroyed or is equal to null then bounce out of the function.
		if(tutorialElement == null){
			return;
		}
		// If the tutorial element has the right name and a condition is set.
		if(tutorialElement.name == GlobalVars.SWIPE_TUTORIAL_SECOND_ELEMENT_NAME && GlobalVars.SWIPE_TUTORIAL_SECOND_SPAWNED){
			// The tutorial is over so stop the timer for mixpanel.
			EndTimer();
			// Set that the swipe tutorial has been watched, so doesn't need to be played again.
			PlayerPrefs.SetInt(GlobalVars.GATHERING_TUTORIAL_WATCHED_SWIPE, 1);
			// Start the game again.
			this.GetComponent<CollectionTimer>().SetIsCountingDown(true);
			// Call event here
			if(onSwipeTutorialComplete != null){
				onSwipeTutorialComplete(timeInTutorial);
			}
		}
		// Turn off the tutorial masks.
		tutorialPanel.SetActive(false);
		ringAndArrow2.SetActive(false);
		// All the tutorial element to move
		tutorialElement.GetComponent<MoveElementDown> ().isAllowedToMove = true;
		tutorialElement = null;
		// Start the game.
		this.GetComponent<GenerationScript> ().spawning = true;
		this.GetComponent<CollectionTimer> ().SetIsCountingDown(true);
	}
	// The function sets all tutorial elements to false and then starts spawning and the timer again.

	// A function called from the GenerationScript that gives us its spawning tutorial elemens
	public void SetTutorialElements(){
		tutorialElements = this.GetComponent<GenerationScript>().GetTutorialElements();
	}
	// Make the first power up tutorial elements fall after activation
	public void StartElementsMove(){
		tutorialElements [0].GetComponent<MoveElementDown> ().isAllowedToMove = true;
		tutorialElements [0].GetComponent<Collider> ().enabled = true;
		tutorialElements [1].GetComponent<MoveElementDown> ().isAllowedToMove = true;
		tutorialElements [1].GetComponent<Collider> ().enabled = true;
	}
	// A function to reset the ringAndArrows position so it can be reassigned
	public void ResetRingAndArrow(){
		ringAndArrowForSwipeTutorial.transform.parent = ringAndArrowRestingPosition.transform;
		ringAndArrow.transform.parent = ringAndArrowRestingPosition.transform;
	}

	// This function will take the array of gameObjects generated by GenerationScript,
	public void StartPowerUpTutorial(){
		// Start the timer for the power up tutorial.
		StartTimer ();
		// Name the power up elements in the scene.
		tutorialElements [2].name = "PowerUpSwipe";
		tutorialElements [3].name = "PowerUpTap";
		// Setting the first powerup to be interactable.
		tutorialElements [2].SetActive (true);
		tutorialElements [3].SetActive (true);

		// Setting the Tutorial Elements on the powerup.
		ringAndArrow.transform.parent = tutorialElements [2].transform;
		correctElementPosition.x = -0.96f; correctElementPosition.y = -0.02f;
		ringAndArrow.transform.localPosition = correctElementPosition;
		// Turning on the tutorial mask.
		tutorialPanel.SetActive (true);
		ringAndArrow.SetActive (true);
	}
	// And use them to create an interactive tutorial.

	// This function is called when the swipe section of the tutorial has ended,
	public void StartPowerUpTutorialTap(){
		// Setting the Tutorial Elements on the powerup.
		ringAndArrow.transform.parent = tutorialElements[3].transform;
		correctElementPosition.x = -0.96f; correctElementPosition.y = -0.02f;
		ringAndArrow.transform.localPosition = correctElementPosition;
		ringAndArrow.transform.localScale = new Vector3 (1,1,1);
		ringAndArrow.transform.GetChild (1).gameObject.SetActive (false);
		ringAndArrow.transform.GetChild (2).gameObject.SetActive (false);
	}
	// It then moves the ring and arrow to the next powerup.

	// A function to start a coroutine that counts.
	private void StartTimer () {
		timeInTutorial = 0;
		TutorialActive = true;
		tutorialTimerActive = true;
		StartCoroutine (Timer ());
	}
	// Purpose is to get how long it takes to complete the tutorial.

	// A function to stop the coroutine from counting.
	private void EndTimer () {
		TutorialActive = false;
		tutorialTimerActive = false;
	}
	// We take this time and use it in an event call to mixpanel information.

	// The coroutine that counts up how long the player has been playing the tutorial.
	IEnumerator Timer () {
		while (tutorialTimerActive) {
			timeInTutorial += Time.deltaTime;
			yield return new WaitForFixedUpdate();
		}
	}

	//called whenever an event is fired from a tutorial element
	private void TutorialElementEventHandler (bool mouseIsDown) {
		if (mouseIsDown) {
			StopFlashingCoroutine();
		} else {
			StartFlashingCoroutine ();
		}
	}

	//starts the flashing coroutine on the specified object
	private void StartFlashingCoroutine () {
		this.gameObject.GetComponent<ChangeArrowScale> ().StartArrowFlashing ();
		this.gameObject.GetComponent<ChangeArrowScale> ().EnableFlashingArrows ();
	}
	//stops the currently active coroutine
	private void StopFlashingCoroutine () {
		this.gameObject.GetComponent<ChangeArrowScale>().StopArrowFlashing();
		this.gameObject.GetComponent<ChangeArrowScale> ().DisableFlashingArrows();
	}
}