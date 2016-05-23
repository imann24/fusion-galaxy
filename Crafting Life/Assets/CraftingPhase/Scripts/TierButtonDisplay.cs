/*
 * Controls the tier buttons on the low left hand of the screen
 * Sets how much progress they have 
 * Which icon they display depending on whether they're fulling unlocked or not
 * Intended to sync with ProgressBarController to coordinate the amount of progress in each tier and show that to the player
 */

//#define DEBUG
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class TierButtonDisplay : MonoBehaviour {
	//event call for tier progress updated
	public delegate void TierProgressUpdatedAction (int tierNumber, float elementsUnlockedFraction);
	public static TierProgressUpdatedAction OnTierProgressUpdated;

	public delegate void LoadTierAction (int tier);
	public static event LoadTierAction OnLoadTier;

	//list of all the events
	public static Dictionary<int, TierButtonDisplay> AllTierButtons = new Dictionary<int, TierButtonDisplay>();


	//references to gameobjects that make up the button
	public CraftingButtonController controller;
	
	private Image ElementClassSelectedBackground;
	private Image ElementClassUnselectedBackground;
	private Image ClassProgressBarFill;

	private Text ClassNumber;
	private static Color TierLockedTextColor = new Color32(81,233,220, 255);
	private static Color TierSelectedTextColor = new Color32(3,139,147, 255);
	private static Color TierUnselectedTextColor = Color.white;

	private Image ZoneUnselectedIndicator;
	private Image ZoneSelectedIndicator;
	private Image ZoneLockedIndicator;

	private Image PowerUpStatusBackgroundSelected;
	private Image PowerUpStatusBackgroundUnselected;
	private Image PowerUpSymbol;
	private Image LockSymbol;

	private Button TierButton;
	
	//string that is used in the title
	private const string TITLE = "Class";

	//to offset the dictionary indexes
	private int dictionarOffset = -1;
	//whether the tier is unlocked
	public bool isUnlocked {get; private set;}

	//tier info
	public int tierNumber;

	//bool to indiciate status
	private enum Status {Locked, Unselected, Selected};

	void Awake () {
		tierNumber = transform.parent.childCount - transform.GetSiblingIndex();
		AllTierButtons.Add(tierNumber + dictionarOffset, this);
		SetReferences ();
		ClassNumber.text = TITLE + " " + tierNumber;
	}
	// Use this for initialization
	void Start () {
		SetReferences();
		updateTierProgress();
		isUnlocked = GlobalVars.TIER_UNLOCKED[tierNumber];
		if (!isUnlocked) {
		//	makeButtonLocked();
			SetLocked();
		} else {
			SetUnselected();
		}

		if (tierNumber == 1) {
			SetSelected();
		}


		//subscribes the event to the button
		TierButton.onClick.AddListener(() => { 
			if (OnLoadTier != null) {
				OnLoadTier(tierNumber);
			}
		});

		//calls the event once to make sure the progress bars in the crafting screen update at start
		if (OnTierProgressUpdated != null) {
			OnTierProgressUpdated(tierNumber, GetProgressFraction());
		}

		// Subscribes to events when the object is created
		SubscribeEvents();
	}

	void OnDestroy () {
		// Removes the script from the dictionary of scripts when it is destroyed
		AllTierButtons.Remove(tierNumber + dictionarOffset);

		// Unsubscribes from events when the object is destroyed
		UnsubscribeEvents();
	}

	//gets all the image and text references 
	private void SetReferences () {
		//sets the button
		TierButton = GetComponent<Button>();

		//sets the background 
		ElementClassUnselectedBackground = GetComponent<Image>();

		//sets all the image references
		foreach (Image image in transform.GetComponentsInChildren<Image>()) {
			if (image.name == "ElementClassSelectedBackground") {
				ElementClassSelectedBackground = image;
			} else if (image.name == "ClassProgressBarFill") {
				ClassProgressBarFill = image;
			} else if (image.name == "ZoneUnselectedIndicator") {
				ZoneUnselectedIndicator = image;
			} else if (image.name == "ZoneSelectedIndicator") {
				ZoneSelectedIndicator = image;
			} else if (image.name == "ZoneLockedIndicator") {
				ZoneLockedIndicator = image;
			} else if (image.name == "PowerUpStatusBackgroundSelected") {
				PowerUpStatusBackgroundSelected = image;
			} else if (image.name == "PowerUpStatusBackgroundUnselected") {
				PowerUpStatusBackgroundUnselected = image;
			} else if (image.name == "PowerUpSymbol") {
				PowerUpSymbol = image;
			} else if (image.name == "LockSymbol") {
				LockSymbol = image;
			}
		}

		//sets the text reference
		foreach (Text text in transform.GetComponentsInChildren<Text>()) {
			if (text.name == "ClassNumber") {
				ClassNumber = text;
			}
		}
	}

	//sets the status based on the enum passed
	private void SetStatus (Status status) {
		if (status == Status.Locked) {
			SetLocked();
		} else if (status == Status.Unselected) {
			SetUnselected();
		} else if (status == Status.Selected) {
			SetSelected();
		}
	}

	//sets the tier button to unselected
	private void SetUnselected () {

		SetZoneIndicator(Status.Unselected);
		if (ElementClassSelectedBackground != null) {
			ElementClassSelectedBackground.enabled = false;
		}
		SetProgressBarLengthAndSymbol();
		SetText(Status.Unselected);
		SetButton(Status.Unselected);
	}

	//sets the tier button to selected
	private void SetSelected () {
		SetZoneIndicator(Status.Selected);
		if (ElementClassSelectedBackground != null) {
			ElementClassSelectedBackground.enabled = true;
		}
		SetProgressBarLengthAndSymbol();
		SetText(Status.Selected);
		SetButton(Status.Selected);
	}

	//sets the tier button locked
	private void SetLocked () {
		SetZoneIndicator(Status.Locked);
		SetBackground(Status.Locked);
		SetProgressBarLengthAndSymbol(0);
		SetText(Status.Locked);
		SetButton(Status.Locked);
	}

	//toggles the right zone indicator
	private void SetZoneIndicator (Status status) {
		//turns all images off
		if (ZoneUnselectedIndicator != null && ZoneSelectedIndicator != null && ZoneLockedIndicator != null) {
			ZoneUnselectedIndicator.enabled = false;
			ZoneSelectedIndicator.enabled = false;
			ZoneLockedIndicator.enabled = false;

			//turns the right image one
			if (status == Status.Locked) {
				ZoneLockedIndicator.enabled = true;
			} else if (status == Status.Unselected) {
				ZoneUnselectedIndicator.enabled = true;
			} else if (status == Status.Selected) {
				ZoneSelectedIndicator.enabled = true;
			}
		}
	}

	//sets the background color
	private void SetBackground (Status status) {
		if (ElementClassSelectedBackground != null) {
			if (status == Status.Selected) {
				ElementClassSelectedBackground.enabled = true;
			} else {
				ElementClassSelectedBackground.enabled = false;
				if (status == Status.Locked) {
					ElementClassUnselectedBackground.color = Utility.SetColorTransparency (ElementClassSelectedBackground.color, 0.5f);
				} else if (status == Status.Unselected) {
					ElementClassUnselectedBackground.color = Utility.SetColorTransparency (ElementClassSelectedBackground.color, 1.0f);
				}
			}
		}
	}

	//sets the the button to non-interactable if the tier is locked
	private void SetButton (Status status) {
		if (status == Status.Locked) {
			TierButton.interactable = false;
		} else {
			TierButton.interactable = true;
		}
	}


	//sets the progress bar length based on how many elements are unlocked
	private void SetProgressBarLengthAndSymbol () {
		SetProgressBarLengthAndSymbol(GetProgressFraction());

	}

	//sets the bar to a specified fraction
	private void SetProgressBarLengthAndSymbol (float progressFraction) {
		//calls the event
		if (OnTierProgressUpdated != null) {
			OnTierProgressUpdated(tierNumber-1, progressFraction);
		}
		
		//sets the bar length
		if(ClassProgressBarFill != null){
			ClassProgressBarFill.fillAmount = progressFraction;
		}
		if (PowerUpStatusBackgroundSelected != null && PowerUpStatusBackgroundUnselected != null) {
			//all the elments in the tier are unlocked, highlight the powerup icon
			PowerUpStatusBackgroundSelected.enabled = (progressFraction == 1f);
			PowerUpStatusBackgroundUnselected.enabled = !(progressFraction == 1f);
		}
		if (PowerUpSymbol != null && LockSymbol != null) {
			//if any of the elements in the tier are unlocked, show the powerup icon
			PowerUpSymbol.enabled = (progressFraction > 0);
			LockSymbol.enabled = (progressFraction == 0);
		}
	}

	//sets the color of the text
	private void SetText (Status status) {
		if (status == Status.Locked) {
			ClassNumber.color = TierLockedTextColor;
		} else if (status == Status.Unselected) {
			ClassNumber.color = TierUnselectedTextColor;
		} else if (status == Status.Selected) {
			ClassNumber.color = TierSelectedTextColor;
		}
	}

	//sets the fraction of the element unlocked out of 1f
	private float GetProgressFraction () {
		float elementsUnlocked;
		float numElementsInTier;

		//returns the number of elements unlocked in the first two tiers (because the first tier is technically two combined into one)
		if (tierNumber == 1) {
			elementsUnlocked = getElementsUnlocked(0) + getElementsUnlocked(1);
			numElementsInTier = getNumElementsInTier(0) + getNumElementsInTier(1);
		} else {
			elementsUnlocked = getElementsUnlocked(tierNumber);
			numElementsInTier = getNumElementsInTier(tierNumber);
		}

		return elementsUnlocked/numElementsInTier; 
	}

	//subscribes to the events
	private void SubscribeEvents () {
		CraftingControl.OnTierUnlocked += HandleOnTierUnlocked;
	}
	

	//unsubscribes from events
	private void UnsubscribeEvents () {
		CraftingControl.OnTierUnlocked -= HandleOnTierUnlocked;
	}

	//getter method for tier number
	public int getTierNumber () {
		return tierNumber;
	}

	//unlocks the tier button
	void HandleOnTierUnlocked (int tierNumber) {

		if (this.tierNumber == tierNumber) {
			SetStatus(Status.Unselected);
			isUnlocked = true;
			//plays the tier switch tutorial if it has not yet been watched
			if (!Utility.PlayerPrefIntToBool(GlobalVars.TIER_SWITCH_TUTORIAL_KEY) && GlobalVars.CRAFTING_CONTROLLER != null) {
				GlobalVars.CRAFTING_CONTROLLER.CallTierSwitchTutorial();
			}
		}
	}

	//updates the surrounding border progress bar to show how many elements have been unlocked
	public void updateTierProgress () {
		SetProgressBarLengthAndSymbol();
	}

	//gets the number of elements unlocked in the tier 
	public static float getElementsUnlocked (int tierNumber) {
		float elementsUnlocked = 0;
		for (int i = 0; i <  GlobalVars.ELEMENTS_BY_TIER[tierNumber].Count; i++) {
			if (GlobalVars.ELEMENTS_BY_TIER[tierNumber][i].isElementUnlocked()) {
				elementsUnlocked++;
			}
		}
		return elementsUnlocked;
	}

	//gets the number of elements in the tier 
	public static float getNumElementsInTier (int tierNumber) {
		return (float) GlobalVars.ELEMENTS_BY_TIER[tierNumber].Count;
	}

	//deactivates the lock button
	public void makeButtonUnlocked () {
		isUnlocked = true;
		SetUnselected();
	}

	//makes the button unlocked
	public void makeButtonLocked () {
		isUnlocked = false;
		SetLocked();
	}
	
	//makes the button active or not depending on the bool passed to it
	public void makeButtonSelected (bool active) {
		if (active) {
			SetSelected();
		} else {
			if (isUnlocked) {
				SetUnselected();
			} else {
				SetLocked();
			}
		}
	}

	//overloaded method to turn button on
	public void makeButtonSelected () {
		SetSelected();
	}
}
