/*
 * Sends the MixPanel events for the game
 * All the game specific MixPanel events are kept here
 * All of the generic MixPanel calls are kept in Mixpanel.cs
 */

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

// Mixpanel events are sent through this class
// Note that this class currently uses PlayerPrefs for all saving - it is not recommended to do this for any user sensitive data
public class MixpanelController : MonoBehaviour
{
	//so that timer doesn't count time spent in start screen
	private bool inStartScreen = true;
	private bool loadedFromStartScreen = true;
	private bool suppressOnLevelLoadFunction = false;

	// Assign to this in the inspector
	// TODO: keep this updated for each build to retain accurate analytics
	public string versionNumber;

	// Singleton implementation
	public static MixpanelController instance;

	//analytics variables for crafting mode
	public static int ElementsUnlockedInSession = 0;
	public static int TiersUnlockedInSession = 0;
	public static int TiersCompletedInSession = 0;
	public static int ElementsCraftedInSession = 0;
	public static int HintsBoughtInSession = 0;
	public static int PowerUpUpgradesBoughtInSession = 0;

	void Awake()
	{
		// singleton setup - don't destroy this when loading
		if(instance == null)
		{
			instance = this;
			DontDestroyOnLoad(this.gameObject);
			//disables the in start screen bool if the game loads from a different scene
			if (inStartScreen && Application.loadedLevel != (int) GlobalVars.Scenes.Start) { 
				inStartScreen = false;
			} 


			// Subscribes to all events
			LinkToEvents();
		}
		else
		{
			suppressOnLevelLoadFunction = true;
			GameObject.Destroy(this.gameObject);
		}

	}

	void Start()
	{
		// Set Mixpanel Token: this is project specific
		Mixpanel.Token = "476095c9962954c0f3ebf45b819df7da";

		// Not sure if this is required or not, clears all super properties in the SuperProperties dictionary
		Mixpanel.SuperProperties.Clear();

		// Quick way to check if this is the first usage
		// Generally put information you care about only recording once in here (install date, version on install, etc)
		// See Mixpanel.SendPeople and look into set_once
		if(PlayerPrefs.GetInt("FirstUse") == 0)
		{	
			PlayerPrefs.SetInt("FirstUse", 1);
			FirstUse(DateTime.Now.ToString(), Mixpanel.DistinctID);
		}

		// Note that versionNumber is not static, this enables it to be set through the inspector - but also means we have to pull the instance
		AddSuperProperties ("Version", MixpanelController.instance.versionNumber);
	}

	//updates the playerprefs count of time to track between game sessions
	void OnLevelWasLoaded (int level) {
		if (!inStartScreen && !suppressOnLevelLoadFunction) { //prevents time from being added during start screen
			Utility.IncreasePlayerPrefValue("TotalPlayTime", Time.timeSinceLevelLoad);

			//increases the playcounts
			if (Application.loadedLevel == (int) GlobalVars.Scenes.Crafting) {
				if (!loadedFromStartScreen) { //increases the time spent in gathering
					Utility.IncreasePlayerPrefValue("GatheringPlayTime", Time.timeSinceLevelLoad);
				}
			} else if (Application.loadedLevel == (int) GlobalVars.Scenes.Gathering) {
				if (!loadedFromStartScreen) { //increases the time spent in crafting
					Utility.IncreasePlayerPrefValue("CraftingPlayTime", Time.timeSinceLevelLoad);
				}
			}
			loadedFromStartScreen = false;
		} else {
			inStartScreen = false;
		}
	}

	void Destroy () {
		// Unsubscribes from event calls
		UnlinkFromEvents ();
	}
	
