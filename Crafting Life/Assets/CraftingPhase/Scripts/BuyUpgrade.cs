/*
 * This script is used to purchase an upgrade for the powerups
 * The powerups are not generated in the crafting scene until the player clicks the powerup button
 * The generation itself is done in GeneratePowerUpList
 * Until that script runs, no instances of this class exist in the Crafting scene
 */

/// <summary>
/// DEBUG is a preprocessor directive used in many scripts to print debugging statements and perform other debugging actions
/// Commenting it out will also comment out all the code wrapped in #if DEBUG statements
/// It can then be uncommented again when debugging is needed again
/// </summary>
//	#define DEBUG

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class BuyUpgrade : MonoBehaviour {
	// An event call for when the player upgrades the powerups
	// Used for analytics and SFX at current
	public delegate void PowerUpUpgradeAction (string powerupName, int powerUpLevel);
	public static event PowerUpUpgradeAction OnPowerUpUpgrade;

	// The powerup the instance is associated iwth
	private PowerUp myPower;

	// Each powerup costs four elements to upgrade
	private string myElem1,myElem2,myElem3,myElem4;
	private int myCost1,myCost2,myCost3,myCost4;

	// A reference the script
	public GeneratePowerUpList powerListScript;

	private const int numCosts = 4;
	
	//when the upgrade option is hit for a powerUp
	public void purchaseUpgrade(){
		powerListScript = GameObject.Find ("EventSystem").GetComponent<GeneratePowerUpList> ();
#if DEBUG
		Debug.Log (GameObject.Find ("EventSystem"));
		Debug.Log (powerListScript);
		Debug.Log ("clicked");
		Debug.Log (myElem1 + " : " + PlayerPrefs.GetInt (myElem1));
		Debug.Log (myElem2 + " : " + PlayerPrefs.GetInt (myElem2));
		Debug.Log (myElem3 + " : " + PlayerPrefs.GetInt (myElem3));
		Debug.Log (myElem4 + " : " + PlayerPrefs.GetInt (myElem4));
#endif
		//runs the powerup upgrade if there's enough
		if (PlayerPrefs.GetInt (myElem1) >= myCost1 &&
			PlayerPrefs.GetInt (myElem2) >= myCost2 &&
			PlayerPrefs.GetInt (myElem3) >= myCost3 &&
			PlayerPrefs.GetInt (myElem4) >= myCost4) {

			PlayerPrefs.SetInt(myElem1,PlayerPrefs.GetInt (myElem1)-myCost1);
			PlayerPrefs.SetInt(myElem2,PlayerPrefs.GetInt (myElem2)-myCost2);
			PlayerPrefs.SetInt(myElem3,PlayerPrefs.GetInt (myElem3)-myCost3);
			PlayerPrefs.SetInt(myElem4,PlayerPrefs.GetInt (myElem4)-myCost4);
			PowerUp.PromotePowerUp(myPower);
			Debug.Log ("bought");
			powerListScript.refreshList();

			//calls the event
			if (OnPowerUpUpgrade != null) {
				OnPowerUpUpgrade(myPower.name, myPower.level);
			}


		}


	}

	// Sets the powerup associated with this upgrader
	public void setPower(PowerUp thePower){
		this.myPower = thePower;
	}


	// Used to set each of the four elements associated with the upgrade
	public void setElem(string elem, int number){
		switch (number) {
		case 1:
			this.myElem1 = elem;
			break;
		case 2:
			this.myElem2 = elem;
			break;
		case 3:
			this.myElem3 = elem;
			break;
		case 4:
			this.myElem4 = elem;
			break;

		}
	}

	// Used to set the amoutns of the four elements needed to buy the upgrades
	public void setCost(int cost, int number){
		switch (number) {
			case 1:
			this.myCost1 = cost;
			break;
			case 2:
			this.myCost2 = cost;
			break;
			case 3:
			this.myCost3 = cost;
			break;
			case 4:
			this.myCost4 = cost;
			break;
			
		}
	}


	// Used to get the amounts of elements and their names
	// Can be used to check whether the player has enough by combining this with the Utility.SufficientElementsToPurchase() method
	public KeyValuePair<string, int>[] GetCosts () {
		KeyValuePair<string, int>[] costs = new KeyValuePair<string, int>[numCosts];
		costs[0] = new KeyValuePair<string, int>(myElem1, myCost1);
		costs[1] = new KeyValuePair<string, int>(myElem2, myCost2);
		costs[2] = new KeyValuePair<string, int>(myElem3, myCost3);
		costs[3] = new KeyValuePair<string, int>(myElem4, myCost4);
		return costs;
	}

	// Used to tell the game how much the first upgrade costs
	// It runs this check with Utility.SufficientElementsToPurchase() to see whether it's able to run the first tutorial
	public static KeyValuePair<string, int>[] GetBaseCosts () {
		KeyValuePair<string, int>[] costs = new KeyValuePair<string, int>[numCosts];
		for (int i = 0; i < numCosts; i++) {
			costs[i] = new KeyValuePair<string, int>(GeneratePowerUpList.FirstPowerUpElements[i], 
			                                         GeneratePowerUpList.FirstPowerUpCosts[i]);
		}
		return costs;
	}


}
