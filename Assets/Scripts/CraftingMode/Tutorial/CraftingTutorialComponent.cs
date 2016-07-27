/*
 * Author: Isaiah Mann
 * Description: Controls behaviour of objects in tutorials
 * Note: Most objects should have a Canvas component attached to them for this script to work correctly (unless they are on the NoSortList)
 */

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class CraftingTutorialComponent: TutorialComponent {

	static CraftingTutorialComponent () {
		CurrentTutorialSteps = new Dictionary<TutorialType, int>();

		for (int i = 0; i < Enum.GetNames(typeof(TutorialType)).Length; i++) {
			CurrentTutorialSteps.Add((TutorialType) i, 0);
		}
	}

	public static int topSortLayer = 500;
	public static Dictionary<TutorialType, List<CraftingTutorialComponent>> AllTutorialsComponents = new Dictionary<TutorialType, List<CraftingTutorialComponent>>();

	//game objects whose sorting layers should not be sorted
	private static string [] NoSortList = {"PowerUpButton(Clone)", "ElementClass"};
	private static TutorialType ActiveTutorialType = TutorialType.None;

	public TutorialType TutorialType;

	//checks if the children objects of each component contain this string and if so, turns them on and off
	private static string TutorialComponentName = "Tutorial";
	private static string ElementPanelName = "ElementPanel";

	//for the tutorials that involve element panels
	private static string GatheringTutorialElementPanelText = "1";
	private static string CraftingTutorialElementPanelText = "2";
	private static int CraftingMaxElementPanelsNeeded = 2;
	private static int BuyHintMaxElementPanelsNeeded = 1;
	public static int ElementPanelsActive = 0;

	//for sorting
	private bool onNoSortList;
	private int mySortLayer;
	private Canvas myCanvas;
	bool hidden;
	public bool Active {get; private set;}
	//for children
	private MaskableGraphic[] MyTutorialComponents;

	public static Dictionary<TutorialType, int> CurrentTutorialSteps;

	// Use this for initialization
	void Awake () {
		EstablishReferences();
		Initialize();
	}

	void OnDestroy () {
		Unitialize();
	}

	// WARNING: Returns null if there is no previous element
	public override TutorialComponent [] GetPrevious () {
		if (IsFirstStepInTutorial()) {
			return null;
		} else {
			return FindTutorialComponentByOffset(-1);
		}
	}

	// WARNING: Returns null if there is no next element
	public override TutorialComponent [] GetNext () {
		if (IsLastStepInTutorial()) {
			return null;
		} else {
			return FindTutorialComponentByOffset(1);
		}
	}

	public override TutorialComponent[] GetCurrent () {
		return FindTutorialComponentByOffset(0);
	}

	// Finds tutorial component based on offset from this component. Returns null if invalid amount
	TutorialComponent [] FindTutorialComponentByOffset (int offset) {
		try {
			return AllTutorialsComponents[TutorialType].FindAll((CraftingTutorialComponent component) => component.TutorialStep == TutorialStep + offset).ToArray();
		} catch {
			Debug.LogError(string.Format("Tutorial Component at offset {0} from {1} not found", offset, gameObject.name));
			return null;
		}
	}

	bool IsFirstStepInTutorial () {
		return TutorialStep <= 0;
	}

	bool IsLastStepInTutorial () {
		int zeroIndexOffset = 1;
		return TutorialStep + zeroIndexOffset >= AllTutorialsComponents[TutorialType].Count;
	}

	//gets an array of the children that are tutorial images in the gameobject
	private MaskableGraphic[] GetChildrenTutorialComponents () {
		List<int> indexes = new List<int>();
		int currentIndex = 0;
		foreach (MaskableGraphic image in GetComponentsInChildren<MaskableGraphic>()) {
			currentIndex++;
			if (image.gameObject.name.Contains(TutorialComponentName)) {
				indexes.Add (currentIndex);
			}
		}

		currentIndex = 0;
		int arrayIndex = 0;

		MaskableGraphic[] components = new MaskableGraphic[indexes.Count];
		foreach (MaskableGraphic image in GetComponentsInChildren<MaskableGraphic>()) {
			currentIndex++;
			if (indexes.Contains(currentIndex)) {
				components[arrayIndex++] = image; 
			}
		}

		return components;
	}

	//toggles the children on and off
	private void ToggleChildrenTutorialComponents (bool isActive) {
		foreach (MaskableGraphic image in MyTutorialComponents) {
			image.enabled = isActive;
			TryToggleAnimation(image, isActive);
		}
	}

	void ToggleRaycaster (bool isActive) {
		GraphicRaycaster raycaster = GetComponent<GraphicRaycaster>();
		if (raycaster != null) {
			raycaster.enabled = isActive;
		}
	}

	// Returns true if object contains an animation script
	bool TryToggleAnimation (MaskableGraphic image, bool isActive) {
		UIImageAnimation [] animations;
		if ((animations = image.GetComponents<UIImageAnimation>()) != null) {
			foreach (UIImageAnimation animation in animations) {
				if (!animation.Hidden && (animation.PlayOnTutorial == TutorialType.Any || animation.PlayOnTutorial == ActiveTutorialType)) {
					switch (isActive) {
					case true:
						animation.Play();
						break;
					case false:
						animation.Stop();
						break;
					}
				} else {
					animation.Stop();
					image.enabled = false;
				}
			}
			return true;
		} else {
			return false;
		}
	}

	//sets up the references
	//should be called at the beginning of the scene and when an object with the script attached is instantiated
	private void EstablishReferences () {
		//checks whether its on the no sort list
		onNoSortList = OnNoSortList(gameObject);
		if (!onNoSortList) {
			myCanvas = GetComponent<Canvas>();
			if (myCanvas == null) {
				myCanvas = gameObject.AddComponent<Canvas>();
			}
			mySortLayer = myCanvas.sortingOrder;
		}
		MyTutorialComponents = GetChildrenTutorialComponents();
	}

	//checks whether the gameobject that has the script attached is an element panel
	private bool IsElementPanel () {
		return GetComponent<SpawnerControl>() !=null;
	}

	bool IsTierButton () {
		return GetComponent<TierButtonDisplay>() != null;
	}

	//checks whether the associated element in the tutorial is insufficent
	private bool? ElementIsInsufficent (int neededAmount) {
		SpawnerControl spawnerControl = GetComponent<SpawnerControl>();

		//if this is not an element panel object with a SpawnerControl class attached
		if (spawnerControl == null || spawnerControl.getCurrentElement() == null) {
			return null;
		} else if (spawnerControl.getCurrentElement().GetInventoryCount() < neededAmount) {
			return true;
		} else {
			return false;
		}
	}

	//checks whether the associated element in the tutorial is unlocked
	private bool? ElementIsUnlocked () {
		SpawnerControl spawnerControl = GetComponent<SpawnerControl>();
		
		//if this is not an element panel object with a SpawnerControl class attached
		if (spawnerControl == null || spawnerControl.getCurrentElement() == null) {
			return null;
		} else {
			return spawnerControl.getCurrentElement().isElementUnlocked();
		}
	}

	//checks whether the hint is unlocked for an element 
	private bool? ElementHintIsUnlocked () {
		SpawnerControl spawnerControl = GetComponent<SpawnerControl>();

		if (spawnerControl == null) {
			return null;
		}

		if (spawnerControl.getCurrentElement() != null) {
			return Utility.PlayerPrefIntToBool(spawnerControl.getCurrentElement().getName()+GlobalVars.HINT_STRING);
		} else {
			return null;
		}
	}

	//checks whether any further element panels need to be added
	private bool ElementPanelsAtMax () {
		if (TutorialType == TutorialType.Crafting && ElementPanelsActive >= CraftingMaxElementPanelsNeeded) {
			return true;
		} else if (TutorialType == TutorialType.BuyHint && ElementPanelsActive >= BuyHintMaxElementPanelsNeeded) {
			return true;
		} else {
			return false;
		}
	}

	bool IsActiveStep () {
		return TutorialStep == CurrentTutorialSteps[TutorialType];
	}

	void CheckActive () {
		if (!IsActiveStep()) {
			DeactivateComponent();
		}
	}
	
	//establishes the reference to the proper tutorial
	public void Initialize () {
		//adds its to the dictionary of all the components in each tutorial
		if (!AllTutorialsComponents.ContainsKey(TutorialType)) {
			AllTutorialsComponents.Add(TutorialType, new List<CraftingTutorialComponent>());
		} else if (TutorialType == ActiveTutorialType && TutorialStep == 0) {
			ActivateComponent();
		}
		
		AllTutorialsComponents[TutorialType].Add(this);
	}
	
	//used as a constructor: changes the tutorial this component is associated with
	public void Reinitialize (TutorialType TutorialType) {
		Unitialize();

		//starts up the references again
		EstablishReferences();

		//changes the type
		this.TutorialType = TutorialType;
		
		//adds its to the dictionary of all the components in each tutorial
		if (!AllTutorialsComponents.ContainsKey(TutorialType)) {
			AllTutorialsComponents.Add(TutorialType, new List<CraftingTutorialComponent>());
		} else if (TutorialType == ActiveTutorialType) {
			ActivateComponent();
		}
		
		AllTutorialsComponents[TutorialType].Add(this);
	}
	
	//removes the component from the tutorial 
	public void Unitialize () {
		//removes the component from the tutorial it is currently associated with
		if (AllTutorialsComponents.ContainsKey(this.TutorialType)) { 
			AllTutorialsComponents[this.TutorialType].Remove(this);
		}

		DeactivateComponent();
		
	}

	//brings the component to the front of the sort order
	public void ActivateComponent () {
		if (hidden) {
			return;
		}

		//checks if the tutorial component is an element panel
		if (IsElementPanel()) {
			ToggleRaycaster(true);

			//checks if no further panels are needed
			if (!IsActiveStep() || ElementPanelsAtMax() ||
			   	(ElementIsInsufficent(1)==null?false:(bool)ElementIsInsufficent(1) && TutorialType == TutorialType.Crafting) ||
			    (ElementIsUnlocked()==null?false:(bool)ElementIsUnlocked() && TutorialType == TutorialType.BuyHint) ||
			    (ElementHintIsUnlocked()==null?false:(bool)ElementHintIsUnlocked() && TutorialType == TutorialType.BuyHint)) {
				return;
			} else {
				ElementPanelsActive++;
				ToggleChildrenTutorialComponents(true);
			}

		} else if (IsTierButton()) {
			int activeTierButton;
			try {
				activeTierButton = (int) CraftingTutorialController.ActiveTierButton;
			} catch {
				return;
			}
			TierButtonDisplay tierButton = GetComponent<TierButtonDisplay>();
			if (IsActiveStep() && tierButton.tierNumber == activeTierButton) {
				ToggleChildrenTutorialComponents(true);
			} else {
				return;
			}
		} else if (IsActiveStep()) {
			ToggleChildrenTutorialComponents(true);
		}
		// Needs to be set below the above check because certain elements should not be activated if there are a sufficient amount
		Active = true;
		if (onNoSortList) {
			return;
		} else {
			myCanvas.sortingOrder = topSortLayer;
			myCanvas.overrideSorting = true;
		}
	}

	//sends the component back to its original sorting layer
	public void DeactivateComponent () {
		Active = false;
		if (IsElementPanel() && ElementPanelsActive > 0) {
			ElementPanelsActive--;
		}

		ToggleChildrenTutorialComponents(false);

		if (IsElementPanel()) {
			ToggleRaycaster(false);
		}

		//does not change sorting if it is on the no sorting list
		if (onNoSortList) {
			return;
		}

		if (myCanvas != null) {
			myCanvas.sortingOrder = mySortLayer;
			myCanvas.overrideSorting = false;
		}
	}

	public void HideComponent () {
		DeactivateComponent();
		hidden = true;
	}

	public void ShowComponent () {
		ActivateComponent();
		hidden = false;
	}

	public void SetText (string text) {
		foreach (MaskableGraphic graphic in MyTutorialComponents) {
			if (graphic is Text) {
				((Text)graphic).text = text;
			}
		}
	}

	//checks whether a gameobject is on the no sort list
	public bool OnNoSortList (GameObject tutorialComponent) {
		foreach (string tutorialComponentName in NoSortList) {
			if (tutorialComponent.name.Contains(tutorialComponentName)) {
				return true;
			}
		}

		return false;
	}
	
	//brings all the components of a certain type to the front
	public static void ActivateTutorialComponents (TutorialType tutorialType) {
		ActiveTutorialType = tutorialType;
		ChangeTextOnElementPanels(tutorialType);

		foreach (CraftingTutorialComponent component in AllTutorialsComponents[tutorialType]) {
			component.ActivateComponent();
		}
	}

	//changes the count on the elment panels based on the tutorial
	public static void ChangeTextOnElementPanels (TutorialType tutorialType) {
		string text;
		if (tutorialType == TutorialType.Gathering) {
			text = GatheringTutorialElementPanelText;
		} else if (tutorialType == TutorialType.Crafting) {
			text = CraftingTutorialElementPanelText;
		} else {
			return;
		}

		foreach (CraftingTutorialComponent component in AllTutorialsComponents[tutorialType]) {
			if (component.gameObject.name.Contains(ElementPanelName)) {
				component.SetText(text);
			}
		}
	}

	//brings all the components in a tutorial back to their natural sort order
	public static void DeactivateTutorialComponents (TutorialType tutorialType) {
		//sets element panels to zero
		ElementPanelsActive = 0;

		ActiveTutorialType = TutorialType.None;

		foreach (CraftingTutorialComponent component in AllTutorialsComponents[tutorialType]) {
			component.DeactivateComponent();
		}
	}

	public static CraftingTutorialComponent GetStep (TutorialType type, int step) {
		return AllTutorialsComponents[type].Find((CraftingTutorialComponent component) => component.TutorialStep == step);
	}
}
