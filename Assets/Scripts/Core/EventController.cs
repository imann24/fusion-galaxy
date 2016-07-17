using UnityEngine;
using System.Collections;

public static class EventController {
	#region Event Names
	public const string ParticleSparklesFallEvent = "SparklesFall";
	public const string ParitcleGlowBurstEvent = "GlowBurst";
	#endregion

	public delegate void NamedPositionEvent (string eventName, Vector3 location);
	public static event NamedPositionEvent OnNamedPositionEvent;

	public static void Event (string eventName, Vector3 location) {
		if (OnNamedPositionEvent != null) {
			OnNamedPositionEvent(eventName, location);
		}
	}
}
