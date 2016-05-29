using UnityEngine;
using System.Collections;

public class SetToOrigin : MonoBehaviour {
	GameObject background;
	// Use this for initialization
	void Start () {
		background = GameObject.Find ("slider");
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.position = background.transform.position;
	}
}
