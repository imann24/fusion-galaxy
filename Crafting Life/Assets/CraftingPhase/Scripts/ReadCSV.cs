using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//reads the CSV to generate the list of elements and the tech tree
public class ReadCSV : MonoBehaviour {
	//reference to the CSV file
	public TextAsset techTreeCSV;

	// runs earlier than to guarantee the game doesn't try to load in the gameobjects before they've been read in by the game
	void Awake () {
		if (!GlobalVars.CSV_READ) { //check's if it's already been read
			//to see whether you've unlocked the tier yet
			GlobalVars.TIER_UNLOCKED = new bool [GlobalVars.TIER_COUNT];
			GlobalVars.TIER_UNLOCKED[0] = true;
			GlobalVars.TIER_UNLOCKED[1] = true;

			//reads one element per line
			string [] elementsFromCSV = techTreeCSV.text.Split('\n');
			//process to read and create the elements 
			foreach (string line in elementsFromCSV) {
				string p1;
				string p2;
				string[]temp;
				string[]recipe;
				Element newElem;
				//element info is divided by commas
				temp = line.Split(new char[] {','});
				newElem = new Element(temp[0].ToLower());

				//cheat script
				//newElem.unlock();
				//PlayerPrefs.SetInt(newElem.getName()+GlobalVars.UNLOCK_STRING, 1);

				if (!GlobalVars.ELEMENTS_BY_NAME.ContainsKey(newElem.getName ())) {
					//adds to the list/dictionary of elements
					GlobalVars.ELEMENTS.Add(newElem);
					GlobalVars.ELEMENTS_BY_NAME.Add (newElem.getName(), newElem);
				}

				//reads in the combinations
				for (int i = 1; i < temp.Length; i++) {
					//marker for a base/tier 1 element
					if (temp[i].Contains("*")) {
						newElem.unlock();
						//cheat scripts
						PlayerPrefs.SetInt(temp[1]+GlobalVars.UNLOCK_STRING, 1);
						newElem.isBaseElement = true;
						continue;
					} else if (string.IsNullOrEmpty(temp[i]) || temp[i].Contains("(")|| (int)temp[i][0]==13) { //if the recipe is the empty string, or a note to the artists
						continue;
					}

					//parses the recipe string
					recipe = temp[i].Split(new char[]{'+'});
					p1 = recipe[0].Substring(0, recipe[0].Length-1);
					p2 = recipe[1].Substring(1);
					newElem.addCombination(new Combination(p1, p2, newElem));
					if (!GlobalVars.RECIPES_BY_NAME.ContainsKey(p1+p2)) {
						//dictionary lookup by a string of the combined element names that equal the new element
						GlobalVars.RECIPES_BY_NAME.Add(p1+p2, newElem);
						if (p1 != p2) { //adds the recipe in reverse, unless the element is created by combining two of the same 
							GlobalVars.RECIPES_BY_NAME.Add(p2+p1, newElem);
						}
					}
					//unlocks the element if it's already unlocked in memory
					if (PlayerPrefs.GetInt(temp[0].ToLower()+GlobalVars.UNLOCK_STRING)==1) {
						newElem.unlock();
						GlobalVars.TIER_UNLOCKED[newElem.queryTier()-1] = true;
					}
				}
			}

			//initiates the TIERS by ELEMENT 
			for (int i = 0; i < GlobalVars.TIER_COUNT; i++) {
				GlobalVars.ELEMENTS_BY_TIER[i] = new List<Element>();
			}

			//calculates the tiers for each element
			foreach (Element e in GlobalVars.ELEMENTS) {
				GlobalVars.ELEMENTS_BY_TIER[e.queryTier()-1].Add(e);
				if (e.isElementUnlocked()) {
					GlobalVars.NUMBER_ELEMENTS_UNLOCKED++;
				}
			}
			//flag set to true for optimization
			GlobalVars.CSV_READ = true;
		}
	}
}
