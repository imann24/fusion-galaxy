#define DEBUG
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//should have a Text component attached to object or children
public class ShowCredit : MonoBehaviour {
	public Text myText;
	public Text [] myChildrenTexts;
	// Use this for initialization
	void Start () {

#if DEBUG
		if (gameObject.name.Contains("Role")) {
			Debug.Log(transform.GetComponent<Text>() + " | " +  myText);
		}
#endif

	}

	/// <summary>
	/// Sets the text of the credit
	/// </summary>
	/// <param name="myCredit"> The intended credit to be displayed </param>
	public void setText (Credit myCredit) {
		myText = transform.GetComponent<Text>();
		myChildrenTexts = transform.GetComponentsInChildren<Text>();
		if (myCredit.type == Credit.Type.Header || myCredit.type == Credit.Type.Name || myCredit.type == Credit.Type.Role ) {
			if (myText != null) {
				myText.text = myCredit.name;
			} else {
				#if DEBUG
					Debug.Log(myText);
				#endif
			}
		} else if (myCredit.type == Credit.Type.NameAndCollege) {
			myText.text = myCredit.name;
			myChildrenTexts[1].text = myCredit.college;
		} else if (myCredit.type == Credit.Type.ThreeName) {
			for (int i = 0; i < myChildrenTexts.Length; i++) {
				myChildrenTexts[i].text = myCredit.names[i];
			}
		} 
	}
}
