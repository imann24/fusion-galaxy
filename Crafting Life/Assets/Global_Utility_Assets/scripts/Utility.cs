/*
 * Utility contains an assortment of functions that are meant to assist with simple scripting/game logic tasks
 * All the contained methods are static: can be called without an instance reference
 * The methods mostly function on manipulating strings, arrays, and PlayerPref variables
 * The game uses PlayerPrefs to save all data and player settings
 */

//#define DEBUG

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility {

	// Used to Capitalize the name of the elements for display
	// Capitalizes the first letter of each word in the passed string
	//taken from: http://www.dotnetperls.com/uppercase-first-letter
	public static string UppercaseWords(string value) {
		char[] array = value.ToCharArray();
		// Handle the first letter in the string.
		if (array.Length >= 1)
		{
			if (char.IsLower(array[0]))
			{
				array[0] = char.ToUpper(array[0]);
			}
		}
		// Scan through the letters, checking for spaces.
		// ... Uppercase the lowercase letters following spaces.
		for (int i = 1; i < array.Length; i++)
		{
			if (array[i - 1] == ' ')
			{
				if (char.IsLower(array[i]))
				{
					array[i] = char.ToUpper(array[i]);
				}
			}
		}
		return new string(array);
	}

	//used to increase a player prefs integer by a specified amount
	//If the player prefs value is null, this will create a playerprefs variable with the passed value
	public static void IncreasePlayerPrefValue (string key, int value) {
		PlayerPrefs.SetInt(key, PlayerPrefs.GetInt(key) + value);
	}

	// overload method that increases a player prefs value by one
	// Usable when the player crafts a new element to add one to their inventory
	public static void IncreasePlayerPrefValue (string key) {
		PlayerPrefs.SetInt(key, PlayerPrefs.GetInt(key) + 1);
	}

	// Overloaded method that increases a float instead of an integer by the specified amount
	public static void IncreasePlayerPrefValue (string key, float value) {
		PlayerPrefs.SetFloat(key, PlayerPrefs.GetFloat(key) + value);
	}

	// Displays the load screen
	// Should be called before each Application.LoadLevel call to ensure the Loading Screen shows during load
	public static void ShowLoadScreen () {
		if (GlobalVars.LOAD_SCREEN != null) {
			GlobalVars.LOAD_SCREEN.GetComponent<ShowOnLoad>().showLoadScreen();
		}
	}

	// Indexes through the tiers querying whether they're unlocked
	// Technicall counts the number of tiers, though in the current implementation, the player cannot unlock a tier without unlocking all prior tiers
	public static int HighestTierUnlocked () {
		int highestTier = 1;
		for (int i = 1; i < GlobalVars.TIER_UNLOCKED.Length; i++) {
			if (GlobalVars.TIER_UNLOCKED[i]) {
				highestTier = i + 1;
			}
		}
		return highestTier;
	}

	// Parses a float representing a number of seconds into a time stamp
	// This is used to send time via MixPanel
	// TODO: find a format of time that MixPanel can read as numeric instead of a string: because MixPanel cannot reason about time in terms of strings
	//from: http://stackoverflow.com/questions/463642/what-is-the-best-way-to-convert-seconds-into-hourminutessecondsmilliseconds
	//converts playtime to proper format
	public static string SecondsToTimeString (float seconds) {
		TimeSpan time = TimeSpan.FromSeconds(seconds);
		//here backslash is must to tell that colon is
		//not the part of format, it just a character that we want in output
		return string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms", 
		                              time.Hours, 
		                              time.Minutes, 
		                              time.Seconds, 
		                              time.Milliseconds);
	}

	// Returns true or false if the string array contains the string
	public static bool ArrayContains (string [] stringArray, string stringToFind) {
		return System.Array.Exists(stringArray, s => {if( s == stringToFind) 
				return true;
			else 
				return false;
		});
	}

	// Generic method returns true or false if the array contains the object
	public static bool ArrayContains<T> (T [] arrayToSearch, T objectToFind) where T : System.IComparable<T> {
		return System.Array.Exists(arrayToSearch, s => {if( s.CompareTo(objectToFind) == 0) 
				return true;
			else 
				return false;
		});
	}

	// Returns a string array via the split array that excludes any empty strings returned 
	// Basically just a modified versioin of the split() method that eliminates any empty strings
	// Used to read in CSV files where empty strings are not important
	public static string [] SplitString (string targetString, char targetChar) {
		string [] untrimmedArray = targetString.Split(targetChar);
		string [] finalArray;
		int finalArrayLength = untrimmedArray.Length;
		int indexInFinalArray = 0;
		for (int i = 0; i < untrimmedArray.Length; i++) {
			// NOTE: there is some difference based on text encoding between Mac and PC:
			// The "or" operator here is intended to account for either OS ind order to detect empty strings
			if (string.IsNullOrEmpty(untrimmedArray[i]) || (int)untrimmedArray[i][0] == 13) {
				finalArrayLength--;
			}
		}

		finalArray = new string[finalArrayLength];

		for (int i = 0; i < untrimmedArray.Length; i++) {
			//skips the empty strings
			if (string.IsNullOrEmpty(untrimmedArray[i]) || (int)untrimmedArray[i][0] == 13) {
				continue;
			}

			finalArray[indexInFinalArray++] = untrimmedArray[i];
		}

		return finalArray;
	}

	// Used to trim object arrays of any null values that they contain
	public static T[] TrimArray<T> (T [] untrimmedArray) {
		T[] finalArray;
		int finalArrayLength = untrimmedArray.Length;
		int indexInFinalArray = 0;
		for (int i = 0; i < untrimmedArray.Length; i++) {
			if (untrimmedArray[i] == null) {
				finalArrayLength--;
			}
		}
		
		finalArray = new T[finalArrayLength];
		
		for (int i = 0; i < untrimmedArray.Length; i++) {
			//skips the empty strings
			if (untrimmedArray[i] == null) {
				continue;
			}
			
			finalArray[indexInFinalArray++] = untrimmedArray[i];
		}
		
		return finalArray;
	}

	//generics method that swaps two object positioins in an array
	public static void SwitchObjectsInArray<T> (T [] array, int index1, int index2) {
		T tempVariable = array[index1];
		array[index1] = array[index2];
		array[index2] = tempVariable;
	}

	//converts player pref int value to bool
	public static bool PlayerPrefIntToBool (string key) {
		if (PlayerPrefs.GetInt(key) == 1) {
			return true;
		} else if (PlayerPrefs.GetInt(key) == 0) {
			return false;
		} else {
			return false;
		}
	}

	//sets player pref int vaue like its a bool
	public static void SetPlayerPrefIntAsBool (string key, bool value) {
		if (value) {
			PlayerPrefs.SetInt(key, 1);
		} else {
			PlayerPrefs.SetInt(key, 0);
		}
	}

	//checks whether a string has a number in it
	public static bool HasNumber(string input) {
		return input.Any(x => Char.IsDigit(x));
	}

	public static Color SetColorTransparency (Color targetColor, float alphaValue) {
		return new Color (targetColor.r, targetColor.g, targetColor.b, alphaValue);
	}

	//pass a set of key values of element names and the needed amounts 
	public static bool SufficientElementsToPurchase (params KeyValuePair<string, int> [] elementCosts) {
		for (int i = 0; i < elementCosts.Length; i++) {
			if (PlayerPrefs.GetInt(elementCosts[i].Key) < elementCosts[i].Value) {
				return false;
			}
		}
		return true;
	}

	//pass a set of key values of element names and the needed amounts and the minimum amount of the elements that need to be sufficient
	public static bool SufficientElementsToPurchase (int necssaryElements, params KeyValuePair<string, int> [] elementCosts) {
		int insufficientCount = 0;
		for (int i = 0; i < elementCosts.Length; i++) {
			if (PlayerPrefs.GetInt(elementCosts[i].Key) < elementCosts[i].Value) {
				insufficientCount++;
			}
		}
	
		if (necssaryElements <= elementCosts.Length - insufficientCount) {
			return true;
		} else {
			return false;
		}
	}

	// Prints a debug statement, only if debug is defined in this class (comment out debug to remove all scripts)
	// Ultimately not very useful because the debug statements aren't associated with their corresponding lines
	public static void Log (string toPrint) {
#if DEBUG
		Debug.Log(toPrint);
#endif
	}
}
