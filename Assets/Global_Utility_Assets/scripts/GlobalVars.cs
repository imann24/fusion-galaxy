/*
 * A set of variables that are relevant to all scripts in game
 * All kept static so they acessible without an object reference
 * Used to track stats, access elements, and toggle the Medical SDK on and off 
 */

using System.Collections.Generic;
using System;
using UnityEngine;
public class GlobalVars {

	#region NEURO*MOTION
	/// <summary>
	/// IMPORTANT: should be set true if making a build for Neuro*motion. 
	/// 	In addition to setting this bool, you must also add AddScene.unity as the 5th scene (scene index 4) to the build settings
	/// Not-toggleable after compile time
	/// </summary>
	public const bool MEDICAL_USE = false;
	#endregion

	#region MIXPANEL
	//game stat tracking
	//Used in sending events to MixPanel: the counts are per session (after which they are reset to zero)
	public static int GATHERING_PLAY_COUNT = 0;
	public static int CRAFTING_PLAY_COUNT = 0;
	#endregion

	#region SCENES
	//Unity related variables
	/// <summary>
	/// For changing between scenes
	/// </summary>
	public enum Scenes:int{Start=0, Gathering=1, Crafting=2, Credits=3, SDK=4, Wiki=5};

	//a persistent refence to the loading screen so that the game can show it between scenes
	public static GameObject LOAD_SCREEN;
	#endregion

	#region GATHERING
	//gathering mode related vars
	// Sets the time to 30 seconds if it is a commercial play session
	// and sets the time 60 if the build is medical
	public static int COLLECT_TIME = MEDICAL_USE?60:30;

	//tracks whether the gathering mode is paused
	public static bool PAUSED = false;

	//tracks how many of the four elements the player gathers
	public static int [] SCORES = {0, 0, 0, 0};

	//tracks how many elements the player misses
	public static int MISSED = 0;

	//how many elments the player loses when an element goes into the wrong buckets
	public static int WRONG_ZONE_PENALTY = 3;

	//the number of lanes in the gathering mode
	public static int NUMBER_OF_LANES = 4;

	//a global reference to the script that controls the gathering mode
	public static GenerationScript GATHERING_CONTROLLER;

	// A global reference to the timer script in gathering
	public static CollectionTimer GATHERING_TIMER;

	// A global reference to the four collision zone tracker buckets in gathering
	public static ZoneCollisionDetection [] GATHERING_ZONES = new ZoneCollisionDetection[NUMBER_OF_LANES];

	// The tag that is used to indentify falling gameobjects that are powerups
	public const string POWER_UP_TAG = "PowerUp";

	// The number of powerups a player uses in a single gathering session (for analytics)
	public static int POWERUP_USE_COUNT;

	// The number of powerups that are spawned in during gathering 
	public static int POWERUP_SPAWN_COUNT;

	// Tracks how long the gathering session goes for
	public static int GATHERING_PLAYTIME = 0;
	#endregion

	#region RESOURCE_LOADING
	//optimization flags to reduce undue work

	// Whether the elements have been read in from the CSV
	public static bool CSV_READ = false;

	// Whether the element sprite images have been loaded in from the resources folder
	public static bool SPRITES_LOADED = false;

	// Whether powerup images have been loaded in from resources
	public static bool POWERUP_SPRITES_LOADED = false;
	#endregion

	#region MAIN_MENU
	//crafting + tech tree related vars

	//the number of elmeents a player has unlocked
	public static int NUMBER_ELEMENTS_UNLOCKED = 0;

	public static int TIER_COUNT = 10;
	public static bool [] TIER_UNLOCKED;
	public static int TECH_TREE_SOURCE;
	public static float ELEMENT_TO_INVENTORY_SPEED = 5.25f;
	public const string SPAWNER_STRING = "Spawner";
	public const string UNLOCK_STRING = "Unlocked";
	public const string HINT_STRING = "Hint";
	public const string ELEMENT_TAG = "Element";
	public static bool CRAFTING_ACTIVE = false;
	public static MainMenuController CRAFTING_CONTROLLER; 
	public static CraftingControl CRAFTER;
	public static CraftingButtonController CRAFTING_BUTTON_CONTROLLER;
	//button names
	public const string CRAFTING_BUTTON_NAME = "Crafting";
	public const string UPGRADE_POWERUP_BUTTON_NAME = "UpgradePowerUp";
	public const string GATHERING_BUTTON_NAME = "Gathering";
	#endregion

	#region ELEMENTS
	//list of all the elements
	public static List<Element> ELEMENTS =  new List<Element>();
	//list of all the combinations
	public static List<Combination> RECIPES = new List<Combination>();
	//look up an element by name
	public static Dictionary<string, Element> ELEMENTS_BY_NAME = new Dictionary<string, Element>();
	//look up a combination by entering a string that is the combined name of two elements
	public static Dictionary<string, Element> RECIPES_BY_NAME = new Dictionary<string, Element>();
	//lists of elements in an array sorted by tier
	public static List<Element>[] ELEMENTS_BY_TIER = new List<Element>[TIER_COUNT];
	//file path within resources to load in new files
	public static string FILE_PATH = "elems_1/";
	//a dictionary that stores all the sprites for the elements by name
	public static Dictionary<string, Sprite> ELEMENT_SPRITES = new Dictionary<string, Sprite>();
	// A dictionary that counts the amount of combinations each element has.
	public static Dictionary<string, int> NUMBER_OF_COMBINATIONS = new Dictionary<string, int> ();
	#endregion

