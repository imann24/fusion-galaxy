TechTree is a scene that is not currently in the build. It’s also sometimes referred to as the “Wiki.” The original concept was a scene that shows all the elements and how they connect to form each other.

Like all scenes it’s structured into four main folders:
- Assets holds the textures, spritesheets, and animations
- Prefabs holds the GameObject prefabs
- Scene holds the corresponding Scene folder
- Scripts holds the C# classes that are only/mainly used in that scene

In TechTree, the main script to be concerned with is:
- GenerateTechTree.cs which uses all the elements to create a massive rotating galaxy

In the scene hierarchy:
- CanvasMain houses the main canvas and all the UI elements

Notes:
- The element positions are determined by polar equations
- On stretch feature was adding pinch to zoom to the tech tree