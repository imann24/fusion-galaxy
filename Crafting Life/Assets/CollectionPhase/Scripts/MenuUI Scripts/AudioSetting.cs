using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioSetting : MonoBehaviour{
	// All of the games audio sources will be saved in this array.
	private AudioSource[] allAudioSources;
	// Music and Sound Effects sources will be saved in these 2 lists.
	private List<AudioSource> allMusicSources, allSoundSources;

	// Awake is called and finds every instance of an Audio Source in our game
	void Awake(){
	//	allAudioSources = GameObject.FindObjectsOfType (typeof(AudioSource)) as AudioSource[];
	}
	void Start(){
	//	allMusicSources = new List<AudioSource> ();
	//	allSoundSources = new List<AudioSource> ();
	//	SortMusicList();
	//	SortSoundEffectsList ();
	}
	// These audio sources are then stored in our allAudioSources array.

	// SortMusicArray splits up allAudioSources into music and everything else
	void SortMusicList(){
		foreach(AudioSource audioPiece in allAudioSources){
			if (audioPiece.clip == null) {
				continue;
			}
			if (audioPiece.clip.name.Contains("music")){
				allMusicSources.Add(audioPiece);
			}
		}
	}
	// After the method runs the allMusicSources array will have all sources of music in the game.

	// SortSoundEffectsArray splits up allAudioSources into sound effects and everything else
	void SortSoundEffectsList(){
		foreach(AudioSource audioPiece in allAudioSources){
			if (audioPiece.clip == null) {
				continue;
			}
			if(audioPiece.clip.name.Contains("sound")){
				allSoundSources.Add(audioPiece);
			}
		}
	}
	// After the method runs the allSoundSources array will have all sources of sound effects in the game.

	// A method to be called by other scripts when they are in menus that allow for the turning off and on of music
	public void TurnMusicOnOrOff(bool answer){
//		if (answer) {
//			foreach(AudioSource musicPiece in allMusicSources){
//				musicPiece.mute = false;
//			}
//		} else {
//			foreach(AudioSource musicPiece in allMusicSources){
//				musicPiece.mute = true;
//			}
//		}
		AudioManager.instance.toggleMuteMusic(!answer);
	}
	// After this method is called the music in the game will have been switched on or off.

	// A method to be called by other scripts when they are in menus that allow for the turning off and on of sound effects
	public void TurnSoundOnOrOff(bool answer){
//		if (answer) {
//			foreach(AudioSource soundPiece in allSoundSources){
//				soundPiece.mute = false;
//			}
//		} else {
//			foreach(AudioSource soundPiece in allSoundSources){
//				soundPiece.mute = true;
//			}
//		}
		AudioManager.instance.toggleMuteSFX(!answer);
	}
	// After this method is called the sound effects in the game will have been switched on or off.
}
