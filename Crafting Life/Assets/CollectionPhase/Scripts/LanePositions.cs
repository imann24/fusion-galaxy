//#define DEBUG
using UnityEngine;
using System.Collections;

public class LanePositions : MonoBehaviour {
	/// <summary>
	/// Gets the lane X positions.
	/// </summary>
	/// <returns>The lane X positions.</returns>
	///
	public static Vector3 [] GetLanePositions (float yPosition, float zPosition) {
		//creates an array to hold screen x points for lanes
		float [] screenQuadrants = new float[GlobalVars.NUMBER_OF_LANES];

		//creates an array to hold Vector3 positions for each of the lanes
		Vector3 [] screenPositions = new Vector3[GlobalVars.NUMBER_OF_LANES];

		float quadrantWidth = (float)Screen.width/(float) GlobalVars.NUMBER_OF_LANES;
			#if DEBUG
		Debug.Log("The quadrant width is " + quadrantWidth);
			#endif
		float currentPosition = quadrantWidth/2f;
		for (int i = 0; i < GlobalVars.NUMBER_OF_LANES; i++) {
			screenQuadrants[i] = currentPosition;
			#if DEBUG
			Debug.Log("Current position is " + currentPosition);
			#endif
			currentPosition += quadrantWidth;
		}

		for (int i = 0; i < GlobalVars.NUMBER_OF_LANES; i++) {
			screenPositions[i] = Camera.main.ScreenToWorldPoint(new Vector3(screenQuadrants[i], Screen.height, 0));
			screenPositions[i] = new Vector3(screenPositions[i].x, yPosition, zPosition);
			#if DEBUG
				Debug.Log("Current vector 3 is " + screenPositions[i]);
			#endif
		}

		return screenPositions;
	}

	public static Vector3 [] GetLanePositions (float zPosition) {
		return GetLanePositions (Screen.height, zPosition);
	}

	public static float GetDistanceBetweenLanes (float zPosition) {
		Vector3[] positions = GetLanePositions(zPosition);
		return Mathf.Abs(positions[0].x - positions[1].x);
	}

	public static float GetLaneTolerance (float zPosition) {
		return GetDistanceBetweenLanes(zPosition)/2f;
	}
}
