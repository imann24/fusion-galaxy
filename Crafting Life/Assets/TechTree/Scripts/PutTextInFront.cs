using UnityEngine;
using System.Collections;

public class PutTextInFront : MonoBehaviour {

	// Use this for initialization
	void Start () {
		this.GetComponent<Renderer> ().sortingOrder = 183;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
