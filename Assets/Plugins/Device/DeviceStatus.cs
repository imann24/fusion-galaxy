using System;

/**
 * Status of what's going on with this device. 
 * 
 * Nick Snietka
 **/
public class DeviceStatus
{
	//Is the baseline being calculated as HR data comes in?
	private Boolean isCalculatingBaseline { get; set; }

	//Is the device reading information?
	private Boolean isReading { get; set; }

	//State of the peripheral (powered off, powered on/ready, unauthorized, unknown, etc)
	private String deviceState { get; set; }

	public DeviceStatus ()
	{
		isCalculatingBaseline = false;
		isReading = false;
		deviceState = "unknown";
	}
}


