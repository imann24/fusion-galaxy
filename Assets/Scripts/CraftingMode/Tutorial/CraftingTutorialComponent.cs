//#define DEBUG
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Canvas))]
public class CraftingTutorialComponent: TutorialComponent {

	static CraftingTutorialComponent () {
		CurrentTutorialSteps = new Dictionary<TutorialType, int>();

		for (int i = 0; i < Enum.GetNames(typeof(TutorialType)).Length; i++) {
			CurrentTutorialSteps.Add((TutorialType) i, 0);
		}
	}

	public static int topSortLayer = 300;
	public static Dictionary<TutorialType, List<CraftingTutorialComponent>> AllTutorialsComponents = new Dictionary<TutorialType, List<CraftingTutorialComponent>>();

	//game objects whose sorting layers should not be sorted
	private static string [] NoSortList = {"PowerUpButton(Clone)"};
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

	//for children
	private MaskableGraphic[] MyTutorialComponents;

	//for the buying hints tutorial
	private const string BuyingHintMessage = "Click a locked element panel to buy its hint";


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
	private void ToggleChildrenTutorialComponents (bool active) {
		foreach (MaskableGraphic image in MyTutorialComponents) {
			image.enabled = active;
		}
	}

	//sets up the references
	//should be called at the beginning of the scene and when an object with the script attached is instantiated
	private void EstablishReferences () {
		//checks whether its on the no sort list
		onNoSortList = OnNoSortList(gameObject);

		myCanvas = GetComponent<Canvas>();
		mySortLayer = myCanvas.sortingOrder;
		MyTutorialComponents = GetChildrenTutorialComponents();
	}

	//checks whether the gameobject that has the script attached is an element panel
	private bool IsElementPanel () {
		return GetComponent<SpawnerControl>()!=null;
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

		return Utility.PlayerPrefIntToBool(spawnerControl.getCurrentElement().getName()+GlobalVars.HINT_STRING);
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
		} else if (TutorialType == ActiveTutorialType) {
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
		//checks if the tutorial component is an element panel
		if (IsElementPanel()) {

			//checks if no further panels are needed
			if (ElementPanelsAtMax() ||
			   	(ElementIsInsufficent(1)==null?false:(bool)ElementIsInsufficent(1) && TutorialType == TutorialType.Crafting) ||
			    (ElementIsUnlocked()==null?false:(bool)ElementIsUnlocked() && TutorialType == TutorialType.BuyHint) ||
			    (ElementHintIsUnlocked()==null?false:(bool)ElementHintIsUnlocked() && TutorialType == TutorialType.BuyHint)) {
				return;
			}

		}

		if (IsActiveStep()) {
			ToggleChildrenTutorialComponents(true);
			if (IsElementPanel()) {
				ElementPanelsActive++;
			}
		}

		if (onNoSortList) {
			return;
		}

		myCanvas.sortingOrder = topSortLayer;
		myCanvas.overrideSorting = true;
	}

	//sends the component back to its original sorting layer
	public void DeactivateComponent () {

		if (IsElementPanel() && ElementPanelsActive > 0) {
			ElementPanelsActive--;
		}

		ToggleChildrenTutorialComponents(false);

		//does not change sorting if it is on the no sorting list
		if (onNoSortList) {
			return;
		}

		if (myCanvas != null) {
			myCanvas.sortingOrder = mySortLayer;
		}
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
			if (tutorialComponent.name == tutorialComponentName) {
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

		if (tutorialType == TutorialType.BuyHint && ElementPanelsActive == 0) {
			DisplayTutorialMessage(TutorialType.BuyHint);
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

		HideTutorialMessage();
	}

	//shows the tutorial message
	public static void DisplayTutorialMessage (TutorialType tutorialType) {
		if (tutorialType == TutorialType.BuyHint) {
			CraftingTutorialController.SetTutorialMessageBoard(BuyingHintMessage);
		}
	}

	//hides the tutorial message
	public static void HideTutorialMessage () {
		CraftingTutorialController.HideTutorialMessageBoard();
	}

	public static CraftingTutorialComponent GetStep (TutorialType type, int step) {
		return AllTutorialsComponents[type].Find((CraftingTutorialComponent component) => component.TutorialStep == step);
	}
}
