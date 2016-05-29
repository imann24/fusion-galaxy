#define DEBUG

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PurchaseHint : MonoBehaviour {
	//event to purchase a hint
	public delegate void PurchaseHintAction(string unlockedElementName);
	public static event PurchaseHintAction OnPurchaseHint;

	public string elementHint; 
	
	public string myElem1,myElem2,myElem3,myElem4;
	public int myCost1,myCost2,myCost3,myCost4=99;
	private const int numberOfCosts = 4;
	public MainMenuController mainScript;
	// Use this for initialization
	void Start () {
		myElem1 = "fire";
		myElem2 = "water";
		myElem3 = "earth";
		myElem4 = "air";
		//Cheats.IncreaseAllElemens (200);
		//Cheats.LockAllElements ();

		mainScript = GlobalVars.CRAFTING_CONTROLLER;
		mainScript.SetTutorialPurchaseHint(this);
		gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void purchaseHint(){

		//calls the event for purchasing a hint
		if (OnPurchaseHint != null) {
			OnPurchaseHint(elementHint.ToLower());
		}

#if DEBUG
		Debug.Log (myElem1 + " : " + PlayerPrefs.GetInt (myElem1));
		Debug.Log (myElem2 + " : " + PlayerPrefs.GetInt (myElem2));
		Debug.Log (myElem3 + " : " + PlayerPrefs.GetInt (myElem3));
		Debug.Log (myElem4 + " : " + PlayerPrefs.GetInt (myElem4));
		Debug.Log ("hint bought for "+elementHint);
#endif

		if (PlayerPrefs.GetInt (myElem1) >= myCost1 &&
		    PlayerPrefs.GetInt (myElem2) >= myCost2 &&
		    PlayerPrefs.GetInt (myElem3) >= myCost3 &&
		    PlayerPrefs.GetInt (myElem4) >= myCost4 &&
		    PlayerPrefs.GetInt(elementHint.ToLower()+GlobalVars.HINT_STRING) != 1) {

			PlayerPrefs.SetInt(myElem1,PlayerPrefs.GetInt (myElem1)-myCost1);
			PlayerPrefs.SetInt(myElem2,PlayerPrefs.GetInt (myElem2)-myCost2);
			PlayerPrefs.SetInt(myElem3,PlayerPrefs.GetInt (myElem3)-myCost3);
			PlayerPrefs.SetInt(myElem4,PlayerPrefs.GetInt (myElem4)-myCost4);
			PlayerPrefs.SetInt(elementHint.ToLower()+GlobalVars.HINT_STRING,1);
			mainScript.callHintPanel();
			mainScript.loadTier(GlobalVars.ELEMENTS_BY_NAME[elementHint.ToLower()].getTier()-1,true);

				//do thing

			//refresh panel?

			//mainScript.updateElementNames;
			
		}
		
		
	}

	public string getCostElemType(int selector){
		string whichElem = "";
		switch (selector) {
		case 1:
			whichElem = myElem1;
			break;
		case 2:
			whichElem = myElem2;
			break;
		case 3:
			whichElem = myElem3;
			break;
		case 4:
			whichElem = myElem4;
			break;
		}

		return whichElem;
	}



	public KeyValuePair<string, int>[] GetCosts () {
		KeyValuePair<string, int> [] costs = new KeyValuePair<string, int>[numberOfCosts];
		costs[0] = new KeyValuePair<string, int>(myElem1, myCost1);
		costs[1] = new KeyValuePair<string, int>(myElem2, myCost2);
		costs[2] = new KeyValuePair<string, int>(myElem3, myCost3);
		costs[3] = new KeyValuePair<string, int>(myElem4, myCost4);
		return costs;
	}

	public void SetCosts (int cost) {
		myCost1 = myCost2 = myCost3 = myCost4 = cost;
	}

	
}
