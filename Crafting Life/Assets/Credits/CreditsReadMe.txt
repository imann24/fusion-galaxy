The credits scene displays the credits. It reads them in from a CSV file. 

Like all scenes it’s structured into four main folders:
- Assets holds the textures, spritesheets, and animations
- Prefabs holds the GameObject prefabs
- Scene holds the corresponding Scene folder
- Scripts holds the C# classes that are only/mainly used in that scene

In Credits, the main script to be concerned with is:
- ReadCredits.CSV: reads in and generates all the credits from a .csv file

In the scene hierarchy:
- the AllCredits game object has an example of each type of credit text object and a GridLayout component that spaces them all evenly. These example types are then employed by the script to generate the real credits and then deleted.

Notes:
- Credits scroll automatically until clicked.