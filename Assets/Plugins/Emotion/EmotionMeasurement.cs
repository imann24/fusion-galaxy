using System;
public struct EmotionMeasurement
{
	//Read measurement value
	private string _measurement;

	//Baseline measurement value
	private string _baselineMeasurement;

	//Measurement type, e.g. BPM
	private string _measurementType;

	//Timestamp of measurement reading
	private DateTimeOffset _timestamp;

	//Create Emotion Measurement instance
	public EmotionMeasurement (string measurement, string baselineMeasurement, string measurementType, DateTimeOffset timestamp)
	{
		_measurement = measurement;
		_baselineMeasurement = baselineMeasurement;
		_measurementType = measurementType;
		_timestamp = timestamp;
	}

	public string Measurement
	{
		get { return _measurement; }
	}

	public string BaselineMeasurement 
	{
		get { return _baselineMeasurement; }
	}

	public string MeasurementType 
	{
		get { return _measurementType; }
	}

	public DateTimeOffset Timestamp
	{
		get { return _timestamp; }
	}
}


