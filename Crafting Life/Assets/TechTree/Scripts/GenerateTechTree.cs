using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//script to create the tech tree
public class GenerateTechTree : MonoBehaviour {
	//an array of lists used to organize the elements into tiers
	private List<Element> [] treeofElements = new List<Element> [GlobalVars.TIER_COUNT];
	private Dictionary <string, GameObject> elementsByName = new Dictionary<string, GameObject>();
	//an empty GameObject used to position lines to connect elements generated 
	private GameObject combinationConnectionNode;
	// Use this for initialization
	void Start () {
		string filePath = GlobalVars.FILE_PATH;

		//loads up the resources
		combinationConnectionNode = Resources.Load<GameObject>("prefabs/Node");
		Sprite noImage;
		//loads in the no image sprite if it doesn't exist already
		if (!GlobalVars.ELEMENT_SPRITES.ContainsKey("no-image")) {
			noImage = Resources.Load<Sprite>(filePath+"no-image");
			GlobalVars.ELEMENT_SPRITES.Add ("no-image", noImage);
		} else {
			noImage = GlobalVars.ELEMENT_SPRITES["no-image"];
		}
		Sprite locked = Resources.Load<Sprite>(filePath+"locked-element");
		GameObject theElement = Resources.Load (filePath+"ElementSpawner")as GameObject;
		//add in getting of values from collection

		//creates the empty lists for each tier
		for (int i = 0; i < GlobalVars.TIER_COUNT; i++) {
			treeofElements[i] = new List<Element>();
		}

		//adds the elements by tier
		for (int i = 0; i < GlobalVars.ELEMENTS.Count; i++) {
			treeofElements[GlobalVars.ELEMENTS[i].getTier()-1].Add(GlobalVars.ELEMENTS[i]);
		}

		//nested for loop to generate each tier as a column in the tree
		for (int i = 0; i< GlobalVars.TIER_COUNT; i++) {
			int elementsInTier = treeofElements[i].Count;
			float initialOffsetRotation = 30;
			float tierRotationOffset = 20;
			for (int j = 0; j < elementsInTier ; j++) {

				//creates and positions the element
				float baseCircleSize = 1.9f;
				float spacingFactor = 2.1f;
				float angle = initialOffsetRotation+(tierRotationOffset*i)+j*(360/(elementsInTier));
				float radius = i*spacingFactor+baseCircleSize;// or: float radius = .04f*angle;
				GameObject element = Instantiate (theElement, polarToCartesian(radius,angle), Quaternion.identity)as GameObject;
				//alternate polar equations
				// 2n petal graph even: r = aCos(n*Theta) or aSin(n*Theta)
				// n petal graph odd: r = aCos(n*Theta) or aSin(n*Theta)
				// archimedies spiral: r = Theta and Theta >= 0
				// curled circle: 1 + (1/k)Sin(k*Theta) and Theta between 0 and 2*pi


				//removes components specific to crafting scene and adds additional ones to display the lines that connect the elements
				Destroy(element.GetComponent<SpawnerControl>());
				element.AddComponent<ShowElementCombinations>();
				element.AddComponent<LineRenderer>();
				element.AddComponent<OrbitCenter>();
				string name = treeofElements[i][j].getName();
				elementsByName.Add(name, element);
				//updates the dictionary of sprites 
				if (!GlobalVars.ELEMENT_SPRITES.ContainsKey(name)) {
					Sprite s = Resources.Load<Sprite>(filePath+name);
					if (s == null) {
						GlobalVars.ELEMENT_SPRITES.Add (name, noImage);
					} else {
						GlobalVars.ELEMENT_SPRITES.Add (name, s);
					}
				}

				//sets the sprite depending on whether or not the element is unlocked
				if (treeofElements[i][j].isElementUnlocked()) {
					element.GetComponent<SpriteRenderer>().sprite = GlobalVars.ELEMENT_SPRITES[name];
				} else {
					element.GetComponent<SpriteRenderer>().sprite = locked;
				}
				element.name = treeofElements[i][j].getName()+GlobalVars.SPAWNER_STRING;
				element.transform.parent = transform;
			}
		}

		//creates the nodes to connect every element in a combination
		for (int i = 0; i < GlobalVars.RECIPES.Count; i++) {
			Combination r = GlobalVars.RECIPES[i];
			//sends references to the node object for the three gameobjects involved in every combination
			if (r.getParents() != null && GlobalVars.ELEMENTS_BY_NAME.ContainsKey(r.getParents()[0]) && 
			    GlobalVars.ELEMENTS_BY_NAME.ContainsKey(r.getParents()[1]) && GlobalVars.ELEMENTS_BY_NAME.ContainsKey(r.getResult().getName()))  {
				print ("working");
				Transform t1 = elementsByName[r.getResult().getName()].transform;
				Transform t2 = elementsByName[r.getParents()[0]].transform;
				Transform t3 = elementsByName[r.getParents()[1]].transform;
				GameObject n = (GameObject) Instantiate(combinationConnectionNode, t1.position, Quaternion.identity);
				n.transform.parent = t1;
				n.GetComponent<LineDrawer>().assignGameObjects(t2.gameObject, t3.gameObject);
			}
		}

		//flag to optimize performance and make sure sprites are only loaded once
		GlobalVars.SPRITES_LOADED = true;
	}

	//takes a radius and theta angle (in degrees) for polar coordinates
	//returns coordinates in x,y
	public Vector2 polarToCartesian (float radius, float theta){
		float thetaRadians = Mathf.Deg2Rad*theta;
		float x = radius*Mathf.Cos(thetaRadians);
		float y = radius*Mathf.Sin(thetaRadians);
		return new Vector2 (x, y);
	
	}

}
