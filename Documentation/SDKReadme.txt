Neuro'motion SDK 1.0

Description:
Neuromotion-SDK is a library used for mobile games to add the Neuro'motion emotional gaming elements. The SDK is implemented as a Unity plugin and supports Unity 5 games built for Android or iPhone.

Folders
	Assets: all SDK code. 
		Documentation: ReadMe and any informational SDK docs.
		Editor: 
		Example: example scenes that implement SDK methods. Helpful for understanding how SDK is used.
			BluetoothLETest: under construction. Will contain prefabs & scenes that fully implement SDK.
			GUISkin: N/A
			Multiple Levels: N/A
			Test: contains TestScript, used to test methods with basic button GUI. Each button calls a different method, prints results to log. 
		Plugins
			BLE: scripts for interfacing SDK with BLE Devices
			Device: classes for defining device attributes.
			Emotion: classes for defining elements related to Emotion (e.g. measurements, zones).
			Implementation: scripts for interfacing Unity game with SDK.
				HRM: implementation of SDK that is used within Unity game.
			Platform
				Android: implementation code for device integration with Android.
				iOS: implementation code for device integration with iOS.

What to focus on for 1.0:
	Example implementation of SDK found in /Assets/Example/Test/TestScript.cs
	Library of functions found in /Assets/Plugins/Implementation/HRM/EmotionDeviceManager.cs

For building:
	iOS: within Xcode, add Corebluetooth.framework to the target Linked Libraries & Frameworks.
