#define DEBUG

using UnityEngine;
using System.Collections;

//Used to move elements between lanes
public class SwipingMovement : MonoBehaviour {
	//event for when you click on the tutorial element\
	public delegate void TutorialElementMouseAction (bool mouseIsDown);
	public static event TutorialElementMouseAction OnTutorialElementMouseAction;

	//for tap to collect
	public static bool TAP_TO_COLLECT = false;
	static IEnumerator TapToCollectCoroutine;
	public delegate void CollectedAction(); 
	public static event CollectedAction OnCollected;

	//used for positioning 
	private Vector3 elementScreenPoint;
	private Vector3 cameraOffset;
	private float gridSpace;
	// 2 Vector3s to work out the position the mouse is so we can set the element position.
	private Vector3 currentMousePosition;
	private Vector3 hoveringMousePosition;
	// A float for the speed of the element.
	private float elementMovementSpeed;
	// A bool to tell us if we are dragging elements.
	private bool isMouseDragging;
	// An array of SpriteRenderers to hold the laneHighLighters.
	private SpriteRenderer [] laneHighLighters = new SpriteRenderer[4];
	// The halo that appears around the element when dragging.
	private SpriteRenderer myRing;
	// An audio source for clicking.
	private AudioSource clickSound;
	// The positions of the elements onClick.
	private Vector3 elementPositionOnClick, gameObjectWorldPosition;
	// A bool to detect if the player has clicked or not.
	private bool hasPlayerClicked = false;
	// The position at which the player clicked.
	private Vector3 clickPosition;
	// The position at which the player clicked rounded to a lanePosition.
	private Vector3 roundedClickPosition;
	// A bool for if the element is lerping or not.
	private bool isLerping = false;
	// Redundent bools.
	private bool isLerpingLeft = false, isLightMovementLerpingRight = false, isLightMovementLerpingLeft = false;
	// A gameobject to hold the controller in the scene.
	private GameObject controller;
	private bool isCorrectMove = false;
	// An array of Vector3s to hold the lane positions.
	private Vector3[] lanes;
	// TUTORIAL BOOLS
	private bool firstSwipeTutorialElementsMoved = false;
	private bool secondSwipeTutorialElementMoved = false;
	// Element sprite.
	public Sprite element {get; private set;}
	// Use this for initialization
	void Start () {
		gridSpace = LanePositions.GetDistanceBetweenLanes (0);
		lanes = LanePositions.GetLanePositions(0);
		//finds references to spriterenders
		myRing = transform.GetChild(1).GetComponent<SpriteRenderer>();

		// Get the contoller game object from the game scene.
		controller = GameObject.Find ("Controller");
		//sets sound
		clickSound = transform.GetComponent<AudioSource>();

		//sets the move speed
		elementMovementSpeed = controller.GetComponent<GenerationScript> ().elementMovementSpeed;
	
		//variables to control movement
		gameObjectWorldPosition = this.transform.position;
		transform.position = new Vector3((gameObjectWorldPosition.x / gridSpace) * gridSpace, gameObjectWorldPosition.y,gameObjectWorldPosition.z);
		element = transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
	}
	
	//to compensate for the object moving down
	void Update () {
		// If the mouse is dragging then keep setting the element y position. Hard coding for the win.
		if(isMouseDragging){
			elementScreenPoint.y -= Time.deltaTime * elementMovementSpeed* 32;
		}
		// This is set to true after the background is clicked on anywhere other than the element.
		if(isLerping){
			if(clickPosition.x < transform.position.x){
				isLerpingLeft = true;
			}
			else if(clickPosition.x > transform.position.x){
				isLerpingLeft = false;
			}
			// Set the y and z position to stay as it is and then lerp the x position.
			clickPosition.y = transform.position.y;
			clickPosition.z = transform.position.z;
			transform.position = Vector3.Lerp (transform.position, clickPosition, elementMovementSpeed/5);
		}
		// This is a test to see if the lerp is finished, if it is then set isLerping to false.
		if(transform.position == clickPosition){
			isLerping =false;
			myRing.enabled = false;
			if(isLerpingLeft){
				transform.position = new Vector3((clickPosition.x / gridSpace) * gridSpace, transform.position.y,transform.position.z);
			}
			else if(!isLerpingLeft){
				transform.position = new Vector3((clickPosition.x / gridSpace) * gridSpace, transform.position.y,transform.position.z);
			}
		}
	}

