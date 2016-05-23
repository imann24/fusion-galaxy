using UnityEngine;
using System.Collections;

public class RotateBackgroundPlanet : MonoBehaviour {
	private float rotateAmount = 2;
	void Update () {
		this.gameObject.transform.Rotate (0, 0, rotateAmount * Time.deltaTime);
	}
}
