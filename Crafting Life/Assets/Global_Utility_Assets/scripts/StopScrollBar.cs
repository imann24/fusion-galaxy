using UnityEngine;
using System.Collections;

//stops the scrollbar when it reaches the bottom
public class StopScrollBar : MonoBehaviour {
	bool onScreen;
	Vector3 maxHeight;
	public static GameObject lastCredit;

	void Start () {
		maxHeight = GetComponent<Camera>().ScreenToWorldPoint(new Vector3(0, 0, GetComponent<Camera>().nearClipPlane));
	}

	void Update () {
		if (lastCredit != null && lastCredit.transform.position.y > maxHeight.y) {
			ScrollDown.StopAllScrolling();
		}
	}
}
