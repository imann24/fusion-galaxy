using UnityEngine;
using System.Collections;

public class CameraPan : MonoBehaviour {
	//cite: http://answers.unity3d.com/questions/517529/pan-camera-2d-by-touch.html
	public float speed = 0.1F;
	void Update() {
		if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved) {
			Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;
			transform.Translate(-touchDeltaPosition.x * speed, -touchDeltaPosition.y * speed, 0);
		}
	}
}