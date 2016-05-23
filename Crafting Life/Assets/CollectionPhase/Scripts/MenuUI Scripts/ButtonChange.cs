using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ButtonChange : MonoBehaviour {
	// Music buttons.
	public GameObject musicOnButton, musicOffButton, soundOnButton, soundOffButton;
	// Sprites of on and off buttons.
	public Sprite onSpriteOn, onSpriteOff, offSpriteOn, offSpriteOff; 

	//toggles the buttons to off if the music is currently set to off on load
	void Awake () {
		if (PlayerPrefs.GetInt("Music", 1) == 0) {
			TurnMusicOff();
		}

		if (PlayerPrefs.GetInt("SFX", 1) == 0) {
			TurnSoundOff();
		}
	}

	// Method to turn the music On button on.
	public void TurnMusicOn(){
		if (musicOnButton.GetComponent<Image> ().sprite != onSpriteOn) {
			musicOnButton.GetComponent<Image> ().sprite = onSpriteOn;
			musicOffButton.GetComponent<Image> ().sprite = offSpriteOff;
		}
	}
	// Method to turn the music Off button on.
	public void TurnMusicOff(){
		if (musicOnButton.GetComponent<Image> ().sprite != onSpriteOff) {
			musicOnButton.GetComponent<Image> ().sprite = onSpriteOff;
			musicOffButton.GetComponent<Image> ().sprite = offSpriteOn;
		}
	}
	// Method to turn the sound On button on.
	public void TurnSoundOn(){
		if (soundOnButton.GetComponent<Image> ().sprite != onSpriteOn) {
			soundOnButton.GetComponent<Image> ().sprite = onSpriteOn;
			soundOffButton.GetComponent<Image> ().sprite = offSpriteOff;
		}
	}
	// Method to turn the sound Off button off.
	public void TurnSoundOff(){
		if (soundOnButton.GetComponent<Image> ().sprite != onSpriteOff) {
			soundOnButton.GetComponent<Image> ().sprite = onSpriteOff;
			soundOffButton.GetComponent<Image> ().sprite = offSpriteOn;
		}
	}
}
