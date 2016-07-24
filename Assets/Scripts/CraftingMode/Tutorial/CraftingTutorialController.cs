/*
 * Calls the events to launch the tutorials in the crafing mode
 * Sends event calls with the completion time to MixPanel Analytics
 * Has a mask to cover the UI components not used in the tutorial: 
 * the mask sometimes uses the BlockRaycasts feature of the CanvasGroup component to disable clicks on non-tutorial components
 * Runs off events called by MainMenuController and an enum system of tutorials in that script
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

	private bool tutorialHasEnded;

	private static Image Mask;
	private static Button MaskButton;
	private static CanvasGroup MaskCanvasGroup;

	public static TutorialType CurrentTutorial = TutorialType.None;

	// For tier button related tutorials
	public static int? ActiveTierButton = null;

	// For power up screen related tutorials
	public static int? ActivePowerUp = null;

	public static bool GatheringTutorialActive {
		get {
			return TutorialActive && CurrentTutorial == TutorialType.Gathering;
		}
	}
	public static bool CraftingTutorialActive {
		get {
			return TutorialActive && CurrentTutorial == TutorialType.Crafting;
		}
	}
	public static bool BuyHintTutorialActive {
		get {
			return TutorialActive && CurrentTutorial == TutorialType.BuyHint;
		}
	}

	public static bool ElementDraggingTutorialActive {
		get {
			return CraftingTutorialActive || GatheringTutorialActive;
		}
	}


	void Awake () {
		Instance = this;
		SubscribeEvents();
	
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
	public void ExecuteTutorial (BeginTutorial tutorial, TutorialType tutorialEnum) {
		if (tutorial == null) {
			return;
		} else {
			if (tutorialEnum == TutorialType.Crafting || tutorialEnum == TutorialType.Gathering) {
				GlobalVars.CRAFTING_CONTROLLER.ToggleRaycastingOnElementSpawners(false);
			}

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

	public void AdvanceTutorial () {
		CraftingTutorialComponent pointerToCurrent = GetCurrentComponent();
		if (pointerToCurrent != null) {
			ToggleComponents(pointerToCurrent.GetCurrent(), false);
			ModifyCurrentTutorialStep(1);
			ToggleComponents(pointerToCurrent.GetNext(), true);
		}
	}

	public void StepTutorialBackward () {
		CraftingTutorialComponent pointerToCurrent = GetCurrentComponent();
		if (pointerToCurrent != null) {
			ToggleComponents(pointerToCurrent.GetCurrent(), false);
			ModifyCurrentTutorialStep(-1);
			ToggleComponents(pointerToCurrent.GetPrevious(), true);
		}
	}

	int GetCurrentTutorialStep () {
		return CraftingTutorialComponent.CurrentTutorialSteps[CurrentTutorial];
	}

	void ModifyCurrentTutorialStep (int delta) {
		CraftingTutorialComponent.CurrentTutorialSteps[CurrentTutorial] += delta;
	}

	CraftingTutorialComponent GetCurrentComponent () {
		return CraftingTutorialComponent.GetStep(CurrentTutorial, GetCurrentTutorialStep());
	}

	// TutorialComponent[] passed should actually contain CraftingTutorialComponent instances
	void ToggleComponents (TutorialComponent[] components, bool active) {
		for (int i = 0; i < components.Length; i++) {
			try {
				CraftingTutorialComponent tutorialComponent = (CraftingTutorialComponent) components[i];
				if (active) {
					tutorialComponent.ActivateComponent();
				} else {
					tutorialComponent.DeactivateComponent();
				}
			} catch {
				Debug.Log(components[i].gameObject + " does have a MonoBehaviour of type [CraftingTutorialComponent]");
			}
		}
	}

	/// <summary>
	/// Ends the tutorial.
	/// </summary>
	public void EndTutorial () {
		//turns of the mask covering the scene
		ToggleMask(false);
		TutorialActive = false;
		tutorialHasEnded = true;
	}

	//ends the tutorial on tap
	public void EndTutorialOnTap () {
		if (CurrentTutorial == TutorialType.BuyHint) {
			EndBuyHintTutorial("none");
		} else if (CurrentTutorial == TutorialType.UpgradePowerup) {
			EndUpgradePowerupTutorial("none", 0);
		}

		EndTutorial();
	}


	/// <summary>
	/// Times the tutorial completion.
	/// </summary>
	/// <returns>The tutorial completion.</returns>
	/// <param name="tutorial">The tutorial that was completed.</param>
	private IEnumerator TimeTutorialCompletion(BeginTutorial tutorial, TutorialType tutorialEnum) { 
		float timeInTutorial = 0;
		while (!tutorialHasEnded) {
			timeInTutorial += Time.deltaTime;
			yield return new WaitForFixedUpdate();
		}

		SetTutorialComplete(tutorialEnum);
		GetEndEvent(tutorial)(timeInTutorial);
	}

	//takes an event call from main menu controller and executes the corresponding tutorial
	//uses an enum Tutorial from MainMenuController to decide which tutorial to execute
	private void TutorialEventHandler (TutorialType tutorial) {
		switch (tutorial) {
		case TutorialType.Gathering:
			ExecuteTutorial(OnElementsDraggedIntoGatheringTutorialBegan, tutorial);
			break;
		case TutorialType.Crafting:
			ExecuteTutorial(OnCraftingModeTutorialBegan, tutorial);
			break;
		}
	}

	// Handles tutorial that take an additional int as an argument
	private void IndexedTutorialEventHandler (TutorialType tutorial, int index) {
		switch (tutorial) {
		case TutorialType.BuyHint:
			ActiveTierButton = index;
			ExecuteTutorial(OnBuyHintTutorialBegan, tutorial);
			int firstTier = 1;
			bool dontSwitchTier = (index == firstTier);
			if (dontSwitchTier) {
				Advance();
			}
			break;
		case TutorialType.TierSwitch:
			ActiveTierButton = index;
			ExecuteTutorial(OnTierSwitchingTutorialBegan, tutorial);
			break;
		case TutorialType.UpgradePowerup:
			ActivePowerUp = index;
			ExecuteTutorial(OnBuyPowerUpUpgradeTutorialBegan, tutorial);
			break;
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
	
	//turns the masks raycast blocking on and off
	public static void ToggleMaskBlockingRayCastsInactive (bool active) {
		MaskCanvasGroup.blocksRaycasts = !active;
	}
	
	public static void Advance () {
		Instance.AdvanceTutorial();
	}

	public static void StepBack () {
		Instance.StepTutorialBackward();
	}

	//triggers the tutorial as complete in the player prefs bool
	private void SetTutorialComplete (TutorialType tutorial) {

		if (tutorial == TutorialType.Gathering) {
			Utility.SetPlayerPrefIntAsBool(GlobalVars.ENTER_GATHERING_TUTORIAL_KEY, true);
		} else if (tutorial == TutorialType.Crafting) {
			Utility.SetPlayerPrefIntAsBool(GlobalVars.CRAFTING_TUTORIAL_KEY, true);
		} else if (tutorial == TutorialType.TierSwitch) {
			Utility.SetPlayerPrefIntAsBool(GlobalVars.TIER_SWITCH_TUTORIAL_KEY, true);
		} else if (tutorial == TutorialType.BuyHint) {
			Utility.SetPlayerPrefIntAsBool(GlobalVars.BUY_HINT_TUTORIAL_KEY, true);
		} else if (tutorial == TutorialType.UpgradePowerup) {
			Utility.SetPlayerPrefIntAsBool(GlobalVars.UPGRADE_POWERUP_TUTORIAL_KEY, true);
		}

	}

	//subscribes to events
	private void SubscribeEvents () {
		//subscribes to the event calls form MainMenuController
		MainMenuController.OnCallTutorialEvent += TutorialEventHandler;
		MainMenuController.OnCallIndexedTutorialEvent += IndexedTutorialEventHandler;

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
		MainMenuController.OnCallIndexedTutorialEvent -= IndexedTutorialEventHandler;

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
		CraftingTutorialComponent.ActivateTutorialComponents(TutorialType.Gathering);
	}
	
	private void TriggerCraftingTutorial () {
		CraftingTutorialComponent.ActivateTutorialComponents(TutorialType.Crafting);
	}

	private void TriggerBuyHintTutorial () {
		CraftingTutorialComponent.ActivateTutorialComponents(TutorialType.BuyHint);
	}

	private void TriggerUpgradePowerupTutorial () {
		CraftingTutorialComponent.ActivateTutorialComponents(TutorialType.UpgradePowerup);
	}

	private void TriggerTierSwitchingTutorial () {
		CraftingTutorialComponent.ActivateTutorialComponents(TutorialType.TierSwitch);
		ToggleMaskBlockingRayCastsInactive(true);
	}

	private void EndOnElementsDraggedIntoGatheringTutorial () {
		if (CurrentTutorial == TutorialType.Gathering && TutorialActive) {
			EndTutorial ();
			CraftingTutorialComponent.DeactivateTutorialComponents(TutorialType.Gathering);
		}
	}

	private void EndCraftingTutorial (string newElement, string parent1, string parent2, bool isNew) {
		if (CurrentTutorial == TutorialType.Crafting && TutorialActive) {
			EndTutorial ();
			CraftingTutorialComponent.DeactivateTutorialComponents(TutorialType.Crafting);
		}
	}
	
	private void EndBuyHintTutorial (string elementHintName) {
		if (CurrentTutorial == TutorialType.BuyHint && TutorialActive) {
			EndTutorial ();
			CraftingTutorialComponent.DeactivateTutorialComponents(TutorialType.BuyHint);
		}
	}
	
	private void EndUpgradePowerupTutorial (string powerupName, int powerUpLevel) {
		if (CurrentTutorial == TutorialType.UpgradePowerup && TutorialActive) {
			EndTutorial ();
			CraftingTutorialComponent.DeactivateTutorialComponents(TutorialType.UpgradePowerup);
		}
	}
	
	private void EndTierSwitchingTutorial (int tier) {
		if (CurrentTutorial == TutorialType.TierSwitch && TutorialActive) {
			EndTutorial ();
			CraftingTutorialComponent.DeactivateTutorialComponents(TutorialType.TierSwitch);
			ToggleMaskBlockingRayCastsInactive(false);
		}
	}

	private static void ToggleMask (bool active) {
		Mask.enabled = active;
		MaskCanvasGroup.alpha = active ? 1.0f : 0.0f;
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