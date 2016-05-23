using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShowOnLoad : MonoBehaviour {
	public static ShowOnLoad instance;
	public Image myImage;
	public Animator loadingBarAnimator;
	private Canvas myCanvas;
	void Awake () {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad(gameObject);
		} else {
			Destroy (gameObject);
		}
	}
	// Use this for initialization
	void Start () {
		GlobalVars.LOAD_SCREEN = gameObject;
		myImage.enabled = false;
		DontDestroyOnLoad(gameObject);
		myCanvas = transform.GetComponent<Canvas>();
	}
	void Update(){
	}

	void OnLevelWasLoaded (int level) {
		myImage.enabled = false;
	}

	public void showLoadScreen () {
		//if(){
			if (loadingBarAnimator != null) {
				loadingBarAnimator.SetTrigger ("gameStarted");
			}
			myImage.enabled = true;
			myCanvas.sortingOrder = 100;
		//}
	}
	public IEnumerator ShowLoadingScreen(){
		yield return new WaitForSeconds (2f);
		showLoadScreen ();
	}

}
