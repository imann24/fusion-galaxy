/*
 * Used to assist with the drag and drop implementation for drop zones
 * This script is used if the zone already contains an element
 * The capturer will not auto grab the new element like it does when it's empty
 * However, if the player releases the element while hovering above the drop zone
 * It will replace the element with the new one
 * This script is removed from the element if its dragged out of the drop zone
 */

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CaptureMe : MonoBehaviour {

	// A reference to the drop zone the element is hovering over
	CaptureScript capturer;

	// Captures the script if its dropped
	void OnDestroy () {
		if (capturer != null && !Input.GetMouseButton(0)) {
			capturer.captureElement(transform.GetComponent<Image>().sprite);
		}
	}

	// Used to set the CaptureScript this element is referring to
	public void setCapturer (CaptureScript capturer) {
		this.capturer = capturer;
	}
}
