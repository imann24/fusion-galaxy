/*
 * Handles the combining of elements
 * Works in tandem with the two CaptureScript instances on the two zones in crafting
 * Uses the dictionary of element combinations as strings to determine which element it produces
 * Fires events when elements are discovered and created, as well as when tiers are discovered and compelted
 * Handles the messages/feedback displayed based on whether elements are sufficient and compatible
 */

#define DEBUG
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//controls the combination of elements via a stack of the elements that collide w/ eachother
public class CraftingControl : MonoBehaviour {

	//crafting events
	public delegate void CraftAction(string newElement, string parent1, string parent2, bool isNew);
	public delegate void DiscoverAction (string newElement);
	public delegate void UnlockTierAction (int tierNumber);
	public delegate void CompleteTierAction (int tierNumber);
	public delegate void IncorrectElementCraftAction ();
	public delegate void ReadyToCraftAction (bool ready);

	public static event CraftAction OnElementCreated;
	public static event DiscoverAction OnElementDiscovered;
	public static event UnlockTierAction OnTierUnlocked;
	public static event CompleteTierAction OnTierCompleted;
	public static event IncorrectElementCraftAction OnIncorrectCraft;
	public static event ReadyToCraftAction OnReadyToCraft;

	//player feedback text (tunable)
	private string elementCreatedMain = "Created";
	private string elementCreatedSub = "Tap to Collect";

	private string defaultMain = "";
	private string defaultSub = "Select \n Materials";

	private string oneElementInMain = "";
	private string oneElementInSub = "Select \n Materials";

	private string bothElementsInMain = "";
	private string bothElementsInSub = "";

	private string insufficientAmountsMain = "[Not Enough Materials]";
	private string insufficientAmountsSub = "!";

	private string wrongCombinationMain = "[Incompatible]";
	private string wrongCombinationSub = "Error 770";

	public Color errorColor = Color.red;
	public Color regularColor = Color.white;

	//info about elements
	public MainMenuController panelControl;

	//stores info about the zones
	public GameObject elementDropZone1;
	public GameObject elementDropZone2;
	private bool zone1HasElement;
	private bool zone2HasElement;
	private bool elementIsReadyToCraft;
	private CaptureScript zone1Capturer;
	private CaptureScript zone2Capturer;
	//a reference to the compiler zone's capture script
	public CaptureScript compiler;
	public const string CRAFTING_PROMPT = "Drop in 2";
	public const string COLLECT_NEW_PROMPT = "Click to Collect";

	//new element display
	public Image myElementSprite;
	public GameObject myElementGameObject;
	public Image myElementContainer;
	public Image myElementConatinerEmpty;

	//feedback messages to player/button 
	public Text mainMessage;
	public Text subMessage;
	public Text elementName;
	public Text inventoryNumber;
	public Button craftButton;

	//element info 
	private string resultElement, parentElement1, parentElement2;
	private int resultTier;
	private bool isNew;
	private bool invalidCombination;
	private bool containsBaseElement;
	//tracks the last zone used
	private enum CaptureZone {Zone1, Zone2};
	private CaptureZone lastZone = CaptureZone.Zone2;
	void Awake () {
//		OnElementCreated = null;
		//sets the global script reference
		GlobalVars.CRAFTER = this;
	}
	void Start () {
		//sets the text box
		toggleCraftedElement(false);

		//references to the capture scripts in the drop zones
		zone1Capturer = elementDropZone1.GetComponent<CaptureScript>();
		zone2Capturer = elementDropZone2.GetComponent<CaptureScript>();
		//reference to the child gameobject that represetns the element
		//myElementGameObject = transform.GetChild (3).gameObject;
		//myElementSprite = myElementGameObject.GetComponent<Image>();

		//sets the starting prompting text
		setEmptyMessage();

		//adds the creation function to the event
		OnElementCreated += createElement;
	}


	void OnDestroy () {
		//adds the creation function to the event
		OnElementCreated -= createElement;
	}

	public void OnMouseDown () {
		//clears the zone if it contains a base element 
		if (containsBaseElement) {
			containsBaseElement = false;
			myElementSprite.enabled = false;
			compiler.OnMouseDown();
			setEmptyMessage();
		}

		//disables crafting when there are not two elements
		if (!zone1HasElement || !zone2HasElement) {
			elementIsReadyToCraft = false;
		} else {
			//plays the zone animations if either one has insuficent resources
			if (PlayerPrefs.GetInt(parentElement1) <= 0) {
				zone1Capturer.playNoElementsAnimation();
			}
			
			if (PlayerPrefs.GetInt(parentElement2) <= 0) {
				zone2Capturer.playNoElementsAnimation();
			}
		}

		if (elementIsReadyToCraft) {
			//calls the event to create an element
			if (OnElementCreated != null && validInventoryAmounts()) {
				Utility.Log("created an element and fired the event");
				OnElementCreated(resultElement, parentElement1, parentElement2, isNew);
			}

			//lowers the flag to craft if the player has run out of either element
			if (!validInventoryAmounts()) {
				elementIsReadyToCraft = false;
				setInsufficientMessage();
				if (OnIncorrectCraft != null) {
					OnIncorrectCraft();
				}

			} else {
				setBothElementsInMessage();
			}
		} else if (invalidCombination || !validInventoryAmounts()) {
			if (OnIncorrectCraft != null) {
				OnIncorrectCraft();
			}
		}
	}

