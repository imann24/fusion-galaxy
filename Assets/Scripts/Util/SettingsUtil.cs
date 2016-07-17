/*
 * Author(s): Isaiah Mann
 * Description: Static functions to adjust settings
 */

public static class SettingsUtil {
	const string vibrateEnabledKey = "Vibrate";

	public static void ToggleVibrate (bool isEnabled) {	
		Utility.SetPlayerPrefIntAsBool(
			vibrateEnabledKey,
			isEnabled);
	}

	public static bool VibrateEnabled () {
		return Utility.PlayerPrefIntToBool (
			vibrateEnabledKey,
			true
		);
	}
}