	//establishes the in game event references that send MixPanel events
	void LinkToEvents () {
		MainMenuController.OnEnterMenu += Progress;
		TimeUpScreen.OnEndGathering += GatheringPlaySession;
		//CraftingControl.OnElementCreated += ElementCreated;
		CraftingControl.OnElementDiscovered += ElementDiscovered;

		//TUTORIAL CALLS
		//tutorial completion calls
		CraftingTutorialController.OnBuyHintTutorialComplete += BuyHintTutorialComplete;
		CraftingTutorialController.OnBuyPowerUpUpgradeTutorialComplete += BuyPowerUpUpgradeTutorialCompelte;
		CraftingTutorialController.OnElementsDraggedIntoGatheringTutorialComplete += LaunchGatheringTutorialComplete;
		CraftingTutorialController.OnCraftingModeTutorialComplete += CraftingTutorialComplete;
		CraftingTutorialController.OnTierSwitchingTutorialComplete += TierSwitchingTutorialComplete;
		PlayTutorial.onPowerUpTutorialComplete += UsePowerUpsTutorialComplete;
		PlayTutorial.onSwipeTutorialComplete += SwipeElementTutorialComplete;

		//in-progress tutorial calls
		CraftingButtonController.OnReadyToEnterGathering += DraggedElementsInForLaunchGatheringTutorial;
		CraftingButtonController.OnEnterScreen += EnterScreenEventHandler;
		CraftingControl.OnReadyToCraft += DraggedInTwoElements;

		//button presses
		ButtonController.OnExitGathering += HandleOnExitGathering;
		TogglePause.OnPause += HandleOnPause;
		StartScreenButtonController.OnLoadScene += TappedSplashScreenToEnter;

		//crafting actions
		CraftingControl.OnElementCreated += HandleOnElementCreated;
		CraftingControl.OnTierUnlocked += HandleOnTierUnlocked;
		CraftingControl.OnTierCompleted += HandleOnTierCompleted;
		BuyUpgrade.OnPowerUpUpgrade += HandleOnPowerUpUpgrade;
		PurchaseHint.OnPurchaseHint += HandleOnPurchaseHint;
		CraftingButtonController.OnEnterGathering += HandleOnEnterGathering;
	}

	// Takes the enter gathering event and creates a dictionary
	// Makes the data from the event call sendeable as event
	void HandleOnEnterGathering (string[] elements)
	{
		// calls the event for a gathering play session
		CraftingPlaySession();

		// Sends the event to enter gathering
		Dictionary<string, object> elementDictionary = new Dictionary<string, object>();
		for (int i = 0; i < elements.Length; i++) {
			elementDictionary.Add("Element " + (i+1), elements[i]);
		}

		// Calls the event for entering gathering
		BeganGatheringSession(elementDictionary);
	}

	// Increases the count for the play session and calls the event
	// The count is used in another event that's called when the user exits the scene
	void HandleOnTierCompleted (int tierNumber) {
		TiersCompletedInSession++;
		TierCompleted(tierNumber);
	}

	// Called when the player exits gathering
	void HandleOnExitGathering (GlobalVars.Scenes toScene) {
		// If the playering is going to crafting
		if (toScene == GlobalVars.Scenes.Crafting) {
			TappedTheReturnHomeFromGatheringButton();
		} 

		// If the player is playing gathering again
		else if (toScene == GlobalVars.Scenes.Gathering) {
			TappedTheReplayGatheringButton();
		}
	}
	
	// When the player pauses the game
	void HandleOnPause (bool paused) {
		if (paused) {
			TappedThePauseGatheringButton();
		}
	}

	//unlinks from the game event references
	void UnlinkFromEvents () {
		MainMenuController.OnEnterMenu -= Progress;
		TimeUpScreen.OnEndGathering -= GatheringPlaySession;
		//CraftingControl.OnElementCreated -= ElementCreated;
		CraftingControl.OnElementDiscovered -= ElementDiscovered;

		//tutorial calls
		CraftingTutorialController.OnBuyHintTutorialComplete -= BuyHintTutorialComplete;
		CraftingTutorialController.OnBuyPowerUpUpgradeTutorialComplete -= BuyPowerUpUpgradeTutorialCompelte;
		CraftingTutorialController.OnElementsDraggedIntoGatheringTutorialComplete -= LaunchGatheringTutorialComplete;
		CraftingTutorialController.OnCraftingModeTutorialComplete -= CraftingTutorialComplete;
		CraftingTutorialController.OnTierSwitchingTutorialComplete -= TierSwitchingTutorialComplete;
		PlayTutorial.onPowerUpTutorialComplete -= UsePowerUpsTutorialComplete;
		PlayTutorial.onSwipeTutorialComplete -= SwipeElementTutorialComplete;

		//in-progress tutorial calls
		CraftingButtonController.OnReadyToEnterGathering -= DraggedElementsInForLaunchGatheringTutorial;
		CraftingButtonController.OnEnterScreen -= EnterScreenEventHandler;
		CraftingControl.OnReadyToCraft -= DraggedInTwoElements;
		
		//button presses
		ButtonController.OnExitGathering -= HandleOnExitGathering;
		TogglePause.OnPause -= HandleOnPause;
		StartScreenButtonController.OnLoadScene -= TappedSplashScreenToEnter;

		//crafting actions
		CraftingControl.OnElementCreated -= HandleOnElementCreated;
		CraftingControl.OnTierUnlocked -= HandleOnTierUnlocked;
		CraftingControl.OnTierCompleted -= HandleOnTierCompleted;
		BuyUpgrade.OnPowerUpUpgrade -= HandleOnPowerUpUpgrade;
		PurchaseHint.OnPurchaseHint -= HandleOnPurchaseHint;
	}
	
