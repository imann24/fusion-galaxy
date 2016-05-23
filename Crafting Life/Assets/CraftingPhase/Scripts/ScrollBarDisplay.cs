//#define DEBUG
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//changes the button and display of the scrollbar
public class ScrollBarDisplay : MonoBehaviour {
	//events
	public delegate void DraggedToGatheringAction();
	public delegate void DraggedToCraftingAction();
	public static event DraggedToGatheringAction OnDraggedToGathering;
	public static event DraggedToCraftingAction OnDraggedToCrafting;

	//speed of scroll 
	public float scrollSpeed = 5f;

	//gameobject references
	public Sprite gatheringButton;
	public Sprite craftingButton;
	public Image scrollButton;
	public Scrollbar scrollBar;
	public enum Mode {Crafting, Gathering};
	public static Mode mode = Mode.Gathering;
	public Text gatheringText;
	public Text craftingText; 

	//to keep track of state
	private bool coroutineRunning;

	void Awake () {
		if (!Utility.PlayerPrefIntToBool(GlobalVars.CRAFTING_TUTORIAL_WATCHED)) {
			//disables scrollbar until scene loads properly
			scrollBar.enabled = false;
		}
	}

	void Start () {
		changeButtonText();
	}

	private void enableScrollBar () {
		scrollBar.value = 0;
		scrollBar.enabled = true;
	} 

	private void disableScrollBar () {
		scrollBar.enabled = false;
	}

	//sets the correct button to the slider
	public void setScrollButton () {
		if (!coroutineRunning) {
			if (scrollBar.value > 0 && scrollBar.value < 0.5f) { //slide to gathering
				if (OnDraggedToGathering != null) {
					OnDraggedToGathering();
				}
				StartCoroutine(moveScrollBar(scrollSpeed, 1f));
			} else if (scrollBar.value < 1) { //slide to crafting
				if (OnDraggedToCrafting != null) {
					OnDraggedToCrafting();
				}
				StartCoroutine(moveScrollBar(-scrollSpeed, 0f));
			}
		}

		//toggles the mode and changes the sprite
		if (scrollBar.value > 0.5) {
			//scrollButton.sprite = craftingButton;
			mode = Mode.Gathering;
		} else {
			//scrollButton.sprite = gatheringButton;
			mode = Mode.Crafting;
		}

		changeButtonText();
	}

	//a function to slide the bar over to the other side if it is clicked
	public void scrollOnClick () {
		#if DEBUG
			Debug.Log("Trying to switch");
		#endif
		if (!scrollBar.enabled) {
			enableScrollBar();
		}
		if (scrollBar.value == 0) {
			StartCoroutine(moveScrollBar(scrollSpeed, 1f));
		} else if (scrollBar.value == 1f) {
			StartCoroutine(moveScrollBar(-scrollSpeed, 0f));
		}
	}
	
	//smoothly moves the bar left or right
	IEnumerator moveScrollBar (float speed, float target) {
		coroutineRunning = true;
		while (Input.touchCount != 0 || Input.GetMouseButton(0)) {
			yield return null;
		}
		while (Mathf.Abs(scrollBar.value - target) > 0.01f) {
			scrollBar.value = Mathf.Clamp(scrollBar.value + speed * Time.deltaTime, 0, 1f);
			yield return null;
		}
		coroutineRunning = false;
	}

	public void changeButtonText () {
		if (mode == Mode.Gathering) {
			gatheringText.enabled = true;
			craftingText.enabled = false;
		} else if (mode == Mode.Crafting) {
			craftingText.enabled = true;
			gatheringText.enabled = false;
		}
	}
}
