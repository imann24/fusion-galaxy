using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;

public class RandomGatherPlanet : MonoBehaviour {

	public Sprite defaultImage;
	public Sprite planet1,planet2,planet3,planet4,planet5,planet6,planet7,planet8,planet9,planet10,planet11;
	public Sprite[] planetSprites; 

	public string defaultTitle = "SURFACE COMPOSITON ARCHIVE";
	public string defaultSubTitle = "SELECT MATERIAL TO START RESEARCH";
	public string name1,name2,name3,name4,name5,name6,name7,name8,name9,name10,name11;
	public string[] planetNames;

	private Text displayTitle;
	private Text displaySubTitle;

	private Color noAlpha;
	private Color halfAlpha;
	private Color fullAlpha;

	private Color lightBlue = new Color (81.0f / 255.0f, 233.0f / 255.0f, 220.0f / 255.0f);

	int whichPlanet;
	Image planetImage;

	private enum Randomness{byTier,byComposition,byDominance,reallyRandom};

	// Use this for initialization
	void Start () {

		fullAlpha = new Color (1, 1, 1, 0);
		noAlpha = new Color (1, 1, 1, 1);
		halfAlpha = new Color (1, 1, 1, 0.5f);

		planetSprites = new Sprite[]{planet1,planet2,planet3,planet4,planet5,planet6,planet7,planet8,planet9,planet10,planet11};
		planetNames = new string[]{name1,name2,name3,name4,name5,name6,name7,name8,name9,name10,name11};
		planetImage = this.GetComponent<Image> ();

		displayTitle = GameObject.Find("Canvas/TopScreen/GatheringScreen/Title").GetComponent<Text>();
		displaySubTitle = GameObject.Find("Canvas/TopScreen/GatheringScreen/SubTitle").GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void setNoPlanet(){
		planetImage.color = fullAlpha;
		planetImage.sprite = defaultImage;
		displayTitle.text = defaultTitle;
		displaySubTitle.text = defaultSubTitle;
		displayTitle.fontSize = 30;
		displayTitle.color = Color.white;
		displaySubTitle.color = Color.white;
	}

	public void setPlanet(string [] selectedElements){
		planetImage.color = noAlpha;
		displaySubTitle.text = "";
		displayTitle.fontSize = 45;
		displayTitle.color = Color.white;
		//displayTitle.color = lightBlue;



		switch (Randomness.byTier) {

		case Randomness.byTier:
			whichPlanet = 1;
			foreach (string element in selectedElements) {
				whichPlanet = Mathf.Max (whichPlanet, GlobalVars.ELEMENTS_BY_NAME [element].getTier ());
			}
				whichPlanet--;


			if (Utility.ArrayContains(selectedElements,"life")&&
			    Utility.ArrayContains(selectedElements,"paradise")&&
			    Utility.ArrayContains(selectedElements,"ethics")&&
			    Utility.ArrayContains(selectedElements,"truth")){
				whichPlanet = 10;
				displayTitle.color = lightBlue;
			}
			planetImage.sprite = planetSprites [whichPlanet];
			displayTitle.text = planetNames[whichPlanet];
			break;
				


		case Randomness.byComposition:
			//not implemented
			break;

		case Randomness.byDominance:
			//not implemented
			break;


		case Randomness.reallyRandom:
			//not implemented
			break;

		}
	}
}






////the Gathering Planet script
//private RandomGatherPlanet planetScript;

//planetScript = GameObject.FindGameObjectWithTag ("GatherPlanet").GetComponent<RandomGatherPlanet> ();


/*private void setGatheringPlanet (bool active) {
	if (active) {
		planetScript.setPlanet (elementsInDropZones);
	} else {
		planetScript.setNoPlanet();
	}
}*/















