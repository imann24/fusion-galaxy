The start screen is more of a splash screen for the game. It basically just provides the user a lead in to the main game.

Like all scenes it’s structured into four main folders:
- Assets holds the textures, spritesheets, and animations
- Prefabs holds the GameObject prefabs
- Scene holds the corresponding Scene folder
- Scripts holds the C# classes that are only/mainly used in that scene

In Start, the main script to be concerned with is:
- StartButtonController.cs: this controls all the function calls available from the buttons

In the scene hierarchy:
- The Canvas holds the UI
- The Scene also contains singleton implementations of the AudioManager, Analytics, and LoadingScreen which persist throughout the rest of the game

Notes:
- Depending on whether the GlobalVars.Medical_USE bool is set to true of false, Start will load into Crafting or the Bluetooth Connection UI respectively
- Start plays the intro cinematic when the next scene is loaded, a future goal is to see whether this can be coupled with Application.LoadLevelAsync to improve the loading time/hide it behind the intro cinematic