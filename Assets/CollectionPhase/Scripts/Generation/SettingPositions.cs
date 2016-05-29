/*
 * Used to scale the elements and buckets to the screen size
 * They're divided equally into four columns on the screen
 */

using UnityEngine;
using System.Collections;

public class SettingPositions : MonoBehaviour {
	// A public array of game objects to hold the given lanes so we can set their positions.
	public GameObject[] lanes = new GameObject[4];

	// A public array of games objects to hold the spawners animation gameobject location and set it's y position based on screen height.
	public GameObject[] spawners = new GameObject[4];

	// A public array of game objects to hold the elements prefabs so we can change their scale.
	public GameObject[] elements = new GameObject[4];

	// The width of the screen based on the orthographic camera size.
	private float widthSetting;

	// The height of the screen based on the orthographic camera size.
	private float heightSetting;

	// The amount the screen is divided into, 10 fit well for portrait devices - may need revision.
	private float widthDivider = 10;

	// Height of the screen:
	// used to reason about world positions in terms of onscreen positions
	private Vector3 screenHeight;

	void Start(){
		//sets the screen height
		screenHeight = Camera.main.ScreenToWorldPoint (new Vector3 (0, Screen.height, 0));

		// Grabbing the screen width.
		widthSetting = (float)(Camera.main.orthographicSize * 2.0 * Screen.width / Screen.height);

		// Grabbing the screen height.
		heightSetting = (float)(Camera.main.orthographicSize * 2.0 * Screen.height / Screen.width);

		// A forloop to iterate through all of the arrays and set their various positions and scales.
		for(int i = 0; i < lanes.Length; i++){
			lanes[i].transform.position = LanePositions.GetLanePositions(-3.65f, -1f)[i];
			lanes[i].transform.localScale = new Vector3(widthSetting / widthDivider, widthSetting / widthDivider, widthSetting / widthDivider);
			spawners[i].transform.position = LanePositions.GetLanePositions(Mathf.Floor(screenHeight.y), -1f)[i];
			elements[i].transform.localScale = new Vector3(widthSetting / widthDivider,widthSetting / widthDivider,widthSetting / widthDivider);
		}
	}
}
