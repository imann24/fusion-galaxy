using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TimeBarCountdown : MonoBehaviour {
	private Image timeBarImage;
	private float timeRemaining;
	private float maxTime = GlobalVars.COLLECT_TIME;
	private CollectionTimer findingTimeRemaining;
	public Color firstColor, secondColor, thirdColor, fourthColor;
	private float colourChangeRate = 1f;
	// Use this for initialization
	void Start () {
		findingTimeRemaining = GameObject.Find ("Controller").GetComponent<CollectionTimer> ();
		timeRemaining = findingTimeRemaining.GetTimeRemaining ();
		timeBarImage = GetComponent<Image> ();

		/*firstColor = new Color(100f/255f,160f/255f,42f/255f,1);
		secondColor = new Color ();
		thirdColor = new Color ();
		fourthColor = new Color ();*/
	}
	
	// Update is called once per frame
	void Update () {
		timeRemaining = findingTimeRemaining.GetTimeRemaining ();
		timeBarImage.fillAmount = timeRemaining / maxTime;
		if(timeBarImage.fillAmount <= 1f && timeBarImage.fillAmount >= 0.75f){
			timeBarImage.color = Color.Lerp(timeBarImage.color, firstColor, Time.deltaTime * colourChangeRate);
		}else if(timeBarImage.fillAmount <= 0.75f && timeBarImage.fillAmount >= 0.5f){
			timeBarImage.color = Color.Lerp(timeBarImage.color, secondColor, Time.deltaTime * colourChangeRate);
		}else if(timeBarImage.fillAmount <= 0.5f && timeBarImage.fillAmount >= 0.25f){
			timeBarImage.color = Color.Lerp(timeBarImage.color, thirdColor, Time.deltaTime * colourChangeRate);
		}else if(timeBarImage.fillAmount < 0.25f){
			timeBarImage.color = Color.Lerp(timeBarImage.color, fourthColor, Time.deltaTime * colourChangeRate);
		}
		//timeBarImage.color = new Color32((byte)MapValues(timeRemaining, maxTime / 2, maxTime, 255, 0), 255, 0, 255);
	}
	private float MapValues(float x, float inMin, float inMax, float outMin, float outMax){
		return (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
	}
}