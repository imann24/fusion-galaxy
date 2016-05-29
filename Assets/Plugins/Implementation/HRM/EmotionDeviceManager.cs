using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Timers;

public class EmotionDeviceManager : IEmotionDeviceManager
{
	public BluetoothDeviceScript bluetoothDeviceScript { get; set;}
	

	private string _deviceInformationServiceUUID = "Device Information"; //180A
	private string _heartRateServiceUUID = "Heart Rate"; //180D

	private string _readHRCharacteristicUUID = "2A37";
	private string[] serviceIds = new string[]{"180A","180D"};

	//Event
	public delegate void DeviceConnectedDeviceHandler(object source, ConnectedEventArgs eventArgs);
	public event DeviceConnectedDeviceHandler ConnectedEvent;
	public delegate void NewZoneEventHandler(object source, NewZoneEventArgs eventArgs);
	public event NewZoneEventHandler NewZoneEvent;
	public delegate void NewEmotionMeasurementEventHandler(object source, NewEmotionMeasurmentEventArgs eventArgs);
	public event NewEmotionMeasurementEventHandler NewEmotionMeasurementEvent;
	public delegate void ThresholdCalculatedEventHandler(object source, ThresholdCalculatedEventArgs eventArgs);
	public event ThresholdCalculatedEventHandler ThresholdCalculatedEvent;
	public delegate void GameInvalidEventHandler(object source, GameInvalidEventArgs eventArgs);
	public event GameInvalidEventHandler GameInvalidEvent; 


	//Emotion Zone
	private int _threshholdAddition;
	private EmotionZone _emotionZone;
	public EmotionZone CurrentEmotionZone
	{
		get
		{
			return _emotionZone;
		}
		set
		{
			BluetoothLEHardwareInterface.Log("current: " + _emotionZone.ToString() + " new: " + value.ToString());
			if(value != _emotionZone)
			{
				if(NewZoneEvent != null)
				{
					NewZoneEvent(this, new NewZoneEventArgs(value));
				}
				_emotionZone = value;
			}
		}
	}
	
	//Device Connection
	private bool _scanning;
	private bool _connected;
	private bool _connecting;
	private string _connectedID;
	private Device _connectedDevice;

	//Baseline
	private static bool _calculatingBaseline;
	private double _baselineSamples;
	private static double _baseline;
	private static Timer _baselineTimer;

	//List of devices found on scan	
	private List<HRMDevice> devicesFoundList;

	//Game play status
	private bool _gameIsActive;

	//Singleton instantiation
	private static EmotionDeviceManager instance;
	private EmotionDeviceManager() {
		Initialize ();
	}
	public static EmotionDeviceManager Instance {
		get {
			if (instance == null) {
				instance = new EmotionDeviceManager ();
			}
			return instance;
		}
	}

	/**
	 * Initialize member vars & BluetoothDeviceScript
	 */
	public void Initialize ()
	{
		CurrentEmotionZone = EmotionZone.CALM_EMOTION_ZONE;
		_connected = false;
		_connecting = false;
		_scanning = false;
		_connectedDevice = null;
		_connectedID = null;
		_calculatingBaseline = false;
		_baselineSamples = 0;
		_baseline = 0;
		_threshholdAddition = 0;
		_gameIsActive = false;
		devicesFoundList = new List<HRMDevice> ();


		bluetoothDeviceScript = BluetoothLEHardwareInterface.Initialize (true, false, () => {}, (error) => {});
	}

	/**
	 * Deinitialize BluetoothDeviceScript
	 */
	public void DeInitialize()
	{
		BluetoothLEHardwareInterface.DeInitialize (null);
	}

	/**
	 * Device connected attribute
	 */
	Boolean Connected
	{
		get { return _connected && _connectedDevice != null; }
		set
		{
			_connected = value;
			
			if (_connected) 
			{
				if(ConnectedEvent != null)
					ConnectedEvent(this, new ConnectedEventArgs(_connected,_connectedDevice.name));
				_connecting = false;
			}
			else //Not connected
			{
				if(ConnectedEvent != null)
					ConnectedEvent(this, new ConnectedEventArgs(_connected,null));
				//If game is active, we want to 
				if(_gameIsActive)
					forceGameEnd("Heart Rate Monitor was disconnected!");
				_connectedID = null;
				_connectedDevice = null;
			}
		}
	}

