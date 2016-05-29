using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CountBeforeStart : MonoBehaviour {
	// A public button assigned in inspector.
	public Button pauseButton;
	// A variable for the speed at which the countdown gameObject falls.
	private float movementSpeed = 35;
	// A boolean for whether or not the countdown can fall.
	private bool canMove = true;
	// The start and end position of the countdown number.
	public GameObject startPosition, endPosition;
	// The length of the countdown.
	private int countdown = 2;
	// The numbers and GO! sprites in an array.
	private Sprite[] countdownSprites;
	// The sprite game objects for the numbers and GO!
	public Sprite number3,number2,number1,numberGo;
	// An array of animators to turn on the downArrrows animation
	public Animator[] arrowAnimators = new Animator[4];
	// The controller object from the scene, used to starting the power up tutorial.
	public GameObject controller;

	void Start () {
		// Initiating the countdownSprites array size.
		countdownSprites = new Sprite[4];
		countdownSprites [0] = numberGo;
		countdownSprites [1] = number1;
		countdownSprites [2] = number2;
		countdownSprites [3] = number3;
		// Make the pause button not interactable until the countdown has finished.
		pauseButton.interactable = false;
	}

	void Update () {
		// If the game isn't paused and you are allowed to move, start the countdown.
		if(GlobalVars.PAUSED && canMove){
			transform.position = Vector3.MoveTowards(transform.position, endPosition.transform.position,Time.deltaTime * movementSpeed);
		}
		// If the countdown reaches the y position of the endPosition object then pause movement and move it back up to startPosition.
		if(transform.position.y <= endPosition.transform.position.y && canMove){
			canMove = false;
			StartCoroutine("WaitForSeconds");
		}
	}
	// This method uses the sprite renender attached to the gameobject and changes it.
	void ChangeCountdownImage(){
		if (countdown >= 0) {
			this.gameObject.GetComponent<SpriteRenderer> ().sprite = countdownSprites[countdown--];
		} else {
			// If the countdown is less than or equal to 0 then start the game and destroy this countdown.
			pauseButton.interactable = true;
			GlobalVars.PAUSED = false;
			Destroy(this.gameObject);
			// Check if the power up tutorial should happen.
			//controller.GetComponent<GenerationScript>().CheckForPowerUpTutorial();
		}
	}
	// It changes the image based on the countdown number and array of sprites.

	// A coroutine that waits 0.6 seconds before reseting the countdowns position.
	IEnumerator WaitForSeconds(){
		yield return new WaitForSeconds(0.6f);
		transform.position = startPosition.transform.position;
		ChangeCountdownImage();
		canMove = true;
	}
	// It also allows the countdown to move downwards again.
}
