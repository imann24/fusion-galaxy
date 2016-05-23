/*
 * Calls the events to launch the tutorials in the crafing mode
 * Sends event calls with the completion time to MixPanel Analytics
 * Has a mask to cover the UI components not used in the tutorial: 
 * the mask sometimes uses the BlockRaycasts feature of the CanvasGroup component to disable clicks on non-tutorial components
 * Runs off events called by MainMenuController and an enum system of tutorials in that script
 */

//#define DEBUG
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

//Script to run the tutorial in crafting
public class CraftingTutorialController : MonoBehaviour {
	//singleton implementation
	public static CraftingTutorialController Instance;
	//bool for whether there is a tutorial active
	public static bool TutorialActive;

	//event calls
	public delegate void BeginTutorial ();
	public delegate void TutorialCompleted (float completionTime);

	public static event BeginTutorial OnElementsDraggedIntoGatheringTutorialBegan;
	public static event BeginTutorial OnCraftingModeTutorialBegan;
	public static event BeginTutorial OnBuyHintTutorialBegan;
	public static event BeginTutorial OnBuyPowerUpUpgradeTutorialBegan;
	public static event BeginTutorial OnTierSwitchingTutorialBegan;

	public static event TutorialCompleted OnElementsDraggedIntoGatheringTutorialComplete;
	public static event TutorialCompleted OnCraftingModeTutorialComplete;
	public static event TutorialCompleted OnBuyHintTutorialComplete;
	public static event TutorialCompleted OnBuyPowerUpUpgradeTutorialComplete;
	public static event TutorialCompleted OnTierSwitchingTutorialComplete;
	
	public int ChildrenBeforeSlides = 1;
	public int SlideIndex;
	public int firstGatheringSlide;
	public int launchMissionSlide;

	private bool tutorialHasEnded;

	private static Image Mask;
	private static Button MaskButton;
	private static CanvasGroup MaskCanvasGroup;

	public static MainMenuController.Tutorial CurrentTutorial = MainMenuController.Tutorial.None;

	//for the tutorial message board
	static CanvasGroup TutorialMessageBoardCanvasGroup;
	static Text TutorialMessageBoardText;

	void Awake () {
		Instance = this;

		InitializeTutorialMessageBoard();
		SubscribeEvents();

#if DEBUG

		Debug.Log("Unlocking first tier and increasing all elements");
		Cheats.IncreaseAllElements(50);
		Cheats.UnlockTier(1);

		for (int i = 0; i < GlobalVars.AllCraftingModeTutorials.Length; i++) {
			Utility.SetPlayerPrefIntAsBool(GlobalVars.AllCraftingModeTutorials[i], false);
		}

		Utility.SetPlayerPrefIntAsBool(GlobalVars.ELEMENTS_DRAGGED_TUTORIAL_KEY, true);
		Utility.SetPlayerPrefIntAsBool(GlobalVars.CRAFTING_TUTORIAL_KEY, true);
		//Utility.SetPlayerPrefIntAsBool(GlobalVars.BUY_HINT_TUTORIAL_KEY, true);
		//Utility.SetPlayerPrefIntAsBool(GlobalVars.UPGRADE_POWERUP_TUTORIAL_KEY, true);
		//Utility.SetPlayerPrefIntAsBool(GlobalVars.TIER_SWITCH_TUTORIAL_KEY, true);
		PowerUp.ResetPowerUpLevel("LaneConversion");


		for (int i = 0; i < 4; i++) {
			PlayerPrefs.SetInt(GeneratePowerUpList.FirstPowerUpElements[i], GeneratePowerUpList.FirstPowerUpCosts[i]);
		}
#endif
		//establishes the reference to the mask component
		foreach (Image image in GetComponentsInChildren<Image>()) {
			if (image.gameObject.name == "Mask") {
				Mask = image;
				MaskButton = image.transform.GetComponent<Button>();
				MaskCanvasGroup = image.transform.GetComponent<CanvasGroup>();
			}
		}
	}
	
	void OnDestroy () {
		UnsubscribeEvents();
	}


