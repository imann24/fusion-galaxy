using UnityEngine;
using System.Collections;

public class LerpBackgroundColor : MonoBehaviour {

	Color currentColor;
	Color nextColor;

	float alphaOffset;
	float mostAlpha;
	float changeRate;

	SpriteRenderer backgroundSprite;

	// Use this for initialization
	void Start () {
		currentColor.r = 230.0f/255.0f;
		currentColor.g = 137.0f/255.0f;
		currentColor.b = 255.0f/255.0f;
		currentColor.a = 1.0f;//213.0f/255.0f;

		//alpha will be alphaOffset + (between 0 and mostAlpha)
		alphaOffset = .9f;
		mostAlpha = .1f;
		changeRate = 1;

		nextColor = currentColor;

		backgroundSprite = this.GetComponent<SpriteRenderer> ();
	
	}
	
	// Update is called once per frame
	void Update () {

		//change line colors over time
		if (currentColor!=nextColor){
			currentColor = Color.Lerp(currentColor,nextColor,Time.deltaTime*changeRate);
		}else{
			nextColor = new Color (Random.value, Random.value, Random.value, alphaOffset + (Random.value % mostAlpha));
		}
		backgroundSprite.color = currentColor;
	
	}
}
