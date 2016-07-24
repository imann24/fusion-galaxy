using UnityEngine;
using System.Collections;

public class LerpImageScale : UIImageAnimation {
	public float MaxScale;
	public float MinScale;
	public bool Repeating;
	// Whether scale should return to 
	public bool Loop;
	IEnumerator lerpCoroutine;

	public override void Play () {
		if (gameObject.activeInHierarchy) {
			base.Play ();
			callLerpScale(MaxScale, MinScale, Repeating, AnimationTime);
		} else {
			shouldPlayOnEnable = true;
		}
	}

	public override void Stop () {
		base.Stop();
		StopAllCoroutines();
	}

	void callLerpScale (float maxScaleValue, float minScaleValue, bool repeating, float time) {
		if (lerpCoroutine != null) {
			StopCoroutine(lerpCoroutine);
		}

		lerpCoroutine = lerpScale(maxScaleValue, minScaleValue, repeating, time);
		StartCoroutine(lerpCoroutine);
	}

	IEnumerator lerpScale (float maxScaleValue, float minScaleValue, bool repeating, float time) {
		float timer = 0;
		float scaleTime = Loop ? time/2f : time;

		Vector3 minScale = VectorUtil.FloatToVector(minScaleValue);
		Vector3 maxScale = VectorUtil.FloatToVector(maxScaleValue);

		transform.localScale = minScale;

		while (timer <= scaleTime) {
			updateScaleProgress(minScale, maxScale, timer/scaleTime);
			timer += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		transform.localScale = maxScale;

		if (Loop) {
			timer = 0;
			while (timer <= scaleTime) {
				updateScaleProgress(maxScale, minScale, timer/scaleTime);
				timer += Time.deltaTime;
				yield return new WaitForEndOfFrame();
			}
			transform.localScale = minScale;
		}

		if (repeating) {
			callLerpScale(maxScaleValue, minScaleValue, repeating, time);
		}
	}

	void updateScaleProgress (Vector3 startScale, Vector3 endScale, float percentAsDecimal) {
		transform.localScale = Vector3.Lerp (
			startScale,
			endScale,
			percentAsDecimal
		);
	}
}