	/**
	 * Scanning for device attribute
	 */
	private Boolean Scanning {
		get { return _scanning; }
	}

	/**
	 * Get the currently connected device
	 */
	public Device ConnectedDevice
	{
		get { return _connectedDevice; } 
	}
	
	#region Device List Management
	
	private void AddPeripheral (string name, string address)
	{
		devicesFoundList.Add (new HRMDevice (name, address));
	}

	private void ClearDevicesFound(){
		devicesFoundList.Clear ();
	}
	
	#endregion


	#region IEmotionDeviceManager implementation

	public void StartDeviceScan ()
	{
		if (_scanning)
		{
			BluetoothLEHardwareInterface.Log ("Stop scanning for peripherals");
			BluetoothLEHardwareInterface.StopScan ();
			_scanning = false;
		}
		if(Connected)
		{
			BluetoothLEHardwareInterface.Log ("Disconnect current peripheral");
			DisconnectDevice (_connectedDevice);
		}
		// the first callback will only get called the first time this device is seen
		// this is because it gets added to a list in the BluetoothDeviceScript
		// after that only the second callback will get called and only if there is
		// advertising data available
		BluetoothLEHardwareInterface.ScanForPeripheralsWithServices (serviceIds, (address, name) => {
			BluetoothLEHardwareInterface.Log("Found Device");
			AddPeripheral (name, address);
			StopDeviceScan();
			ConnectToDevice(new HRMDevice(name,address));
			
		}, (address, name, rssi, advertisingInfo) => {
			
		});
		_scanning = true;
	}

	/**
	 * Stop scanning for devices
	 */
	public void StopDeviceScan ()
	{
		BluetoothLEHardwareInterface.StopScan ();
		_scanning = false;
	}


	public void ConnectToDevice(Device device)
	{
		if (!_connecting)
		{
			if (Connected)
			{
				DisconnectDevice (_connectedDevice);
			}
			else
			{
				BluetoothLEHardwareInterface.Log ("Connecting to peripheral");
				_connecting = true;
				BluetoothLEHardwareInterface.ConnectToPeripheral (device.address, (address) => {
				},
				(address, serviceUUID) => {
				},
				(address, serviceUUID, characteristicUUID) => {
					BluetoothLEHardwareInterface.Log ("uuidFound: " + serviceUUID + " uuidExpected: " + _heartRateServiceUUID);
					// discovered characteristic
					if (IsEqual(serviceUUID, _heartRateServiceUUID))
					{
						_connectedID = address;
						_connectedDevice = new HRMDevice(device.name, device.address);
						Connected = true;
						BluetoothLEHardwareInterface.Log ("^^^Connected to peripheral");
						if (IsEqual(characteristicUUID, _readHRCharacteristicUUID))
						{
							BluetoothLEHardwareInterface.Log ("Subscribing to HR characteristic");
							BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress (_connectedID, _heartRateServiceUUID, _readHRCharacteristicUUID, (deviceAddress, notification) => {
								
							}, (deviceAddress2, characteristic, data) => {
								OnNewHRMeasurement(deviceAddress2,characteristic,data);
							});
						}
					}
				}, (address) => {
					// this will get called when the device disconnects
					// be aware that this will also get called when the disconnect
					// is called above. both methods get call for the same action
					// this is for backwards compatibility
					Connected = false;
				});
				_connecting = false;
			}
		}
	}

	/**
	 * Disconnect device
	 */
	public void DisconnectDevice (Device device)
	{
		BluetoothLEHardwareInterface.DisconnectPeripheral (device.address, null);
		Connected = false;
	}

	/**
	 * Disconnect currently connected device
	 */
	public void DisconnectDevice ()
	{
		if (Connected) {		
			BluetoothLEHardwareInterface.DisconnectPeripheral (_connectedDevice.address, null);
			Connected = false;
			Initialize();
		}
	}

	/**
	 * Start calculating baseline
	 */
	public void CalculateEmotionBaseline ()
	{
		_calculatingBaseline = true;
		_baselineTimer = new Timer (HRMGlobals.BASELINE_CALCULATION_TIME_MS);
		_baselineTimer.Elapsed += new ElapsedEventHandler (_baseline_Timer_Elapsed);
		_baselineTimer.Enabled = true; //Enable it
	}

