using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ScrollDown : MonoBehaviour, IPointerDownHandler {
	private static bool MOVEMENT_ENABLED = true;
	private bool unClicked = true;
	public float speed;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (unClicked && MOVEMENT_ENABLED) {
			transform.position += Vector3.up * Time.deltaTime * speed;
		}
	}

	public void OnPointerDown (PointerEventData pointerEventData) {
		unClicked = false;
	}

	public static void StopAllScrolling () {
		MOVEMENT_ENABLED = false;
	}
}
