using UnityEngine;
using System.Collections;

public class EndGatheringOnDestroy : MonoBehaviour {
	CollectionTimer timerInstance;
	// Use this for initialization
	void OnDestroy () {
		GameObject highestElement;

		//ends the game if this is the highest element
		if ((highestElement = GenerationScript.findHighestSpawnedElement ()) == gameObject || highestElement == null) {
			timerInstance.endCollectMode ();
		} else {
			//if not the highest element, passes the script on and turns off spawning
			highestElement.AddComponent<EndGatheringOnDestroy>().setTimerInstance(timerInstance);
			GlobalVars.GATHERING_CONTROLLER.SetSpawning(false);
		}
	}

	//sets the reference to the timer script
	public void setTimerInstance (CollectionTimer timerInstance) {
		this.timerInstance = timerInstance;
	}
}
