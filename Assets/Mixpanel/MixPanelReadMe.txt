Mixpanel contains the scripts and assemblies necessary to send events to the Mixpanel analytics service. Little modification should be needed to Mixpanel.cs itself, unless you want to add global variables that are sent with every event. Examples of events can be found in MixpanelController.cs. You can add more there and tie them to event calls in the game to send them to the Mixpanel service.


NOTE: Remember to update the version number on the Analytics.prefab in Global_Utility_Assets/prefabs in order for the event number to be up to date in Mixpanel.