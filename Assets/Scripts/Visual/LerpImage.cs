/*
 * Author: Isaiah Mann
 * Description: Lerps image in specified directions (back and forth)
 */

using UnityEngine;
using System.Collections;

public class LerpImage : UIImageAnimation {
	public bool Vertical;
	public bool VertUp;
	public bool Horizontal;
	public bool HorRight;

	public bool Repeating;
	public float Height = 1f;
	public float Width = 1f;
	public float DelayTime = 1f;

	IEnumerator verticalCoroutine = null;
	IEnumerator horizontalCoroutine = null;

	protected override void Awake () {
		base.Awake();
	}

	public override void Play () {
		base.Play ();
		if (Vertical) {
			lerpInDirection(VertUp ? Vector3.up : Vector3.down, Height, verticalCoroutine, AnimationTime);
		}
		if (Horizontal) {
			lerpInDirection(HorRight ? Vector3.right : Vector3.left, Width, horizontalCoroutine, AnimationTime);
		}
	}

	public override void Stop () {
		base.Stop ();
		StopAllCoroutines();
		resetPosition();
	}

	void lerpInDirection (Vector3 direction, float modifier, IEnumerator coroutine, float time = 1.0f) {
		if (coroutine != null) {
			StopCoroutine(coroutine);
		}
		coroutine = lerpImage(direction, Repeating, modifier, coroutine, time);
		StartCoroutine(coroutine);
	}

	IEnumerator lerpImage (Vector3 direction, bool repeating, float modifier, IEnumerator coroutine, float time = 1.0f) {
		float timer = 0.0f;

		Vector3 startPosition = transform.localPosition;

		float step = modifier/time;

		while (timer <= time) {

			transform.localPosition += direction * step;

			timer += Time.deltaTime;

			yield return new WaitForEndOfFrame();
		}

		transform.localPosition = startPosition;

		if (repeating) {
			image.enabled = false;
			yield return new WaitForSeconds(DelayTime);
			image.enabled = true;
			lerpInDirection(direction, modifier, coroutine, time);
		}
	}

}
