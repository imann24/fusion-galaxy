/*
 * Author: Isaiah Mann
 * Description: Description: Spawns temp game objects
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// TODO: Add enum for different particle effect types
public class ParticleController : MonoBehaviour {
	public float ZOffset = 100;
	public GameObject ParticleSparklesFall;
	Queue<GameObject> spawnPool = new Queue<GameObject>();
	// Use this for initialization
	void Awake () {
		Init();
	}

	void Init () {
		Subscribe();
	}

	void OnDestroy () {
		Unsubscribe();
	}

	void Subscribe () {
		EventController.OnNamedPositionEvent += HandlePositionEvent;	
	}

	void Unsubscribe () {
		EventController.OnNamedPositionEvent -= HandlePositionEvent;
	}

	void HandlePositionEvent (string eventName, Vector3 position) {
		if (eventName == EventController.ParticleSparklesFallEvent) {
			ParticleSystem effect = SpawnEffect(position).GetComponent<ParticleSystem>();
			effect.Play();
			StartCoroutine(CollectOnEffectComplete(effect));
		}
	}

	IEnumerator CollectOnEffectComplete (ParticleSystem particleEffect) {
		float effectTime = particleEffect.duration + particleEffect.startLifetime;
		yield return new WaitForSeconds(effectTime);
		AddToSpawnPool(particleEffect.gameObject);
	}

	void AddToSpawnPool (GameObject uneededEffect) {
		uneededEffect.SetActive(false);
		spawnPool.Enqueue(uneededEffect);
	}

	bool TryGetFromSpawnPool (out GameObject effect) {
		if (spawnPool.Count > 0) {
			effect = spawnPool.Dequeue();
			effect.SetActive(true);
			return true;
		} else {
			effect = null;
			return false;
		}
	}

	GameObject SpawnEffect (Vector3 position) {
		GameObject effect;
		if (TryGetFromSpawnPool(out effect)) {
			effect.transform.position = position;
			return effect;
		} else {
			return SpawnNewObject(position);
		}
	}

	GameObject SpawnNewObject (Vector3 position) {
		GameObject effect = (GameObject) Instantiate(ParticleSparklesFall);
		Transform effectTransform = effect.transform;
		effectTransform.SetParent(transform);
		effectTransform.position = position;
		effectTransform.localPosition += Vector3.back * ZOffset;
		return effect;
	}

}
