/*
 * Controls the progress bars and percent progress through each tier in the crafting mode
 * Relies on an event call from the tier button dispalys down below
 * Should be kept in sync with the tier progress live
 */

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class ProgressBarController : MonoBehaviour {

	public Color noProgressColor; 

	private GameObject[] progressBars;
	private Image[] progressBarFills;
	private Text[] progressBarPerecents;

	void Awake () {
		InitializeReferences();
		TierButtonDisplay.OnTierProgressUpdated += UpdateTierProgress;
	}
	// Use this for initialization
	void Start () {

	}
	
	void OnDestroy () {
		TierButtonDisplay.OnTierProgressUpdated -= UpdateTierProgress;
	}
	
	void UpdateTierProgress (int i, float unlockedFraction) {

		//shows the tier progress as a fill bar
		if (i < progressBarFills.Length) {
			progressBarFills[i].fillAmount = unlockedFraction;
		} else {
			return;
		}

		//rounds the number to a proper perecent
		float percent = (float)Math.Round(unlockedFraction * 100, 1);
		progressBarPerecents[i].text = (percent%1==0?percent+".0":percent.ToString())+"%";

		if (unlockedFraction * 100 < 10) {
			progressBarPerecents[i].text = "0" + progressBarPerecents[i].text;
		}

		if (unlockedFraction == 0) {
			progressBarPerecents[i].color = noProgressColor;
		} else {
			progressBarPerecents[i].color = Color.white;
		}
	}
	
	void InitializeReferences () {
		//intializes the indicator array
		progressBars = new GameObject[transform.childCount];
		for (int i = 0; i < transform.childCount; i++) {
			if (transform.GetChild(i).name.Contains("Progress")) {
				progressBars[i] = transform.GetChild(i).gameObject;
	
				#if DEBUG
					Debug.Log(progressBars[i].name);
				#endif
			}
		}

		progressBars = Utility.TrimArray(progressBars);
		progressBarFills = new Image[progressBars.Length];
		progressBarPerecents = new Text[progressBars.Length];

		
		for (int i = 0; i < progressBars.Length; i++) {
			progressBarFills[i] = progressBars[i].transform.GetChild(0).GetComponent<Image>();
			progressBarPerecents[i] = progressBars[i].transform.GetChild(1).GetComponent<Text>();
		}
	}
}
