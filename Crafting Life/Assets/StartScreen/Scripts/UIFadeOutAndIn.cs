/*
 * Fades UI elements in and out
 * Used on the splash screen "Tap to Continue" button
 */

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIFadeOutAndIn : MonoBehaviour {
	// Tuning variables
	public float fadeOutTime = 1.5f;
	public float fadeInTime = 0.6f;
	private float steps = 50;

	// Used to fade in and out
	private bool shouldFadeIn = false;
	private float pauseTimeBetween = 0.5f;
	private bool inTransition = false;

	// References to UI elements
	Image myImage;
	Button myButton;

	// Use this for initialization
	// Establishes references
	void Start () {
		myImage = transform.GetComponent<Image>();
		myImage.enabled = true;

		//reference to the button
		myButton = transform.GetComponent<Button>();
	}
	
	// Update is called once per frame
	void Update () {

		// Fades the element in and out
		if (!inTransition && myButton.enabled) {
			if (shouldFadeIn) {
		
				StartCoroutine (fadeIn (fadeInTime));
			} else {
				StartCoroutine (fadeOut (fadeOutTime));
			}
		}
		
	}
	
	//fades out the image
	IEnumerator fadeOut (float time) {
		inTransition = true;
		float increment = time/steps;
		for (int i = 0; i < steps; i++) {
			myImage.color = new Color(1f, 1f, 1f, 1 - (float)i/steps);
			yield return new WaitForSeconds(increment);
		}
		yield return new WaitForSeconds(pauseTimeBetween);
		shouldFadeIn = true;
		inTransition = false;
	}

	//fades in the image
	IEnumerator fadeIn (float time) {
		inTransition = true;
		float increment = time/steps;
		for (int i = 0; i < steps; i++) {
			myImage.color = new Color(1f, 1f, 1f, 0 + (float)i/steps);
			yield return new WaitForSeconds(increment);
		}
		shouldFadeIn = false;
		inTransition = false;
	}
}
