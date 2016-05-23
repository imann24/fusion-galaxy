using System.Collections.Generic;
using UnityEngine;

//model for an element type
public class Element {
	//event call for unlocking life
	public delegate void LifeUnlockedAction();
	public static event LifeUnlockedAction OnLifeUnlocked;
	//element info
	private string elementName;
	public bool isBaseElement {get; set;}
	private bool tierIsKnown = false;
	private List<Combination> myCombinations = new List<Combination>();
	private int tier;
	private bool isUnlocked;
	private int numberOfCombinations;

	//constructor sets the name of the element
	public Element(string name) {
		this.elementName = name;
		isBaseElement = false;
		foreach(KeyValuePair<string, int> combo in GlobalVars.NUMBER_OF_COMBINATIONS){
			Debug.Log(combo.Key + " : " + combo.Value);
		}
		GlobalVars.NUMBER_OF_COMBINATIONS.TryGetValue(elementName, out numberOfCombinations);
	}

	//get the name of element
	public string getName () {
		return elementName;
	}

	//returns the tier number
	public int getTier () {
		if (tierIsKnown) {
			return tier;
		} else { //calculates the tier before returning if the tier is not already known
			return queryTier();
		}
	}

	//adds a recipe
	public void addCombination (Combination r) {
		GlobalVars.RECIPES.Add(r);
		myCombinations.Add(r);
	}

	//adds a recipe given two elements
	public void addCombination(Element p1, Element p2) {
		Combination r = (new Combination(p1.getName(), p2.getName(), this));
		GlobalVars.RECIPES.Add(r);
		myCombinations.Add(r);
	}

	//adds a recipe given two strings
	public void addCombination (string p1, string p2) {
		Combination r = new Combination(p1, p2, this);
		GlobalVars.RECIPES.Add(r);
		myCombinations.Add (r);
	} 

	//prints a elements name and info
	public override string ToString () {
		string elementInfo =  "{NAME: " + elementName + ", TIER: " + tier + ", RECIPES: ";
		for (int i = 0; i < myCombinations.Count; i++) {
			elementInfo += myCombinations[i].getParents()[0] + " + " + myCombinations[i].getParents()[1] + ", "; 
		}
		return elementInfo.Substring(0, elementInfo.Length - 2) + "}";
	}

	//used to check element equality
	public override bool Equals (object obj) {
		if (this.elementName == ((Element) obj).getName()) {
			return true;
		} else {
			return false;
		}
	}

	//returns whether the element is unlocked
	public bool isElementUnlocked () {
		if (Utility.PlayerPrefIntToBool(elementName)) {
			isUnlocked = true;
		}
		return isUnlocked;
	}

	//unlocks the elemnt
	public void unlock () {
		//event call for unlocking life
		if (elementName == "life" && OnLifeUnlocked != null) {
			OnLifeUnlocked();
		}
		isUnlocked = true;
	}

	//locks the element
	public void relock () {
		isUnlocked = false;
	}

	//calculates the tier via recursion, can also be used to get the tier
	public int queryTier () {
		if (tierIsKnown)
			return tier;
		if (isBaseElement) {
			//sets the tier
			tier = 1;
			tierIsKnown = true;
			return 1;
		}

		//calculates the max tier from the elements used to create element
		List<int> maxTierSeen = new List<int>();
		foreach (Combination r in myCombinations) {
			string [] temp = r.getParents();
			int t1 = 0;
			int t2 = 0;
			//recursively calls the queryTier method
			if (GlobalVars.ELEMENTS_BY_NAME.ContainsKey(temp[0])) {
				t1 = GlobalVars.ELEMENTS_BY_NAME[temp[0]].queryTier();
			}
			if (GlobalVars.ELEMENTS_BY_NAME.ContainsKey(temp[1])) {
				t2 = GlobalVars.ELEMENTS_BY_NAME[temp[1]].queryTier();
			}
			maxTierSeen.Add(Mathf.Max(t1, t2));
		}
		int t = 0;
		foreach (int i in maxTierSeen)
			if (i > t)
				t = i;

		//sets the tier
		tier = t+1;
		tierIsKnown = true;
		return t+1;
	}


	public List<string[]> getCombinations () {
		List<string[]> allCombinations = new List<string[]>(); 
		for (int i = 0; i < myCombinations.Count; i++) {
			allCombinations.Add (new string[2]);
			allCombinations[i][0] = myCombinations[i].getParents()[0];
			allCombinations[i][1] = myCombinations[i].getParents()[1];
		}
		return allCombinations;
	}

	//checks if all the elements in the 
	public static bool AllTierElementsUnlocked (int tierNumber) {
		foreach (Element element in GlobalVars.ELEMENTS_BY_TIER[tierNumber]) {
			if (!element.isUnlocked) {
				return false;
			}
		}

		return true;
	}

	// Public getter method for numberOfCombinations that an element may have
	public int GetNumberOfCombinations(){
		return numberOfCombinations;
	}

	public int GetInventoryCount () {
		return PlayerPrefs.GetInt(elementName);
	}
}
