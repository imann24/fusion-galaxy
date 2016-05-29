using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChangeArrowScale : MonoBehaviour {
	// A float for how quickly the scale will change.
	private float scaleLerpSpeed = 1f;
	// A bool to let us know whether the arrow should be flasing or not.
	private bool isArrowFlashing = true;
	// 2 Vector3s to record the two scale sizes that the arrow will shift between.
	private Vector3 smallerSize, normalSize;
	// Take in the tutorialArrow gameObjects.
	public GameObject swipeTutorialArrow, swipeTutorialArrowTwo;
	// A bool to let us know whether the arrow should be shrinking or not.
	private bool isNormalSizeShrinking = true;
	//pointer to flashing coroutine 
	private IEnumerator flashingCoroutine;
	// The bucket gameobject for use with the MoveSwipeTutorialArrows.
	public GameObject farRightBucket;
	// A vector3 to hold the correctPosition of the second tutorial arrow.
	private Vector3 correctSecondArrowPosition = new Vector3(0,0,0);
	// Use this for initialization
	void Start () {
		// Setting the normal scale of the arrow.
		normalSize = swipeTutorialArrow.transform.localScale;
		// Taking a small amount away from the normal size.
		smallerSize = new Vector3 (normalSize.x - 0.2f, normalSize.y - 0.2f, normalSize.z - 0.2f);
	}
	// A function to start the arrows flashing.
	public void StartArrowFlashing(){
		isArrowFlashing = true;
		if(flashingCoroutine != null){
			StopCoroutine(flashingCoroutine);
		}
		StartCoroutine(flashingCoroutine = ChangingArrowScale ());
	}
	// A function to stop the flashingCoroutine coroutine.
	public void StopArrowFlashing(){
		isArrowFlashing = false;
		StopCoroutine (flashingCoroutine);
	}
	// Enabling the arrows.
	public void EnableFlashingArrows(){
		swipeTutorialArrow.SetActive (true);
		swipeTutorialArrowTwo.SetActive (true);
	}
	// Disabling the arrows.
	public void DisableFlashingArrows(){
		swipeTutorialArrow.SetActive (false);
		swipeTutorialArrowTwo.SetActive (false);
	}
	// Two random variables in the middle of no where.
	// Floats for timing the length a coroutine will run.
	private float currentTimeLerping;
	private float timeToReach = 5.0f;
	// Changes the scale of the arrows to appear as if they are flashing.
	IEnumerator ChangingArrowScale(){
		// While the time we are counting up has not reached timeToReach keep iterating through the coroutine.
		while(currentTimeLerping < timeToReach){
			// Just keep switching between shrinking and growing.
			if(swipeTutorialArrow.transform.localScale == normalSize){
				isNormalSizeShrinking = true;
			}
			else if(swipeTutorialArrow.transform.localScale == smallerSize){
				isNormalSizeShrinking = false;
				currentTimeLerping = 0;
			}
			if(isNormalSizeShrinking){
				swipeTutorialArrow.transform.localScale = Vector3.MoveTowards (swipeTutorialArrow.transform.localScale, smallerSize, Time.deltaTime * scaleLerpSpeed);
				swipeTutorialArrowTwo.transform.localScale = Vector3.MoveTowards (swipeTutorialArrow.transform.localScale, smallerSize, Time.deltaTime * scaleLerpSpeed);
			}
			else if(!isNormalSizeShrinking){
				swipeTutorialArrow.transform.localScale = Vector3.MoveTowards (swipeTutorialArrow.transform.localScale, normalSize, Time.deltaTime * scaleLerpSpeed);
				swipeTutorialArrowTwo.transform.localScale = Vector3.MoveTowards (swipeTutorialArrow.transform.localScale, normalSize, Time.deltaTime * scaleLerpSpeed);
			}
			currentTimeLerping += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
	}
	// A function to move the second tutorial arrow over to the new correct bucket.
	public void MoveSwipeTutorialArrows(){
		swipeTutorialArrowTwo.transform.position = farRightBucket.transform.position;
		correctSecondArrowPosition.x = farRightBucket.transform.position.x + 0.86f;
		correctSecondArrowPosition.y = farRightBucket.transform.position.y + 0.6f;
		swipeTutorialArrowTwo.transform.position = correctSecondArrowPosition;
	}
}
