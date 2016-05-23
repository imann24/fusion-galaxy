/*
 * Used in the medical build to re-establish references to UI Text components
 * Combined with a DontDestroyOnLoad singleton object that persists between scenes 
 * Object is loaded into the scene and needs to re-find the text assets it was referring to
 * This script creates a dictionary of the Text components accessible by the name of the GameObject they're attached to
 * The Dictionary is static so can be referenced from anywhere without an instance reference
 * Script should be attached to attached to each text component in the scene that you want a reference to
 */


/// <summary>
/// DEBUG is a preprocessor directive used in many scripts to print debugging statements and perform other debugging actions
/// Commenting it out will also comment out all the code wrapped in #if DEBUG statements
/// It can then be uncommented again when debugging is needed again
/// </summary>
//#define DEBUG

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

//requires a UI Text Component: because the purpose of the script is to reference it
[RequireComponent(typeof(Text))]

/// <summary>
/// Access UI Text Components that have the script attached to them
/// </summary>
public class AccessUIText : MonoBehaviour {
	//every script object with this script attached
	public static Dictionary <string, Text> AllTextsByName = new Dictionary <string, Text>();

	//connection between the text object and its script
	private static Dictionary <Text, AccessUIText> ScriptsByText = new Dictionary <Text, AccessUIText>();

	//reference to my text component
	private Text myText;

	// Use this for initialization
	void Awake () {
		//sets reference to text component
		myText = GetComponent<Text>();

		//stores the text by its gameobject's name
		AllTextsByName.Add (gameObject.name, myText);

		//stores the script by its text
		ScriptsByText.Add (myText, this);

#if DEBUG 
		Debug.Log("this text has been added to the dictionary: " + gameObject.name);
#endif
	}

	//removes the Text Components from the Dictionary when there corresponding gameobject's are destroyed
	void OnDestroy () {
		//removes the text by its gameobject's name
		AllTextsByName.Remove(gameObject.name);

		//removes the script by its text
		ScriptsByText.Remove(myText);
	}

	//sets the text of the attached text component
	void SetMyText (string text) {
		myText.text = text;
	}

	//sets the text of the specified text object
	public static void SetText (string gameObjectName, string text) {
		ScriptsByText[AllTextsByName[gameObjectName]].SetMyText(text);
	}

	//returns the specified text object
	public static Text GetUIText (string gameObjectName) {
		return AllTextsByName[gameObjectName];
	}
}