	/// <summary>
	/// Exectes the tutorial. And times how long it takes the user to complete
	/// </summary>
	/// <param name="tutorial">The tutorial that is being run.</param>
	public void ExecuteTutorial (BeginTutorial tutorial, MainMenuController.Tutorial tutorialEnum) {
		if (tutorial == null) {
			return;
		} else {
			tutorial();
		}

		//turns on the mask to cover the game
		ToggleMask(true);

		tutorialHasEnded = false;
		TutorialActive = true;

		//sets the enum to track which tutorial the script is executing
		CurrentTutorial = tutorialEnum;

		StartCoroutine (TimeTutorialCompletion(tutorial, tutorialEnum));
	}

	/// <summary>
	/// Ends the tutorial.
	/// </summary>
	public void EndTutorial () {
		//turns of the mask covering the scene
		Utility.Log("Ending the tutorial");
		ToggleMask(false);
		TutorialActive = false;
		tutorialHasEnded = true;
	}

	//ends the tutorial on tap
	public void EndTutorialOnTap () {
		if (CurrentTutorial == MainMenuController.Tutorial.BuyHint) {
			EndBuyHintTutorial("none");
		} else if (CurrentTutorial == MainMenuController.Tutorial.UpgradePowerup) {
			EndUpgradePowerupTutorial("none", 0);
		}

		EndTutorial();
	}


	/// <summary>
	/// Times the tutorial completion.
	/// </summary>
	/// <returns>The tutorial completion.</returns>
	/// <param name="tutorial">The tutorial that was completed.</param>
	private IEnumerator TimeTutorialCompletion(BeginTutorial tutorial, MainMenuController.Tutorial tutorialEnum) { 
		float timeInTutorial = 0;
		while (!tutorialHasEnded) {
			timeInTutorial += Time.deltaTime;
			yield return new WaitForFixedUpdate();
		}
#if DEBUG
		Debug.Log("This tutorial took " + timeInTutorial + " to complete");
#endif
		SetTutorialComplete(tutorialEnum);
		GetEndEvent(tutorial)(timeInTutorial);
	}

	//takes an event call from main menu controller and executes the corresponding tutorial
	//uses an enum Tutorial from MainMenuController to decide which tutorial to execute
	private void TutorialEventHandler (MainMenuController.Tutorial tutorial) {
		Utility.Log(tutorial + " is now playing");
		if (tutorial == MainMenuController.Tutorial.Gathering) {
			ExecuteTutorial(OnElementsDraggedIntoGatheringTutorialBegan, tutorial);
		} else if (tutorial == MainMenuController.Tutorial.Crafting) {
			ExecuteTutorial(OnCraftingModeTutorialBegan, tutorial);
		} else if (tutorial == MainMenuController.Tutorial.TierSwitch) {
			ExecuteTutorial(OnTierSwitchingTutorialBegan, tutorial);
		} else if (tutorial == MainMenuController.Tutorial.BuyHint) {
			ExecuteTutorial(OnBuyHintTutorialBegan, tutorial);
		} else if (tutorial == MainMenuController.Tutorial.UpgradePowerup) {
			ExecuteTutorial(OnBuyPowerUpUpgradeTutorialBegan, tutorial);
		}

	}

	//Gets the event that ends the tutorial in correspondce to the tutorail
	private TutorialCompleted GetEndEvent (BeginTutorial beginningEvent) {

		if (beginningEvent == OnBuyHintTutorialBegan) {
			return OnBuyHintTutorialComplete;
		} else if (beginningEvent == OnBuyPowerUpUpgradeTutorialBegan) {
			return OnBuyPowerUpUpgradeTutorialComplete;
		} else if (beginningEvent == OnCraftingModeTutorialBegan) {
			return OnCraftingModeTutorialComplete;
		} else if (beginningEvent == OnElementsDraggedIntoGatheringTutorialBegan) {
			return OnElementsDraggedIntoGatheringTutorialComplete;
		} else if (beginningEvent == OnTierSwitchingTutorialBegan) {
			return OnTierSwitchingTutorialComplete;
		} else {
			return null;
		}
	}

