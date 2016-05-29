using UnityEngine;
using System.Collections;

public class BucketShieldController : MonoBehaviour {
	public GameObject [] shields;

	// Use this for initialization: establishes referecnes
	void Start () {
		ZoneCollisionDetection.OnBucketShieldCreated += activateBucketShieldImage;
		ZoneCollisionDetection.OnBucketShieldHit += playBucketShieldHitAnimation;
		ZoneCollisionDetection.OnBucketShieldDestroyed += deactivateBucketShieldImage;
		foreach (GameObject shield in shields) {
			shield.GetComponent<SpriteRenderer>().enabled = false;
		}
	}

	//unassociates references
	void OnDestroy () {
		ZoneCollisionDetection.OnBucketShieldCreated -= activateBucketShieldImage;
		ZoneCollisionDetection.OnBucketShieldHit -= playBucketShieldHitAnimation;
		ZoneCollisionDetection.OnBucketShieldDestroyed -= deactivateBucketShieldImage;
	}

	//shows the bucket shield
	private void activateBucketShieldImage (int lane) {
		shields[lane].GetComponent<SpriteRenderer>().enabled = true;
	}

	//every time the bucket shield takes a hit
	private void playBucketShieldHitAnimation (int lane) {
		shields[lane].GetComponent<ParticleSystem>().Play();
	}

	//hides the bucket shield
	private void deactivateBucketShieldImage (int lane) {
		shields[lane].GetComponent<SpriteRenderer>().enabled = false;
	}
}
