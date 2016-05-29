using UnityEngine;
using System.Collections;

public class HRMGlobals : MonoBehaviour {
	public static readonly int MAX_ALLOWED_BASELINE_VALUE = 130;
	public static readonly int MIN_ALLOWED_BASELINE_VALUE = 50;
	public static readonly int WARNING_ZONE_BUFFER = 2;
	public static readonly int BASELINE_CALCULATION_TIME_MS = 10000;
}
