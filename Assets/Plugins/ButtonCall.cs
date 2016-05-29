using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public class ButtonCall : MonoBehaviour {

	public Image IndicatorLight;
	public GameObject ConnectButton;
	public Text EmotionText;
	public Text HrText;
	public Text BaselineText;

	public int myEmotionMeasurement;
	public double myBaseline;
	public EmotionZone myEmotionZone;

	private EmotionDeviceManager manager;

	//events to be subscribed and unsubscribed from the device
	
	EmotionDeviceManager.NewZoneEventHandler newZoneEventHandler;
	EmotionDeviceManager.DeviceConnectedDeviceHandler deviceConnectedDeviceHandler;
	EmotionDeviceManager.NewEmotionMeasurementEventHandler newEmotionMeasurementEventHandler;
	EmotionDeviceManager.ThresholdCalculatedEventHandler thresholdCalculatedEventHandler;
	EmotionDeviceManager.GameInvalidEventHandler gameInvalidEventHandler;

	
	//Singleton instantiation
	private static ButtonCall instance;
	private ButtonCall() {}
	public static ButtonCall Instance {
		get {
			if (instance == null) {
				instance = new ButtonCall ();
			}
			return instance;
		}
	}

	// Use this for initialization
	void Start () {
		//EmotionDeviceManager.Instance.Initialize ();
		manager = EmotionDeviceManager.Instance;

		Invoke ("SubscribeEvents", 1);
	}
	
	// Update is called once per frame
	void Update () {
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
		HrText.text = measurement.MeasurementType + ": " + (Convert.ToInt32 (measurement.Measurement));
	}

	private void UpdateEmotionZoneDisplay(EmotionZone emotionZone){
		EmotionText.text = emotionZone.ToString ();
		ChangeIndicator (emotionZone);
	}

	private void UpdateThresholdCalculationDisplay(String threshold){
		BaselineText.text = "Threshold: " + threshold;
	}
	
	/*Optional SceneChangeButton, switches to previous scene numerically (order scenes properly in Build Settings)
	Better practice to name scene specifically in LoadLevel. Remember to use DontDestroyOnLoad */
	public void SceneChange(){
		int currentLevel = Application.loadedLevel;
		Application.LoadLevel(currentLevel - 1);
	}

	public void ChangeIndicator(EmotionZone emotionZone){
		if (emotionZone == EmotionZone.CALM_EMOTION_ZONE) {
			IndicatorLight.color = Color.green;
		} else if (emotionZone == EmotionZone.WARNING_EMOTION_ZONE) {
			IndicatorLight.color = Color.yellow;
		} else if (emotionZone == EmotionZone.OVER_EMOTION_ZONE) {
			IndicatorLight.color = Color.red;
		}
	}

	//Starts Device Scan, Connects if available device, begins pulling heartrate data; bpm and emotion zones
	public void StartScan(){
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
			//Unsubscribe NewZoneEvent
			manager.NewZoneEvent -= newZoneEventHandler;
			//Unsubscribe ConnectedEvent
			manager.ConnectedEvent -= deviceConnectedDeviceHandler;
			//Unsubscribe NewEmotionMeasurementEvent
			manager.NewEmotionMeasurementEvent -= newEmotionMeasurementEventHandler;
			//Unsubscribe ThresholdCalculatedEvent
			manager.ThresholdCalculatedEvent -= thresholdCalculatedEventHandler;
			//Unsubscribe GameInvalidEvent
			manager.GameInvalidEvent -= gameInvalidEventHandler;
		}
	}

	void OnNewZone(object source, NewZoneEventArgs e){
		manager.Log ("**** New Zone Event: " + e.NewZone + "****");
		UpdateEmotionZoneDisplay (e.NewZone);
	}
	
	void OnConnected(object source, ConnectedEventArgs e){
		//Update connection button state
		UpdateConnectionButton (e.IsConnected);

		if(e.IsConnected)
			manager.Log ("**** New Connection: " + e.ConnectedDevice + " ****");
			
		else 
			manager.Log ("**** Disconnected ****");
	}

	void OnNewEmotionMeasurement(object source, NewEmotionMeasurmentEventArgs e){
		UpdateEmotionMeasurementDisplay (e.EmotionMeasurement);
	}

	void OnThresholdCalculated(object source, ThresholdCalculatedEventArgs e){
		UpdateThresholdCalculationDisplay (e.Threshold);
	}

	void OnGameInvalid(object source, GameInvalidEventArgs e)
	{
		
	} 
}
