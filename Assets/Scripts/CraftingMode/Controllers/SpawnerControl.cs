using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//placed on an elementSpawner GameObject to create new elements from it
public class SpawnerControl : MonoBehaviour {
	//the slider pages
	//public ScrollRect scrollBar;
	public CraftingButtonController controller;
	public Text myName;
	public Text myNameShadow;
	public Text whatAmI;
	public Text myID;
	public Text myInventoryCount;
	public Image myIcon;
	public GameObject myGlow;
	public GameObject newPanel;
	public GameObject mainCanvas;
	private Element currentElement;
	//element info
	private bool nameIsKnown = false;
	private string elementName;
	private int elementTier;
	private int inventoryElementCount;
	private static GameObject newElem;
	private static bool elementCreated;
	private bool panelIsLocked = false;
	//button info
	private bool shouldSpawnAnElement = false;

	//component references
	AudioSource buttonClick;
	Renderer myRenderer;

	void Start () {
	}

	//changes button color based on whether element is spawnable and plays sound
	void OnMouseDown () {
//		print("Should spawn");
//		GameObject draggeableElement = Instantiate(gameObject) as GameObject;
//		draggeableElement.GetComponent<Image>().color = new Color (1f, 1f, 1f, 0.5f);
//		draggeableElement.transform.parent = mainCanvas.transform;
//		print (draggeableElement.transform.parent.name );
//		draggeableElement.transform.localScale = new Vector2(0.1f, 0.1f);
//		string name;
//		int elemCount = PlayerPrefs.GetInt((name = currentElement.getName ()));
//		if (!GlobalVars.CRAFTING_ACTIVE) {
//			spawn();		
//		} else if (elemCount > 0) { //if there is enough elements to spawn
//			spawn();
//			PlayerPrefs.SetInt(name, --elemCount);
//		} else if (elemCount < 0) { //if the element count is too low
//			PlayerPrefs.SetInt(name, 0);
//		}
//
//		buttonClick.Play();
//		shouldSpawnAnElement = true;
//		if (ableToSpawn()) {
//			myRenderer.material.color = Color.gray;
//		} else {
//			myRenderer.material.color = Color.red;
//		}
	}

	//sets the spawn to undo it
	void OnMouseExit () {
		shouldSpawnAnElement = false;
		myRenderer.material.color = Color.white;
		//scrollBar.enabled = true;
	}

	//spawns an element if it's allowed to
	void OnMouseUp () {
//		if (shouldSpawnAnElement) {
//			if (ableToSpawn()) {
//				//spwans an element
//				spawn();
//				inventoryElementCount.spawn();
//			}
//			myRenderer.material.color = Color.white;
//			shouldSpawnAnElement = false;
//		}
	}

	//creates a new draggeable element box
	public void spawn () {
		newPanel.SetActive(true);
		controller.drag(newPanel);
		newPanel.transform.GetChild (0).GetComponent<Image>().sprite = myIcon.sprite;
		newPanel.transform.GetChild (1).GetComponent<Text>().text = myInventoryCount.text;
		//print("Should spawn");
//		draggeableElement.transform.parent = mainCanvas.transform;
		//draggeableElement.GetComponent<SpawnerControl>().setElement(currentElement);
		//draggeableElement.AddComponent<Drag>();
		//draggeableElement.transform.position = transform.position;
		//draggeableElement.transform.localScale = new Vector2 (1f, 1f);
		//draggeableElement.transform.parent = transform.parent;
		//Destroy (draggeableElement.GetComponent<Button>());
//		for (int i = 0; i < draggeableElement.GetComponentsInChildren<Image>().Length; i++) {
//			draggeableElement.GetComponentsInChildren<Image>()[i].color = new Color (1f, 1f, 1f, 0.5f);
//
//		}
	}
	//unlocks the element
	public void unlock () {
		//saves the unlock in the global variables script
		GlobalVars.ELEMENTS_BY_NAME[elementName].unlock();
		//saves the unlock to memory
		PlayerPrefs.SetInt(elementName+GlobalVars.UNLOCK_STRING, 1);
		//changes the sprite from the lock incon
		gameObject.GetComponent<SpriteRenderer>().sprite = GlobalVars.ELEMENT_SPRITES[elementName];
//		myName.unlock();
	}

	//used to add one element to inventory
	public void addToInventory () {
//		inventoryElementCount.add();
	}

	//spawns a new element when the spawner is clicked
//	private void spawn () {
//		//creates and positions
//		GameObject e = Instantiate (newElem, new Vector2 (this.transform.position.x, this.transform.position.y), Quaternion.identity)as GameObject;
//		e.transform.position += Vector3.forward *10;
//		
//		//loads the sprite
//		Sprite s = GlobalVars.ELEMENT_SPRITES[elementName];
//		e.GetComponent<SpriteRenderer>().sprite = s;
//		e.name = elementName;
//		e.transform.parent = transform;
//	}	

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
		panelIsLocked = locked;
	}

	//overloaded method to lock the panel
	public void togglePanelLock () {
		togglePanelLock(true);
	}

	public void updatePanel () {
		setName();
		//setID();
		setInventoryCount();
		setIcon ();
	}

	public Element getCurrentElement () {
		return currentElement;
	}
}