	//sets the internal record of the zones to false
	public void setZoneAsEmpty (int zoneNumber) {
		if (OnReadyToCraft != null) {
			OnReadyToCraft (false);
		}

		toggleCraftedElement(false);
		if (zoneNumber == 1) {
			zone1HasElement = false;
		} else if (zoneNumber == 2) {
			zone2HasElement = false;
		}
		if (!(zone1HasElement || zone2HasElement)) {
			setEmptyMessage();
		} else {
			setOneElementInMessage();
		}
	}
	//makes sure both zones have an element, and combines them if possible
	public void checkForCombination(int zoneNumber) {
		if (zoneNumber == 1) {
			zone1HasElement = true;
			setOneElementInMessage();
			parentElement1 = zone1Capturer.myElementGameObject.name;

		} else if (zoneNumber == 2) {
			zone2HasElement = true;
			setOneElementInMessage();
			parentElement2 = zone2Capturer.myElementGameObject.name;
		}
	
		if (zone1HasElement && zone2HasElement) {
			if (validInventoryAmounts()) {

				//sends the event call
				if (OnReadyToCraft != null) {
					OnReadyToCraft(true);
				}
				combine ();
			} else {
				if (GlobalVars.RECIPES_BY_NAME.ContainsKey(parentElement1 + parentElement2)) {
					resultElement = GlobalVars.RECIPES_BY_NAME[parentElement1 + parentElement2].getName();
					myElementSprite.enabled = true;

					//alerts the capture zone that it contains an elmeent
					compiler.elementHasBeenCapured();
					myElementSprite.sprite = GlobalVars.ELEMENT_SPRITES[resultElement];
				} else {
					elementIsReadyToCraft = false;
					//myElementSprite.enabled = false;
				}
				setInsufficientMessage();
			}
			toggleCraftedElement(true);
		} else {
			//clears the compiler
			compiler.OnMouseDown();
			myElementSprite.enabled = false;
		}
	}

	public void setEmptyMessage () {
		mainMessage.text = defaultMain;
		subMessage.text = defaultSub;
		elementName.text = "";
		inventoryNumber.text = "";
		setTextToRegularColor();
	}

	public void setOneElementInMessage () {
		//feedback to player
		mainMessage.text = oneElementInMain;
		subMessage.text = oneElementInSub;	
		elementName.text = "";
		inventoryNumber.text = "";
		setTextToRegularColor();
	}

	public void setBothElementsInMessage () {
		//feedback to player
		mainMessage.text = "";
		subMessage.text = bothElementsInSub;
		elementName.text = Utility.UppercaseWords(resultElement) + bothElementsInMain;
		inventoryNumber.text = PlayerPrefs.GetInt(resultElement).ToString();
		setTextToRegularColor();
	}

	public void setInsufficientMessage () {
		//feedback to player
		mainMessage.text = insufficientAmountsMain;
		subMessage.text = insufficientAmountsSub;
		elementName.text = "";
		inventoryNumber.text = "";
		setTextToRegularColor();
	}

	public void setIncompatibleMessage () {
		//feedback to player
		mainMessage.text = wrongCombinationMain;
		subMessage.text = wrongCombinationSub;
		elementName.text = "";
		inventoryNumber.text = "";
		setTextToErrorMessageColor();
	}

	public void setBaseElementMessage (string element) {
		mainMessage.text = "";
		subMessage.text = "";
		elementName.text = "Base Element: Cannot be Deconstructed";
		inventoryNumber.text = PlayerPrefs.GetInt(element).ToString();
		setTextToErrorMessageColor();
	}

	public void setTextToErrorMessageColor () {
		mainMessage.color = errorColor;
		subMessage.color = errorColor;
		elementName.color = errorColor;
	}

	public void setTextToRegularColor () {
		mainMessage.color = regularColor;
		subMessage.color = regularColor;
		elementName.color = regularColor;
	}

