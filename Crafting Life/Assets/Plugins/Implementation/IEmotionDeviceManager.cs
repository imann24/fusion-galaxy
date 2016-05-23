using System;

/**
 * HRM Device Utility method interface
 * 
 * Nick Snietka
 **/
public interface IEmotionDeviceManager
{
	/**
	 * Initialize and set/reset all Emotion Device Manager attributes
	 */
	void Initialize();

	/**
	 * Denitialize and set/reset all Emotion Device Manager
	 */
	void DeInitialize();

	/**
	 * Begin scanning for devices in range
	 */
	void StartDeviceScan();

	/**
	 * Stop scanning
	 */
	void StopDeviceScan();

	/**
	 * Connect to the peripheral. Once connected, subscribe to emotion measurement events.
	 * 
	 * @param: Device instance to connect to
	 */
	void ConnectToDevice(Device device);

	/**
	 * Disconnect from device. Unsubscribe from device events.
	 * 
	 * @param: Device instance to disconnect from
	 */
	void DisconnectDevice(Device device);

	/**
	 * Disconnect currently connected device
	 */
	void DisconnectDevice();

	/**
	 * Calculate the baseline emotion measurement. If the baseline measurement is out of the specified range,
	 * the baseline will continue to be calculated until it falls within the appropriate range.
	 **/
	void CalculateEmotionBaseline();

	/**
	 * Called when the game has started
	 **/
	void OnStartGame();

	/**
	 * Called when the game has ended
	 **/
	void OnStopGame();

	/**
	 * Set the threshold addition value to be added to the baseline value to 
	 * calculate the threshold value. If not set, will set to default.
	 * 
	 * @param Integer of the added value to calculate threshold
	 */
	void SetThresholdAddition(int additionVal);
	

	/**
	 * Get the current device connection state, e.g. CONNECTED, CONNECTING, DISCONNECTED
	 * 
	 * @return DeviceConnectionState enum
	 */
	DeviceConnectionState GetDeviceConnectionState();
	
	/**
	 * Get the Device currently connected, if any
	 * 
	 * @return Instance of currently connected device
	 */
	Device GetDevice();

	/**
	 * Get the battery level of the currently connected device
	 *
	 */
	int GetDeviceBatteryLevel();


	/**
	 * NOT YET IMPLEMENTED
	 */
	DeviceStatus GetDeviceStatus();

	/**
	 * Log message
	 */
	void Log(string logMessage);
	
}


