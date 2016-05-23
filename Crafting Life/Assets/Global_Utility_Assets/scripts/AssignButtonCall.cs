/*
 * A script that reassigns button references to other scripts
 * Similiar to AccessUIText but for buttons
 * Saves all the buttons into a static dictionary by GameObject name
 * Can also be used to assign functions to the buttons using Linq statements (passing blocks of code as arguments)
 * Used in the SDK scene to reassign references for Don't Destroy on Load implementation
 * Gameobject is preserved between scenes and reloads into scene without references to the in scene objects: 
 * this script can be used to reassign references to the button objects
 */

/// <summary>
/// DEBUG is a preprocessor directive used in many scripts to print debugging statements and perform other debugging actions
/// Commenting it out will also comment out all the code wrapped in #if DEBUG statements
/// It can then be uncommented again when debugging is needed again
/// </summary>
//#define DEBUG

using UnityEngine;
using UnityEngine.UI;
using System.Linq; // Uses Linq to assign functions to the button
using System.Collections;
using System.Collections.Generic;

// Requires a Button UI component on the attached GameObject to work
[RequireComponent(typeof(Button))]

public class AssignButtonCall : MonoBehaviour {
	// All the buttons saved as in a dictionary by GameObject name
	public static Dictionary<string,Button> AllButtons = new Dictionary<string,Button>();

	// Used to pass a function to the button to run
	public delegate void ButtonAction ();

	// Reference to the Button
	Button myButton;

	// Use this for initialization
	void Awake () {
		//adds reference to button
		myButton = GetComponent<Button>();

		//a global dictionary of all the buttons that have the script attached
		AllButtons.Add(gameObject.name, myButton);
	
	}

	// Removes static references when the gameobject is destroyed
	void OnDestroy () {
		//removes the button from the dictionary of buttons
		AllButtons.Remove(gameObject.name);
		myButton.onClick = null;
	}

	//adds a function to the button
	public void AssignButton (ButtonAction myAction) {
		myButton.onClick.AddListener(() => { myAction();});
#if DEBUG
		Debug.Log("Setting " + gameObject.name + " to this function: " + myAction);
#endif
	}

	//adds a function to the button (usable from outside the script)
	public static bool AssignButton (string nameOfButtonGameObject, ButtonAction myAction) {
		if (AllButtons.ContainsKey(nameOfButtonGameObject)) {
			AllButtons[nameOfButtonGameObject].GetComponent<AssignButtonCall>().AssignButton(myAction);
			return true;
		} else {
			return false;
		}
	}

	// A function that can be called on the class to return a button
	// (basically just a safe way of acecessing the dictionary stored in the class)
	public static Button GetButton (string gameObjectName) {
		if (AllButtons.ContainsKey(gameObjectName)) {
			return AllButtons[gameObjectName];
		} else {
			return null;
		}
	}
}
