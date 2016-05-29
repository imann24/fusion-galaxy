Crafting is the main hub of “Crafting Life.” From here players can launch into missions to gather elements, access the crafting UI to combine elements, access the credits, settings, and the power up upgrade menu.

Like all scenes it’s structured into four main folders:
- Assets holds the textures, spritesheets, and animations
- Prefabs holds the GameObject prefabs
- Scene holds the corresponding Scene folder
- Scripts holds the C# classes that are only/mainly used in that scene

In Crafting, the main scripts to be concerned with are:
- ReadCSV.cs: reads in all the elements from a .csv file and combinations that makes them. It then associates them with their sprite icons.
- MainMenuController.cs: handles loading all the elements in and controlling how they are displayed on screen
- CraftingControl: handles the combining of the elements and the player feedback upon crafting.
- CaptureScript.cs: is attached to each drop zone and the compiler. The drop zones are the four slots on the main UI and the two slots on the crafting screen. The compiler is the blue ring that the product of two combined elements is shown into. The capture script senses which elements are dragged in and relays that feedback to the appropriate sources.
- CraftingButtonController.cs: handles most of the button presses and cals the appropriate scripts

In the scene hierarchy:
- The DropZones are places you can drag elements into
- The Canvas is the whole UI: it’s broken down into the top screens which scroll in and out, and the static bottom half which remains unchanged and displays the elements
- The main controlling scripts are split between the CraftingCamera and the EventSystem game objects

Notes:
- Optimization is a concern for Crafting mode as all the UI elements make for a long scene load time