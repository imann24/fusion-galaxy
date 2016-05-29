//#define DEBUG
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
public class CraftingAnimationOverwatch : MonoBehaviour {
	public Animator plusOneAnimator;
	public Animator craftedElementAnimator;
	private List<Animator> allAnimators = new List<Animator>();
	private Image [] allAnimatorSprites;
	// Use this for initialization
	void Start () {
		//adding all the animations to the list
		allAnimators.Add(plusOneAnimator);
		allAnimators.Add (craftedElementAnimator);

		//intializing the array of Images conencted to the animators in a parallel array
		allAnimatorSprites = new Image[allAnimators.Count]; 
		findAllAnimatorSprites();
		linkToEvents();
	}
	
	//Makes sure to take away event references when the gameobject is destroyed to prevent null calls
	void OnDestroy () {
		unlinkEvents();
	}

	//plays plus one
	public void playPlusOne (string resultElement, string parent1, string parent2, bool isNew) {
		playAnimation(allAnimators.IndexOf(plusOneAnimator));
	}

	//plays the appropritate crafted animation
	public void playCrafted (string resultElement, string parent1, string parent2, bool isNew) {
		if (isNew) { //play the animation for a new element
			playNewElementCraftedAnimation();
		} else { //play the animation for a standard (non new) crafted element
			playStandardCraftedAnimation();
		}

	}

	//plays the incorrect craft animation
	public void playIncorrectCraft () {
		craftedElementAnimator.Play ("IncorrectCraft");		
		//craftedElementAnimator.SetTrigger("incorrect");
	}
	
	//links to all events
	public void linkToEvents () {
		CraftingControl.OnElementCreated += playPlusOne;
		CraftingControl.OnElementCreated += playCrafted;
		CraftingControl.OnIncorrectCraft += playIncorrectCraft;
	}

	//unlinks all events to prevent null calls
	public void unlinkEvents () {
		CraftingControl.OnElementCreated -= playPlusOne;
		CraftingControl.OnElementCreated -= playCrafted;
		CraftingControl.OnIncorrectCraft -= playIncorrectCraft;
	}

	//plays the regular crafted aniamtion 
	private void playStandardCraftedAnimation () { 
		craftedElementAnimator.Play ("CorrectCraft");	
		//craftedElementAnimator.SetTrigger("regularCraft");
	}

	//plays the animation for a newly crafted element
	private void playNewElementCraftedAnimation () {
		craftedElementAnimator.Play ("NewCraft");
		//craftedElementAnimator.SetTrigger("newElementCraft");
	}

	//takes the index of the animator in the array and plays it
	private void playAnimation (int animationIndex) {
		toggleAnimation(animationIndex, true);
	}

	private void toggleAnimation (int animationIndex, bool active) {
		//starts the animation
		allAnimators[animationIndex].enabled = active;

		//turns on the sprite
		allAnimatorSprites[animationIndex].enabled = active;
	}

	//finds all the sprites and puts them into an array to refer to them later
	private void findAllAnimatorSprites (){
		for (int i = 0; i < allAnimators.Count; i++) {
			allAnimatorSprites[i] = allAnimators[i].gameObject.GetComponent<Image>();
		}
	}
}
