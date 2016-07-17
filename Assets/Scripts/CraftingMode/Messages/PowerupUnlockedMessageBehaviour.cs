using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PowerupUnlockedMessageBehaviour : MessageBehaviour {
	[Header("References")]
	public Text TitleText;
	public Text PowerupNameText;
	public Text PowerUpDescriptionText;
	public Image PowerupIcon;

	[Header("Tuning")]
	public string MessageTitle = "New Powerup Unlocked";

	public void Init (PowerUp powerUp) {
		SetTitle();
		SetIcon(powerUp);
	}

	void SetTitle () {
		TitleText.text = MessageTitle;
	}

	void SetIcon (PowerUp powerUp) {
		PowerupIcon.sprite = powerUp.GetSprite();
	}

	void SetPowerupText (PowerUp powerUp) {
		PowerupNameText.text = powerUp.name;
		PowerUpDescriptionText.text = powerUp.GetDescription();
	}

}
