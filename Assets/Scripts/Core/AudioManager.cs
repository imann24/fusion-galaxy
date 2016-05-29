/*
 * A singleton script (only one instance can exist at a time)
 * Plays all the Audio
 * Each Audio File is attached as an AudioSource
 * These AudioSources are then called to play on events
 * And on scenes loads
 */


// TODO: Change Components from AudioSources to AudioClips that are simply attached to the script
// Have two audiosources attached to the AudioManager prefab: one for music and one for SFX
// Change the AudioClip associated with the music/sfx audiosource as need be and call play as need be
// This will improve efficiency and readability

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour {
	//for singleton implementation
	public static AudioManager instance;

	// Stops the OnLevelWasLoaded function from being called on a GameObject that's going to be destroyed
	private bool suppressOnLevelLoad;

	#region MUSIC 
	// for start screen
	public AudioSource startLoop;

	//for crafting
	public AudioSource craftingLoop;

	//for gathering 
	public AudioSource gatheringLoop;

	// When pushing the prefab/scene in perforce, Unity likes to get rid of the reference to the audioclip
	// This is used to reassign that clip
	public AudioClip gatheringMusicAudioClip;

	#endregion

	#region SFX
	//for crafting
	public AudioSource newTierUnlockedSFX;
	public AudioSource elementCraftedSFX;
	public AudioSource wrongElementCombinationSFX;
	public AudioSource elementDroppedIntoZoneSFX;
	public AudioSource elementRemovedFromZoneSFX;
	public AudioSource sliderBarDraggedIntoGatheringSFX;
	public AudioSource sliderBarDraggedIntoCraftingSFX;
	public AudioSource newElementCraftedSFX;
	public AudioSource newElementDiscoveredSFX;
	public AudioSource readyToGatherSFX;
	public AudioSource upgradePowerUpSFX;

	//for gathering
	public AudioSource elementIntoRightZoneSFX;
	public AudioSource elementIntoWrongZoneSFX;
	public AudioSource bucketSwitchSFX;
	public AudioSource activateShieldSFX;
	public AudioSource addTimeSFX;
	public AudioSource bucketShieldHitSFX;
	public AudioSource convertElementToTypeSFX;
	public AudioSource powerUpUsedSFX;
	public AudioSource activateMultiplierSFX;
	public AudioSource activateSlowTimeSFX;
	public AudioSource tapElementToCollectSFX;
	
	//array of all the sfx
	private List<AudioSource> allSFX = new List<AudioSource>();

	//for UI 
	public AudioSource buttonPressSFX;
	#endregion
	
	void Awake () {
		//singleton implementation so the gameobject is perpetual throughout modes but so there is also one in each scene for testing
		if (instance == null) {
			//makes sure gameobject is persistent between game states
			instance = this;
			DontDestroyOnLoad(gameObject);

			//adds the SFX event calls
			toggleAllSFXSubscriptions(true);

			// Gets a reference to all the SFX so they can be muted
			foreach (AudioSource audio in transform.GetComponents<AudioSource>()) {

				// Sets the unassigned clip to the gathering music
				// TODO: find a more permanent solution
				// This is more of a hack than a bug fix
				if (audio.clip == null) {
					setAudioSourceClip(audio, gatheringMusicAudioClip);
				}

				if (audio.clip.name.Contains("music")) {
					continue;
				} else {
					allSFX.Add(audio);
				}
			}

			// If the saved settings tell the game to mute the SFX or Music, it mutes them
			if (PlayerPrefs.GetInt("SFX", 1) == 0) {
				toggleMuteSFX(true);
			}

			if (PlayerPrefs.GetInt("Music", 1) == 0) {
				toggleMuteMusic(true);
			}
		} else {
			// Destroys the script and GameObject if there is already an instance from a previous scene
			suppressOnLevelLoad = true;
			Destroy(gameObject);
		}
	}

	void Start () {
		// Updates the music to the current scene
		SwitchAudioToScene(Application.loadedLevel);
	}
	

	//switches audio to the proper scene right before scene load
	void OnLevelWasLoaded (int level) {
		SwitchAudioToScene(level);
	}

	// Unsubscribes all the events if this is the active instance of the AudioManager
	void OnDestroy () {
		if (suppressOnLevelLoad) {
			return;
		}

		toggleAllSFXSubscriptions(false);
	}

	#region Music Functions

	// Switches the music to the proper scene
	void SwitchAudioToScene (int level) {
		// Keeps the music from switching if the GameObject is going to be destroyed
		if (suppressOnLevelLoad) {
			return;
		}

		// Switches based on the level
		if(level == (int)GlobalVars.Scenes.Start){
			switchMusic("Start");
		}
		else if (level == (int) GlobalVars.Scenes.Gathering) {
			switchMusic("Gathering");
		} else if (level == (int) GlobalVars.Scenes.Crafting) {
			switchMusic("Crafting");
			CraftingControl.OnElementCreated += playNewElementCraftedSFX;
		} else if (level == (int) GlobalVars.Scenes.Start) {
			switchMusic("Start");
		}
	}

	#endregion

	/// <summary>
	/// All the scripts in the below region function the same:
	/// They're bound to an event which triggers the SFX
	/// They in turn call the "Play" function on the associated AudioSource
	// TODO: Change these audiosources into audio clips and use these functions to insert them into the proper audiosources
	/// </summary>
	#region Play SFX Functions
	
	public void playNewTierUnlockedSFX (int tierNumber) {
		if (!newTierUnlockedSFX.isPlaying) {
			newTierUnlockedSFX.Play();
		}
	}

	public void playNewElementCraftedSFX (string newElement, string parent1, string parent2, bool isNew) {
		if (isNew) {
			newElementCraftedSFX.Play();
		} else {
			elementCraftedSFX.Play();
		}
	}

	public void playWrongElementCraftedSFX () {
		wrongElementCombinationSFX.Play();
	}

	public void playElementDroppedIntoZoneSFX () {
		elementDroppedIntoZoneSFX.Play();
	}

	public void playElementRemovedFromZoneSFX () {
		elementRemovedFromZoneSFX.Play();
	}

	public void playButtonPressedSFX () {
		buttonPressSFX.Play();
	}

	public void playElementIntoRightZoneSFX () {
		elementIntoRightZoneSFX.Play ();
	}

	public void playElementIntoWrongZoneSFX () {
		elementIntoWrongZoneSFX.Play ();
	}

	public void playSliderBarDraggedIntoCraftingSFX () {
		sliderBarDraggedIntoCraftingSFX.Play();
	}

	public void playSliderBarDraggedIntoGatheringSFX () {
		sliderBarDraggedIntoGatheringSFX.Play();
	}

	public void playBucketSwitchSFX () {
		bucketSwitchSFX.Play();
	}
	
	public void playNewElementDiscoveredSFX (string elementName) {
		newElementDiscoveredSFX.Play ();
	}


	public void playReadyToGatherSFX () {
		readyToGatherSFX.Play ();
	}

	public void playUpgradePowerUpSFX (string powerupName, int powerUpLevel) {
		upgradePowerUpSFX.Play ();
	}

	//not yet implemented 
	public void playActivateShieldSFX (int lane) {
		activateShieldSFX.Play ();
	}

	public void playAddTimeSFX () {
		addTimeSFX.Play();
	}

	public void playBucketShieldHitSFX (int lane) {
		bucketShieldHitSFX.Play();
	}

	public void playConvertElementToTypeSFX () {
		convertElementToTypeSFX.Play ();
	}

	public void playPowerUpUsedSFX (string powerUpName, int powerUpLevel) {
		powerUpUsedSFX.Play ();
	}

	public void playActivateMultiplierSFX () {
	 activateMultiplierSFX.Play();
	}

	public void playActivateSlowTimeSFX () {
		activateSlowTimeSFX.Play();
	}

	public void playTapElementToCollectSFX () {
		tapElementToCollectSFX.Play();
	}

	#endregion


	// Toggles the sound effects for crafting mode
	// Should only need to call on Awake and on Destroy
	public void toggleCraftingSFXSubscriptions (bool active) {
		//binds the sound effects to events
		if (active) {
			CraftingControl.OnElementCreated += playNewElementCraftedSFX;
			CraftingControl.OnTierUnlocked += playNewTierUnlockedSFX;
			CraftingControl.OnIncorrectCraft += playWrongElementCraftedSFX;
			CraftingControl.OnElementDiscovered += playNewElementDiscoveredSFX;
			CaptureScript.OnElementCaptured += playElementDroppedIntoZoneSFX;
			CaptureScript.OnElementCleared += playElementRemovedFromZoneSFX;
			CraftingButtonController.OnButtonPress += playButtonPressedSFX;
			CraftingButtonController.OnReadyToEnterGathering += playReadyToGatherSFX;
			MainMenuController.OnButtonPress += playButtonPressedSFX;
			ScrollBarDisplay.OnDraggedToCrafting += playSliderBarDraggedIntoCraftingSFX;
			ScrollBarDisplay.OnDraggedToGathering += playSliderBarDraggedIntoGatheringSFX;
			BuyUpgrade.OnPowerUpUpgrade += playUpgradePowerUpSFX;
		} else { //removes/mutes the sound effects
			CraftingControl.OnElementCreated -= playNewElementCraftedSFX;
			CraftingControl.OnTierUnlocked -= playNewTierUnlockedSFX;
			CraftingControl.OnIncorrectCraft -= playWrongElementCraftedSFX;
			CraftingControl.OnElementDiscovered -= playNewElementDiscoveredSFX;
			CaptureScript.OnElementCaptured -= playElementDroppedIntoZoneSFX;
			CaptureScript.OnElementCleared -= playElementRemovedFromZoneSFX;
			CraftingButtonController.OnButtonPress -= playButtonPressedSFX;
			CraftingButtonController.OnReadyToEnterGathering -= playReadyToGatherSFX;
			MainMenuController.OnButtonPress -= playButtonPressedSFX;
			ScrollBarDisplay.OnDraggedToCrafting -= playSliderBarDraggedIntoCraftingSFX;
			ScrollBarDisplay.OnDraggedToGathering -= playSliderBarDraggedIntoGatheringSFX;
			BuyUpgrade.OnPowerUpUpgrade -= playUpgradePowerUpSFX;
		}
	}

	// Toggles the sound effects for gathering mode
	// Should only need to call on Awake and on Destroy
	public void toggleGatheringSFXSubscriptions (bool active) {
		if (active) {
			ZoneCollisionDetection.OnRightElement += playElementIntoRightZoneSFX;
			ZoneCollisionDetection.OnWrongElement += playElementIntoWrongZoneSFX;
			ZoneCollisionDetection.OnBucketShieldCreated += playActivateShieldSFX;
			ZoneCollisionDetection.OnBucketShieldHit += playBucketShieldHitSFX;
			ButtonController.OnButtonPress += playButtonPressedSFX;
			TogglePause.OnButtonPress += playButtonPressedSFX;
			switchBucketScript.OnBucketSwitch += playBucketSwitchSFX;
			SwipingMovement.OnCollected += playElementIntoRightZoneSFX;
			ActivatePowerUp.OnPowerUpUsed += playPowerUpUsedSFX;
			Fuel.OnFuelAdded += playAddTimeSFX;
			SlowFall.OnSlowFallActivated += playActivateSlowTimeSFX;
			Multiply.OnElementMultiplyActived += playActivateMultiplierSFX;
			SwipingMovement.OnCollected += playTapElementToCollectSFX;
			LaneConversion.OnLaneConversionActivated += playConvertElementToTypeSFX;
		} else { 
			ZoneCollisionDetection.OnRightElement -= playElementIntoRightZoneSFX;
			ZoneCollisionDetection.OnWrongElement -= playElementIntoWrongZoneSFX;
			ZoneCollisionDetection.OnBucketShieldCreated -= playActivateShieldSFX;
			ZoneCollisionDetection.OnBucketShieldHit -= playBucketShieldHitSFX;
			ButtonController.OnButtonPress -= playButtonPressedSFX;
			TogglePause.OnButtonPress -= playButtonPressedSFX;
			switchBucketScript.OnBucketSwitch -= playBucketSwitchSFX;
			SwipingMovement.OnCollected -= playElementIntoRightZoneSFX;
			ActivatePowerUp.OnPowerUpUsed -= playPowerUpUsedSFX;
			Fuel.OnFuelAdded -= playAddTimeSFX;
			SlowFall.OnSlowFallActivated -= playActivateSlowTimeSFX;
			Multiply.OnElementMultiplyActived -= playActivateMultiplierSFX;
			SwipingMovement.OnCollected -= playTapElementToCollectSFX;
			LaneConversion.OnLaneConversionActivated -= playConvertElementToTypeSFX;
		}
	}


	// Toggles the sound effects for gathering mode
	// Should only need to call on Awake and on Destroy
	public void toggleCreditSFXSubscriptions (bool active) {
		if (active) {
			CreditsController.OnBackButtonClick += playButtonPressedSFX;
		} else {
			CreditsController.OnBackButtonClick -= playButtonPressedSFX;
		}
	}

	// Toggles the sound effects for start mode
	// Should only need to call on Awake and on Destroy
	public void toggleStartSFXSubscriptions (bool active) {
		if (active) {
			StartScreenButtonController.OnButtonPress += playButtonPressedSFX;
		} else {
			StartScreenButtonController.OnButtonPress -= playButtonPressedSFX;
		}
	}

	// subscribes and unsubscribes the sound effects for start mode
	// Should only need to call on Awake and on Destroy
	public void toggleAllSFXSubscriptions (bool active) {
		toggleCraftingSFXSubscriptions(active);
		toggleGatheringSFXSubscriptions(active);
		toggleCreditSFXSubscriptions(active);
		toggleStartSFXSubscriptions(active);
	}

	// Switches to the proper music track for the corresponding mode and mutes the others
	public void switchMusic (string mode) {
		if (mode == "Gathering") {
			gatheringLoop.Play ();
			craftingLoop.Stop();
			startLoop.Stop();
		} else if (mode == "Crafting") {
			gatheringLoop.Stop ();
			craftingLoop.Play();
			startLoop.Stop();
		} else if(mode == "Start") {
			gatheringLoop.Stop();
			craftingLoop.Stop();
			startLoop.Play();
		}
	}

	// Used to mute and untmute the music
	public void toggleMuteMusic (bool muted) {
		//sets the player pref int so that settings persists between game sessions 
		PlayerPrefs.SetInt ("Music", muted?0:1);

		//mutes or plays the music
		gatheringLoop.mute = muted;
		craftingLoop.mute = muted;
		startLoop.mute = muted;
	}

	// Used to mute and unmute the SFX
	public void toggleMuteSFX (bool muted) {

		//mutes the audio
		foreach (AudioSource audio in allSFX) {
			audio.mute = muted;
		}

		//sets the player pref int so that settings persists between game sessions
		PlayerPrefs.SetInt("SFX", muted?0:1);
	}

	// Used to specifically address an issue where the gathering music loop becomes unassigned when it is pushed over version control
	// Reassigns the audioclip at runtime
	private void setAudioSourceClip (AudioSource audioSource, AudioClip audioClip) {
		audioSource.clip = audioClip;
	}
	
}