	#region POWERUPS
	//powerups

	// How many types of powerups there are
	public const int POWERUP_COUNT = 9;

	// Used as part of a key in player prefs combined with the powerup's name to query what level it is
	public const string POWERUP_LEVEL_STRING = "PowerUpLevel";

	// Used as part of key in PlayerPrefs to query whether the powerup has been unlocked
	public const string POWERUP_UNLOCK_STRING = "Unlocked";

	// The filepath within the resources folder where the powerup sprites are located
	public const string POWERUP_FILE_PATH = "powerups/";

	// The names of the powerup sprites to load them in
	public static string [] POWERUP_SPRITE_FILENAMES = {"laneconversion", "time slow", "add time", "multiplier2", "bucket shield", "tap to collect", "invincible",  "totalconversion", "collectall"};   

	// Stores the powerup sprites
	public static Sprite [] POWERUP_SPRITES = new Sprite[POWERUP_SPRITE_FILENAMES.Length];

	// A dictionary to determine which powerup is at which index
	public static Dictionary<string, int> POWERUP_INDEXES;

	// Used to count the number of powerups during a single 
	public static void INCREASE_POWER_UP_USE_COUNT (string powerUpName, int powerUpLevel) {
		POWERUP_USE_COUNT++;
	}

	#endregion

	#region TUTUORIALS
	//tutorials
	// Keys used with player pref ints (treated as booleans) to tell whether each tutorial has been watched
	public const string CRAFTING_TUTORIAL_WATCHED = "CraftingTutorialWatched";
	public const string GATHERING_TUTORIAL_WATCHED_SWIPE = "GatheringSwipeTutorial";
	public const string GATHERING_TUTORIAL_WATCHED_POWER_UP = "GatheringPowerUpTutorial";

	// Tracks the players progress during each tutorial
	public static bool SWIPE_TUTORIAL_FIRST_SPAWNED = false;
	public static bool SWIPE_TUTORIAL_SECOND_SPAWNED = false;
	public static bool POWER_UP_TUTORIAL_SWIPE = false;
	public static bool POWER_UP_TUTORIAL_TAP = false;
	public static bool IS_SWIPE_TUTORIAL_CURRENTLY_PLAYING = false;

	// String names for the tutorial elements
	public static string SWIPE_TUTORIAL_ELEMENT_NAME = "FirstSwipeTutorialElement";
	public static string SWIPE_TUTORIAL_SECOND_ELEMENT_NAME = "SecondSwipeTutorialElement";
	public static string POWER_UP_TUTORIAL_SWIPE_NAME = "PowerUpSwipe";
	public static string POWER_UP_TUTORIAL_TAP_NAME = "PowerUpTap";
	#endregion

	#region MAIN_MENU_TUTORIALS
	// Keys used with player pref ints (treated as booleans) to tell whether each tutorial has been watched
	public const string ELEMENTS_DRAGGED_TUTORIAL_KEY = "ElementsDraggedIntoGatheringTutorial";
	public const string CRAFTING_TUTORIAL_KEY = "CraftingTutorial";
	public const string BUY_HINT_TUTORIAL_KEY = "BuyHintTutorial";
	public const string UPGRADE_POWERUP_TUTORIAL_KEY = "UpgradePowerupTutorial";
	public const string TIER_SWITCH_TUTORIAL_KEY = "TierSwitchTutorial";

	// An Array of all the keys for crafting tutorials
	public static string [] AllCraftingModeTutorials = {ELEMENTS_DRAGGED_TUTORIAL_KEY, CRAFTING_TUTORIAL_KEY, BUY_HINT_TUTORIAL_KEY, UPGRADE_POWERUP_TUTORIAL_KEY, TIER_SWITCH_TUTORIAL_KEY};
	#endregion

	#region INITIALIZATION_METHODS

	/// <summary>
	/// Initializes the powerup indexes. 
	/// Used by the ActivatePowerUp script to associate powerups with tiers
	/// </summary>
	public static void InitializePowerupIndexes () {
		POWERUP_INDEXES = new Dictionary<string, int>();
		POWERUP_INDEXES.Add("LaneConversion", 0);
		POWERUP_INDEXES.Add("SlowFall", 1);
		POWERUP_INDEXES.Add("Fuel", 2);
		POWERUP_INDEXES.Add("Multiply", 3);
		POWERUP_INDEXES.Add("BucketShield", 4);
		POWERUP_INDEXES.Add("TapToCollect", 5);
		POWERUP_INDEXES.Add("Invincible", 6);
		POWERUP_INDEXES.Add("TotalConversion", 7);
		POWERUP_INDEXES.Add("CollectAll", 8);
	}


	// Loads in the powerup sprites array
	// Should be called before the powerups are used in gathering
	public static void InitializePowerUpSprites () {
		if (!POWERUP_SPRITES_LOADED) {
			for (int i = 0; i < POWERUP_SPRITE_FILENAMES.Length; i++) {
				POWERUP_SPRITES[i] = Resources.Load<Sprite>(POWERUP_FILE_PATH + POWERUP_SPRITE_FILENAMES[i]);
			}
			POWERUP_SPRITES_LOADED = true;
		}
	}
	#endregion
}
