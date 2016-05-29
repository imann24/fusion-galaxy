/*
 * Used to control the buttons in the Start Scene
 * Also controls how it loads the next scene
 * And the cut scene it plays before load
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StartScreenButtonController : MonoBehaviour {

	// Delegates for event calls
	public delegate void ButtonPressAction();
	public delegate void SceneLoadAction(GlobalVars.Scenes toScene);

	// An event: mostly used to call the bottom press SFX
	public static event ButtonPressAction OnButtonPress;

	// An event used to specify which scene is being loaded
	// Depending on whether the game is medical use or not
	// It will load AddScene.unity for Medical Use
	// And Crafting.unity for Commercial Use
	public static event SceneLoadAction OnLoadScene;

	// A reference to the buttom that loads the game
	private Button myButton;

	// Used to load the scene asynchonously (in the background)
	AsyncOperation backgroundLoadGame;

	// Use this for initialization
	void Start () {
#if UNITY_WEBGL
		//sets the camera aspect for standalone builds
		Camera.main.aspect = 3.0f/4.0f;
		Screen.SetResolution(600,800,false);
#endif
		// An outdated call that sets which scene the tech tree loads back into when the user exits
		// The tech tree is not currently an active scene in the game: the scene file and code exists,
		// But it was removed from builds to further polish other parts of the game
		GlobalVars.TECH_TREE_SOURCE = (int)GlobalVars.Scenes.Crafting;

		//do not allow the scene to be activated until loading is done
	}

	public void loadMainGame () {
		// Calls the event
		callOnButtonPress();

		// Displays the loading screen over the UI
		Utility.ShowLoadScreen();

		// Incremenets play count for analytics
		Utility.IncreasePlayerPrefValue("TotalPlaycount");

		// Begins to load game in the background
		StartCoroutine ("load");

		//plays the movie
		/* TODO: Find a way to play the movie while also loading the game
		* This may be impossible because in Unity's documentation: http://docs.unity3d.com/ScriptReference/Handheld.PlayFullScreenMovie.html
		* they state that calling this functions play the movie and pauses Unity
		* it may be possible if the movie is played in a different way
		*/
#if UNITY_ANDROID || UNITY_IOS
		Handheld.PlayFullScreenMovie ("Sequence 01_1.mp4", Color.black, FullScreenMovieControlMode.CancelOnInput);
#endif
		//loads/activates the main game: 
		// this functions triggers the actual scene change which should only happen after the game is loaded
		StartCoroutine ("loadAfterMovie");

	}


	/// <summary>
	/// Load the scene in the background
	/// </summary>
	IEnumerator load() {
		Debug.LogWarning("ASYNC LOAD STARTED - " +
		                 "DO NOT EXIT PLAY MODE UNTIL SCENE LOADS... UNITY WILL CRASH");

		//loads either the crafting mode or the SDK connector screen, depending on the mode
		GlobalVars.Scenes toLoad = GlobalVars.MEDICAL_USE ? GlobalVars.Scenes.SDK : GlobalVars.Scenes.Crafting;		

		//calls the event
		if (OnLoadScene != null) {
			OnLoadScene(toLoad);
		}

		backgroundLoadGame = Application.LoadLevelAsync ((int) toLoad);		
		backgroundLoadGame.allowSceneActivation = false;
		yield return backgroundLoadGame;
	}

	/// <summary>
	/// Allows the background load to change scenes
	/// </summary>
	public void ActivateScene() {
		backgroundLoadGame.allowSceneActivation = true;
	}


	/// <summary>
	/// Ensures that the game does not load until the movie is done
	/// </summary>
	IEnumerator loadAfterMovie(){
		while (true) {// this

			//loading progress seems to stop around 90%
			if (backgroundLoadGame.isDone || backgroundLoadGame.progress >= 0.9f) {
				ActivateScene ();
				break;
			}
			yield return null;
		}
	}



	/// <summary>
	/// Calls the on button press event
	/// Currently used to play audio
	/// </summary>
	public void callOnButtonPress () {
		if (OnButtonPress != null) {
			OnButtonPress();
		}
	}
}
