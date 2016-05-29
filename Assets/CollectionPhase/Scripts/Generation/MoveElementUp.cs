/*
 * Used to hover the element up for bucket switch
 * MoveElementDown should be disabled or its bool of isAllowedToMove set to false before this script is added to the element
 * Script is intended to be added with AddComponent<MoveElementUp>() and then removed with Destroy(GetComponent<MoveElementUp>()
 */

using UnityEngine;
using System.Collections;

public class MoveElementUp : MonoBehaviour {

	//reference to the controller
	GenerationScript generationScript;

	// Use this for initialization
	void Start () {
		//creates reference to controller
		generationScript = GlobalVars.GATHERING_CONTROLLER;
	}
	
	// Update is called once per frame
	void Update () {
		//moves the element upward at the instructed speed
		if(!GlobalVars.PAUSED) {
			transform.position += Vector3.up * Time.deltaTime * generationScript.elementMovementSpeed/1.5f * generationScript.defaultFallSpeedModifier;
		}
	}
}
