using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//used to display the lines that connect an element with the elements that create it
public class ShowElementCombinations : MonoBehaviour {
	//plays on click
	AudioSource click;
	public bool combinationsAreShown = false;

	string elementName;
	Element thisElement;

	GameObject resultElem;
	Image resultImage;
	Text resultElemLabel;

	GameObject formulaElem1;
	Image formulaElem1Image;
	Text formulaElem1Label;

	GameObject formulaElem2;
	Image formulaElem2Image;
	Text formulaElem2Label;

	SpriteRenderer myRenderer;

	GameObject subElem1;
	SpriteRenderer subElem1Renderer;
	string subElem1Name;
	
	GameObject subElem2;
	SpriteRenderer subElem2Renderer;
	string subElem2Name;

	// Use this for initialization
	void Start () {
		elementName = this.transform.name.Substring(0, transform.name.LastIndexOf('S'));
		thisElement = GlobalVars.ELEMENTS_BY_NAME [elementName];

		click = transform.GetComponent<AudioSource>();

		resultElem = GameObject.Find("CanvasMain/resultElem");
		resultImage = resultElem.GetComponent<Image> ();
		resultElemLabel = resultElem.GetComponentInChildren<Text> ();

		formulaElem1 = GameObject.Find("CanvasMain/formulaElem1");
		formulaElem1Image = formulaElem1.GetComponent<Image> ();
		formulaElem1Label = formulaElem1.GetComponentInChildren<Text> ();

		formulaElem2 = GameObject.Find("CanvasMain/formulaElem2");
		formulaElem2Image = formulaElem2.GetComponent<Image> ();
		formulaElem2Label = formulaElem2.GetComponentInChildren<Text> ();


		myRenderer = this.GetComponent<SpriteRenderer> ();
		if (!thisElement.isBaseElement) {
			subElem1 = GameObject.Find("slider/"+thisElement.getCombinations()[0][0]+"Spawner");
			if (subElem1 != null) {
				subElem1Renderer = subElem1.GetComponent<SpriteRenderer> ();
				subElem1Name = subElem1.name.Substring(0, subElem1.name.LastIndexOf('S'));
			}



			subElem2 = GameObject.Find("slider/"+thisElement.getCombinations()[0][1]+"Spawner");
			if (subElem2 != null) {
				subElem2Renderer = subElem2.GetComponent<SpriteRenderer> ();
				subElem2Name = subElem2.name.Substring(0, subElem2.name.LastIndexOf('S'));

			}
		}
			

	}
	
	// Update is called once per frame
	void Update () {
	}

	//if the element is unlocked, displays all the lines that connect it
	void OnMouseDown () {
		displayTreeLines (!combinationsAreShown);
		resultElemLabel.text = thisElement.getName();
		formulaElem1Label.text = "";
		formulaElem2Label.text = "";

		resultImage.sprite = myRenderer.sprite;
		formulaElem1Image.sprite = GlobalVars.ELEMENT_SPRITES["no-image"];
		formulaElem2Image.sprite = GlobalVars.ELEMENT_SPRITES["no-image"];

		if (GlobalVars.ELEMENTS_BY_NAME[elementName].getTier()== 1){
			formulaElem1Label.text = "*";
			formulaElem2Label.text = "*";
		}
		if (combinationsAreShown) {
			formulaElem1Image.sprite = subElem1Renderer.sprite;
			formulaElem2Image.sprite = subElem2Renderer.sprite;
			//TODO 
			formulaElem1Label.text = subElem1Name;
			formulaElem2Label.text = subElem2Name;
		}



	}


	public void displayTreeLines(bool turnLineOn ){
		if (thisElement.isElementUnlocked()) {
			if (!click.isPlaying) {
				click.Play ();
			}
			for (int i = 0; i < transform.childCount; i++) {
				if (transform.GetChild(i).name.Contains("Node")){
					transform.GetChild(i).GetComponent<LineRenderer>().enabled = turnLineOn;
				}
				combinationsAreShown = turnLineOn;
			}
		}

	}
}
