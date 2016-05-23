using UnityEngine;
using System.Collections;

public class SetToLastChild : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if (this.gameObject.transform.GetSiblingIndex() < (this.gameObject.transform.parent.childCount - 1)) {
			this.gameObject.transform.SetAsLastSibling ();
		}
	}
}