	//selects the element on user tap.
	void OnMouseDown(){
		if(this.gameObject.name == GlobalVars.SWIPE_TUTORIAL_SECOND_ELEMENT_NAME && secondSwipeTutorialElementMoved){
#if DEBUG
			Debug.Log("Exiting the function here because this is the second tutorial element");
#endif
			return;
		}
		// Quit out of the OnMouseDown function if they player is trying to move the power up tutorial elements
		if(this.gameObject.name == "TutorialElement1" || this.gameObject.name == "TutorialElement2" || this.gameObject.name == "TutorialElement4" || this.gameObject.name == "TutorialElement5"){
#if DEBUG
			Debug.Log("Exiting the function here because this is one of the other tutorial elements");
#endif
			return;
		}
		if(this.gameObject.name == GlobalVars.SWIPE_TUTORIAL_ELEMENT_NAME || this.gameObject.name == GlobalVars.SWIPE_TUTORIAL_SECOND_ELEMENT_NAME){
			if (OnTutorialElementMouseAction != null) {
				OnTutorialElementMouseAction(true);
			}
		}

		// Collects the element if tap to collect is enabled
		// Tap to Collect is enabled by a powerup
		if (GenerationScript.TAP_TO_COLLECT && tag != GlobalVars.POWER_UP_TAG) {
			if (OnCollected != null) {
				OnCollected();
			}
			collectElement();
			return;
		}

		elementPositionOnClick = transform.position;
		clickSound.Play();
		isMouseDragging = true;
		myRing.enabled = true;
	}

	//moves the element when the player drags there finger.
	void OnMouseDrag() {
		if(this.gameObject.name == "TutorialElement1" || this.gameObject.name == "TutorialElement2" || this.gameObject.name == "TutorialElement4" || this.gameObject.name == "TutorialElement5"){
			return;
		}
		if(this.gameObject.name == GlobalVars.POWER_UP_TUTORIAL_TAP_NAME && PlayerPrefs.GetInt(GlobalVars.GATHERING_TUTORIAL_WATCHED_POWER_UP) == 0){
			return;
		}
		if(this.gameObject.name == GlobalVars.SWIPE_TUTORIAL_ELEMENT_NAME && firstSwipeTutorialElementsMoved){
			return;
		}
		if(this.gameObject.name == GlobalVars.SWIPE_TUTORIAL_SECOND_ELEMENT_NAME && secondSwipeTutorialElementMoved){
			return;
		}
		if(this.gameObject.name == GlobalVars.POWER_UP_TUTORIAL_SWIPE_NAME && PlayerPrefs.GetInt(GlobalVars.GATHERING_TUTORIAL_WATCHED_POWER_UP) == 0 && this.gameObject.transform.position.x >= -2.8f){
			this.gameObject.GetComponent<ActivatePowerUp>().OnMouseUp();
		}
		currentMousePosition = new Vector3 (Input.mousePosition.x, elementScreenPoint.y, elementScreenPoint.z);
		hoveringMousePosition = Camera.main.ScreenToWorldPoint (currentMousePosition) + cameraOffset;
		if (!GlobalVars.PAUSED && hoveringMousePosition.x <= (LanePositions.GetLanePositions(0)[3].x + LanePositions.GetLaneTolerance(0)) && hoveringMousePosition.x >= (LanePositions.GetLanePositions(0)[0].x - LanePositions.GetLaneTolerance(0))) {
			hoveringMousePosition.y = transform.position.y;
			hoveringMousePosition.z = -1;
			transform.position = Vector3.Lerp (transform.position, hoveringMousePosition, elementMovementSpeed * 10);
			// Redundent code for highlighting lanes.
			if (hoveringMousePosition.x < -4.8) {
			} else if (hoveringMousePosition.x >= -4.8 && hoveringMousePosition.x < -0.5) {
			} else if (hoveringMousePosition.x >= -0.5 && hoveringMousePosition.x < 1.5) {
			} else if (hoveringMousePosition.x > 1.5) {
			}
		}
	}

