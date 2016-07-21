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

		if (!Utility.PlayerPrefIntToBool(GlobalVars.CRAFTING_TUTORIAL_KEY)) {
			checkForSufficientBaseElements();
		}
		//Utility.ShowLoadScreen();
		//Application.LoadLevel((int)GlobalVars.Scenes.Crafting);
	}

	void checkForSufficientBaseElements () {
		int minOfEachElement = 1;
		for (int i = 0; i < GlobalVars.BASE_ELEMENT_NAMES.Length; i++) {
			if (PlayerPrefs.GetInt(GlobalVars.BASE_ELEMENT_NAMES[i]) == 0) {
				PlayerPrefs.SetInt(GlobalVars.BASE_ELEMENT_NAMES[i], minOfEachElement);
			}
		}
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

	public void toggleVibrateEnabled (bool isEnabled) {
		SettingsUtil.ToggleVibrate(isEnabled);
	}
}
