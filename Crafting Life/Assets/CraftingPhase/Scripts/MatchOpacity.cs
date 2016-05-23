using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MatchOpacity : MonoBehaviour {
	public Image mainImage;
	public Image [] childImages;
	public float transitionTime = 0.5f;
	public Button myButton;

	//color
	private Color activeColor;
	private Color inactiveColor;

	//links the events and establishes the colors
	void Start () {
		//event bindings
		CraftingButtonController.OnNotReadyToEnterGathering += ImagesInactive;
		CraftingButtonController.OnReadyToEnterGathering += imagesActive;

		activeColor = myButton.colors.normalColor;
		inactiveColor = myButton.colors.disabledColor;

		//sets the starting color to inactive
		foreach(Image i in childImages) {
			i.color = inactiveColor;
		}
	}

	//unlinks the events
	void OnDestroy () {
		//event unbindings
		CraftingButtonController.OnNotReadyToEnterGathering -= ImagesInactive;
		CraftingButtonController.OnReadyToEnterGathering -= imagesActive;
	}

	//fades the images in 
	private void imagesActive () {
		StartCoroutine(syncColor(transitionTime, inactiveColor, activeColor));
	}

	//fades the images out
	private void ImagesInactive () {
		StartCoroutine(syncColor(transitionTime, activeColor, inactiveColor));
	}

	//coroutine to smoothly fade in and out
	IEnumerator syncColor (float duration, Color startColor, Color endColor) {
		float steps = 20f;
		float timeStep = duration/steps;
		float time = 0;
		while (time < duration) {
			foreach (Image i in childImages) {
				i.color = Color.Lerp(startColor, endColor, time);
			}
			time+=timeStep;
			yield return new WaitForSeconds (timeStep);
		}
	}
}