	//sets up the combination for a new element
	public void combine () {
		//element gameobject

		if (GlobalVars.RECIPES_BY_NAME.ContainsKey(parentElement1+parentElement2)) {//checks if the elements combine to form a third
			containsBaseElement = false;

			//gets the new element
			Element result = GlobalVars.RECIPES_BY_NAME[parentElement1+parentElement2];

			//updates the record of the elements
			if (!result.isElementUnlocked()) {
				result.unlock();
				GlobalVars.NUMBER_ELEMENTS_UNLOCKED++;
				panelControl.updatePercentUnlocked();
				isNew = true;

				//calls the event
				if (OnElementDiscovered != null) {
					OnElementDiscovered(result.getName());
				}

				//checks if the whole tier of elements is unlocked and sends the event
				if (OnTierCompleted != null) {
					if ((result.getTier() == 1 || result.getTier() == 2) && 
					    Element.AllTierElementsUnlocked(0) && 
					    Element.AllTierElementsUnlocked(1)) {
						OnTierCompleted(1);
					} else if (Element.AllTierElementsUnlocked(result.getTier()-1)) {
						OnTierCompleted(result.getTier());
					}
				}

			}else {
				isNew = false;
			}
			PlayerPrefs.SetInt(result.getName()+GlobalVars.UNLOCK_STRING, 1); 

			//makes the new element
			myElementGameObject.name = result.getName();
			resultElement = myElementGameObject.name;
			resultTier = result.getTier()-1;
			setBothElementsInMessage();
			elementIsReadyToCraft = true;
			invalidCombination = false;
		} else { // if the combination is incorrect, plays the rejection sound
			setIncompatibleMessage();
			invalidCombination = true;
			elementIsReadyToCraft = false;
		} 
	}

	//changes the result display between empty and full and displays/hides the element
	public void toggleCraftedElement (bool active) {
		bool spriteToggled = false;
		if (active) {
			myElementGameObject.name = resultElement;
			if (GlobalVars.RECIPES_BY_NAME.ContainsKey(parentElement1+parentElement2) && resultElement == GlobalVars.RECIPES_BY_NAME[parentElement1+parentElement2].getName()) {
				myElementSprite.sprite = GlobalVars.ELEMENT_SPRITES[resultElement];
			} else {
				//removes element from compiler
				compiler.OnMouseDown();
				myElementSprite.enabled = false;
				spriteToggled = true;
			}
		} else {
			myElementGameObject.name = "NoElement";
		}
		if (!spriteToggled) {
			myElementSprite.enabled = active;
			if (active) {
				compiler.elementHasBeenCapured();
			} else {
				compiler.OnMouseDown();
			}
		}

		myElementContainer.enabled = active;
		myElementConatinerEmpty.enabled = !active;
	}

	//used to add an element to a capture zone when clicked
	public void enqueueElement (string elementName) {
		if (lastZone == CaptureZone.Zone1) {
			zone2Capturer.captureElement(GlobalVars.ELEMENT_SPRITES[elementName]);
			lastZone = CaptureZone.Zone2;
		} else if (lastZone == CaptureZone.Zone2) {
			zone1Capturer.captureElement(GlobalVars.ELEMENT_SPRITES[elementName]);
			lastZone = CaptureZone.Zone1;
		}
	}

	//used to craft an element that is dragged into the compiler
	public void forceUpdateBothCaptureZones (string elementName) {
		//gets the element reference
		Element resultElement = GlobalVars.ELEMENTS_BY_NAME[elementName];

		//sets the sprites for the two capture zones
		zone1Capturer.captureElement(GlobalVars.ELEMENT_SPRITES[resultElement.getCombinations()[0][0]]);
		zone2Capturer.captureElement(GlobalVars.ELEMENT_SPRITES[resultElement.getCombinations()[0][1]]);

		//sets their flags to true
		zone1HasElement = true;
		zone2HasElement = true;

		//reverse engineers the element dragged in
		checkForCombination(0);
	}

	public void baseElementError (Sprite element) {
		//clears the capture zones
		clearDropzZones ();

		//sets the error message
		setBaseElementMessage(element.name);

		//shows the element sprite
		myElementSprite.enabled = true;

		myElementSprite.sprite = element;

		//sets the flag
		containsBaseElement = true;
	}

	public void clearDropzZones () {
		zone1Capturer.OnMouseDown ();
		zone2Capturer.OnMouseDown ();
		zone1HasElement = false;
		zone2HasElement = false;
	}

	//makes the child gameobject into the new element
	private void createElement (string newElement, string parent1, string parent2, bool isNew) {
		//updates the playerprefs variables
		if (PlayerPrefs.GetInt (parent1) > 0 && PlayerPrefs.GetInt (parent2) > 0) {
			Utility.IncreasePlayerPrefValue (parent1, -1);
			Utility.IncreasePlayerPrefValue (parent2, -1);
			Utility.IncreasePlayerPrefValue (newElement, 1);
		}
		//updates the text in the scene
		zone1Capturer.setElementCountText();
		zone2Capturer.setElementCountText();
		panelControl.updatePanelCounts();

		//if tier is not yet unlocked
		if (!GlobalVars.TIER_UNLOCKED[resultTier]) {
			OnTierUnlocked(resultTier);
			GlobalVars.TIER_UNLOCKED[resultTier] = true;
		}
	}

	
	//method to check whether there's enough of an element to craft
	private bool validInventoryAmounts () {
		if ((PlayerPrefs.GetInt(parentElement1) > 0 && PlayerPrefs.GetInt(parentElement2) > 0 && parentElement1 != parentElement2) || 
		    (parentElement1 == parentElement2 && PlayerPrefs.GetInt(parentElement1) >= 2)) {
			return true;
		} else {
			return false;
		}
	}
}