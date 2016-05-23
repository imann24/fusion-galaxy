/*
 * Moves the elements down in gathering mode
 * Their speed is determines by the fallSpeedModifier correspondig to their lane and the global fall speed
 * An element can be stopped by setting isAllowedToMove to false
 */

using UnityEngine;
using System.Collections;

//moves elements down in collection mode
public class MoveElementDown : MonoBehaviour {
	//the controller of the scene
	GenerationScript generationScript;

	//the lane number that the element is in, only updated on drag
	private int lane;

	//controls whether individual element can move
	public bool isAllowedToMove = true;

	// Use this for initialization
	void Start () {
		//reference to the controller in gathering mode
		generationScript = GlobalVars.GATHERING_CONTROLLER;

		//determines which lane the element is in
		lane = GlobalVars.GATHERING_CONTROLLER.whichLane(transform.position.x);
	}
	
	// Update is called once per frame
	void Update () {
		//moves the element down at arate sat by the modifiers and sped from the controller
		if(!GlobalVars.PAUSED && isAllowedToMove) {
			transform.position +=  Vector3.down * Time.deltaTime * generationScript.elementMovementSpeed * generationScript.fallSpeedModifiers[lane];
		}
	}

	//updates the lane tat the element is in: only called on MouseUp after a drag
	public void UpdateLane () {
		//determines which lane the element is in
		lane = GlobalVars.GATHERING_CONTROLLER.whichLane(transform.position.x);
	}
}
