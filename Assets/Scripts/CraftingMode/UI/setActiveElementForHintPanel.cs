using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class setActiveElementForHintPanel : MonoBehaviour {
	//I apologize for the long script name...

	public string myName;
	public GameObject nameObject;
	private MainMenuController mainScript;

	// Use this for initialization
	void Start () {
		nameObject = transform.parent.FindChild ("WhatAmI").gameObject;
		myName = nameObject.GetComponent<Text>().text;
		mainScript = GameObject.Find ("CraftingCamera").GetComponent<MainMenuController> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseUp(){

	}

	public void setActiveElem(){
		myName = nameObject.GetComponent<Text>().text;
		mainScript.activeElement = myName;
		mainScript.activePosition = this.gameObject.transform.position;
		Debug.Log ("activeElem set to: " + myName);
	}

}
