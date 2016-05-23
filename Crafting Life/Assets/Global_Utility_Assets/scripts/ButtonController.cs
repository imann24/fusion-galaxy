using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//A script to control the buttons found in the gathering and tech tree scenes
//NOTE: Crafting and Start have their own button controller scripts: CraftingButtonController and StartButtonCtroller
public class ButtonController : MonoBehaviour {

	//events
	public delegate void ButtonPressAction();
	public delegate void ExitGatheringAction(GlobalVars.Scenes toScene);

	public static event ButtonPressAction OnButtonPress;
	public static event ExitGatheringAction OnExitGathering;

	//GameObject components
	private Transform instructionScreenController;
	private SpriteRenderer buttonUnclickedSprite;
	private SpriteRenderer buttonClickedSprite;

	//Buttons
	public GameObject playButton;
	public GameObject menuButton;
	public GameObject pauseBackground;
	public GameObject pauseButton;

	// Use this for initialization
	void Start () {
		//adds an event call to exit the game on invalid game event
		if (GlobalVars.MEDICAL_USE) {
			SDKEventManager.OnInvalidGame += loadSDKSettings;
		}
	}

	void OnDestroy () {
		//removes the event call to exit the game on invalid game event
		if (GlobalVars.MEDICAL_USE) {
			SDKEventManager.OnInvalidGame -= loadSDKSettings;
		}
	}

	//starts gathering mode
	public void playGathering () {
		if (OnButtonPress != null) {
			OnButtonPress();
		}
		GlobalVars.PAUSED = false;
		playButton.SetActive(false);
		menuButton.SetActive(false);
		pauseBackground.SetActive (false);
		pauseButton.SetActive (true);
	}

	//loads crafting mode
	public void loadCrafting () {
		//sends the event for a button press
		if (OnButtonPress != null) {
			OnButtonPress();
		}

		//calls the exit event
		if (OnExitGathering!=null) {
			OnExitGathering(GlobalVars.Scenes.Crafting);
		}

		Utility.ShowLoadScreen ();
		Application.LoadLevel ((int)GlobalVars.Scenes.Crafting);
		//Utility.ShowLoadScreen();
		//Application.LoadLevel((int)GlobalVars.Scenes.Crafting);
	}

	//loads gathering mode
	public void loadGathering () {
		//sends the event for a button press
		if (OnButtonPress != null) {
			OnButtonPress();
		}

		//calls the exit event
		if (OnExitGathering!=null) {
			OnExitGathering(GlobalVars.Scenes.Gathering);
		}

		Utility.ShowLoadScreen();
		Application.LoadLevel((int)GlobalVars.Scenes.Gathering);
	}


	//loads the SDK Settings menu
	public void loadSDKSettings () {
		//sends the event for a button press
		if (OnButtonPress != null) {
			OnButtonPress();
		}

		//shows the load screen
		Utility.ShowLoadScreen();

		SDKEventManager.LoadSDKScene();
	}
}