	//sets the reference to the tutorialmessageboard
	private void InitializeTutorialMessageBoard () {
		foreach (CanvasGroup canvasGroup in GetComponentsInChildren<CanvasGroup>()) {
			if (canvasGroup.gameObject.name == "TutorialMessageBoard") {
				TutorialMessageBoardCanvasGroup = canvasGroup;
				TutorialMessageBoardText = TutorialMessageBoardCanvasGroup.transform.GetComponentInChildren<Text>();
			}
		}
	}

	//turns the masks raycast blocking on and off
	public static void ToggleMaskBlockingRayCastsInactive (bool active) {
		MaskCanvasGroup.blocksRaycasts = !active;
	}

	//sets the tutorial message board
	public static void SetTutorialMessageBoard (string text) {
		TutorialMessageBoardCanvasGroup.alpha = 1f;
		TutorialMessageBoardText.text = text;

		//allows the user to end the tutorial on tap
		ToggleEndTutorialOnTap(true);
	}

	//hides the tutorial message board
	public static void HideTutorialMessageBoard () {
		TutorialMessageBoardCanvasGroup.alpha = 0;

		//disallows the user to end the tutorial on tap
		ToggleEndTutorialOnTap(false);
	}
	
	//triggers the tutorial as complete in the player prefs bool
	private void SetTutorialComplete (MainMenuController.Tutorial tutorial) {

		if (tutorial == MainMenuController.Tutorial.Gathering) {
			Utility.SetPlayerPrefIntAsBool(GlobalVars.ELEMENTS_DRAGGED_TUTORIAL_KEY, true);
		} else if (tutorial == MainMenuController.Tutorial.Crafting) {
			Utility.SetPlayerPrefIntAsBool(GlobalVars.CRAFTING_TUTORIAL_KEY, true);
		} else if (tutorial == MainMenuController.Tutorial.TierSwitch) {
			Utility.SetPlayerPrefIntAsBool(GlobalVars.TIER_SWITCH_TUTORIAL_KEY, true);
		} else if (tutorial == MainMenuController.Tutorial.BuyHint) {
			Utility.SetPlayerPrefIntAsBool(GlobalVars.BUY_HINT_TUTORIAL_KEY, true);
		} else if (tutorial == MainMenuController.Tutorial.UpgradePowerup) {
			Utility.SetPlayerPrefIntAsBool(GlobalVars.UPGRADE_POWERUP_TUTORIAL_KEY, true);
		}

	}

	//subscribes to events
	private void SubscribeEvents () {
		//subscribes to the event calls form MainMenuController
		MainMenuController.OnCallTutorialEvent += TutorialEventHandler;

		//subscribes to internal events
		OnElementsDraggedIntoGatheringTutorialBegan += TriggerOnElementsDraggedIntoGatheringTutorial;
		OnCraftingModeTutorialBegan += TriggerCraftingTutorial;
		OnBuyHintTutorialBegan += TriggerBuyHintTutorial;
		OnBuyPowerUpUpgradeTutorialBegan += TriggerUpgradePowerupTutorial;
		OnTierSwitchingTutorialBegan += TriggerTierSwitchingTutorial;

		//events to end the tutorial
		CraftingButtonController.OnExitCrafting += EndOnElementsDraggedIntoGatheringTutorial;
		CraftingControl.OnElementCreated += EndCraftingTutorial;
		MainMenuController.OnLoadTier += EndTierSwitchingTutorial;
		BuyUpgrade.OnPowerUpUpgrade += EndUpgradePowerupTutorial;
		PurchaseHint.OnPurchaseHint += EndBuyHintTutorial;

		//to turn the mask's raycast blocking on and off
		GeneratePowerUpList.OnTogglePowerUpUpgradeScreen += ToggleMaskBlockingRayCastsInactive;
	}

