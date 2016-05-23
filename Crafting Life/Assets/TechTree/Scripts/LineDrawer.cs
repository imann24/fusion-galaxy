using UnityEngine;
using System.Collections;
//used to draw lines between combinations
public class LineDrawer : MonoBehaviour {
	GameObject g1 = null;
	GameObject g2 = null;
	LineRenderer l;
	Color currentColor1;
	Color currentColor2;
	Color nextColor;
	float alphaOffset;
	float mostAlpha;
    float changeRate;

	// Use this for initialization
	void Start () {
		l = transform.GetComponent<LineRenderer>();
		l.SetWidth(0.29f, 0.05f);
		l.SetVertexCount(3);

		l.SetColors (Color.magenta, Color.white);

		//alpha will be alphaOffset + (between 0 and mostAlpha)
		alphaOffset = .5f;
		mostAlpha = .3f;
		changeRate = 5;

		currentColor1 = new Color (Random.value, Random.value, Random.value, Random.value);
		currentColor2 = new Color (Random.value, Random.value, Random.value, Random.value);
		nextColor = new Color (Random.value, Random.value, Random.value, alphaOffset + (Random.value % mostAlpha));
		//l.GetComponent<LineRenderer> ().material = Resources.Load ("ui/appicon")as Material;

	}
	
	// Update is called once per frame
	void Update () {
		if (g1 != null) { //sets the positions of the lines
			l.SetPosition(0, g1.transform.position);
			l.SetPosition(1, transform.position);
			l.SetPosition(2, g2.transform.position);

			//change line colors over time
			if (currentColor1!=nextColor){
				currentColor1 = Color.Lerp(currentColor1,nextColor,Time.deltaTime*changeRate);
				currentColor2 = Color.Lerp(currentColor2,currentColor1,Time.deltaTime*changeRate);
			}else{
				nextColor = new Color (Random.value, Random.value, Random.value, alphaOffset + (Random.value % mostAlpha));
			}
			l.SetColors (currentColor1, currentColor2);
		}
	}

	//assigns the gameobjects that are linked to the parent gameobject (should be set at creation)
	public void assignGameObjects (GameObject g1, GameObject g2) {
		this.g1 = g1;
		this.g2 = g2;
	}
}