	/**
	 * Called when the game has started, so we want to start measuring the emotion measurements
	 **/
	public void OnStartGame()
	{
		_gameIsActive = true;
		this.Log ("Started recording emotion measurement");
	}
	
	/**
	 * Called when the game has ended, so we want to start measuring the emotion measurements
	 **/
	public void OnStopGame()
	{
		_gameIsActive = false;
		this.Log ("Stopped recording emotion measurement");
	}

	public void SetThresholdAddition (int additionVal)
	{
		_threshholdAddition = additionVal;
	}

	/**
	 * Method triggered on baseline timer elsapsed
	 */
	private static void _baseline_Timer_Elapsed(object sender, ElapsedEventArgs eventArgs)
	{
		stopCalculatingBaseline ();
		if (isValidHeartRate(_baseline)) {
			instance.triggerThresholdCalculatedEvent(_baseline);
			instance.Log("New Baseline: " + _baseline);
		} else {
			instance.triggerThresholdCalculatedEvent(0);
		}
	}

	private static bool isValidHeartRate(double heartRate)
	{
		if (heartRate > HRMGlobals.MIN_ALLOWED_BASELINE_VALUE && heartRate < HRMGlobals.MAX_ALLOWED_BASELINE_VALUE)
			return true;
		else 
			return false;
	}

	public DeviceConnectionState GetDeviceConnectionState ()
	{
		if (_connecting)
			return DeviceConnectionState.CONNECTING;
		if (_connected)
			return DeviceConnectionState.CONNECTED;

		return DeviceConnectionState.DISCONNECTED;
	}

	public DeviceStatus GetDeviceStatus ()
	{
		throw new System.NotImplementedException ();
	}

	public int GetDeviceBatteryLevel()
	{
		//Temporary implementation, return 75% on call if connected and -1 if not connected
		if (_connected)
			return 75;
		else
			return -1;
	}

	public Device GetDevice ()
	{
		return _connectedDevice;
	}

	public void Log(string logMessage)
	{
		BluetoothLEHardwareInterface.Log(logMessage);
	}

	#endregion

	private static void stopCalculatingBaseline()
	{
		_baselineTimer.Enabled = false;
		_calculatingBaseline = false;
	}

	private void forceGameEnd(String message)
	{
		OnStopGame ();

		if(GameInvalidEvent != null)
			GameInvalidEvent(this, new GameInvalidEventArgs(message));
	}

	private void triggerThresholdCalculatedEvent(double _baseline)
	{
		//Calculation threshold by adding threshold addition to the ceiling of the baseline
		int threshold = (int)Math.Ceiling(_baseline) + _threshholdAddition;
		if (ThresholdCalculatedEvent != null)
			ThresholdCalculatedEvent (this, new ThresholdCalculatedEventArgs (threshold.ToString ()));
	}	

	// Use this for initialization
	void Start ()
	{
	}

	// Update is called once per frame
	void Update ()
	{
	}

	/**
	 * Method called on each new HR Measurement
	 */
	private void OnNewHRMeasurement (string deviceAddress, string characteristicUUID, byte[] bytes)
	{
		BluetoothLEHardwareInterface.Log("New Heart Rate Triggered");
		if (deviceAddress.CompareTo (_connectedDevice.address) == 0)
		{
			if (IsEqual(characteristicUUID, _readHRCharacteristicUUID))
			{
				if(_baseline == 0)
					CalculateEmotionBaseline();

				EmotionMeasurement hrMeasurement = ProcessData(bytes);

				//Check if player is in new emotion zone
				EmotionZone currentZone = getEmotionZone(Convert.ToInt32(hrMeasurement.Measurement));
				CurrentEmotionZone = currentZone;
				BluetoothLEHardwareInterface.Log("Heart Rate: " + hrMeasurement.Measurement + " Baseline: " + _baseline + " Emotion Zone: " + currentZone);
			}
		}
	}
	

