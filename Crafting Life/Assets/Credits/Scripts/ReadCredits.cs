//#define DEBUG
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ReadCredits : MonoBehaviour {
	public TextAsset credits;
	public GameObject TitleCredit;
	public GameObject NameCredit;
	public GameObject NameAndCollegeCredit;
	public GameObject ThreeNameCredit;
	public GameObject RoleCredit;
	public GameObject Spacer;

	public GameObject creditParent;
	private Credit[] creditees;
	// Use this for initialization
	void Start () {
#if DEBUG
		//Debug.Log(This is working: + "Header=Partners at Neuro'motion".Contains("="));
#endif
		readCSVFile();
		createCredits();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//reads in the text file and generates a list of credits
	private void readCSVFile () {
		//reads in the entire csv
		string [] allTextAsArray = credits.text.Split('\n');
		
		//splits it into people by line
		creditees = new Credit[allTextAsArray.Length-1];

		int personIndex = 0;

		//the loop should terminate before creditees.length, so we need a variable to resize the array
		int endIndex = creditees.Length;
		bool isGroup = false;
		//loops through the people and creates a list of credits
		for (int i = 0; i < creditees.Length; i++) {
			//exits the loop if we've hit the final credit
			if (personIndex >= allTextAsArray.Length) {
#if DEBUG
				Debug.Log("Breaking out of the loop here: " + i);
#endif
				endIndex = i;
				break;
			}

			string [] person = Utility.SplitString(allTextAsArray[personIndex], ',');
			personIndex++;

			if (isGroup) {
				if (person[0].Contains("=")) {
					isGroup = false;
				}
			}
			//if there is only a name
			if (isGroup)  { 
				string person2 = processGroupName(ref personIndex, allTextAsArray, ref isGroup);
				string person3 = processGroupName(ref personIndex, allTextAsArray, ref isGroup);
				creditees[i] = new Credit(Credit.Type.ThreeName, person[0], person2, person3);
			} else if (person.Length == 1) {
				//if the credit is a header
				if (person[0].Contains("Header")) {
					creditees[i] = new Credit(Credit.Type.Header, person[0].Split('=')[1]);
				} else if (person[0].Contains("Role")) {
					creditees[i] = new Credit(Credit.Type.Role, person[0].Split('=')[1]);
				} else if (person[0].Contains("Group")) {
					creditees[i] = new Credit(Credit.Type.Header, person[0].Split('=')[1]);
					isGroup = true;
				}else {
					creditees[i] = new Credit(Credit.Type.Name, person[0]);
				}
			} else if (person.Length == 2) { //if there's a name and a role
				creditees[i] = new Credit (Credit.Type.NameAndCollege, person[0], person[1]);
			}

			//trims the array of credits to the proper length
			System.Array.Resize(ref creditees, endIndex);

		}

		//trims the creditees array
//		creditees
	}

	/// <summary>
	/// Processes the name of groups of three elements
	/// </summary>
	/// <returns>the name of the invidual from the group</returns>
	/// <param name="personIndex">Person index used to index through the credits</param>
	/// <param name="allTextAsArray">All the CSV text as an array.</param>
	private string processGroupName (ref int personIndex, string [] allTextAsArray, ref bool isGroup) {
		if (personIndex >= allTextAsArray.Length || !isGroup) {
			return "";
		} else if (allTextAsArray[personIndex].Contains("=")) {
			isGroup = false;
#if DEBUG
			Debug.Log("*****"+allTextAsArray[personIndex]+"******");
#endif
			return "";
		} else {
			return allTextAsArray[personIndex++].Replace(",", "");
		}
	}

	/// <summary>
	/// Creates the credit gameobjects from the generated list of credit objects
	/// </summary>
	private void createCredits () {
		for (int i = 0; i < creditees.Length; i++) {
			GameObject creditText = null;
			GameObject spacer;
			if (creditees[i] == null) {
				//skips over null valeus
				continue;
			} else if (creditees[i].type == Credit.Type.Header) {
				//adds a spacer in before each title
				spacer = (GameObject) Instantiate(Spacer);
				spacer.transform.SetParent(creditParent.transform);
				spacer.transform.localScale = new Vector3(1,1,1);

				creditText = (GameObject) Instantiate(TitleCredit);
			} else if (creditees[i].type == Credit.Type.Name) {
				creditText = (GameObject) Instantiate(NameCredit);
			} else if (creditees[i].type == Credit.Type.NameAndCollege) {
				creditText = (GameObject) Instantiate(NameAndCollegeCredit);
			} else if (creditees[i].type == Credit.Type.Role) {
				creditText = (GameObject) Instantiate(RoleCredit);
			} else if (creditees[i].type == Credit.Type.ThreeName) {
				creditText = (GameObject) Instantiate(ThreeNameCredit);
			} 

			creditText.transform.SetParent(creditParent.transform);
			creditText.transform.localScale = new Vector3(1,1,1);
			creditText.GetComponent<ShowCredit>().setText(creditees[i]);

			if (i == creditees.Length - 1) { //for the last gameobject, add the scroll stopping script
#if DEBUG 
				Debug.Log("Got in here");
#endif
				StopScrollBar.lastCredit = creditText;
			}
		}

		//deletes all the prefab game objects
		Destroy(TitleCredit);
		Destroy(NameCredit);
		Destroy(NameAndCollegeCredit);
		Destroy(ThreeNameCredit);
		Destroy(RoleCredit);
	}
}