	// When the player unlocks a new tier
	void HandleOnTierUnlocked (int tierNumber) {
		// Calls the event and counts number in session
		TiersUnlockedInSession++;
		TierUnlocked(tierNumber);
	}

	// When the player buys a hint
	void HandleOnPurchaseHint (string unlockedElementName) {
		// Calls the event and counts number in session
		HintsBoughtInSession++;
		HintPurchased(unlockedElementName);
	}

	// When the player buys an upgrade
	void HandleOnPowerUpUpgrade (string powerupName, int powerUpLevel) {
		// Calls an event and counts the number in a session
		PowerUpUpgradesBoughtInSession++;
		PowerUpUpgraded(powerupName, powerUpLevel);
	}

	// When the player creates a new element
	void HandleOnElementCreated (string newElement, string parent1, string parent2, bool isNew) {
		// Used to count the number of elements for an event call
		ElementsCraftedInSession++;
	}

	//resets the variables for a session
	//for use after they've been logged
	static void ResetSessionVariables (GlobalVars.Scenes scene) {
		if (scene == GlobalVars.Scenes.Crafting) {
			ElementsUnlockedInSession = 0;
			TiersUnlockedInSession = 0;
			TiersCompletedInSession = 0;
			ElementsCraftedInSession = 0;
			HintsBoughtInSession = 0;
			PowerUpUpgradesBoughtInSession = 0;
		}
	}

	#region Example event 
	// Example event
	// Parameters for the function are a handy way to have an event take in multiple Properties
	public static void GamePlay(bool isReplay)
	{
		Mixpanel.SendEvent("Game Play", new Dictionary<string, object>{
			{"Replay", isReplay},
			});
	}
	#endregion 

	#region Crafting Life Events

	//tracking overall player progress: sent every time the player loads up crafting
	public static void Progress () {
		Mixpanel.SendEvent("Progress", new Dictionary<string, object> {
			{"Elements Unlocked", GlobalVars.NUMBER_ELEMENTS_UNLOCKED},
			{"Highest Tier", Utility.HighestTierUnlocked()}
		});
	}

	//event sent every time the player completes a session in gathering
	public static void GatheringPlaySession () {
		Mixpanel.SendEvent("Gathering Play Session", new Dictionary<string, object> {
			{"Zone 1 Element", PlayerPrefs.GetString("ELEMENT1")},
			{"Zone 1 Score", GlobalVars.SCORES[0]},
			{"Zone 2 Element", PlayerPrefs.GetString("ELEMENT2")},
			{"Zone 2 Score", GlobalVars.SCORES[1]},
			{"Zone 3 Element", PlayerPrefs.GetString("ELEMENT3")},
			{"Zone 3 Score", GlobalVars.SCORES[2]},
			{"Zone 4 Element", PlayerPrefs.GetString("ELEMENT4")},
			{"Zone 4 Score", GlobalVars.SCORES[3]},
			{"Powerup Spawn Count", GlobalVars.POWERUP_SPAWN_COUNT},
			{"Powerup Use Count", GlobalVars.POWERUP_USE_COUNT},
			{"Play Time", GlobalVars.GATHERING_PLAYTIME + " Seconds"},
			{"Total Number Retained", GlobalVars.SCORES[0] + GlobalVars.SCORES[1] + GlobalVars.SCORES[2] + GlobalVars.SCORES[3]},
			{"Total Number Missed", GlobalVars.MISSED},
			{"Initial Spawn Rate", GlobalVars.GATHERING_CONTROLLER.initialCreationFrequency},
			{"Final Spawn Rate", GlobalVars.GATHERING_CONTROLLER.creationFrequency},
			{"Initial Fall Speed", GlobalVars.GATHERING_CONTROLLER.initialElementMovementSpeed},
			{"Final Fall Speed", GlobalVars.GATHERING_CONTROLLER.elementMovementSpeed}
		});
	}

	//not currently in use, sent every time a player crafts an element
	public static void ElementCreated (string newElement, string parent1, string parent2, bool isNew) {
		Mixpanel.SendEvent("Element Created", new Dictionary<string, object> {
			{"Element", newElement},
			{"Combination", parent1 + " + " + parent2},
			{"Is New", isNew}
		});
	}

	//sent every time the player discovers a new element
	public static void ElementDiscovered (string newElement) {
		ElementsUnlockedInSession++;
		Mixpanel.SendEvent("Element Discovered", new Dictionary<string, object> {
			{"Element", newElement}
		});

		//calls the tutorial event, if the tutorial is currently active
		if (CraftingTutorialController.CurrentTutorial == MainMenuController.Tutorial.Crafting && 
		    CraftingTutorialController.TutorialActive) {
			CraftedFirstElement(newElement);
		}
	}

