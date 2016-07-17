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

	[System.Serializable]
	internal class GameData {
		Queue<string> mostRecentElementsDiscovered;
		int elementsDicoveredSinceLastReward;

		internal GameData () {
			mostRecentElementsDiscovered = new Queue<string>();
			ResetElementsSinceRewardCount();
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
	}

}
