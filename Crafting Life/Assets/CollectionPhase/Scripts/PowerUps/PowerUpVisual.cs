using UnityEngine;
using System.Collections;

public class PowerUpVisual : MonoBehaviour {
		


	//colors the gameObjcet will lerp between
	public Color visualColor1 = new Color(255.0f/255.0f,117.0f/255.0f,255.0f/255.0f);
	public Color visualColor2 = new Color(255.0f/255.0f,255.0f/255.0f,255.0f/255.0f);
	public Color visualColor3 = new Color(75.0f/255.0f,75.0f/255.0f,255.0f/255.0f);
	public Color visualColor4 = new Color(116.0f/255.0f,255.0f/255.0f,255.0f/255.0f);
	public Color visualColor5 = new Color(103.0f/255.0f,255.0f/255.0f,103.0f/255.0f);
	public Color visualColor6 = new Color(124.0f/255.0f,21.0f/255.0f,178.0f/255.0f);
	public Color visualColor7 = new Color(242.0f/255.0f,53.0f/255.0f,53.0f/255.0f);
	public Color visualColor8 = new Color(238.0f/255.0f,152.0f/255.0f,74.0f/255.0f);
	public Color visualColor9 = new Color(246.0f/255.0f,236.0f/255.0f,121.0f/255.0f);
	public Color visualColor10 = new Color(0.0f/255.0f,255.0f/255.0f,149.0f/255.0f);
		
	//public Color visualColor1 = new Color(255.0f/255.0f,0.0f/255.0f,248.0f/255.0f);
	//public Color visualColor2 = new Color(255.0f/255.0f,255.0f/255.0f,255.0f/255.0f);
	//public Color visualColor3 = new Color(0.0f/255.0f,0.0f/255.0f,248.0f/255.0f);
	//public Color visualColor4 = new Color(0.0f/255.0f,255.0f/255.0f,255.0f/255.0f);
	//public Color visualColor5 = new Color(0.0f/255.0f,255.0f/255.0f,0.0f/255.0f);
	//public Color visualColor6 = new Color(139.0f/255.0f,25.0f/255.0f,199.0f/255.0f);
	//public Color visualColor7 = new Color(255.0f/255.0f,0.0f/255.0f,0.0f/255.0f);
	//public Color visualColor8 = new Color(255.0f/255.0f,121.0f/255.0f,0.0f/255.0f);
	//public Color visualColor9 = new Color(255.0f/255.0f,235.0f/255.0f,4.0f/255.0f);
	//public Color visualColor10 = new Color(0.0f/255.0f,25.0f/255.0f,149.0f/255.0f);
	
	
	Color[] validColorArray = new Color[10];
		
		Color currentColor;
		Color nextColor;
		
		float changeRate;
		
		SpriteRenderer backgroundSprite;
		
		// Use this for initialization
		void Start () {



		//set color array
		validColorArray [0] = visualColor1;
		validColorArray [1] = visualColor2;
		validColorArray [2] = visualColor3;
		validColorArray [3] = visualColor4;
		validColorArray [4] = visualColor5;
		validColorArray [5] = visualColor6;
		validColorArray [6] = visualColor7;
		validColorArray [7] = visualColor8;
		validColorArray [8] = visualColor9;
		validColorArray [9] = visualColor10;



			currentColor = validColorArray [Random.Range (0, validColorArray.Length-1)];
			
			changeRate = 80;
			
			nextColor = currentColor;
			
			backgroundSprite = this.GetComponentInChildren<SpriteRenderer> ();
			
		}
		
		// Update is called once per frame
		void Update () {
			
			//change line colors over time
			if (currentColor!=nextColor){
				currentColor = Color.Lerp(currentColor,nextColor,Time.deltaTime*changeRate);
			}else{
			nextColor = validColorArray [Random.Range (0, validColorArray.Length-1)];
			}
			if (backgroundSprite != null) {
				backgroundSprite.color = currentColor;
			}
			
		}

	void stopColors(){
		backgroundSprite.color = Color.white;
		Destroy (this);
		}
	}
