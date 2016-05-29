using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class SDKDeviceConnector : MonoBehaviour {

	//tuning variables
	public int threshholdTolerance = 5; //basically a difficulty modifier
	public float searchFrequency = 0.5f; //how often in seconds the coroutine tries to connect (if no device is found initially)

	//script references
	private static SDKDeviceConnector instance;
	//bools to determin behavior
	private static bool firstLoad = true;
	private bool suppressOnLoadCall = false;

	//singleton implemenation
	void Awake () {

		if (!GlobalVars.MEDICAL_USE) { // destroys the gameobject if medical use is not turned on
			Destroy(gameObject);
		} else if (instance == null) {
			instance = this;
			DontDestroyOnLoad (gameObject);

		} else {
			suppressOnLoadCall = true;
			Destroy(gameObject);
		}
	}

	void OnLevelLoad (int level) {
		if (suppressOnLoadCall) {
			return;
		}

		if (firstLoad) {
			firstLoad = false;
			return;
		}

		EmotionDeviceManager.Instance.StopDeviceScan ();
	}
	// Use this for initialization
	void Start () {
		InitializeDeviceManager ();
	}
	
	// Update is called once per frame
	void Update () {

	}

	void OnNewZone (object souce, NewZoneEventArgs e) {
		EmotionDeviceManager.Instance.Log("***** New Zone Event: " + e.NewZone + " *****");
	}

	void OnConnected (object source, ConnectedEventArgs e) {
		ReadyToEnterGame ();

		if (e.IsConnected) {
			EmotionDeviceManager.Instance.Log("***** New Connection: " + e.ConnectedDevice + " *****");
			Debug.Log ("This is the device: " + EmotionDeviceManager.Instance.ConnectedDevice);
		}
	}

	//called when the game locates a device
	void ReadyToEnterGame () {

	}

	private void InitializeDeviceManager () {
		EmotionDeviceManager.Instance.Initialize ();
		EmotionDeviceManager manager= EmotionDeviceManager.Instance;
		manager.ConnectedEvent += new EmotionDeviceManager.DeviceConnectedDeviceHandler(OnConnected);
		manager.NewZoneEvent += new EmotionDeviceManager.NewZoneEventHandler(OnNewZone);
		//EmotionDeviceManager.Instance.SetThresholdAddition (threshholdTolerance);
	}

	public void setThreshold () {
		EmotionDeviceManager.Instance.CalculateEmotionBaseline();
	}

	public void scanForDevices () {
		EmotionDeviceManager.Instance.StartDeviceScan ();
		if (EmotionDeviceManager.Instance.bluetoothDeviceScript != null && 
		    EmotionDeviceManager.Instance.bluetoothDeviceScript.DiscoveredDeviceList != null &&
		    EmotionDeviceManager.Instance.bluetoothDeviceScript.DiscoveredDeviceList.Count > 0) {
			EmotionDeviceManager.Instance.ConnectToDevice (new HRMDevice("",EmotionDeviceManager.Instance.bluetoothDeviceScript.DiscoveredDeviceList [0]));
		}
	}

	IEnumerator KeepTryingToConnect () {
		while (EmotionDeviceManager.Instance.GetDeviceConnectionState() == DeviceConnectionState.DISCONNECTED) {
			if (EmotionDeviceManager.Instance.bluetoothDeviceScript != null && 
			    EmotionDeviceManager.Instance.bluetoothDeviceScript.DiscoveredDeviceList != null &&
			    EmotionDeviceManager.Instance.bluetoothDeviceScript.DiscoveredDeviceList.Count > 0) {
				EmotionDeviceManager.Instance.ConnectToDevice (new HRMDevice("",EmotionDeviceManager.Instance.bluetoothDeviceScript.DiscoveredDeviceList [0]));
			}
			yield return new WaitForSeconds(searchFrequency);
		}
	}
}