	//sent when the player exits crafting
	public static void CraftingPlaySession () {
		Mixpanel.SendEvent("Crafting Play Session", new Dictionary<string, object> {
			{"Session Play Time", Utility.SecondsToTimeString(Time.timeSinceLevelLoad)},
			{"Elements Unlocked in Session", ElementsUnlockedInSession},
			{"Tiers Unlocked in Session", TiersUnlockedInSession},
			{"Tiers Completed in Session", TiersCompletedInSession},
			{"Elements Crafted In Session", ElementsCraftedInSession},
			{"Hints Bought in Session", HintsBoughtInSession},
			{"Power Up Upgrades Bought in Session", PowerUpUpgradesBoughtInSession}
		});

		ResetSessionVariables(GlobalVars.Scenes.Crafting);
	}

	//event for when you unlock a new tier
	public static void TierUnlocked (int tierNumber ) {
		Mixpanel.SendEvent("Tier Unlocked", new Dictionary<string, object> {
			{"Tier Number", tierNumber}
		});
	}

	//event for when you complete a new tier
	public static void TierCompleted (int tierNumber ) {
		Mixpanel.SendEvent("Tier Completed", new Dictionary<string, object> {
			{"Tier Number", tierNumber}
		});
	}

	//event for when you purchase a powerup upgrade
	public static void PowerUpUpgraded (string powerUpName, int powerUpLevel) {
		Mixpanel.SendEvent("PowerUp Upgraded", new Dictionary<string, object> {
			{"Power Up", powerUpName},
			{"Level", powerUpLevel}
		});
	}

	//event for when you purchase a hint
	public static void HintPurchased (string elementName) {
		Mixpanel.SendEvent("Hint Purchased", new Dictionary<string, object> {
			{"Element", elementName}
		});
	}

	//event for entering the gathering mode
	public static void BeganGatheringSession (Dictionary<string, object> elements) {
		Mixpanel.SendEvent("Gathering Session Launched", elements); 
	}

	//event for using a powerup
	public static void PowerUpUsed (string powerUpName, int powerUpLevel) {
		Mixpanel.SendEvent("Power Up Used", new Dictionary<string, object> {
			{"Power Up", powerUpName},
			{"Level", powerUpLevel}
		});
	}

	#region TUTORIAL
	//for the MAIN MENU HUB
	public static void EnterScreenEventHandler (string screenName) {
		if (screenName == GlobalVars.CRAFTING_BUTTON_NAME) {
			EnteredCraftingMenu();
		} else if (screenName == GlobalVars.UPGRADE_POWERUP_BUTTON_NAME) {
			EnteredPowerUpMenu();
		} else if (screenName == GlobalVars.GATHERING_BUTTON_NAME) {
			TappedTheEnterGatheringButton();
		}
	}

	//when the player drags four elments in and the game is ready to launch
	public static void DraggedElementsInForLaunchGatheringTutorial () {
		if (CraftingTutorialController.CurrentTutorial == MainMenuController.Tutorial.Gathering && 
		    CraftingTutorialController.TutorialActive) {
			Mixpanel.SendEvent("Dragged Elements in For Launch Gathering Mission Tutorial", new Dictionary<string, object>());
		}
	}

	//when the player launches the game into gather for the tutorial
	public static void LaunchGatheringTutorialComplete (float time) {
		Mixpanel.SendEvent("Launch Gathering Mission Tutorial Complete", new Dictionary<string, object> {
			{"Completion Time", time}
		});
	}

	//when the player opens the crafting menu for the tutorial
	public static void EnteredCraftingMenu () {
		if (CraftingTutorialController.CurrentTutorial == MainMenuController.Tutorial.Crafting && 
		    CraftingTutorialController.TutorialActive) {
			Mixpanel.SendEvent("Opened Crafting Menu For Crafting Tutorial", new Dictionary<string, object>());
		}
	}

	//when the player drags in two elements for the tutorial
	public static void DraggedInTwoElements (bool ready) {
		if (ready && 
		    CraftingTutorialController.CurrentTutorial == MainMenuController.Tutorial.Crafting &&
		    CraftingTutorialController.TutorialActive) {
			Mixpanel.SendEvent("Dragged Two Elements In For Crafting Tutorial", new Dictionary<string, object>());
		}
	}

