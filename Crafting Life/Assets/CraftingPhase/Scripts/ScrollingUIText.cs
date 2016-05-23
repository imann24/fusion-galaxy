using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Text))]
public class ScrollingUIText : MonoBehaviour {
	public float scrollSpeed = 300;
	private float counter = 0;
	private string myString;
	private string spacer = "                                     ";
	Text myText;
	// Use this for initialization
	void Start () {
		myText = transform.GetComponent<Text>();
		myString = myText.text;
		myString += "         " + spacer;
	}
	
	// Update is called once per frame
	void Update () {
		counter+= scrollSpeed * Time.deltaTime;
		if (counter > 10000/scrollSpeed) {
			//changes the string
			myString = shiftStringLeft();
			myText.text = myString;

			//resets the counter
			counter = 0;
		}
	}
	public void UpdateStats(){
		myText = transform.GetComponent<Text>();
		myString = myText.text;
		myString += "         " + spacer;
	}
	private string shiftStringLeft () {
		return  myString.Substring(1) + myString[0];
	}

	//updates the text of the scrolling ui element
	public void setText (string text) {
		myString = text;
	}

	public void shiftGameObjectHorizontal (float amount) {
		transform.position += new Vector3 (amount, 0);
	}
}
