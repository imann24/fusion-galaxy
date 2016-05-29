using UnityEngine;
using System.Collections;

//begins collection mode
public class StartCollect : MonoBehaviour {
	void Awake () {
		GlobalVars.PAUSED = true;
	}
	void Update () {
		//destroys the gameobject if it exists while the game is not paused
		if (!GlobalVars.PAUSED) 
			Destroy(gameObject);
	}
}
