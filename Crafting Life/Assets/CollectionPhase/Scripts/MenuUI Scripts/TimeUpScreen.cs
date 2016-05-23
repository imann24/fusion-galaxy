using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class TimeUpScreen : MonoBehaviour {
	//tuning variables
	private float timeToCountUp = 1f;
	private int numberOfCountUpSteps = 20;

	//event refences
	public delegate void EndGatheringAction();
	public static event EndGatheringAction OnEndGathering;

	public GameObject elementName1,elementName2,elementName3,elementName4, elementTier1,elementTier2,elementTier3,elementTier4;
	// Change to Enum
	//public enum ElementNames{element1 };
	public GameObject element1, element2, element3, element4;
	public GameObject harvestText1, harvestText2, harvestText3, harvestText4;
	public GameObject missedText;
	public GameObject totalText1, totalText2, totalText3, totalText4;

	private int harvestCount1,harvestCount2,harvestCount3,harvestCount4;
	private int missCount;
	private int totalCount1,totalCount2,totalCount3,totalCount4;

	private int realHarvest1,realHarvest2,realHarvest3,realHarvest4;
	private int realMissed;
	private int realTotal1, realTotal2, realTotal3, realTotal4;

	public bool showNumbers = false;

	void Start () {
		//uncomment to set element amounts
		//PlayerPrefs.SetInt (PlayerPrefs.GetString ("ELEMENT1"),16) ;
		//PlayerPrefs.SetInt (PlayerPrefs.GetString ("ELEMENT2"),24);
		//PlayerPrefs.SetInt (PlayerPrefs.GetString ("ELEMENT3"),5);
		//PlayerPrefs.SetInt (PlayerPrefs.GetString ("ELEMENT4"),10);


		harvestCount1=harvestCount2=harvestCount3=harvestCount4 = 0;
		missCount = 0;
		totalCount1=totalCount2=totalCount3=totalCount4 = 0;

		//sets the count up variables
		/*for(int i = 0; i < 4; i++){
			elementName + i.ToString().GetComponent<Text>().text = PlayerPrefs.GetString("ELEMENT1");
			elementTier1.GetComponent<Text>().text =GlobalVars.ELEMENTS_BY_NAME [PlayerPrefs.GetString("ELEMENT1")].getTier().ToString();
			element1.GetComponent<Image>().sprite = (Sprite) Resources.Load(GlobalVars.FILE_PATH + PlayerPrefs.GetString("ELEMENT1"));
			totalText1.GetComponent<Text>().text = PlayerPrefs.GetInt(PlayerPrefs.GetString("ELEMENT1")).ToString();
		}*/
	}
	void Update () {

		/*if (showNumbers) {
			//this block makes the scores count upwards toward the total
			
			countUp(ref harvestCount1,realHarvest1);
			countUp(ref harvestCount2,realHarvest2);
			countUp(ref harvestCount3,realHarvest3);
			countUp(ref harvestCount4,realHarvest4);

			countUp(ref retainCount1,realRetain1);
			countUp(ref retainCount2,realRetain2);
			countUp(ref retainCount3,realRetain3);
			countUp(ref retainCount4,realRetain4);

			countUp(ref totalCount1,realTotal1);
			countUp(ref totalCount2,realTotal2);
			countUp(ref totalCount3,realTotal3);
			countUp(ref totalCount4,realTotal4);
			
			// Setting the number of elements harvested in a round.
			harvestText1.GetComponent<Text> ().text = harvestCount1.ToString ();
			harvestText2.GetComponent<Text> ().text = harvestCount2.ToString ();
			harvestText3.GetComponent<Text> ().text = harvestCount3.ToString ();
			harvestText4.GetComponent<Text> ().text = harvestCount4.ToString ();
			// Setting the number of retained elements in a round.
			retainText1.GetComponent<Text> ().text = retainCount1.ToString ();
			retainText2.GetComponent<Text> ().text = retainCount2.ToString ();
			retainText3.GetComponent<Text> ().text = retainCount3.ToString ();
			retainText4.GetComponent<Text> ().text = retainCount4.ToString ();
			// Setting the TOTAL Amount of the choosen elements.
			totalText1.GetComponent<Text> ().text = totalCount1.ToString ();
			totalText2.GetComponent<Text> ().text = totalCount2.ToString ();
			totalText3.GetComponent<Text> ().text = totalCount3.ToString ();
			totalText4.GetComponent<Text> ().text = totalCount4.ToString ();


		//if the count has reached the corect amounts
		if ((harvestCount1 == realHarvest1 && harvestCount2 == realHarvest2 && harvestCount3 == realHarvest3 && harvestCount4 == realHarvest4)&&
				(retainCount1 == realRetain1 && retainCount2 == realRetain2 && retainCount3 == realRetain3 && retainCount4 == realRetain4)&&
				(totalCount1 == realTotal1 && totalCount2 == realTotal2 && totalCount3 == realTotal3 && totalCount4 == realTotal4)){
			showNumbers = false;
		}
		}*/
	
	}
	public void TimesUp(){
		//calls the event reference
		if (OnEndGathering != null) {
			OnEndGathering();
		}

		//updates the amount for each element collected
		for (int i = 0; i < GlobalVars.SCORES.Length; i++) {
			Utility.IncreasePlayerPrefValue(PlayerPrefs.GetString("ELEMENT"+(i+1)), GlobalVars.SCORES[i]);
		}

		element1.GetComponent<Canvas> ().sortingOrder = 17;
		// Getting the element NAMES.
		elementName1.GetComponent<Text> ().text = PlayerPrefs.GetString ("ELEMENT1");
		elementName2.GetComponent<Text> ().text = PlayerPrefs.GetString ("ELEMENT2");
		elementName3.GetComponent<Text> ().text = PlayerPrefs.GetString ("ELEMENT3");
		elementName4.GetComponent<Text> ().text = PlayerPrefs.GetString ("ELEMENT4");
		// Getting the element TIERS.
		elementTier1.GetComponent<Text> ().text = "(TIER " + GlobalVars.ELEMENTS_BY_NAME [PlayerPrefs.GetString ("ELEMENT1")].getTier ().ToString () + ")";
		elementTier2.GetComponent<Text> ().text = "(TIER " + GlobalVars.ELEMENTS_BY_NAME [PlayerPrefs.GetString ("ELEMENT2")].getTier ().ToString () + ")";
		elementTier3.GetComponent<Text> ().text = "(TIER " + GlobalVars.ELEMENTS_BY_NAME [PlayerPrefs.GetString ("ELEMENT3")].getTier ().ToString () + ")";
		elementTier4.GetComponent<Text> ().text = "(TIER " + GlobalVars.ELEMENTS_BY_NAME [PlayerPrefs.GetString ("ELEMENT4")].getTier ().ToString () + ")";
		// Changing the sprite to the choosen element SPRITE.
		element1.GetComponent<Image> ().sprite = Resources.Load<Sprite> (GlobalVars.FILE_PATH + PlayerPrefs.GetString ("ELEMENT1"));
		element2.GetComponent<Image> ().sprite = Resources.Load<Sprite> (GlobalVars.FILE_PATH + PlayerPrefs.GetString ("ELEMENT2"));
		element3.GetComponent<Image> ().sprite = Resources.Load<Sprite> (GlobalVars.FILE_PATH + PlayerPrefs.GetString ("ELEMENT3"));
		element4.GetComponent<Image> ().sprite = Resources.Load<Sprite> (GlobalVars.FILE_PATH + PlayerPrefs.GetString ("ELEMENT4"));

		showNumbers = true;

		realHarvest1 = GlobalVars.SCORES [0];
		realHarvest2 = GlobalVars.SCORES [1];
		realHarvest3 = GlobalVars.SCORES [2];
		realHarvest4 = GlobalVars.SCORES [3];

		realMissed = GlobalVars.MISSED;

		realTotal1 = PlayerPrefs.GetInt (PlayerPrefs.GetString ("ELEMENT1"));
		realTotal2 = PlayerPrefs.GetInt (PlayerPrefs.GetString ("ELEMENT2"));
		realTotal3 = PlayerPrefs.GetInt (PlayerPrefs.GetString ("ELEMENT3"));
		realTotal4 = PlayerPrefs.GetInt (PlayerPrefs.GetString ("ELEMENT4"));
		
		//updates scores with coroutines
		StartCoroutine(countUp(harvestText1.GetComponent<Text>(),harvestCount1, realHarvest1, timeToCountUp, numberOfCountUpSteps));
		StartCoroutine(countUp(harvestText2.GetComponent<Text>(),harvestCount2, realHarvest2, timeToCountUp, numberOfCountUpSteps));
		StartCoroutine(countUp(harvestText3.GetComponent<Text>(),harvestCount3, realHarvest3, timeToCountUp, numberOfCountUpSteps));
		StartCoroutine(countUp(harvestText4.GetComponent<Text>(),harvestCount4, realHarvest4, timeToCountUp, numberOfCountUpSteps));

		StartCoroutine(countUp(missedText.GetComponent<Text>(),missCount, realMissed, timeToCountUp, numberOfCountUpSteps));

		StartCoroutine(countUp(totalText1.GetComponent<Text>(),totalCount1, realTotal1, timeToCountUp, numberOfCountUpSteps));
		StartCoroutine(countUp(totalText2.GetComponent<Text>(),totalCount2, realTotal2, timeToCountUp, numberOfCountUpSteps));
		StartCoroutine(countUp(totalText3.GetComponent<Text>(),totalCount3, realTotal3, timeToCountUp, numberOfCountUpSteps));
		StartCoroutine(countUp(totalText4.GetComponent<Text>(),totalCount4, realTotal4, timeToCountUp, numberOfCountUpSteps));
	}



	public void countUp(ref int incrementMe, int untilThisNumber){
		if(incrementMe < untilThisNumber){
			incrementMe++;
		}

	}
	//coroutine method to count up the scores
	IEnumerator countUp (Text targetText, int targetValue, int finalValue, float time, int steps) {
		//makes sure we're not dividing by zero
		float timeStep = time/(float)steps;
		int valueStep = Mathf.Clamp(finalValue/steps, 1, int.MaxValue);
		if (finalValue > 0) {
			//determines the timestep between each count up
			while (targetValue < finalValue + valueStep) {
				//increases value by the specified amount
				targetValue+= valueStep;
				//changes text
				targetText.text = targetValue.ToString();
				yield return new WaitForSeconds(timeStep);
			}
			targetValue = finalValue;
			targetText.text = targetValue.ToString();
		}
	}

}
