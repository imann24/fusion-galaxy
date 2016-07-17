/*
 * Author(s): Isaiah Mann
 * Description: Simple class to serialize game data
 * Notes: Most of Fusion Galaxy's save functionality is handled through PlayerPrefs (tech debt):
 * this class only handles more recent additions to data
 */

using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

public static class DataController {
	static DataController () {
		Load();
	}

	const string filePath = "save.dat";
	internal const int elementDiscoverTrackCount = 5;

	static bool _dataLoaded;
	static GameData _data;
	
	public static bool hasLoaded {
		get {
			return _dataLoaded;
		}
	}

	public static void ResetSave () {
		InitDataAsNew();
		Save();
	}

	static string GetPath () {
		return Path.Combine(Application.persistentDataPath, filePath);
	}

	static void InitDataAsNew () {
		_data = new GameData();
	}

	static void CheckDataForInit () {
		if (!_dataLoaded) {
			InitDataAsNew();
		}
	}

	static void Save () {
		CheckDataForInit();
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		FileStream file;
		try {
			file = File.Open(GetPath(), FileMode.Open);
		} catch {
			file = File.Create(GetPath());
		}
		binaryFormatter.Serialize(file, _data);
		file.Close();
	}
	
	static void Load () {
		try {
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			FileStream file;
			file = File.Open(GetPath(), FileMode.Open);
			_data = (GameData) binaryFormatter.Deserialize(file);
			_data.CheckForNullValues();
			file.Close();
		} catch {
			InitDataAsNew();
		}
		_dataLoaded = true;
	}

	public static string[] GetMostRecentElementsDiscovered () {
		return _data.GetMostRecentElementsDiscovered();
	}

	public static void AddDiscoveredElementToLog (string elementName) {
		_data.AddDiscoveredElement(elementName);
		Save();
	}

	public static bool ShouldGiveElementDiscoveryReward () {
		return _data.ShouldGiveElementDiscoveryReward();
	}

	public static void ResetElementsSinceRewardCount () {
		_data.ResetElementsSinceRewardCount();
		Save();
	}

	public static bool GetPowerUpUnlocked (string powerUpName) {
		return _data.GetPowerUpUnlocked(powerUpName);
	}
	
	public static void SetPowerUpUnlock (string powerUpName, bool isUnlocked) {
		_data.SetPowerUpUnlock(powerUpName, isUnlocked);
		Save ();
	}

	[System.Serializable]
	internal class GameData {
		Queue<string> mostRecentElementsDiscovered = new Queue<string>();
		int elementsDicoveredSinceLastReward;
		Dictionary<string, bool> powerUpUnlocks = new Dictionary<string, bool>();

		internal GameData () {
			mostRecentElementsDiscovered = new Queue<string>();
			ResetElementsSinceRewardCount();
			powerUpUnlocks = new Dictionary<string, bool>();
			foreach (string powerupName in GlobalVars.POWERUP_INDEXES.Keys) {
				powerUpUnlocks.Add(powerupName, false);
			}
		}

		internal GameData (Queue<string> mostRecentElementsDiscovered, 
		                   int elementsDicoveredSinceLastReward) {
			this.mostRecentElementsDiscovered = mostRecentElementsDiscovered;
			this.elementsDicoveredSinceLastReward = elementsDicoveredSinceLastReward;
		} 

		internal void AddDiscoveredElement (string elementName) {
			if (mostRecentElementsDiscovered.Contains(elementName)) {
				return;
			}

			mostRecentElementsDiscovered.Enqueue(elementName);
			elementsDicoveredSinceLastReward++;
			if (mostRecentElementsDiscovered.Count > elementDiscoverTrackCount) {
				mostRecentElementsDiscovered.Dequeue();
			}
		}

		internal bool ShouldGiveElementDiscoveryReward () {
			return elementsDicoveredSinceLastReward >= elementDiscoverTrackCount;
		}

		internal string[] GetMostRecentElementsDiscovered () {
			return mostRecentElementsDiscovered.ToArray();
		}

		internal void ResetElementsSinceRewardCount () {
			elementsDicoveredSinceLastReward = 0;
		}

		internal bool GetPowerUpUnlocked (string powerUpName) {
			bool isUnlocked;
			if (powerUpUnlocks.TryGetValue(powerUpName, out isUnlocked)) {
				return isUnlocked;
			} else {
				return false;
			}
		}

		internal void SetPowerUpUnlock (string powerUpName, bool isUnlocked) {
			if (powerUpUnlocks.ContainsKey(powerUpName)) {
				powerUpUnlocks[powerUpName] = isUnlocked;
			} else {
				powerUpUnlocks.Add(powerUpName, isUnlocked);
			}
		}

		internal void CheckForNullValues () {
			if (mostRecentElementsDiscovered == null) {
				mostRecentElementsDiscovered = new Queue<string>();
			}
			if (powerUpUnlocks == null) {
				powerUpUnlocks = new Dictionary<string, bool>();
			}
		}
	}

}
