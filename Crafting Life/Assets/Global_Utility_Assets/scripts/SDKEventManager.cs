#define DEBUG
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public class SDKEventManager : MonoBehaviour {
	//event call for over threshold
	public delegate void OverThresholdAction();
	public static event OverThresholdAction OnOverThreshold;

	public delegate void UnderThresholdAction ();
	public static event UnderThresholdAction OnUnderThreshold;

	public delegate void NearThresholdAction (bool zoneRising);
	public static event NearThresholdAction OnNearThreshold;

	public delegate void ConnectedToDeviceAction ();
	public static event ConnectedToDeviceAction OnConnectedToDevice;

	public delegate void InvalidGameAction ();
	public static event InvalidGameAction OnInvalidGame;

	public static Image IndicatorLight;
	public GameObject ConnectButton;
	public Text EmotionText;
	public Text HrText;
	public Text BaselineText;

	public int myHr;
	public double myBaseline;
	public static EmotionZone MyEmotionZone;
	public static int PreviousLevel;

	private EmotionDeviceManager manager;

	//tracks the scene SDK was loaded
	private static GlobalVars.Scenes SceneLoadedFrom = GlobalVars.Scenes.Gathering;

	//events to be subscribed and unsubscribed from the device

	EmotionDeviceManager.NewZoneEventHandler newZoneEventHandler;
	EmotionDeviceManager.DeviceConnectedDeviceHandler deviceConnectedDeviceHandler;
	EmotionDeviceManager.NewEmotionMeasurementEventHandler newEmotionMeasurementEventHandler;
	EmotionDeviceManager.ThresholdCalculatedEventHandler thresholdCalculatedEventHandler;
	EmotionDeviceManager.GameInvalidEventHandler gameInvalidEventHandler;
	
	//Singleton instantiation
	private static SDKEventManager instance;
	private SDKEventManager() {}
	public static SDKEventManager Instance {
		get {
			if (instance == null) {
				instance = new SDKEventManager ();
			}
			return instance;
		}
	}

	//singleton  implementation
	void Awake () {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad(gameObject);
		} else {
			Destroy(gameObject);
		}
	}

	// Use this for initialization
	void Start () {
		//EmotionDeviceManager.Instance.Initialize ();
		manager = EmotionDeviceManager.Instance;
		//creates the events to be subscribed 
		Invoke ("AssignButtonCalls", 0.5f);
		Invoke ("SubscribeEvents", 1);

		//sets the reference to load the 
		PreviousLevel = (int) GlobalVars.Scenes.Gathering;

		//calls to handle when to start and stop tracking heartrate based on the start and stop of the game timer
		CollectionTimer.OnStartGame += OnStartGame;
		CollectionTimer.OnEndGame += OnStopGame;

#if DEBUG
		// debugging statements to give indiciation of feedback
		OnOverThreshold += printOverThresholdMessage;
		OnNearThreshold += printNearThresholdMessage;
		OnUnderThreshold += printUnderThresholdMessage;
		OnConnectedToDevice += printConnectedMessage;
		OnConnectedToDevice += ChangeIndicator;

#endif
	}

	void OnDestroy () {
#if DEBUG
		//unsubscribing from debugging statements
		OnOverThreshold -= printOverThresholdMessage;
		OnNearThreshold -= printNearThresholdMessage;
		OnUnderThreshold -= printUnderThresholdMessage;		
		OnConnectedToDevice -= printConnectedMessage;
		OnConnectedToDevice -= ChangeIndicator;
#endif

		//unsubscribing from all events
		UnsubscribeEvents();
	}
	
	// Update is called once per frame
	void Update () {


 	}

	void OnLevelWasLoaded (int level) {
		//assigns the button call because it will be null if the scene is reloaded (due to singleton implementation) 
		Invoke ("AssignButtonCalls", 0.5f);
		Invoke ("UnsubscribeEvents", 1);
		Invoke ("SubscribeEvents", 1);
		if (level == (int)GlobalVars.Scenes.SDK) {
			SetReferencesToUI();
		}
	}

	/// <summary>
	/// Sets the references to the User Interface.
	/// </summary>
	private void SetReferencesToUI () {
		//sets the references to the text components on the UI again
		EmotionText = AccessUIText.GetUIText("EmotionText");
		HrText = AccessUIText.GetUIText("HrText");
		BaselineText = AccessUIText.GetUIText("BaselineText");
		ConnectButton = AssignButtonCall.GetButton("ConnectButton").gameObject;
	}

	public void UpdateConnectionButton(Boolean isConnected){
		if (isConnected == true) {
			ConnectButton.GetComponent<Button> ().interactable = false;
		} else {
			ConnectButton.GetComponent<Button> ().interactable = true;
		}
	}

	//Updates Hr Display and EmotionZone Display
	private void UpdateEmotionMeasurementDisplay(EmotionMeasurement measurement){
		if (HrText != null) {
			HrText.text = measurement.MeasurementType + ": " + (Convert.ToInt32 (measurement.Measurement));
		}
	}

	private void UpdateEmotionZoneDisplay(EmotionZone emotionZone){
		if (EmotionText != null) {
			EmotionText.text = emotionZone.ToString ();
		}
		ChangeIndicator (emotionZone);
	}

	private void UpdateThresholdCalculationDisplay(String threshold){
		if (BaselineText != null) {
			BaselineText.text = "Threshold: " + threshold;
		}
	}
	

	//loads the scene and set the load call to the scene it was loaded from
	public static void LoadSDKScene () {
		SceneLoadedFrom = (GlobalVars.Scenes) Application.loadedLevel;
		Application.LoadLevel((int)GlobalVars.Scenes.SDK);
	}

	/*Optional SceneChangeButton, switches to previous scene numerically (order scenes properly in Build Settings)
	Better practice to name scene specifically in LoadLevel. Remember to use DontDestroyOnLoad */
	public void SceneChange() {
		//int currentLevel = Application.loadedLevel;
		Utility.ShowLoadScreen();
		Application.LoadLevel((int) SceneLoadedFrom);
	}

	/// <summary>
	/// Assigns the button calls.
	/// </summary>
	public void AssignButtonCalls () {
		//assigns the button call to exit the scene
		AssignButtonCall.AssignButton("SceneChangeButton", SceneChange); 

		//assigns the button call to exit the scene
		AssignButtonCall.AssignButton("ConnectButton", ConnectToDevice); 

		//assigns the button call to exit the scene
		AssignButtonCall.AssignButton("StartScanningButton", StartScan); 

		//assigns the button call to exit the scene
		AssignButtonCall.AssignButton("SetBaselineButton", CalculateEmotionalBaseline); 

		//assigns the button call to exit the scene
		AssignButtonCall.AssignButton("DisconnectButton", Disconnect); 
	}

	public void ChangeIndicator(EmotionZone emotionZone){
		if (IndicatorLight != null) {
			if (emotionZone == EmotionZone.CALM_EMOTION_ZONE) {
				IndicatorLight.color = Color.green;
				MyEmotionZone = emotionZone;
			} else if (emotionZone == EmotionZone.WARNING_EMOTION_ZONE) {
				IndicatorLight.color = Color.yellow;
				MyEmotionZone = emotionZone;
			} else if (emotionZone == EmotionZone.OVER_EMOTION_ZONE) {
				IndicatorLight.color = Color.red;
				MyEmotionZone = emotionZone;
			}
		}
	}

	public void ChangeIndicator () {
		ChangeIndicator(MyEmotionZone);
	}

	//Starts Device Scan, Connects if available device, begins pulling heartrate data; bpm and emotion zones
	public void StartScan(){
#if DEBUG
		Debug.Log("Start the scan for the devices");
#endif
		EmotionDeviceManager.Instance.StartDeviceScan();
	}

	//Used to (re)calculate baseline
	public void CalculateEmotionalBaseline(){
		EmotionDeviceManager.Instance.CalculateEmotionBaseline();
	}
	
	//Call when game is started
	public void OnStartGame(){
 		EmotionDeviceManager.Instance.OnStartGame ();
	}
	 
	//Call when game is stopped
	public void OnStopGame(){
		EmotionDeviceManager.Instance.OnStopGame ();
	}


	//Connects to found device
	public void ConnectToDevice(){
		EmotionDeviceManager.Instance.ConnectToDevice (new HRMDevice("",EmotionDeviceManager.Instance.bluetoothDeviceScript.DiscoveredDeviceList [0]));
	}

	//Disconnects from current device
	public void Disconnect(){
		EmotionDeviceManager.Instance.DisconnectDevice ();
	}
	
	// *** Supplementary functions

	//Initializes an instance of BluetoothDeviceScript, already taken care of in Start
	public void Initialize(){
		EmotionDeviceManager.Instance.Initialize ();
		MyEmotionZone = EmotionDeviceManager.Instance.CurrentEmotionZone;
	}

	//Should be unecessary if playing emotional feedback game (always using Biomedical device)
	public void DeInitialize(){
		EmotionDeviceManager.Instance.DeInitialize ();
	}

	//Stops Device Scan, should be unecessary, already taken care of in EmotionDeviceManger
	public void StopScan(){
		EmotionDeviceManager.Instance.StopDeviceScan();
	}

	//Sets threshold should be unecessary, already taken care of in EmotionDeviceManager
	public void SetThreshold(){
		EmotionDeviceManager.Instance.SetThresholdAddition(2);
	}

	//Subscribes functions to Events, already taken care of in Start
	public void SubscribeEvents(){
#if DEBUG 
		Debug.Log("Subscribing to all events");
#endif
		//Subscribe NewZoneEvent
		manager.NewZoneEvent += (newZoneEventHandler = new EmotionDeviceManager.NewZoneEventHandler (OnNewZone));
		//SubscribeConnectedEvent
		manager.ConnectedEvent += (deviceConnectedDeviceHandler = new EmotionDeviceManager.DeviceConnectedDeviceHandler (OnConnected));
		//Subscribe NewEmotionMeasurementEvent
		manager.NewEmotionMeasurementEvent += (newEmotionMeasurementEventHandler = new EmotionDeviceManager.NewEmotionMeasurementEventHandler (OnNewEmotionMeasurement));
		//Subscribe ThresholdCalculatedEvent
		manager.ThresholdCalculatedEvent += (thresholdCalculatedEventHandler = new EmotionDeviceManager.ThresholdCalculatedEventHandler (OnThresholdCalculated));
		//Subscribe GameInvalidEvent
		manager.GameInvalidEvent += (gameInvalidEventHandler = new EmotionDeviceManager.GameInvalidEventHandler (OnGameInvalid));

	}

	//unsubscribes from all events
	public void UnsubscribeEvents () {
#if DEBUG 
		Debug.Log("Unsubscribing from all events");
#endif
		if (newZoneEventHandler != null) {
			//Subscribe NewZoneEvent
			manager.NewZoneEvent -= newZoneEventHandler;
			//SubscribeConnectedEvent
			manager.ConnectedEvent -= deviceConnectedDeviceHandler;
			//Subscribe NewEmotionMeasurementEvent
			manager.NewEmotionMeasurementEvent -= newEmotionMeasurementEventHandler;
			//Subscribe ThresholdCalculatedEvent
			manager.ThresholdCalculatedEvent -= thresholdCalculatedEventHandler;
			//Unsubscribe GameInvalidEvent
			manager.GameInvalidEvent -= gameInvalidEventHandler;
		}
	}

	void OnNewZone(object source, NewZoneEventArgs e){
		BluetoothLEHardwareInterface.Log ("**** New Zone Event: " + e.NewZone + "****");

		//triggers the zone events calls
		if (e.NewZone == EmotionZone.CALM_EMOTION_ZONE && OnUnderThreshold != null) { //if the player becomes calm
			OnUnderThreshold();
		} else if (e.NewZone == EmotionZone.WARNING_EMOTION_ZONE && OnNearThreshold != null) { //if the player goes from calm to near threshold
			OnNearThreshold(MyEmotionZone == EmotionZone.CALM_EMOTION_ZONE);
		} else if (e.NewZone == EmotionZone.OVER_EMOTION_ZONE && OnOverThreshold != null) { //if the player goes over threshold
			OnOverThreshold();
		}

		//updates the emotion zone stored in the script
		UpdateEmotionZoneDisplay (e.NewZone);
	}
	
	void OnConnected(object source, ConnectedEventArgs e){
		//Update connection button state
		UpdateConnectionButton (e.IsConnected);

		if(e.IsConnected)
			BluetoothLEHardwareInterface.Log ("**** New Connection: " + e.ConnectedDevice + " ****");
			
		else 
			BluetoothLEHardwareInterface.Log ("**** Disconnected ****");
	}

	void OnNewEmotionMeasurement(object source, NewEmotionMeasurmentEventArgs e){
		UpdateEmotionMeasurementDisplay (e.EmotionMeasurement);
	}

	void OnThresholdCalculated(object source, ThresholdCalculatedEventArgs e){
		UpdateThresholdCalculationDisplay (e.Threshold);
	}

	void OnGameInvalid(object source, GameInvalidEventArgs e) {
		if (OnInvalidGame != null) {
			OnInvalidGame();
		}
	}

	public static void SetIndicatorLight (Image indicatorLight) {
#if DEBUG
		Debug.Log("Set the indicator light to " + indicatorLight.name);
#endif
		IndicatorLight = indicatorLight;
	}

#region DEBUG
	private void printOverThresholdMessage () {
		Debug.Log("*****" + "Over Threshold" + "*****");
	}

	private void printUnderThresholdMessage () {
		Debug.Log("*****" + "Under Threshold" + "*****");
	}

	private void printNearThresholdMessage (bool zoneRising) {
		Debug.Log("*****" + "Near Threshold: Zone is rising" +  zoneRising + " *****");
	}

	private void printConnectedMessage () {
		Debug.Log("*****" + "Connected to Device" + "*****");
	}

#endregion
}