	//drops the elemnt into a lane after the player raises their finger from the screen.
	public void OnMouseUp(){
		if(this.gameObject.name == GlobalVars.SWIPE_TUTORIAL_SECOND_ELEMENT_NAME && (transform.position.x >= (LanePositions.GetLanePositions(0)[3].x - LanePositions.GetLaneTolerance(0)) && transform.position.x <= (LanePositions.GetLanePositions(0)[3].x + LanePositions.GetLaneTolerance(0))) && PlayerPrefs.GetInt(GlobalVars.GATHERING_TUTORIAL_WATCHED_SWIPE)==0 ){
			isCorrectMove = true;
			secondSwipeTutorialElementMoved = true;
			// Let the PlayTutorial script know that the ring and arrow has to mvoe and that the first element has been moved into the correct position.
			controller.GetComponent<PlayTutorial>().ResetRingAndArrow();
			controller.GetComponent<PlayTutorial>().MovedFirstElementOver();
		}
		if(this.gameObject.name == GlobalVars.SWIPE_TUTORIAL_ELEMENT_NAME && (transform.position.x >= (LanePositions.GetLanePositions(0)[1].x - LanePositions.GetLaneTolerance(0)) && transform.position.x <= (LanePositions.GetLanePositions(0)[1].x + LanePositions.GetLaneTolerance(0))) && PlayerPrefs.GetInt(GlobalVars.GATHERING_TUTORIAL_WATCHED_SWIPE)==0 ){
			isCorrectMove = true;
			firstSwipeTutorialElementsMoved = true;
			// Let the PlayTutorial script know that the ring and arrow has to mvoe and that the first element has been moved into the correct position.
			controller.GetComponent<PlayTutorial>().ResetRingAndArrow();
			controller.GetComponent<PlayTutorial>().MovedFirstElementOver();
		}
		// If the player drags the element to far to the right then reset the element position.
		if((this.gameObject.name == GlobalVars.SWIPE_TUTORIAL_ELEMENT_NAME 
		   || this.gameObject.name == GlobalVars.SWIPE_TUTORIAL_SECOND_ELEMENT_NAME)
		   && (this.gameObject.tag == "Zone2" || this.gameObject.tag == "Zone4") 
		   && (transform.position.x >= -5.73f || transform.position.x <= 3.08f) 
		   && PlayerPrefs.GetInt(GlobalVars.GATHERING_TUTORIAL_WATCHED_SWIPE)==0 
		   && !isCorrectMove){
			// Set the position of the element back to the location from which it was dragged.
			transform.position = new Vector3(-5.2f, 6.5f, 0);
			isCorrectMove = false;

			if (OnTutorialElementMouseAction != null) {
				OnTutorialElementMouseAction(false);
			}
			return;
		}
		// On mouse up set the x position of the element to that of it's closest lane.
		transform.position = new Vector3(lanes[controller.GetComponent<GenerationScript> ().whichLane(this.transform.position.x)].x, transform.position.y, transform.position.z);
		isMouseDragging = false;
		// Disable the ring after the player is finished swiping.
		myRing.enabled = false;
		//makes sure the element updates it lane when it settles in a lane
		GetComponent<MoveElementDown>().UpdateLane();
	}
	//turns off all highlighters if the object is destroyed
	void OnDestroy () {
	}
	// All lanes that the hoveringMousePosition isn't in have their highlight turned off.
	public void setIsLerping(bool answer){
		isLerping = answer;
	}
	//a method that collects the element on tap
	public void collectElement () {
		//finds which lane the zone is in
		int laneIndex = System.Array.IndexOf(GlobalVars.GATHERING_CONTROLLER.elementSprites, element);
		//increases the score
		int points = GlobalVars.GATHERING_ZONES[laneIndex].getCurrentPointValue();
		GlobalVars.GATHERING_CONTROLLER.updateScore(laneIndex, points);
		Destroy(gameObject);
	}
}
