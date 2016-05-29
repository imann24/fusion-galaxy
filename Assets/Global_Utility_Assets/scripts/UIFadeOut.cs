using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//script to make the splash screen fade out
public class UIFadeOut : MonoBehaviour {
	public float fadeOutTime = 1.5f;
	private float steps = 50;
	Image myImage;
	// Use this for initialization
	void Start () {
		myImage = transform.GetComponent<Image>();
		myImage.enabled = true;
		StartCoroutine(fadeOut(fadeOutTime));
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//fades out the image
	IEnumerator fadeOut (float time) {
		float increment = time/steps;
		for (int i = 0; i < steps; i++) {
			myImage.color = new Color(1f, 1f, 1f, 1 - (float)i/steps);
			yield return new WaitForSeconds(increment);
		}
		Destroy(gameObject);
	}
}
