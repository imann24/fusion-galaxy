using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//controls the pause condition/button in collect mode
public class TogglePause : MonoBehaviour {
	//event
	public delegate void ButtonPressAction();
	public delegate void PauseAction(bool paused);
	public static event PauseAction OnPause;
	public static event ButtonPressAction OnButtonPress;

	//scene references
	public GameObject playButton;
	public GameObject exitButton;
	private SpriteRenderer playIcon;
	private SpriteRenderer pauseIcon;
	public GameObject time;
	//game states
	private bool buttonShouldChange = false;
	private bool gameIsPaused = false;
	public Sprite pauseSprite, playSprite;
	public GameObject actualPauseButton;
	public GameObject pauseBackground;

	void Start () {
		// This is to fix a strange bug where even though the sorting layer is higher than other elements of the canvas it still appears behind them,
		// But is fixed as soon as you change the number.
		time.GetComponent<Canvas>().sortingOrder = 12;
	}

	void Update () {
		//checks if the button should change
		if (gameIsPaused != GlobalVars.PAUSED) {
			buttonShouldChange = true;
			gameIsPaused = GlobalVars.PAUSED;
		}
	}

	//note: this is not currently active, instead togglePause is called by the Unity UI event system
	void OnMouseDown () {
		togglePause();
	}

	// Changes the paused selection and turns on or off, and activates/deactivates the exit and menu buttons
	public void togglePause () {
		//calls the event
		if (OnButtonPress != null) {
			OnButtonPress();
		}
		GlobalVars.PAUSED = !GlobalVars.PAUSED;

		if (OnPause != null) {
			OnPause(GlobalVars.PAUSED);
		}

		if (GlobalVars.PAUSED) {
			playButton.SetActive(true);
			exitButton.SetActive(true);
			pauseBackground.SetActive(true);
			actualPauseButton.SetActive(false);
		} else {
			playButton.SetActive(false);
			exitButton.SetActive(false);
			pauseBackground.SetActive(false);
			actualPauseButton.SetActive(true);
		}
	}
}