	/** 
	* Process the raw data received from the device into application usable data, 
	* according the the Bluetooth Heart Rate Profile.
	*/ 
	private EmotionMeasurement ProcessData(byte[] data)
	{
		// Heart Rate profile defined flag values
		const byte HEART_RATE_VALUE_FORMAT = 0x01;

		byte currentOffset = 0;
		byte flags = data[currentOffset];
		bool isHeartRateValueSizeLong = ((flags & HEART_RATE_VALUE_FORMAT) != 0);

		currentOffset++;
		
		ushort heartRateMeasurementValue = 0;
		
		if (isHeartRateValueSizeLong)
		{
			heartRateMeasurementValue = (ushort)((data[currentOffset + 1] << 8) + data[currentOffset]);
			currentOffset += 2;
		}
		else
		{
			heartRateMeasurementValue = data[currentOffset];
			currentOffset++;
		}

		//Calculate baseline
		if (_calculatingBaseline) {
			calculateBaseline(heartRateMeasurementValue);
		}
		EmotionMeasurement newMeasurement = new EmotionMeasurement (Convert.ToString (heartRateMeasurementValue), Convert.ToString (_baseline), "BPM", new DateTimeOffset ());
		if(NewEmotionMeasurementEvent != null)
			NewEmotionMeasurementEvent(this, new NewEmotionMeasurmentEventArgs(newMeasurement));
		return newMeasurement;
	}

	/**
	 * Add to calculating baseline based on heart rate measurement value
	 */
	private void calculateBaseline(ushort heartRateMeasurementValue)
	{
		this.Log("Calculating baseline...");
		_baselineSamples += 1;
		_baseline = _baseline * ((_baselineSamples - 1.0) / _baselineSamples * 1.0) + heartRateMeasurementValue * (1.0 / _baselineSamples * 1.0);
	}

	/**
	 * Get the emotion zone by comparing the current hr measurement and the baseline
	 */
	private EmotionZone getEmotionZone (int heartRateMeasurementValue)
	{
		if (heartRateMeasurementValue >= _baseline + _threshholdAddition)
			return EmotionZone.OVER_EMOTION_ZONE;
		if (heartRateMeasurementValue >= (_baseline + _threshholdAddition - HRMGlobals.WARNING_ZONE_BUFFER))
			return EmotionZone.WARNING_EMOTION_ZONE;
		else
			return EmotionZone.CALM_EMOTION_ZONE;
	}

	bool IsEqual(string uuid1, string uuid2)
	{
		if (uuid1.Length == 4)
			uuid1 = FullUUID (uuid1);
		if (uuid2.Length == 4)
			uuid2 = FullUUID (uuid2);
		
		return (uuid1.ToUpper().CompareTo(uuid2.ToUpper()) == 0);
	}

	string FullUUID (string uuid)
	{
		//TODO get surrounding uuid
		return "0000" + uuid + "-0000-1000-8000-00805f9b34fb";
	}
}

public class NewZoneEventArgs : EventArgs
{
	private EmotionZone _newZone;
	public NewZoneEventArgs(EmotionZone newZone)
	{
		_newZone = newZone;
	}
	public EmotionZone NewZone
	{
		get
		{
			return _newZone;
		}
	}
}

public class NewEmotionMeasurmentEventArgs : EventArgs
{
	private EmotionMeasurement _measurement;
	public NewEmotionMeasurmentEventArgs(EmotionMeasurement measurement)
	{
		_measurement = measurement;
	}
	public EmotionMeasurement EmotionMeasurement
	{
		get
		{
			return _measurement;
		}
	}
}

public class ThresholdCalculatedEventArgs : EventArgs
{
	private String _threshold;
	public ThresholdCalculatedEventArgs (String threshold)
	{
		_threshold = threshold;
	}
	public string Threshold
	{
		get
		{
			return _threshold;
		}
	}
}

public class GameInvalidEventArgs : EventArgs
{
	private String _invalidMessage;
	public GameInvalidEventArgs (String message)
	{
		_invalidMessage = message;
	}
	public String Message
	{
		get
		{
			return _invalidMessage;
		}
	}
}

public class ConnectedEventArgs : EventArgs
{
	private bool _deviceConnectionState;
	private string _deviceName;
	public ConnectedEventArgs(bool deviceConnectionState, string deviceName)
	{
		_deviceConnectionState = deviceConnectionState;
		_deviceName = deviceName;
	}
	public string ConnectedDevice
	{
		get
		{
			return _deviceName;
		}
	}
	public bool IsConnected
	{
		get
		{
			return _deviceConnectionState;
		}
	}
}




