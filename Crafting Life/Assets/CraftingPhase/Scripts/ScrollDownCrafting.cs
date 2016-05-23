using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScrollDownCrafting : MonoBehaviour {

	public float speed = 900;
	private float scrollUpFactor = 4;
	private Vector3 startPosition;

	//assign a gameobject that the crafting panel will scroll to
	public GameObject endLocation;
	private float offset = 1f;

	public enum Mode {Crafting, Gathering};
	public static Mode mode = Mode.Gathering;
	private GameObject gatheringScreenObject;
	public GameObject craftingScreenObject;
	public GameObject scrollingText;
	// Taking in the craftAnimator image so I can manually reset it when we scroll up the crafting screen.
	public GameObject craftAnimatorImage;
	public Sprite blankImage;

	public GameObject resultZone;
	// Use this for initialization
	void Start () {
		gatheringScreenObject = GameObject.Find ("Canvas/TopScreen/GatheringScreen");
		startPosition = transform.position;
		StartCoroutine (StartSortOverride (gatheringScreenObject));
		StartCoroutine (StartSortOverride (craftingScreenObject));

	}
	// Update is called once per frame
	void Update () {
		if ((transform.position.y <= endLocation.transform.position.y - offset) ||
			(transform.position.y >= endLocation.transform.position.y + offset)) {
			transform.position = Vector3.MoveTowards (transform.position, endLocation.transform.position, Time.deltaTime * speed);
			gatheringScreenObject.GetComponent<Canvas> ().overrideSorting = false;
			craftingScreenObject.GetComponent<Canvas>().overrideSorting = true;
		} else {
			scrollingText.SetActive(false);
			gatheringScreenObject.SetActive (false);
			craftingScreenObject.GetComponent<Canvas>().overrideSorting = true;
			craftingScreenObject.GetComponent<Canvas>().overrideSorting = false;
		}
	}

	public void resetLocation(){
		StartCoroutine (scrollUp());
	}

	public IEnumerator scrollUp(){
		gatheringScreenObject.SetActive (true);
		craftingScreenObject.GetComponent<Canvas> ().overrideSorting = true;
		while((transform.position.y <= startPosition.y-offset)||
		   (transform.position.y >= startPosition.y +offset)){
			transform.position = Vector3.MoveTowards(transform.position,startPosition,Time.deltaTime * speed * scrollUpFactor);
		yield return new WaitForSeconds(0.015f);
		}
		craftAnimatorImage.GetComponent<Image>().sprite = blankImage;
		scrollingText.SetActive(true);
		craftingScreenObject.SetActive (false);
		craftingScreenObject.GetComponent<Canvas> ().overrideSorting = false;
		gatheringScreenObject.GetComponent<Canvas> ().overrideSorting = true;
		transform.position = startPosition;
		yield return null;
	}
	private IEnumerator StartSortOverride(GameObject screen){
		screen.GetComponent<Canvas> ().overrideSorting = true;
		yield return new WaitForSeconds (1.5f);
		screen.GetComponent<Canvas> ().overrideSorting = false;
	}
}
