Gathering is the element collecting mini-game in Crafting Life. The elements fall from the top of the screen into the buckets at the bottom.

Like all scenes it’s structured into four main folders:
- Assets holds the textures, spritesheets, and animations
- Prefabs holds the GameObject prefabs
- Scene holds the corresponding Scene folder
- Scripts holds the C# classes that are only/mainly used in that scene

In Gathering, the main scripts to be concerned with are:
- GenerationScript.cs: handles all the element spawning and controls many of the powers
- ZoneCollisionDection.cs: handles all the collisions between the elements and the buckets
- CollectionTimer.cs controls the clock which ticks down based on whether the game is paused

In the scene hierarchy:
- The Zones are the buckets that receive elements. Each one has an instance of ZoneCollisionDetection.cs attached to it
- The Canvas is contains many of the UI elements
- The Controller has many of the main scripts on it

Notes:
- Gathering is sometimes referred to as “Collection.” This is a prior term and some of the scripts/scene files have not been updated to reflect the new term.