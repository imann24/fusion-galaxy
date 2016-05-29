/*
 * Used to bring the gathering screen forward in the sort order
 */

using UnityEngine;
using System.Collections;

public class BringGatheringToFront : MonoBehaviour {
	// Use this for initialization
	void Start () {
		this.gameObject.GetComponent<Canvas>().overrideSorting = true;
	}
}
