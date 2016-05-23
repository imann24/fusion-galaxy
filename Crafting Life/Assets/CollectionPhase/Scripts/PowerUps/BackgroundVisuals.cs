using UnityEngine;
using System.Collections;

public class BackgroundVisuals : MonoBehaviour {
	SpriteRenderer myRenderer;
	Color currentColor;
	Color revertColor;
	float changeRate;
	bool lerpTheColors;

	// Use this for initialization
	void Start () {
		myRenderer = GetComponent<SpriteRenderer> ();
		currentColor = myRenderer.color;
		revertColor = currentColor;
		changeRate = 5.0f;
		lerpTheColors = false;

	}
	
	// Update is called once per frame
	void Update () {
		currentColor = myRenderer.color;
		if (lerpTheColors){
			if (currentColor != revertColor) {
				currentColor = Color.Lerp(currentColor,revertColor,Time.deltaTime*changeRate);
				myRenderer.color = currentColor;
			}
			if (currentColor == revertColor){
				lerpTheColors = false;
			}
		}
	}

	//after powerUp is used, set the screen back to normal
	public void revertScreen(){
		revertColor = Color.white;
		lerpTheColors = true;
	}


	//this function should call all other power-up game effects as Coroutines for the duration
	public void beginVisualCoroutine(float duration){
		StartCoroutine(doPowerUpVisualEffects(duration));
	}

	//do visual effects of the powerup (needed after object is destroyed)
	IEnumerator doPowerUpVisualEffects (float seconds) {
		yield return new WaitForSeconds(seconds);
		revertScreen();
		
	}
}
