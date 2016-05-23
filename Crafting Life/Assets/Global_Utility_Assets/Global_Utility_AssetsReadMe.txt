Global/Utility Assets is meant to be a folder that holds scripts and assets that are necessary throughout all scenes in the game.

By Folder:

audio: contains all the audio files used in the game
prefabs: contain prefabs that are used in all scenes of the game
scripts: contain scripts that are/could be used in all scenes of the game

Most Important:

AudioManager is a prefab that has all the sound files attached to it as AudioSources. While we recognize that this is not the most efficient, it makes for any easy combination with the AudioManager.cs script and a singleton implementation so that this GameObject persists between scenes and the music contains to play uninterrupted. All sound effects are played via event calls from other scripts.

Analytics is a prefab used to send events to MixPanel and track user activity.

LoadingScreen is another singleton implementation that persists between and overlays a static loading screen while the scenes are loading.

GlobalVars.cs is a class with many static variables that a great many classes access and use to reason about the state/elements of the game.