	private void UnsubscribeEvents () {
		//unsubscribes from the event calls from MainMenuController
		MainMenuController.OnCallTutorialEvent -= TutorialEventHandler;

		//unsubscribes to internal events
		OnCraftingModeTutorialBegan -= TriggerOnElementsDraggedIntoGatheringTutorial;
		OnCraftingModeTutorialBegan -= TriggerCraftingTutorial;
		OnBuyHintTutorialBegan -= TriggerBuyHintTutorial;
		OnBuyPowerUpUpgradeTutorialBegan -= TriggerUpgradePowerupTutorial;
		OnTierSwitchingTutorialBegan -= TriggerTierSwitchingTutorial;

		//events to end the tutorial
		CraftingButtonController.OnExitCrafting -= EndOnElementsDraggedIntoGatheringTutorial;
		CraftingControl.OnElementCreated -= EndCraftingTutorial;
		MainMenuController.OnLoadTier -= EndTierSwitchingTutorial;
		BuyUpgrade.OnPowerUpUpgrade -= EndUpgradePowerupTutorial;
		PurchaseHint.OnPurchaseHint -= EndBuyHintTutorial;

		//to turn the mask's raycast blocking on and off
		GeneratePowerUpList.OnTogglePowerUpUpgradeScreen -= ToggleMaskBlockingRayCastsInactive;
	}

	//brings all the necessary components front for the tutorial
	private void TriggerOnElementsDraggedIntoGatheringTutorial () {
		CraftingTutorialComponent.ActivateTutorialComponents(MainMenuController.Tutorial.Gathering);
	}
	
	private void TriggerCraftingTutorial () {
		CraftingTutorialComponent.ActivateTutorialComponents(MainMenuController.Tutorial.Crafting);
	}

	private void TriggerBuyHintTutorial () {
		CraftingTutorialComponent.ActivateTutorialComponents(MainMenuController.Tutorial.BuyHint);
	}

	private void TriggerUpgradePowerupTutorial () {
		CraftingTutorialComponent.ActivateTutorialComponents(MainMenuController.Tutorial.UpgradePowerup);
	}

	private void TriggerTierSwitchingTutorial () {
		CraftingTutorialComponent.ActivateTutorialComponents(MainMenuController.Tutorial.TierSwitch);
		ToggleMaskBlockingRayCastsInactive(true);
	}

	private void EndOnElementsDraggedIntoGatheringTutorial () {
		Utility.Log("Received the event to end the tutorial");
		if (CurrentTutorial == MainMenuController.Tutorial.Gathering && TutorialActive) {
			EndTutorial ();
			CraftingTutorialComponent.DeactivateTutorialComponents(MainMenuController.Tutorial.Gathering);
		}
	}

	private void EndCraftingTutorial (string newElement, string parent1, string parent2, bool isNew) {
		Utility.Log("Should be turning off the crafting tutorial");
		if (CurrentTutorial == MainMenuController.Tutorial.Crafting && TutorialActive) {
			Utility.Log("Actually turning off the crafting tutorial");
			EndTutorial ();
			CraftingTutorialComponent.DeactivateTutorialComponents(MainMenuController.Tutorial.Crafting);
		}
	}
	
	private void EndBuyHintTutorial (string elementHintName) {
		if (CurrentTutorial == MainMenuController.Tutorial.BuyHint && TutorialActive) {
			EndTutorial ();
			CraftingTutorialComponent.DeactivateTutorialComponents(MainMenuController.Tutorial.BuyHint);
		}
	}
	
	private void EndUpgradePowerupTutorial (string powerupName, int powerUpLevel) {
		if (CurrentTutorial == MainMenuController.Tutorial.UpgradePowerup && TutorialActive) {
			EndTutorial ();
			CraftingTutorialComponent.DeactivateTutorialComponents(MainMenuController.Tutorial.UpgradePowerup);
		}
	}
	
	private void EndTierSwitchingTutorial (int tier) {
		if (CurrentTutorial == MainMenuController.Tutorial.TierSwitch && TutorialActive) {
			EndTutorial ();
			CraftingTutorialComponent.DeactivateTutorialComponents(MainMenuController.Tutorial.TierSwitch);
			ToggleMaskBlockingRayCastsInactive(false);
		}
	}

	private static void ToggleMask (bool active) {
		Mask.enabled = active;
	}

	private static void ToggleEndTutorialOnTap (bool active) {
		if (active) {
			MaskButton.onClick.AddListener(() => {
				Instance.EndTutorialOnTap();
			});
		} else {
			MaskButton.onClick.RemoveAllListeners();
		}

	}
}