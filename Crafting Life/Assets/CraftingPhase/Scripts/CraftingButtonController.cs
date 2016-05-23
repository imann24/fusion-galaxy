#define DEBUG
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CraftingButtonController : MonoBehaviour {
	//global event for any button press (mostly to play the SFX)
	public delegate void ButtonPressAction();
	public delegate void ExitCraftingAction();
	public delegate void ReadyToEnterGatheringAction();
	public delegate void NotReadyToEnterGatheringAction();
	public delegate void EnteredScreenAction (string screenName);
	public delegate void EnterGatheringAction (params string [] elements);

	public static event ButtonPressAction OnButtonPress;
	public static event ExitCraftingAction OnExitCrafting;
	public static event ReadyToEnterGatheringAction OnReadyToEnterGathering;
	public static event NotReadyToEnterGatheringAction OnNotReadyToEnterGathering;
	public static event EnteredScreenAction OnEnterScreen;
	public static event EnterGatheringAction OnEnterGathering;
	//tunabled variables
	public string craftingInstructions = "Drag Two Elements into the Zones to Combine Them";
	public string gatheringInstructions = "Drag Four Distinct Elments into the Zones to Gather Them";

	//Canvas variables
	public CanvasScaler scaler;
	public ScrollingUIText instructionsController;
	//clicked buttons
	public AudioSource buttonClicked;
	public Sprite[] tierProgressButtons;
	public GameObject gatheringButton;
	public GameObject craftingButton;
	public GameObject gatheringSelector;
	public GameObject craftingSelector;

	//for gathering selector
	private const string noElementString = "none";
	private bool [] gatheringZoneOccupied = {false, false, false, false}; 
	private string [] elementsInDropZones = {noElementString, noElementString, noElementString, noElementString};
	public CaptureScript [] gatheringDropZones;
	private List<CaptureScript> gatheringDropZoneQueue = new List<CaptureScript>();
	private bool readyToEnterGathering = false;
	public Button playGatheringButton;
	public Text instructionScroll;

	//page changer buttons
	public Color activeColor;
	public Color inactiveColor;

	public GameObject page1Button;
	public GameObject page2Button;
	private Image page1ButtonIcon;
	private Image page2ButtonIcon;
	private Text page1ButtonText;
	private Text page2ButtonText;

	//settings menu buttons
	public GameObject musicOnActive;
	public GameObject musicOffActive;
	public GameObject SFXOnActive;
	public GameObject SFXOffActive;

	//the Gathering Planet script
	private RandomGatherPlanet planetScript;
	
	//for loading the scene asynchronolously
	AsyncOperation async;

	//cheats
	private enum Cheat {Reset, UnlockAll, None};
	private Cheat activeCheat;

	void Awake () {
		//global reference to script
		GlobalVars.CRAFTING_BUTTON_CONTROLLER = this;
	}

	// Use this for initialization
	void Start () {
		planetScript = GameObject.FindGameObjectWithTag ("GatherPlanet").GetComponent<RandomGatherPlanet> ();

		//assigns the button references
		page1ButtonIcon = page1Button.GetComponent<Image>();
		page1ButtonText = page1Button.transform.GetChild (0).GetComponent<Text>();
		page2ButtonIcon = page2Button.GetComponent<Image>();
		page2ButtonText = page2Button.transform.GetChild (0).GetComponent<Text>();
		instructionsController.setText (craftingInstructions);

		//sets the music buttons correctly
		bool music;
		musicOnActive.SetActive(music = (PlayerPrefs.GetInt("Music", 1) == 1));
		musicOffActive.SetActive(!music);

		//sets the sfx buttons correctly
		bool sfx;
		SFXOnActive.SetActive(sfx = (PlayerPrefs.GetInt("SFX", 1) == 1));
		SFXOffActive.SetActive(!sfx);

	}
	
	//loads a new scene
	public void loadMode (string sceneName) {
		bool loadingAsync = false;
		if (OnButtonPress != null) {
			OnButtonPress();
		}

		if (CraftingTutorialController.TutorialActive) {
			Utility.Log("Trying to load scene asynchronoushly");
			StartCoroutine(LoadLevelAsync((int) GlobalVars.Scenes.Gathering));
			loadingAsync = true;
		}

		if (sceneName == GlobalVars.GATHERING_BUTTON_NAME) {
			if (!readyToEnterGathering) { //exits the loop if the user doesn't have four elements selected
				return;
			} else {
				for (int i = 0; i < elementsInDropZones.Length; i++) { //sets the elements for gathering
					PlayerPrefs.SetString("ELEMENT" + (i+1).ToString(), elementsInDropZones[i]);
				}

				if (OnEnterGathering != null) {
					OnEnterGathering(elementsInDropZones);
				}
			}
		}

		if (OnExitCrafting != null) {
			Utility.Log("Exiting the crafting mode event");
			OnExitCrafting();
			
			if (sceneName == GlobalVars.GATHERING_BUTTON_NAME && OnEnterScreen != null) {
				OnEnterScreen(GlobalVars.GATHERING_BUTTON_NAME);
			}
		}

		Utility.ShowLoadScreen();

		if (loadingAsync) {
			return;
		}

		if (sceneName == "Gathering") {
			Application.LoadLevel((int)GlobalVars.Scenes.Gathering);
		} else if (sceneName == "Wiki") {
			Application.LoadLevel((int)GlobalVars.Scenes.Wiki);
		} else if (sceneName == "Credits") {
			Application.LoadLevel((int)GlobalVars.Scenes.Credits);
		} else if (sceneName == "SDK" && GlobalVars.MEDICAL_USE) { //loads up the SDK settings
			SDKEventManager.LoadSDKScene();
		}
	}

	//loads the scene asynchronously to give the tutorial complete event time to fire
	IEnumerator LoadLevelAsync(int scene) {
		Debug.LogWarning("ASYNC LOAD STARTED - " +
		                 "DO NOT EXIT PLAY MODE UNTIL SCENE LOADS... UNITY WILL CRASH");
		async = Application.LoadLevelAsync(scene);
		yield return async;
		Utility.Log("Loading complete");
	}
	
	//toggles each of the four drop zones occupied and not depending on the boolean received
	public void toggleZoneReadyGathering (CaptureScript zone) {
		if (gatheringDropZoneQueue.Contains(zone)) {
			gatheringDropZoneQueue.Remove(zone);
		}
		gatheringDropZoneQueue.Add(zone);
		int index;
		gatheringZoneOccupied[index = zone.getZoneNumber()-1] = zone.isElementCaptured();
		bool readyToEnterGathering = zone.isElementCaptured();
		if (zone.isElementCaptured()) { //checks whether the rest of the zones are captured if this zone is
			//sets the name to pass on to gathering mode
			elementsInDropZones[index] = zone.myElementGameObject.name;
			for (int i = 0; i < gatheringZoneOccupied.Length; i++) {
				if (elementsInDropZones[i] == elementsInDropZones[index] && i != index) { //if there's already an element that is the same, delete it from the first drop zone
					gatheringDropZones[i].OnMouseDown();
					readyToEnterGathering = false;
				} 
				if (!gatheringZoneOccupied[i]) { //if a zone is unoccpied
					readyToEnterGathering = false;
				}
			}

		} else {
			elementsInDropZones[index] = noElementString;
		}
		//toggles the play button on and off
		toggleGatheringPlayButton(readyToEnterGathering);
		setGatheringPlanet(readyToEnterGathering);
	}

	public void enqueueElement (string element) {
		for (int i = 0; i < elementsInDropZones.Length; i++) {
			if (element == elementsInDropZones[i]) {
				return;
			}
		}

		bool elementPlaced = false; 

		for (int i = 0; i < gatheringDropZones.Length; i++) {
#if DEBUG
			Debug.Log("Gathering drop zone " + i + " has an element: " + gatheringDropZones[i].isElementCaptured());
#endif
			if (!gatheringDropZones[i].isElementCaptured()) {
				gatheringDropZones[i].captureElement(GlobalVars.ELEMENT_SPRITES[element]);
				elementPlaced = true;
				break;
			}
		}

		if (!elementPlaced) {
			gatheringDropZoneQueue[0].captureElement(GlobalVars.ELEMENT_SPRITES[element]);
		}

		//checks whether the game is ready to enter gathering
		toggleGatheringPlayButton(readyToEnterGathering);
		setGatheringPlanet(readyToEnterGathering);
	}

	public void drag (GameObject g) {
			Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x*scaler.scaleFactor*scaler.referencePixelsPerUnit, Input.mousePosition.y*scaler.scaleFactor*scaler.referencePixelsPerUnit, 0));
	}

	public void hide (GameObject g) {
		g.SetActive(false);
	}

	//turns on and off the button icon when there isn't a second page
	public void togglePageButton (int pageNumber, bool active) {
		if (pageNumber == 1) {
			page1Button.SetActive(active);
		} else {
			page1Button.SetActive(active);
			page2Button.SetActive(active);
		}
	}

	//turns the page buttons on and of
	public void setButtonActive (int pageNumber) {
		Color button1Color = Color.white;
		Color button2Color = Color.white;
		if (pageNumber == 1) {
			button1Color = activeColor;
			button2Color = inactiveColor;
		} else if (pageNumber == 2) {
			button1Color = inactiveColor;
			button2Color = activeColor;
		}
		page1ButtonText.color = button1Color;
		page1ButtonIcon.color = button1Color;
		if (page2Button.activeSelf) {
			page2ButtonText.color = button2Color;
			page2ButtonIcon.color = button2Color;
		}
	}

	//sets the scrolling text based on the game objects position
	public void setInstructionScroll (GameObject scrollBar) {
		if (scrollBar.GetComponent<Scrollbar>().value > 0.5f) {
			instructionsController.setText(gatheringInstructions + "     ");
		} else {
			instructionsController.setText(craftingInstructions + "     ");
		}
	}

	//toggles the music on and off
	public void toggleMuteMusic (bool muted) {
		AudioManager.instance.toggleMuteMusic(muted);
	}

	//toggles the sfx on and off
	public void toggleMuteSFX (bool muted) {
		AudioManager.instance.toggleMuteSFX(muted);
	}


	//sets the cheat to be executed on click
	public void setCheat (string cheat) {
		if (cheat == "UnlockAll") {
			activeCheat = Cheat.UnlockAll;
		} else if (cheat == "Reset") {
			activeCheat = Cheat.Reset;
		}
	}
	
	//executes a cheat upon button click of confirm
	public void executeCheat () {
		if (activeCheat == Cheat.Reset) {
			deleteAllProgress();
		} else if (activeCheat == Cheat.UnlockAll) {
			unlockAllElements();
		}
	}

	//cancels the cheat to be executed
	public void cancelCheat () {
		activeCheat = Cheat.None;
	}

	//deletes all the player's progress: reset button
	public void deleteAllProgress () {
		Cheats.LockAllElements();
		GlobalVars.CRAFTING_CONTROLLER.loadTier(0, true);
	}

	public void unlockAllElements () {
		Cheats.UnlockAllElements();
		GlobalVars.CRAFTING_CONTROLLER.loadTier(0, true);
	}

	//plays the button click sound
	public void simpleButtonClick () {
		if (OnButtonPress != null) {
			OnButtonPress();
		}
	}

	private void toggleGatheringPlayButton (bool active) {
		readyToEnterGathering = active;

		//toggles the play button on and off if it is necessary
		if (playGatheringButton.interactable != active) {
			playGatheringButton.interactable = active;

			//fades the other elements of the play button in and out with it
			if (active && OnReadyToEnterGathering != null) {
				OnReadyToEnterGathering();
			} else if (!active && OnReadyToEnterGathering != null) {
				OnNotReadyToEnterGathering();
			}
		}
	}

	private void setGatheringPlanet (bool active) {
		if (active) {
			planetScript.setPlanet (elementsInDropZones);
		} else {
			planetScript.setNoPlanet();
		}
	}
	
	public void toggleCrafting (bool active) {
		if (active) {
			ScrollBarDisplay.mode = ScrollBarDisplay.Mode.Crafting;
		} else {
			ScrollBarDisplay.mode = ScrollBarDisplay.Mode.Gathering;
		}
	}

	public void EnterScreen (string screenName) {
		if (OnEnterScreen != null) {
			OnEnterScreen(screenName);
		}
	}
}
