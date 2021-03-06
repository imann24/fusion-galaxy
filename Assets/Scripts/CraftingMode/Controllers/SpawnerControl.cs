using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//placed on an elementSpawner GameObject to create new elements from it
public class SpawnerControl : MonoBehaviour {
	//the slider pages
	//public ScrollRect scrollBar;
	const string unlockedBackground = "Unlocked/BackgroundUnlocked";

	public CraftingButtonController controller;
	public Text myName;
	public Text myNameShadow;
	public Text whatAmI;
	public Text myID;
	public Text myInventoryCount;
	public Image myIcon;
	public GameObject newPanel;
	private Element currentElement;
	//element info
	private string elementName;
	private int elementTier;
	//button info
	GraphicRaycaster raycaster;

	//component references
	Renderer myRenderer;

	public int PanelIndex {
		get {
			return transform.GetSiblingIndex ();
		}
	}

	void Awake () {
		Init();
	}

	void Init () {
		raycaster = GetComponent<GraphicRaycaster>();
	}
	
	public void ToggleRaycaster (bool isActive) {
		raycaster.enabled = isActive;
	}

	public void SetBackground (Sprite image) {
		if (elementName == null) {
			return;
		}

		if (GlobalVars.ELEMENTS_BY_NAME[elementName].isElementUnlocked()) {
			Transform background = transform.FindChild(unlockedBackground);
			Image backgroundImage = background.GetComponent<Image>();
			if (backgroundImage != null) {
				backgroundImage.sprite = image;
			}
		}
	}

	//sets the spawn to undo it
	void OnMouseExit () {
		myRenderer.material.color = Color.white;
		//scrollBar.enabled = true;
	}

	//creates a new draggeable element box
	public void spawn () {
		newPanel.SetActive(true);
		controller.drag(newPanel);
		newPanel.transform.GetChild (0).GetComponent<Image>().sprite = myIcon.sprite;
		newPanel.transform.GetChild (1).GetComponent<Text>().text = myInventoryCount.text;
	}
	//unlocks the element
	public void unlock () {
		//saves the unlock in the global variables script
		GlobalVars.ELEMENTS_BY_NAME[elementName].unlock();
		//saves the unlock to memory
		PlayerPrefs.SetInt(elementName+GlobalVars.UNLOCK_STRING, 1);
		//changes the sprite from the lock incon
		gameObject.GetComponent<SpriteRenderer>().sprite = GlobalVars.ELEMENT_SPRITES[elementName];
	}

	//determines whether the spawner is able to create another element
	private bool ableToSpawn () {
		if (GlobalVars.ELEMENTS_BY_NAME[elementName].isElementUnlocked() && PlayerPrefs.GetInt(elementName) >= elementTier) {
			return true;
		} else {
			return false;
		}
	}

	//sets the name of the element panel
	public void setName () {
		string s;
		myName.text = (s = Utility.UppercaseWords(currentElement.getName()));
		myNameShadow.text = s;
		elementName = currentElement.getName();
//		if (t.text.Length > 10) {
//			t.fontSize = 25;
//		} else if (t.text.Length > 7) {
//			t.fontSize = 35;
//		} else if (t.text.Length > 5) {
//			t.fontSize = 45;
//		} else {
//			t.fontSize = 50;
//		}
	}

	private void setID () {
		Text t = myID;
		t.text = "CLT";
		int i;
		if ((i = currentElement.getTier()) < GlobalVars.TIER_COUNT) {
			t.text += "0";
		}
		t.text += i;
		t.text += "E";
		if ((i = GlobalVars.ELEMENTS_BY_TIER[currentElement.getTier()-1].IndexOf(currentElement)) < 10) {
			t.text += "0";
		}
		t.text += (i+1);
	}

	public void setInventoryCount () {
		myInventoryCount.text = PlayerPrefs.GetInt (currentElement.getName()).ToString();
	}

	private void setIcon () {
		myIcon.sprite = GlobalVars.ELEMENT_SPRITES[currentElement.getName()];
	}

	public void OnMouseEnter () {
		//scrollBar.enabled = false;
	}

	//sets the element the panel is displaying
	public void setElement (Element e) {
		currentElement = e;
		whatAmI.text = Utility.UppercaseWords(e.getName());

		transform.FindChild("ElementName").gameObject.SetActive(true);
		setName();
		transform.FindChild("ElementName").gameObject.SetActive(false);

		togglePanelLock (!e.isElementUnlocked ());
		if (e.isElementUnlocked()) {
			updatePanel ();
		}
		if (PlayerPrefs.GetInt (e.getName ().ToLower () + "Hint") == 1) {
			//Debug.Log("showing hint for "+e.getName());
			transform.FindChild("ElementName").gameObject.SetActive(true);
			setName();
		}

	}

	//sets the panel locked or unlocked
	public void togglePanelLock (bool locked) {
		for (int i = 0; i < transform.childCount; i++) {
			if (transform.GetChild (i).name == "WhatAmI"){
				transform.GetChild(i).gameObject.SetActive(true);
			} else if (transform.GetChild (i).name == "TutorialComponents") { 
				transform.GetChild(i).gameObject.SetActive(true);
			} else{

			if (transform.GetChild (i).name == "BackgroundLocked") {
				transform.GetChild(i).gameObject.SetActive(locked);
			} else {
				transform.GetChild (i).gameObject.SetActive(!locked);
				}}
		}
	}

	//overloaded method to lock the panel
	public void togglePanelLock () {
		togglePanelLock(true);
	}

	public void updatePanel () {
		if (currentElement != null) {
			setName();
			//setID();
			setInventoryCount();
			setIcon ();
		}
	}

	public Element getCurrentElement () {
		return currentElement;
	}
}
