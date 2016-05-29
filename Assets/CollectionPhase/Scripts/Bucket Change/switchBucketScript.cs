/*
 * Script used to swap the buckets in gathering
 * Buckets swap every 15 seconds in the commercial mode
 * This behavior remains in the medical version, but can be overriden by the over emotion zone
 * This causes the buckets to swap uncontrollably
 */

//#define DEBUG
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class switchBucketScript : MonoBehaviour {
	//event call
	public delegate void BucketSwitchAction();
	public static event BucketSwitchAction OnBucketSwitch;

	// Time Passed is set to 1f to stop it from calling the update methods.
	private float timePassed = 1f;

	// stores the buckets
	private GameObject[] bucketPositions;

	// stores the lanes to move the buckets to
	private GameObject[] choosenLanes;

	// The laneChoices are the lanes that will swap.
	private int laneChoice1, laneChoice2;
	// A variable to set the speed of the MoveTowards method.
	private float moveTowardsSpeed = 8.0f;
	// The new positions of 
	private Vector3 bucketLocationChoice1,bucketLocationChoice2;
	private int[] swapChoices = {-1,1};
	// Dont allow bucket swapping is the coroutine has started to swap the buckets.
	public bool isSwappingCurrently = false;
	// A list of lanes choosen so far.
	private List<int> listOfLanesChoosen;
	// A counter for the amount of repeats in a list.
	private int repeatCounter;
	// A counter to keep track of the amount of swap in a game.
	private int swapCounter = 0;
	// The position elements, that are in lanes that are going to be swapped, are going to be moved to.
	private Vector3 elementMovePosition;
	// A raycast for detecting an element in a lane.
	private RaycastHit elementHit;
	// An array to store which lanes have elements in them when the raycast is fired.
	private List<GameObject> lanesWithHits;
	// A int that counts the size of lanesWithHits
	private int laneHitCounter =0;
	// Arrays to hold the animators in the 4 lanes.
	public Animator[] antiGravAnimators = new Animator[4];
	public Animator[] middleLanes = new Animator[4];
	// A reference to a swap bucket coroutines
	private IEnumerator swapBucketsReference;

	// A reference to the coroutine that stops the swap bucket coroutine
	private IEnumerator stopSwapBucketsReference; 

	//tracks the resume spawning coroutine
	private IEnumerator resumeSpawningCoroutine;

	void Start () {
		for (int i = 0; i < 4; i++) {
			middleLanes[i].SetTrigger("laneBeam");
		}
		// Create the array sizes
		choosenLanes = new GameObject[2];
		bucketPositions = new GameObject[4];
		listOfLanesChoosen = new List<int> ();
		lanesWithHits = new List<GameObject> ();

		// Find the buckets and set them to an array of Vector3's.
		bucketPositions [0] = GameObject.Find ("Zone1");
		bucketPositions [1] = GameObject.Find ("Zone2");
		bucketPositions [2] = GameObject.Find ("Zone3");
		bucketPositions [3] = GameObject.Find ("Zone4");

		// Subscribes to an event to stop spawning element when the game time ends
		CollectionTimer.OnEndGame += HaltResumeSpawning;

		//binds the bucket swap function to the over threshold event
		// only used for the Neuro'motion Medical implementation
		if (GlobalVars.MEDICAL_USE) {

			//subscribes to events
			SDKEventManager.OnOverThreshold += StartOverThresholdSwitching;
			SDKEventManager.OnUnderThreshold += EndOverThresholdSwitching;
			SDKEventManager.OnNearThreshold += NearThreshold;
			PlayTutorial.OnTutorialComplete += HandleOnTutorialComplete;

			//transparent orange
			startColor = new Color(1.0f, 110f/255f, 38f/255f, 0);
			//half transparent orange
			endColor = new Color(1.0f, 110f/255f, 38f/255f, 0.5f);

			SDKEventManager.Instance.ChangeIndicator();
#if DEBUG
			Debug.Log("The time is currently set to: " +  GlobalVars.COLLECT_TIME);
#endif
			//if already over threshold, starting switching
			if (SDKEventManager.MyEmotionZone == EmotionZone.OVER_EMOTION_ZONE) {
				StartOverThresholdSwitching();
#if DEBUG
				Debug.Log("*********I'm going to start switching now**********");
#endif
			}
		}
	}

	// Unbinds the references to the events when the gameobject is destroyed
	void OnDestroy () {
		CollectionTimer.OnEndGame -= HaltResumeSpawning;

		//unbinds the bucket swap function to the over threshold event
		if (GlobalVars.MEDICAL_USE) {
			SDKEventManager.OnOverThreshold -= StartOverThresholdSwitching;
			SDKEventManager.OnNearThreshold -= NearThreshold;
			SDKEventManager.OnUnderThreshold -= EndOverThresholdSwitching;
			PlayTutorial.OnTutorialComplete -= HandleOnTutorialComplete;
		}
	}

	void Update () {

#if DEBUG
		if (Input.GetKeyDown(KeyCode.Space)) {
			StartOverThresholdSwitching();
		}

		if (Input.GetKeyDown(KeyCode.A)) {
			EndOverThresholdSwitching();
		}
#endif


		// Switches the buckets every fifteen seconds
		if (!GlobalVars.PAUSED) { // Check if the game is paused.
			if(this.GetComponent<CollectionTimer> ().getIsCountingDown()){
				timePassed += Time.deltaTime;
			}
			if ((int)timePassed % 15 == 0) {// && swapCounter <= 1) { // If a certain amount of seconds have passed then swap the buckets.
				//TODO: Add a condition that checks if the tutorial is playing.
				if(!isSwappingCurrently && !isOverThresholdSwapping && !GlobalVars.IS_SWIPE_TUTORIAL_CURRENTLY_PLAYING){
					isSwappingCurrently = true;
					FireRaycast();
					ChooseSwappingBuckets();
					MoveElementsOnScreenUp();
					bucketLocationChoice1 = bucketPositions [laneChoice1].transform.position;
					bucketLocationChoice2 = bucketPositions [laneChoice2].transform.position;
					StartCoroutine(WaitBeforeSwap());
					swapCounter++;
					timePassed++;
				}
			}
		}

		// Checks if there are no on screen elements and resumes the spawning if so
		if(ZoneCollisionDetection.ON_SCREEN_ELEMENTS == null){
			//stops the coroutine to replace it with the current coroutine
			HaltResumeSpawning();

			//starts the resume spawning coroutine
			resumeSpawningCoroutine = StartSpawning();
			StartCoroutine(resumeSpawningCoroutine);
		}
	}

	// Fire raycasts to detect elements in lane
	void FireRaycast(){
		foreach (GameObject bucketPosition in bucketPositions) {
			if (Physics.Raycast (bucketPosition.transform.position, Vector3.up, out elementHit)) {
				if (elementHit.collider != null) {
					lanesWithHits.Add(bucketPosition);
				}
			}
		}
		// If there is an element in a lane, randomly set laneChoice1 to one of the lanes.
		if(lanesWithHits.Count >= 1){
			if(lanesWithHits[Random.Range(0, lanesWithHits.Count)].name.Contains("1")){
				laneChoice1 = 0;
			}
			else if(lanesWithHits[Random.Range(0, lanesWithHits.Count)].name.Contains("2")){
				laneChoice1 = 1;
			}
			else if(lanesWithHits[Random.Range(0, lanesWithHits.Count)].name.Contains("3")){
				laneChoice1 = 2;
			}
			else if(lanesWithHits[Random.Range(0, lanesWithHits.Count)].name.Contains("4")){
				laneChoice1 = 3;
			}
		}
	}
	// If we detect an element pass the lane number in as laneChoice1, if there is more than one hit randomly choose which lane.

	// Choosing which collection buckets are going to swap.
	void ChooseSwappingBuckets(){
		if (lanesWithHits.Count <= 0) {
			laneChoice1 = Random.Range (0, 4);
			// START OF SUDO RANDOM
			listOfLanesChoosen.Add (laneChoice1);
			foreach (int lane in listOfLanesChoosen) {
				if (lane == laneChoice1) {
					repeatCounter++;
				}
			}
			int temporaryLaneChoice1 = laneChoice1;
			if (repeatCounter >= 2) {
				while (laneChoice1 == temporaryLaneChoice1) {
					laneChoice1 = Random.Range (0, 4);
				}
			}
		}
		// END OF SUDO RANDOM
		if(laneChoice1 == 0){
			laneChoice2 = laneChoice1 + 1;
		}
		else if(laneChoice2 == 3){
			laneChoice2 = laneChoice1 - 1;
		}
		else if(laneChoice1 >= 1){
			laneChoice2 = laneChoice1 + swapChoices[Random.Range(0,1)];
		}
		//generic method to swap objects in these arrays to their proper places
		Utility.SwitchObjectsInArray(bucketPositions, laneChoice1, laneChoice2);
		Utility.SwitchObjectsInArray(GetComponent<GenerationScript>().elements, laneChoice1, laneChoice2);
		Utility.SwitchObjectsInArray(GetComponent<GenerationScript>().elementSprites, laneChoice1, laneChoice2);
		Utility.SwitchObjectsInArray(GlobalVars.GATHERING_ZONES, laneChoice1, laneChoice2);
		Utility.SwitchObjectsInArray(middleLanes, laneChoice1, laneChoice2);
	}
	// The choices must be buckets that are beside each other. 

	// This method is given the 2 lanes choosen to be spawned.
	void MoveElementsOnScreenUp(){
		// Stop the CollectionTimer script from counting down while a swap is happening.
		this.GetComponent<CollectionTimer> ().SetIsCountingDown (false);
		this.GetComponent<GenerationScript> ().SetSpawning(false);
		for (int i = 0; i < 4; i++) {
			antiGravAnimators [i].SetTrigger ("preSwapping");
		}
		foreach(GameObject elementInLane in ZoneCollisionDetection.GetOnScreenElements ()){
			if(elementInLane != null){
				elementInLane.GetComponent<MoveElementDown>().enabled = false;
				elementInLane.GetComponent<MoveElementUp>().enabled = true;
				StartCoroutine(WaitBeforeResetComponents());
			}
		}
	}
	// Then turns on their MoveElementUp component which makes them start to rise.

	// Calling this method turns on and off the MoveElementDown and MoveElementUp components respectively.
	void SetMoveComponents(MoveElementDown moveDownComponent, MoveElementUp moveUpComponent, bool moveDown, bool moveUp){
		moveDownComponent.enabled = moveDown;
		moveUpComponent.enabled = moveUp;
	}
	// This stops the elements from moving up and starts them moving down again or vice versa.

	// Selects the elements to swap
	void ChooseElementsToMove(){
		middleLanes [laneChoice1].SetTrigger ("swapping");
		middleLanes [laneChoice2].SetTrigger ("swapping");
		for (int i = 0; i < 4; i++) {
			antiGravAnimators [i].SetTrigger ("preSwapping");
		}
		// bucketposition x move towards instead
		// Move the elements in the lanes.
		foreach(GameObject elementInLane in ZoneCollisionDetection.ON_SCREEN_ELEMENTS){
			if(elementInLane != null){
				if(elementInLane.transform.position.x <= bucketPositions[laneChoice1].transform.position.x + 0.1f && elementInLane.transform.position.x >= bucketPositions[laneChoice1].transform.position.x - 0.1f){
					SetMoveComponents(elementInLane.GetComponent<MoveElementDown>(), elementInLane.GetComponent<MoveElementUp>(), false, true);
					elementMovePosition = elementInLane.transform.position;
					elementMovePosition.x = bucketPositions[laneChoice2].transform.position.x;
					// Adding an arch, minus means it becomes an underarch, plus means its an over arch. No change to y means it swaps directly across.
					//elementMovePosition.y += 0.7f;
					StartCoroutine(SwapElement(elementInLane, elementMovePosition));
				}else if(elementInLane.transform.position.x <= bucketPositions[laneChoice2].transform.position.x + 0.1f && elementInLane.transform.position.x >= bucketPositions[laneChoice2].transform.position.x - 0.1f){
					SetMoveComponents(elementInLane.GetComponent<MoveElementDown>(), elementInLane.GetComponent<MoveElementUp>(), false, true);
					elementMovePosition = elementInLane.transform.position;
					elementMovePosition.x = bucketPositions[laneChoice1].transform.position.x;
					// Adding an arch, minus means it becomes an underarch, plus means its an over arch. No change to y means it swaps directly across.
					//elementMovePosition.y += 0.7f;
					StartCoroutine(SwapElement(elementInLane, elementMovePosition));
				}
			}
		}
		StartCoroutine(ChangeBackLaneBeam ());
		// End of moving elements
	}

	// This method is called to reset all the components of MoveElementDown and MoveElementUp
	void ResetMovementComponents(){
		foreach(GameObject onScreenElement in ZoneCollisionDetection.ON_SCREEN_ELEMENTS){
			if(onScreenElement != null){
				SetMoveComponents(onScreenElement.GetComponent<MoveElementDown>(), onScreenElement.GetComponent<MoveElementUp>(), true, false);
			}
		}
		if(this.GetComponent<CollectionTimer>().GetTimeRemaining() >= 0){
			StartCoroutine (StartSpawning());
		}
	}
	// From the elements currently on screen.


	// Stops the resume spawning coroutine: 
	// This will ensure that elements don't start spawning after the game ends
	private void HaltResumeSpawning () {
		if (resumeSpawningCoroutine != null) {
			StopCoroutine(resumeSpawningCoroutine);
		}
	}

	// This method is given a gameObject that needs to be swapped and the position it's to be moved to.
	IEnumerator SwapElement(GameObject elementToSwap, Vector3 endOfSwapPosition){
		if (elementToSwap != null) {
			while (!(Vector3.Distance(elementToSwap.transform.position, endOfSwapPosition) <= 0.1f)) {
				elementToSwap.transform.position = Vector3.MoveTowards (elementToSwap.transform.position, endOfSwapPosition, Time.deltaTime * (moveTowardsSpeed + 1));
				yield return null;
			}
			elementToSwap.GetComponent<SwipingMovement>().OnMouseUp();
			elementToSwap.GetComponent<MoveElementDown>().UpdateLane();
			ResetMovementComponents ();
		}
	}
	// The gameObject is thrown into a while loop and made move towards the given positions until it's reached it.

	// This method is given the selected lanes for swapping.
	public IEnumerator SwapBuckets(int lane1, int lane2, Vector3 bucketChoice1, Vector3 bucketChoice2){
		// Stops the coroutine to stop this coroutine (otherwise this coroutine can be stopped too early)
		if (stopSwapBucketsReference != null) {
			StopCoroutine(stopSwapBucketsReference);
		}

		//calls the event
		if (OnBucketSwitch != null) {
			OnBucketSwitch();
		}
		if (!isOverThresholdSwapping) {
			ChooseElementsToMove ();
		}
		if (bucketChoice1.x != bucketChoice2.x && lane1 != lane2) {
			while (!(Vector3.Distance(bucketChoice1,bucketPositions[lane2].transform.position) == 0.001f)) {
				bucketPositions [lane1].transform.position = Vector3.MoveTowards (bucketPositions [lane1].transform.position, bucketChoice2, Time.deltaTime * moveTowardsSpeed);
				bucketPositions [lane2].transform.position = Vector3.MoveTowards (bucketPositions [lane2].transform.position, bucketChoice1, Time.deltaTime * moveTowardsSpeed);
				#if DEBUG
					Debug.Log("We're also stuck in this loop");
				#endif
				yield return new WaitForEndOfFrame ();
			}
		}
#if DEBUG
		Debug.Log("We've reached the end of the loop");
#endif
		isSwappingCurrently = false;
	}
	// The buckets positions are switched with each other as they moveTowards their respective starting positions.

	// Tells the program to wait 1 second before starting the SwapBuckets coroutine.
	// Gives the elements time to hover up above
	public IEnumerator WaitBeforeSwap(){
		yield return new WaitForSeconds (1.5f);
		swapBucketsReference = SwapBuckets (laneChoice1, laneChoice2, bucketLocationChoice1, bucketLocationChoice2);
		StartCoroutine (swapBucketsReference);
		StartCoroutine (StopBucketSwap(1.5f, swapBucketsReference));
		StartCoroutine (WaitBeforeCounting (3));
	}
	// This is wanted so that the elements can get pushed before the buckets swap.

	// Another coroutine to wait before calling a method.
	public IEnumerator WaitBeforeResetComponents(){
		yield return new WaitForSeconds (3);
		ResetMovementComponents ();
	}
	// Calls the resetMovementComponents method after 2 seconds.

	// A coroutine to wait 1 second,
	public IEnumerator StartSpawning(){
		yield return new WaitForSeconds (3f);
		// If the collection timer has ended then don't allow spawning to be set to true.
		if (this.GetComponent<CollectionTimer> ().GetTimeRemaining () > 0) {
			this.GetComponent<GenerationScript> ().SetSpawning (true);
		} else {
			this.GetComponent<GenerationScript> ().SetSpawning (false);
		}
	}
	// After that second let the generation script start spawning elements again.

	// A coroutine to wait a second before turning the laneBeam animation back on.
	IEnumerator ChangeBackLaneBeam(){
		yield return new WaitForSeconds (1f);
		middleLanes [laneChoice1].SetTrigger ("laneBeam");
		middleLanes [laneChoice2].SetTrigger ("laneBeam");
	}
	// After the swapping animation is finished turn back on the laneBeam animation.

	// When the bucket swap coroutine begins start a coroutine,
	IEnumerator WaitBeforeCounting(int seconds){
		yield return new WaitForSeconds (seconds);
		// Allows the timeRemaining to continue counting down.
		this.GetComponent<CollectionTimer> ().SetIsCountingDown (true);
		isSwappingCurrently = false;
	}
	// that waits for a number of seconds then tells sets counting down to be true and spawning to be true

	// This coroutine waits for a few seconds then stops the swap bucket coroutine because otherwise it goes on forever.
	IEnumerator StopBucketSwap(float seconds, IEnumerator swapBuckets){
		yield return new WaitForSeconds (seconds);
		StopCoroutine (swapBuckets);
#if DEBUG
		Debug.Log("I should no longer be swapping");
#endif
		isSwappingCurrently = false;
	}
	// The SwapBucket coroutine doesn't stop because it never gets close enough to 0 distance to exit.

	//returns a lane index that is adjacent to the given one
	private int chooseAdjacentLane (int firstLaneIndex) {
		if (firstLaneIndex == 0) {
			return 1;
		} else if (firstLaneIndex == GlobalVars.NUMBER_OF_LANES-1) {
			return GlobalVars.NUMBER_OF_LANES-2;
		} else {
			return (Random.value > 0.5f)? firstLaneIndex + 1:firstLaneIndex -1;
		}
	}

	/*
	 * The below code is only used for the Neuro*motion implementation
	 * Only runs when GlobalVars.MEDICAL_USE is set to true
	 * Makes the buckets switch uncontrollably when the player goes over emotion zone
	 * Turns the bucket switching off when the players goes under threshold
	 */
	#region MEDICAL_USE
	//variable to reference the coroutine
	private IEnumerator overThresholdCoroutine;
	private IEnumerator nearThresholdCoroutine;

	// Turns threshold swapping off when it's set to false
	private bool isOverThresholdSwapping;

	// Determines the color that the screen flashes when the player gets near over threshold
	private Color startColor;
	private Color endColor;

	// Reference to the overlayed image that flashes
	public Image screenFlash;

	//coroutine that keeps switching buckets without hovering elements until the player exits the threshold
	private void StartOverThresholdSwitching () {
		//breaks out of the function if the even is already being called
		if (isOverThresholdSwapping || PlayTutorial.TutorialActive) {
			return;
		}
		overThresholdCoroutine = OverThresholdSwitching();
		isOverThresholdSwapping = true;
		StartCoroutine(overThresholdCoroutine);
	}

	// Stops the buckets from switching
	// Bound to the calm emotion zone and near threshold zone events
	//  Triggers anytime the player goes from over threshold to any other emotion zone
	private void EndOverThresholdSwitching () {
		if (overThresholdCoroutine != null) {
			StopCoroutine(overThresholdCoroutine);
			isOverThresholdSwapping = false;
		}
	}

	//handles the near threshold event
	//stops the switching if it's coming from over threshold
	//flashes orange if the player is approaching threshold from calm
	private void NearThreshold (bool zoneRising) {
		//if (GlobalVars.

		if (nearThresholdCoroutine != null) {
			StopCoroutine(nearThresholdCoroutine);
		}

		if (zoneRising) {
			nearThresholdCoroutine = FlashWarningColor ();
			StartCoroutine (nearThresholdCoroutine);
		} else {
			EndOverThresholdSwitching();
		}
	}

	IEnumerator FlashWarningColor () {
		//exits the function if the game is currently paused
		if (GlobalVars.PAUSED) {
			yield return null;
		}

		//starts at the color that the screenFlash currently is at
		Color initialColor = screenFlash.color;

		float time = 0;
		while (screenFlash.color != endColor) {
			screenFlash.color = Color.Lerp(initialColor, endColor, time);
			time += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		time = 0;
		while (screenFlash.color != startColor) {
			screenFlash.color = Color.Lerp(endColor, initialColor, time);
			time += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
	}

	//swaps the buckets repeatedly while the ply
	IEnumerator OverThresholdSwitching () {
#if DEBUG
		Debug.Log("should be swapping now");
#endif

		while (GlobalVars.PAUSED) {
			yield return new WaitForFixedUpdate();
		}
		while (isOverThresholdSwapping) {
			isSwappingCurrently = true;
#if DEBUG
			Debug.Log("Choosing two buckets to swap");
#endif
			ChooseSwappingBuckets();
			bucketLocationChoice1 = bucketPositions [laneChoice1].transform.position;
			bucketLocationChoice2 = bucketPositions [laneChoice2].transform.position;
#if DEBUG 
			Debug.Log("Swapping the two buckets: numbers " + laneChoice1 + " and " + laneChoice2);
#endif
			swapBucketsReference = SwapBuckets (laneChoice1, laneChoice2, bucketLocationChoice1, bucketLocationChoice2);
			StartCoroutine (swapBucketsReference);

			//a coroutine to stop the bucket swap
			stopSwapBucketsReference = StopBucketSwap(0.5f, swapBucketsReference); 
			StartCoroutine (stopSwapBucketsReference);
			//waits until the current swap is done to swap again 
			while (isSwappingCurrently) {
				#if DEBUG 
				Debug.Log("Still stuck in this stupid loop");
				#endif
				yield return new WaitForEndOfFrame();

			}
		}
	}

	// Starts the bucket swapping once the tutorial is complete if the player is currently over emotion
	void HandleOnTutorialComplete () {
		if (GlobalVars.MEDICAL_USE) { 
			if (SDKEventManager.MyEmotionZone == EmotionZone.OVER_EMOTION_ZONE) {
				StartOverThresholdSwitching();
				#if DEBUG
				Debug.Log("*********I'm going to start switching now**********");
				#endif
			}
		}
	}

	#endregion
}
