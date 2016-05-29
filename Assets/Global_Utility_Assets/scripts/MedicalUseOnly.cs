using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//used to destroy assets that are only meant for the medical version of the game
public class MedicalUseOnly : MonoBehaviour {
	private Image zoneIndicator;

	void Awake () {
		if (!GlobalVars.MEDICAL_USE) {
			Destroy (gameObject);
		} else {
			//shows the gameobject
			Image myImage = gameObject.GetComponent<Image>();
			if (myImage != null) {
				myImage.enabled = true;
			}

			if (gameObject.name == "MedicalUI" && Application.loadedLevel == (int) GlobalVars.Scenes.Gathering) {
				//sets the zone indicator
				for (int i = 0; i < transform.childCount; i++) {
					if (transform.GetChild(i).name == "ZoneIndicator") {
						SDKEventManager.SetIndicatorLight((zoneIndicator = transform.GetChild(i).GetComponent<Image>()));
					}
				}
			}

			Text myText = gameObject.GetComponentInChildren<Text>();
			if (myText != null) {
				myText.enabled = true;
			}
			CollectionTimer.OnEndGame += HideIndicator;
		}
	}

	void OnDestroy () {
		CollectionTimer.OnEndGame -= HideIndicator;
	}
	
	/// <summary>
	/// Hides the indicator.
	/// </summary>
	private void HideIndicator () {
		zoneIndicator.enabled = false;
	}
}
