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
	public GameObject ParticleSparklesFallPrefab;
	public GameObject ParticleGLowBurstPrefab;
	Dictionary<ParticleEffectType, Queue<GameObject>> spawnPools = new Dictionary<ParticleEffectType, Queue<GameObject>>();

	// Use this for initialization
	void Awake () {
		Init();
	}

	void Init () {
		Subscribe();
		InitSpawnPools();
	}

	void InitSpawnPools () {
		for (int effectIndex = 0; 
		     effectIndex < System.Enum.GetNames(typeof(ParticleEffectType)).Length; 
		     effectIndex++) {
			spawnPools.Add((ParticleEffectType)effectIndex, new Queue<GameObject>());
		}
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
		GameObject particleObject = null;
		if (eventName == EventController.ParticleSparklesFallEvent) {
			particleObject = SpawnEffect(ParticleEffectType.SparkleFall, position);
		} else if (eventName == EventController.ParitcleGlowBurstEvent) {
			particleObject = SpawnEffect(ParticleEffectType.GlowBurst, position);
		}
		if (particleObject != null) {
			ParticleSystem effect = particleObject.GetComponent<ParticleSystem>();
			if (effect != null) {
				effect.Play();
				StartCoroutine(CollectOnEffectComplete(effect));
			}
		}
	}

	IEnumerator CollectOnEffectComplete (ParticleSystem particleEffect) {
		float effectTime = particleEffect.duration + particleEffect.startLifetime;
		yield return new WaitForSeconds(effectTime);
		AddToSpawnPool(particleEffect.gameObject);
	}

	void AddToSpawnPool (GameObject uneededEffect) {
		uneededEffect.SetActive(false);
		ParticleSystemBehaviour behaviour = 
			uneededEffect.GetComponent<ParticleSystemBehaviour>();
		if (behaviour != null) {
			ParticleEffectType effectType = behaviour.EffectType;
			spawnPools[effectType].Enqueue(uneededEffect);
		}
	}

	bool TryGetFromSpawnPool (ParticleEffectType effectType, out GameObject effect) {
		if (spawnPools[effectType].Count > 0) {
			effect = spawnPools[effectType].Dequeue();
			effect.SetActive(true);
			return true;
		} else {
			effect = null;
			return false;
		}
	}

	GameObject SpawnEffect (ParticleEffectType effectType, Vector3 position) {
		GameObject effect;
		if (TryGetFromSpawnPool(effectType, out effect)) {
			effect.transform.position = position;
			return effect;
		} else {
			return SpawnNewObject(effectType, position);
		}
	}

	GameObject SpawnNewObject (ParticleEffectType effectType, Vector3 position) {
		GameObject effect = null;
		switch(effectType) {
		case ParticleEffectType.SparkleFall:
			effect = (GameObject) Instantiate(ParticleSparklesFallPrefab);
			break;
		case ParticleEffectType.GlowBurst:
			effect = (GameObject) Instantiate(ParticleGLowBurstPrefab);
			break;
		}
		Transform effectTransform = effect.transform;
		effectTransform.SetParent(transform);
		effectTransform.position = position;
		effectTransform.localPosition += Vector3.back * ZOffset;
		return effect;
	}

}
