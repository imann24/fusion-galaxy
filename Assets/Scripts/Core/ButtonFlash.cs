using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonFlash : MonoBehaviour {
	private Text myButtonText;
	private Button myButton;
	public Color startColor = Color.white;
	public Color endColor = Color.grey;
	public float changeSpeed = 1.5f;
	private bool forward = true;
	private bool backward;
	// Use this for initialization
	void Start () {
		myButton = gameObject.GetComponent<Button>();
		myButtonText = gameObject.GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
		if (myButton.interactable) {
			if (forward) {
				myButtonText.color = Color.Lerp(startColor, endColor, changeSpeed * Time.deltaTime);
				if (myButtonText.color == endColor) {
					forward = false;
					backward = true;
				}
			} else if (backward) {
				myButtonText.color = Color.Lerp(endColor, startColor, changeSpeed * Time.deltaTime);
				if (myButtonText.color == startColor) {
					forward = true;
					backward = false;
				}
			}
		}
		print (myButtonText.color);
	}
}
