using UnityEngine;
using System.Collections;

public class GatheringPowerUpAnimationController : MonoBehaviour {
	public static GatheringPowerUpAnimationController instance;
	public Animator animator;
	void Awake(){
		instance = this;
		animator = GetComponent<Animator>();
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
