using UnityEngine;
using System.Collections;

public class CameraInitialZoom : MonoBehaviour {

	Camera camera;
	float zoomSpeed;
	float zoomRate;
	float screenViewport;
	bool uiRevealed;

	// Use this for initialization
	void Start () {
		camera = Camera.main;
		zoomSpeed = 1.2f;//.5f
		//set inital camera screen size
		camera.orthographicSize = 60;//30
		//set end camera screen size
		screenViewport = 9.0f;//7.1f;//10.5f
		zoomRate = 18;

		uiRevealed = false;

	}
	
	// Update is called once per frame
	void Update () {

		//camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, camera.orthographicSize + Input.GetAxis("Mouse ScrollWheel") * 1000, Time.deltaTime * zoomSpeed);

		if (camera.orthographicSize > screenViewport) {
			camera.orthographicSize = Mathf.Lerp (camera.orthographicSize, camera.orthographicSize - zoomRate, Time.deltaTime * zoomSpeed);
		} else {
			if (!uiRevealed){
				uiRevealed = true;
				GameObject.Find("CanvasMain").GetComponent<Canvas>().enabled = true;
				GradualBackgroundRotation backgroundRotateScript = GameObject.Find("backgroundMain").GetComponent<GradualBackgroundRotation>();
				backgroundRotateScript.rotateRate = 0.05f;
			}
		}


	}
}
