using UnityEngine;
using System.Collections;
//cheat scripts to be called from other classes
public static class Cheats {
	private static int defaultIncreaseAmount = 5;

	//unlocks all elements
	public static void UnlockAllElements () {
		//increases all elements by default amount
		IncreaseAllElements();
		GlobalVars.NUMBER_ELEMENTS_UNLOCKED = GlobalVars.ELEMENTS.Count;
		GlobalVars.CRAFTING_CONTROLLER.updatePercentUnlocked();
		foreach (Element element in GlobalVars.ELEMENTS) {
			element.unlock();
			PlayerPrefs.SetInt(element.getName()+GlobalVars.UNLOCK_STRING, 1);
		}
		for (int i = 0; i < GlobalVars.TIER_COUNT; i++) {
			UnlockTier(i);
			GlobalVars.TIER_UNLOCKED[i] = true;
		}

		// Unlocks all the tier buttons
		for (int i = 0; i < TierButtonDisplay.AllTierButtons.Count; i++) {
			TierButtonDisplay.AllTierButtons[i].makeButtonUnlocked();
		}
	}

	public static void LockAllElements () {
		//makes it so that the tutorials play again on launch
		ResetTutorialsWatched();
		ResetHintsUnlocked();
		GlobalVars.NUMBER_ELEMENTS_UNLOCKED = GlobalVars.NUMBER_OF_LANES;
		GlobalVars.CRAFTING_CONTROLLER.updatePercentUnlocked();
		foreach (Element element in GlobalVars.ELEMENTS) {
			//reset inventory amount to zero
			PlayerPrefs.SetInt(element.getName(), 0);

			//skip base elments
			if (element.isBaseElement) {
				continue;
			}
			element.relock();
			PlayerPrefs.SetInt(element.getName()+GlobalVars.UNLOCK_STRING, 0);
		}
		for (int i = 2; i < GlobalVars.TIER_COUNT; i++) {
			GlobalVars.TIER_UNLOCKED[i] = false;
		}
		DataController.ResetSave();
	}
	
	public static void IncreaseAllElements (int amount) {
		foreach (Element element in GlobalVars.ELEMENTS) {
			Utility.IncreasePlayerPrefValue(element.getName(), amount);
		}
	}

	public static void IncreaseAllElements () {
		IncreaseAllElements(defaultIncreaseAmount);
	}

	// unlocks a full tier of element 
	// NOTE: this uses the original conception of ten tiers
	// whereas the current game splists the tiers into 9 (with 1 and 2 becoming tier 1)
	public static void UnlockTier (int tier) {
		if (GlobalVars.CRAFTING_CONTROLLER != null) {
			GlobalVars.CRAFTING_CONTROLLER.updatePercentUnlocked();
		}

		if (GlobalVars.TIER_UNLOCKED == null) {
			GlobalVars.TIER_UNLOCKED = new bool[GlobalVars.TIER_COUNT];
		}

		//unlocks tier
		GlobalVars.TIER_UNLOCKED[tier] = true;

		//unlocks all elements in tier
		foreach (Element element in GlobalVars.ELEMENTS_BY_TIER[tier]) {
			if (!element.isElementUnlocked()) {
				element.unlock();
				PlayerPrefs.SetInt(element.getName()+GlobalVars.UNLOCK_STRING, 1);
				GlobalVars.NUMBER_ELEMENTS_UNLOCKED++;
			}
		}
	}

	public static void UnlockAllPowerups () {
		ActivatePowerUp.UnlockAllPowerups();
	}

	public static void ResetPowerUpUpgradePurchases () {
		foreach (string powerupName in GlobalVars.POWERUP_INDEXES.Keys) {
			PowerUp.ResetPowerUpLevel(powerupName);
		}
	}

	//resets whether player has watched the tutorials
	public static void ResetTutorialsWatched (bool ignoreGatheringTutorials = false) {
		ToggleAllTutorialsWatched(false, ignoreGatheringTutorials);
	}

	public static void SetAllTutorialsAsWatched (bool ignoreGatheringTutorials = false) {
		ToggleAllTutorialsWatched(true, ignoreGatheringTutorials);
	}

	public static void ToggleAllTutorialsWatched (bool tutorialWatched, bool ignoreGatheringTutorials = false) {
		//bools for crafting
		foreach (string tutorialKey in GlobalVars.AllCraftingModeTutorials) {
			Utility.SetPlayerPrefIntAsBool(tutorialKey, tutorialWatched);
		}

		if (!ignoreGatheringTutorials) {
			//bools for gathering
			Utility.SetPlayerPrefIntAsBool(GlobalVars.GATHERING_TUTORIAL_WATCHED_SWIPE, tutorialWatched);
			Utility.SetPlayerPrefIntAsBool(GlobalVars.GATHERING_TUTORIAL_WATCHED_POWER_UP, tutorialWatched);
		}
	}

	public static void SetTutorialWatched (string tutorialName, bool hasBeenWatched = true) {
		Utility.SetPlayerPrefIntAsBool(tutorialName, hasBeenWatched);
	}

	public static void IncreaseElement (string elementName, int amount) {
		Utility.IncreasePlayerPrefValue(elementName, amount);
	}

	public static void IncreaseBaseElements (int amount) {
		foreach (string elementName in GlobalVars.BASE_ELEMENT_NAMES) {
			IncreaseElement(elementName, amount);
		}
	}

	public static void ResetToCraftingTutorial () {
		ResetTutorialsWatched();
		SetTutorialWatched(GlobalVars.ENTER_GATHERING_TUTORIAL_KEY);
		IncreaseBaseElements(1);
	}

	public static void ResetHintsUnlocked () {
		foreach (Element element in GlobalVars.ELEMENTS) {
			Utility.SetPlayerPrefIntAsBool(element.getName() + GlobalVars.HINT_STRING, false);
		}
	}

	public static void ResetToBuyHintTutorial () {
		LockAllElements();
		SetTutorialWatched(GlobalVars.ENTER_GATHERING_TUTORIAL_KEY);
		SetTutorialWatched(GlobalVars.CRAFTING_TUTORIAL_KEY);
		IncreaseBaseElements(50);
	}

	public static void ResetToPurchaseUpgradeTutorial () {
		ResetPowerUpUpgradePurchases();
		bool ignoreGatheringTutorials = true;
		SetAllTutorialsAsWatched(ignoreGatheringTutorials);
		SetTutorialWatched(GlobalVars.UPGRADE_POWERUP_TUTORIAL_KEY, false);
		UnlockTier(0);
		UnlockTier(1);
		IncreaseAllElements(50);
	}

	public static void ResetToUnlockTierTutorial () {
		LockAllElements();
		SetAllTutorialsAsWatched();
		SetTutorialWatched(GlobalVars.TIER_SWITCH_TUTORIAL_KEY, false);
		UnlockTier(2);
	}
}
