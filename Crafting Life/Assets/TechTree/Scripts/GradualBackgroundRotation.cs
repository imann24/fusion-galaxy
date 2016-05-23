using UnityEngine;
using System.Collections;

public class GradualBackgroundRotation : MonoBehaviour {

	public float rotateRate;
	// Use this for initialization
	void Start () {
		rotateRate = 0.3f;
	}
	
	// Update is called once per frame
	void Update () {
		transform.rotation = Quaternion.Euler(new Vector3(0, 0, transform.rotation.eulerAngles.z+rotateRate));

	}
}
