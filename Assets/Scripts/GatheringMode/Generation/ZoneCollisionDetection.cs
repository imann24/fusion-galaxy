/*
 * Used to tell when an element has fall into a bucket
 * Relies on 2D Colliders and the OnTriggerEnter script - 3D Colliders?
 * isTrigger is set to true on all the elements and buckets
 * Calls events based on whether the element was correct or incorrect
 * Uses tags on the elements and buckets to reason about whether they were correct
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Used to collect elements that fall into the zones 
public class ZoneCollisionDetection : MonoBehaviour {
	//event calls
	public delegate void RightElementAction();
	public delegate void WrongElementAction();
	public static event RightElementAction OnRightElement;
	public static event WrongElementAction OnWrongElement;

	//for bucket shield
	public delegate void BucketShieldCreateAction(int lane);
	public delegate void BucketShieldHitAction(int lane);
	public delegate void BucketShieldDestoyAction(int lane);
	public static event BucketShieldCreateAction OnBucketShieldCreated;
	public static event BucketShieldHitAction OnBucketShieldHit;
	public static event BucketShieldDestoyAction OnBucketShieldDestroyed;

	//bucket shield variables
	private int bucketShieldHitPoints = 0;
	public int basePointsForElement = 1;

	// Reference to the controller of the gathering mode
	GenerationScript generationScript;

	// Used to check against the tags of the elements that collide with the bucket
	private string collectionBucketType;

	// Used to tell the bucket which type of element its collected
	// Reasoned by the index of the lane
	private int collectionElement;

	// A list of all the onscreen elements
	public static List<GameObject> ON_SCREEN_ELEMENTS = new List<GameObject>();

	// A list of the tags used on the elements
	public static List<string> COLLECTION_TAGS = new List<string>();
	// A gameobject for the controller in scene.
	private GameObject controller;
	// The amount each element is worth.
	private int pointsForElement;
	// Whether or not the player is invunerable
	private bool invunerable;
	// The rate of colour change.
	private float colourChangeRate = 0.1f;

	//references to Coroutines
	private IEnumerator deactivateInvunerabilityCoroutine;
	private IEnumerator multiplierActiveCoroutine;

	//tutorial element
	private GameObject swipeTutorialElement;

	// An animator for the correct and incorrect collection animation.
	public Animator bucketAnimator;

	// An animator to show the number when an element goes into the bucket.
	public Animator numberChangeAnimator;

	void Awake () {
		// Sets the tags on the elements: reasoned in term of zone
		// The zones represent the four lanes/columns in the gathering game
		for (int i = 0; i < GlobalVars.NUMBER_OF_LANES; i++) {
			COLLECTION_TAGS.Add ("Zone" + (i+1));
		}

		// Sets the collection index based on the tag on the bucket
		// Used to reason about what ype of element it collects
		if(this.gameObject.tag == "Zone1Collector"){
			collectionElement = 0;
		}
		else if(this.gameObject.tag == "Zone2Collector"){
			collectionElement = 1;
		}
		else if(this.gameObject.tag == "Zone3Collector"){
			collectionElement = 2;
		}
		else if(this.gameObject.tag == "Zone4Collector"){
			collectionElement = 3;
		}

		// Sets the element type the bucket it collecting, based on its index
		collectionBucketType = COLLECTION_TAGS[collectionElement];

		// Assigns a global reference to each version of the script, based on 
		GlobalVars.GATHERING_ZONES[collectionElement] = this;
	}

	void Start () {
		//sets the starting score an element
		pointsForElement = basePointsForElement;

		//finds the controller and script references 
		controller = GameObject.FindGameObjectWithTag ("Controller");
		// Assigning the generation script.
		generationScript = controller.GetComponent<GenerationScript>();
	}

	// Called when an element enters the collider zone
	void OnTriggerEnter(Collider elementCollider){
		// Starts the spawning again
		// NOTE: Spawning is paused during the bucket swap to keep elements from falling into the zones while they're switching
		// For this code: see switchBucketScript
		if(controller.GetComponent<switchBucketScript>().isSwappingCurrently){
			controller.GetComponent<switchBucketScript>().StartSpawning();
		}

		// Controls the behavior of the game when the elements fall into the buckets during the tutorial
		if(elementCollider.gameObject.name == GlobalVars.SWIPE_TUTORIAL_ELEMENT_NAME && !GlobalVars.SWIPE_TUTORIAL_SECOND_SPAWNED){
			controller.GetComponent<GenerationScript>().SpawnInSecondSwipeTutorialElement();
			controller.GetComponent<GenerationScript>().SetSpawning(false);
		}

		// Further tutorial logic
		if(elementCollider.gameObject.name == GlobalVars.SWIPE_TUTORIAL_SECOND_ELEMENT_NAME && GlobalVars.SWIPE_TUTORIAL_SECOND_SPAWNED){
			controller.GetComponent<PlayTutorial>().EndSwipeTutorial();
			controller.GetComponent<GenerationScript>().SetSpawning(true);
		}

		// If the correct element enters the bucket
 		if (elementCollider.gameObject.tag == collectionBucketType) {

			// Plays the animation for a correct element collection
			bucketAnimator.SetTrigger("correctCollection");

			// Updates the displayed and stored score
			generationScript.updateScore (collectionElement, pointsForElement);

			// Destroys the element
			Destroy(elementCollider.gameObject);

			//calls event to play the sound and trigger any other actions that should happen on element collection
			if (OnRightElement != null) {
				OnRightElement();
			}
			return;
		}
		// Return, when a powerup orb has entered a bucket:
		// The buckets should ignore powerups
		else if (elementCollider.gameObject.tag == GlobalVars.POWER_UP_TAG){
			return;
		}
		//destroys the element if it is incorrect
		else if(elementCollider.gameObject.tag != collectionBucketType){

			//Plays the animation
			bucketAnimator.SetTrigger("incorrectCollection");

			// Increase the number of elements that went into this bucket that didn't belong.
			GlobalVars.MISSED++;

			// Start a coroutine ChangeMiddleLane.
			StartCoroutine(ChangeMiddleLane());
			// Destroy the elementCollider.gameObject.
			Destroy(elementCollider.gameObject);

			//returns if it's invincible
			if (invunerable) {
				return;
			}

			//returns if the bucket shield is up and deactivates from its ponits
			if (bucketShieldHitPoints > 0) { 
				bucketShieldHitPoints--;
				if (OnBucketShieldHit != null) {
					OnBucketShieldHit(collectionElement);
				}

				if (bucketShieldHitPoints == 0 && OnBucketShieldDestroyed != null) {
					OnBucketShieldDestroyed(collectionElement);
				}
				return;
			}
			// Sets the trigger that plays the minus animation.
			numberChangeAnimator.SetTrigger("minus3");
			//calls event
			if (OnWrongElement != null) {
				OnWrongElement();
			}
			//detriment to score and destroys all onscreen units
			generationScript.wipeScore(collectionElement);
			destroyOnscreenUnits();

		}
		// Remove the on screen element that has collided.
		ON_SCREEN_ELEMENTS.Remove(elementCollider.gameObject);
	}
	// Gets the ON_SCREEN_ELEMENTS.
	void destroyOnscreenUnits () {
		foreach (GameObject go in ON_SCREEN_ELEMENTS) {
			Destroy(go);
		}
		// Clear the array.
		ON_SCREEN_ELEMENTS.Clear();
	}
	// Goes through each element on screen and destroys them.

	// Return all the ON_SCREEN_ELEMENTS
	public static List<GameObject> GetOnScreenElements(){
		return ON_SCREEN_ELEMENTS;
	}

	// PUBLIC VARIABLES
	// Gets the newest spawned in element.
	public static void addToOnScreenElements(GameObject onScreenElement){
		ON_SCREEN_ELEMENTS.Add (onScreenElement);
	}
	// The given gameobject is added to the ON_SCREEN_ELEMENTS.

	// Adds a multiplier for a certain amount of time.
	public void modifyPoints (int multiplier, float? duration) {
		this.pointsForElement = multiplier * basePointsForElement;
		if (duration != null) { 
			//stops the currently running instance if any
			if (multiplierActiveCoroutine != null) {
				StopCoroutine(multiplierActiveCoroutine);
			}
			float timer = (float) duration;
			multiplierActiveCoroutine = multiplierActive(timer);
			StartCoroutine(multiplierActiveCoroutine);
		}
	}

	// The amount that an element is worth is doubled
	public void doublePoints (float? duration) {
		modifyPoints(2, duration);
	}
	// This method allows you to modify the basePointsForElement's.
	public void modifyBasePoints (int multiplier) {
		basePointsForElement *= multiplier;
	}
	// The basePointsForElement gets mutliplied by the multiplier input parameter.

	// This resets the amount an element is worth,
	IEnumerator multiplierActive (float duration) {
		yield return new WaitForSeconds (duration);
		pointsForElement = basePointsForElement;
	}
	// After a certain amount of time it is reset to the basePointsForElement

	// Gets the current score in this bucket,
	public void multiplyCurrentScore (int multipler) {
		generationScript.multiplyScore(collectionElement, multipler);
	}
	// Then multiplies that score by the amount given in.

	// Method is given the amount of time to be Invunerable
	public void makeInvunerable (float duration) {
		if (deactivateInvunerabilityCoroutine != null) {
			StopCoroutine(deactivateInvunerabilityCoroutine);
		}
		invunerable = true;
		deactivateInvunerabilityCoroutine = deactivateInvunerability(duration);
		StartCoroutine(deactivateInvunerabilityCoroutine);
	}
	// Method will set invunerable to true so that the script ignores incorrect collections.

	// A method to take a certain amount to time.
	IEnumerator deactivateInvunerability (float duration) {
		yield return new WaitForSeconds(duration);
		invunerable = false;
	}
	// To wait that time and then set invunerable to false so that incorrect collections are now registered.

	// This method will get the value of an element.
	public int getCurrentPointValue () {
		return pointsForElement;
	}
	// It can be used with animations so place a number in front of the element to show how much it's worth - also to work out how much to add to the bucket.

	// A coroutine that waits a second.
	IEnumerator ChangeMiddleLane(){
		this.transform.GetChild(2).GetComponent<SpriteRenderer>().color = Color.Lerp(Color.red, Color.white, Time.deltaTime * colourChangeRate);
		yield return new WaitForSeconds (1);
		this.transform.GetChild(2).GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, Color.red, Time.deltaTime * colourChangeRate);
	}
	// Then changes the colour the 3 child of its gameobject from red to white.

	// Activates the bucket shield, if it's already active, adds to its hitpoints
	public void bucketShieldActive (int hitPoints) {
		if (bucketShieldHitPoints == 0 && OnBucketShieldCreated != null) {
			OnBucketShieldCreated(collectionElement);
		}
		//increases the hit points of the bucket shield
		bucketShieldHitPoints += hitPoints;
	}
}
