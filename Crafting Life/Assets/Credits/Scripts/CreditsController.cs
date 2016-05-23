using UnityEngine;
using System.Collections;

public class CreditsController : MonoBehaviour {
	public delegate void BackButtonClickAction();
	public static BackButtonClickAction OnBackButtonClick;
	// Use this for initialization

	void Start () {
	}

	public void loadMenu () {
		if (OnBackButtonClick != null) {
			OnBackButtonClick();
		}
		Utility.ShowLoadScreen();
		Application.LoadLevel((int)GlobalVars.Scenes.Crafting);

		//CHEAT
		Cheats.IncreaseAllElements();
	}
}
