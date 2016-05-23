/*
 * Used to pan the background image behind the start/splash screen
 * Moves the background to the left
 * Starts from the left side of the image
 */

using UnityEngine;
using System.Collections;
using UnityEngine.UI; 

public class PanBackground : MonoBehaviour {

	// Used to stop the background from moving offscreen
	public float leftmostMarker;
	public float rightmostMarker;

	// Controls direction
	bool goLeft = false;

	// Speed
	public float moveAmount;

	// Used to change the position
	public Vector2 moveRight;
	public Vector2 moveLeft;
	public Vector2 goDirection;
	public float panSpeed;

	// A reference to the transform which allows you to reference position
	// Note: UI elements use RectTransforms instead of regular Transforms to do their positioning
	RectTransform myRect;
	float offset;

	// Use this for initialization
	void Start () {

		// Sets the speed
		moveAmount = 0.2f;
		moveRight = new Vector2 (moveAmount,0);
		moveLeft = new Vector2 (-moveAmount, 0);
		panSpeed = 1.0f;

		// Reference to the objects transform
		myRect = this.GetComponent<RectTransform> ();

		offset = 0;

		//image begins with left side on left line of screen
		leftmostMarker = -500;
		rightmostMarker = 500;

		// Sets initial direction
		goDirection = moveLeft;
	}
	
	// Update is called once per frame
	void Update () {

		// Moves the image 
		if ((myRect.anchoredPosition.x > rightmostMarker)||(myRect.anchoredPosition.x < leftmostMarker)) {

			// If the image moves all the way to the right
			if((myRect.anchoredPosition.x >= rightmostMarker) && goLeft){
				goDirection = moveLeft;
				goLeft=false;
			}

			// If the image moves all the way to the left 
			if((myRect.anchoredPosition.x <= leftmostMarker) && !goLeft){
				goDirection = moveRight;
				goLeft=true;
			}

			// Keeps the position limited to the borders of the image
			Mathf.Clamp(myRect.anchoredPosition.x,leftmostMarker+offset,rightmostMarker-offset);

		}

		// Changes the image position
		myRect.anchoredPosition += goDirection;

	}
}
