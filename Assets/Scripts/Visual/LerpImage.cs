using UnityEngine;
using System.Collections;

public class LerpImage : MonoBehaviour {
	public bool UpAndDown;
	public bool Repeating;
	public float Height;
	// Use this for initialization
	void Start () {
		if (UpAndDown) {
			StartCoroutine(

				lerpImage(
					transform.position + Vector3.up * Height,
					Repeating));
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	IEnumerator lerpImage (Vector3 targetPosition, bool repeating, float speed = 1.0f) {
		float timer = 0.0f;

		Vector3 startPosition = transform.position;

		while (timer <= 1) {

			transform.position = Vector3.Lerp(
				startPosition,
				targetPosition,
				timer);

			timer += Time.deltaTime * speed;

			yield return new WaitForEndOfFrame();
		}

		if (repeating) {
			StartCoroutine(lerpImage(
				startPosition,
				repeating));
		}
	}
}
