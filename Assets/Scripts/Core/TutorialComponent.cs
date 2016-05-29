using UnityEngine;
using System.Collections;

public abstract class TutorialComponent : MonoBehaviour {

	public int TutorialStep; // Indicates an ordering in which tutorial component this is. Should be zero indexed.

	public abstract TutorialComponent [] GetCurrent ();
	public abstract TutorialComponent [] GetNext ();
	public abstract TutorialComponent [] GetPrevious ();
}
