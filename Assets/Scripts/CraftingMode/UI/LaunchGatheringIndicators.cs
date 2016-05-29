#define DEBUG
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//used to modify the gathering UI indicators of whether the user is ready to enter gathering
public class LaunchGatheringIndicators : MonoBehaviour {
	//colors to toggle the buttons on and off
	public Color ShipActiveColor;
	public Color ShipInactiveColor;
	
	private GameObject[] indicatorGameObjects;
	private Image[] indicatorShips;
	private Image[] indicatorCircles;
	private Text[] indicatorNumbers;
	
	// Use this for initialization
	void Start () {
		InitializeReferences();
		CaptureScript.OnToggleGatheringZone += UpdateButtons;
	}

	void OnDestroy () {
		CaptureScript.OnToggleGatheringZone -= UpdateButtons;
	}

	void UpdateButtons (int i, bool active) {
		if (active) {
			indicatorShips[i].color = ShipActiveColor;
			indicatorCircles[i].color = Color.white;
			indicatorNumbers[i].color = ShipInactiveColor;
		} else {
			indicatorShips[i].color = ShipInactiveColor;
			indicatorCircles[i].color = ShipInactiveColor;
			indicatorNumbers[i].color = Color.white;
		}
	}

	void InitializeReferences () {
		//intializes the indicator array
		indicatorGameObjects = new GameObject[transform.childCount];
		for (int i = 0; i < transform.childCount; i++) {
			if (Utility.HasNumber(transform.GetChild(i).name)) {
				indicatorGameObjects[i] = transform.GetChild(i).gameObject;
			}
		}
		indicatorGameObjects = Utility.TrimArray(indicatorGameObjects);
		indicatorShips = new Image[indicatorGameObjects.Length];
		indicatorCircles = new Image[indicatorGameObjects.Length];
		indicatorNumbers = new Text[indicatorGameObjects.Length];

		for (int i = 0; i < indicatorGameObjects.Length; i++) {
			indicatorShips[i] = indicatorGameObjects[i].transform.GetChild(0).GetComponent<Image>();
			indicatorCircles[i] = indicatorGameObjects[i].GetComponent<Image>();
			indicatorNumbers[i] = indicatorGameObjects[i].transform.GetChild(1).GetComponent<Text>();
			UpdateButtons(i, false);
		}
	}
}