	//when the players crafts their first element for the tutorial
	public static void CraftedFirstElement (string newElement) {
		Mixpanel.SendEvent("Crafting An Element For Crafting Tutorial", 
		                   new Dictionary<string, object> {
			{"Element", newElement}
		});
	}

	//when the player successfully crafts their first element
	public static void CraftingTutorialComplete (float time) {
		Mixpanel.SendEvent("Crafting Tutorial Complete", new Dictionary<string, object> {
			{"Completion Time", time}
		});
	}

	//when the player buys/chooses not to buy their first hint
	public static void BuyHintTutorialComplete (float time) {
		Mixpanel.SendEvent("Buy Hint Tutorial Tutorial Complete", new Dictionary<string, object> {
			{"Completion Time", time}
		});
	}

	//when the player enters the powerup menu for the tutorial 
	public static void EnteredPowerUpMenu () {
		if (CraftingTutorialController.CurrentTutorial == MainMenuController.Tutorial.UpgradePowerup && 
		    CraftingTutorialController.TutorialActive) {
			Mixpanel.SendEvent("Opened PowerUp Menu For Buy Power Up Upgrade Tutorial", new Dictionary<string, object>());
		}
	}

	//when the player buys/chooses not to buy their first powerup upgrade
	public static void BuyPowerUpUpgradeTutorialCompelte (float time) {
		Mixpanel.SendEvent("Buy Power Up Upgrade Tutorial Complete", new Dictionary<string, object> {
			{"Completion Time", time}
		});
	}


	//when the player switches tiers for the first time
	public static void TierSwitchingTutorialComplete (float time) {
		Mixpanel.SendEvent("Tier Switching Tutorial Complete", new Dictionary<string, object> {
			{"Completion Time", time}
		});
	}

	//for the GATHERING GAME

	//when the user completes the dragging elements tutorial
	public static void SwipeElementTutorialComplete (float time) {
		Mixpanel.SendEvent("Swipe Element Tutorial Complete", new Dictionary<string, object> {
			{"Completion Time", time}
		});
	}

	//when the user completes the using powerups tutorial
	public static void UsePowerUpsTutorialComplete (float time) {
		Mixpanel.SendEvent("Using powerups Tutorial Complete", new Dictionary<string, object> {
			{"Completion Time", time}
		});
	}
	#endregion

	#region BUTTON PRESSES

	/// <summary>
	/// Tapped the splash screen to enter.
	/// </summary>
	public static void TappedSplashScreenToEnter (GlobalVars.Scenes toScene) {
		Mixpanel.SendEvent("Tapped The Splash Screen To Enter Game", new Dictionary<string, object>());
	}

	
	/// <summary>
	/// Tapped the pause gathering button.
	/// </summary>
	public static void TappedThePauseGatheringButton () {
		Mixpanel.SendEvent("Tapped The Pause Gathering Button", new Dictionary<string, object>());
	}


	/// <summary>
	/// Tapped the replay gathering button.
	/// </summary>
	public static void TappedTheReplayGatheringButton () {
		Mixpanel.SendEvent("Tapped The Replay Gathering Button", new Dictionary<string, object>());
	}

	/// <summary>
	/// Tapped the return home from gathering button.
	/// </summary>
	public static void TappedTheReturnHomeFromGatheringButton () {
		Mixpanel.SendEvent("Tapped The Return Home From Gathering Button", new Dictionary<string, object>());
	}

	/// <summary>
	/// Tapped the enter gathering button.
	/// </summary>
	public static void TappedTheEnterGatheringButton () {
		Mixpanel.SendEvent("Tapped The Enter Gathering Button", new Dictionary<string, object>());
	}
	#endregion

	#endregion


	#region People Properties

	// Exampe people property
	// Common setup for the first use
	private static void FirstUse(string date, string distinct_id)
	{
		Mixpanel.SendPeople(new Dictionary<string ,object>{
			{"First Use", date},
			{"distinct_id", distinct_id},
		}, "set_once");
	}

	#endregion


	#region Super Properties
	//Add or Remove SuperProperties

	static public void AddSuperProperties(string propertyName, string propertyValue)
	{
		Mixpanel.SuperProperties.Add(propertyName, propertyValue);
	}

	static public void RemoveSuperProperties(string property)
	{
		Mixpanel.SuperProperties.Remove(property);
	}
	#endregion

	
	#region Helper Functions
	// Converts a float into a string that fits the format Minutes:Seconds:MilliSeconds
	public static string ConvertFloatToTimeString(float _time)
	{
		TimeSpan time = TimeSpan.FromSeconds(_time);
		return string.Format("{0:D2}:{1:D2}:{2:D2}", time.Minutes, time.Seconds, time.Milliseconds);
	}
	#endregion
}