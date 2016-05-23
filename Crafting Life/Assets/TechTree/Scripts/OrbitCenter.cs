using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class OrbitCenter : MonoBehaviour {
	[Range(0.0f,500.0f)]
	public float orbitRate;

	[Range(-10.0f,10.0f)]
	public float elementRate;

	float myRadius;
	float orbitFactor;
	float aimOffset;
	Vector2 center;
	float[] goToPolar;
	Dictionary<string,Element> elementDict; 

	// Use this for initialization
	void Start () {
		elementDict = GlobalVars.ELEMENTS_BY_NAME;
		elementRate = .5f;
		float myTier = elementDict [this.name.Substring (0, this.name.LastIndexOf ("Spawner"))].getTier ();
		//using a ternary operator to determine spin of orbit
		//float isOddTier = (myTier % 2 == 1) ? 1 : -1;
		orbitFactor = (1 /(-myTier));

		orbitRate = 0;
	    center = GameObject.Find("centerOfUniverse").transform.position;
		myRadius = Vector2.Distance (this.transform.position, center);
		aimOffset = 90;

		}
	
	// Update is called once per frame
	void Update () {

		goToPolar = cartesianToPolar (this.transform.position) ;


		transform.rotation = Quaternion.Euler(new Vector3(0, 0, transform.rotation.eulerAngles.z+elementRate));

		//Quaternion.Slerp (
			//this.transform.position,
			//polarToCartesian (myRadius,goToPolar[1] + aimOffset),
			//Time.deltaTime);
		this.transform.RotateAround (center, new Vector3(0,0,1), orbitRate*orbitFactor*Time.deltaTime);
		//this.transform.rotation = Quaternion.identity;

	}

	//takes a radius and theta angle (in degrees) for polar coordinates
	//returns coordinates in x,y
	public Vector2 polarToCartesian (float radius, float theta){
		float thetaRadians = Mathf.Deg2Rad*theta;
		float x = radius*Mathf.Cos(thetaRadians);
		float y = radius*Mathf.Sin(thetaRadians);
		return new Vector2 (x, y);
		
	}

	//takes an x,y (standard Cartesian coordinates)
	//returns polar coordinates (in degrees) radius and theta angle
	public float[] cartesianToPolar (Vector2 cartesianPosition){
		float x = cartesianPosition.x;
		float y = cartesianPosition.y;
		float radius = Mathf.Sqrt(Mathf.Pow(x,2) + Mathf.Pow (y,2)); //sqrt(x^2+y2)
		float theta = Mathf.Atan2(y,x)*Mathf.Rad2Deg;//tanInverse(y/x)
		float[] polarCoordinates = {radius,theta};
		return polarCoordinates;
		
	}
}
