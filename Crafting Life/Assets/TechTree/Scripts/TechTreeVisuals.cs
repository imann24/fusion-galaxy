using UnityEngine;
using System.Collections;

public class TechTreeVisuals : MonoBehaviour {
	GameObject allElementsContainer;
	bool toggleLines;
	public float orbitChange;
	public float spinChange;
	// Use this for initialization
	void Start () {
		allElementsContainer = GameObject.Find ("slider");
		toggleLines = true;

		orbitChange = 20;
		spinChange = 1;
	}
	
	// Update is called once per frame
	void Update () {
	}

	//OnMouseDown determine which object was selected
	void OnMouseDown(){
		switch (gameObject.name) {
		
			case ("OrbitUp"):
					changeOrbitRate(orbitChange);
					break;
				

			case ("OrbitDown"):
					changeOrbitRate(-orbitChange);
					break;
				

			case ("SpinUp"):
				changeSpinRate(spinChange);
				break;


			case ("SpinDown"):
				changeSpinRate(-spinChange);
				break;


			case ("SeeFullGraph"):
				showAllLines();
				break;
			

		}

	}



	//showAllLines displays all the tree combination lines that are currently available
	//it may be tapped again to hide all visible lines of the tree
	public void showAllLines(){
		foreach (Transform element in allElementsContainer.transform) {
			ShowElementCombinations showScript = element.GetComponent<ShowElementCombinations> ();
			showScript.displayTreeLines (toggleLines);
			showScript.combinationsAreShown = toggleLines;
		}
			toggleLines = !toggleLines;

	}

	//changeOrbitRate increments or decrements the speed of an elements orbit
	//based on a positive or negative passed in value
	//incrementOrDecrement is an int which determines whether the element orbits faster or slower
	public void changeOrbitRate(float incrementOrDecrement){
		foreach (Transform element in allElementsContainer.transform) {
			OrbitCenter orbitScript = element.GetComponent<OrbitCenter> ();
			orbitScript.orbitRate += incrementOrDecrement;
		}
	}

	//changeSpinRate creates clockwise or counterclockwise spin on the element
	//based on a positive or negative passed in value
	//incrementOrDecrement is an int which determines which direction the element spins in
	public void changeSpinRate(float incrementOrDecrement){
		foreach (Transform element in allElementsContainer.transform){
			OrbitCenter orbitScript = element.GetComponent<OrbitCenter>();
			orbitScript.elementRate+=incrementOrDecrement;
		}

		
	}
}



