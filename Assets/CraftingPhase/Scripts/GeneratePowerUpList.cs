using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GeneratePowerUpList : MonoBehaviour {
	//event call
	public delegate void TogglePowerUpUpgradeScreenAction (bool active);
	public static event TogglePowerUpUpgradeScreenAction OnTogglePowerUpUpgradeScreen;

	//list of the elements and powerups needed to generate the first powerup
	public static string [] FirstPowerUpElements = {"inferno","ice","sand","steam"};
	public static int [] FirstPowerUpCosts = {50, 50, 50, 50};

	//referenced game objects, scripts, & constants
	public int DEFAULT_NUMBER = 0;
	public const int TOTAL_BASE_POWERUPS = 9;
	public const int UPGRADE_LEVELS = 3;
	public const int TIMES_UPGRADED = 2;
	public const int TYPES_OF_ELEMENTS_PER_UNLOCK = 4;
	public GameObject contentList;
	public GameObject scrollView;
	public GameObject powerUpButton;
	private GameObject upgrader;
	private GameObject powerScreen;
	public PowerUp currentPower;
	private BuyUpgrade upgradeScript;

	//set the prefab of PowerUpButton to the correct size
	float buttonSize = 1;
	Vector3 buttonScale;

	//these are the power names for in-game
	string[] powerNames = new string[]{
		"Lane Conversion",
		"Slow Fall",
		"Fuel",
		"Element Multiply",
		"Zone Shield",
		"Tap To Collect",
		"Invincible",
		"Total Conversion",
		"Collect All"};
	private static PowerUp[] PowerUps;

	Sprite[] upgradeBarImages;
	public Sprite upgradeBar0,upgradeBar1,upgradeBar2,upgradeBar3;
	Sprite[] upgradeCardImages;
	public Sprite upgradeCard0,upgradeCard1,upgradeCard2,upgradeCard3;

	//powerUp Upgrade Specific arrays
	string[] powerDescriptions;
	string[] bonusTexts;
	string[,] elem; 
	int[,] cost;

	string elem1,elem2,elem3,elem4;
	int cost1,cost2,cost3,cost4;

	int myPowerLevel;

	private Color notEnoughColor; 

	// Use this for initialization
	void Start () {
		//make PowerUpScreen active since Content is referenced
		powerScreen = GameObject.Find ("Canvas/PowerUpScreen");
		powerScreen.SetActive (true);
		contentList = GameObject.Find ("Canvas/PowerUpScreen/ScrollView/Content");
		powerScreen.SetActive (false);

		buttonScale = new Vector3 (buttonSize, buttonSize, buttonSize);
		upgradeBarImages = new Sprite[]{upgradeBar0,upgradeBar1,upgradeBar2,upgradeBar3};
		upgradeCardImages = new Sprite[]{upgradeCard0,upgradeCard1,upgradeCard2,upgradeCard3};

		PowerUps = new PowerUp[GlobalVars.POWERUP_COUNT];
		
		//the array of all possible PowerUps the spawned PowerUp could be
		PowerUps[0] = new LaneConversion(DEFAULT_NUMBER);
		PowerUps[1] = new SlowFall(DEFAULT_NUMBER,DEFAULT_NUMBER,DEFAULT_NUMBER);
		PowerUps[2] = new Fuel(DEFAULT_NUMBER) ;
		PowerUps[3] = new Multiply(DEFAULT_NUMBER,DEFAULT_NUMBER);
		PowerUps[4] = new BucketShield(DEFAULT_NUMBER);
		PowerUps[5] = new TapToCollect(DEFAULT_NUMBER);
		PowerUps[6] = new Invincible(DEFAULT_NUMBER, DEFAULT_NUMBER);
		PowerUps[7] = new TotalConversion(DEFAULT_NUMBER);
		PowerUps[8] = new CollectAll();

		powerDescriptions = new string[TOTAL_BASE_POWERUPS*UPGRADE_LEVELS]{
			"Tap or drag this ability into a specific lane to convert the elements contained to the lane’s elemental type.",//power 1, level 1
			"Tap or drag this ability into a specific lane to convert the elements contained, and an adjacent lane, to each lane’s elemental type.",//power 1, level 2
			"Tap or drag this ability to convert the elements contained within each lane to the lane’s elemental type.",//power 1, level 3
			"Tap or drag this ability into a specific lane to slow down the fall of elements contained.",//power 2, level 1
			"Tap or drag this ability into a specific lane to significantly slow down the fall of elements contained.",//power 2, level 2
			"Tap or drag this ability to significantly slow down the fall all elements.",//power 2, level 3
			"Your fuel does not deplete for a certain duration.",//power 3, level 1
			"You gain additional fuel and it does not deplete for a certain duration.",//power 3, level 2
			"You gain a significant amount of additional fuel and it does not deplete for a certain duration.",//power 3, level 3
			"Tap or drag this ability into a specific lane to receive a score multiplier for the elements collected of that lane’s elemental type.",//power 4, level 1
			"Tap or drag this ability into a specific lane to receive a score multiplier for the elements collected of that lane’s elemental type, and an adjacent lane’s elemental type.",//power 4, level 2
			"Tap or drag this ability to receive a score multiplier for elements collected of all elemental types.",//power 4, level 3
			"Tap or drag this ability into a specific lane to give that lane no miss penalty for the next 2 incorrect elements that fall into it.",//power 5, level 1
			"Tap or drag this ability into a specific lane to give that lane no miss penalty for the next 4 incorrect elements that fall into it.",//power 5, level 2
			"Tap or drag this ability to give all lanes no miss penalty for the next 4 incorrect elements that fall into them, for each lane.",//power 5, level 3
			"Simply tap elements to automatically collect them.",//power 6, level 1
			"Simply tap elements to automatically collect them.",//power 6, level 2
			"Simply tap elements to automatically collect them.",//power 6, level 3
			"Tap or drag this ability into a specific lane to give that lane no miss penalty for a small amount of time.",//power 7, level 1
			"Tap or drag this ability into a specific lane to give that lane no miss penalty for a decent amount of time.",//power 7, level 2
			"Tap or drag this ability to give all lanes no miss penalty for a decent amount of time.",//power 7, level 3
			"Tap or drag this ability into a specific lane to convert all spawned elements to the lane’s elemental type for a small amount of time.",//power 8, level 1
			"Tap or drag this ability into a specific lane to convert all spawned elements to the lane’s elemental type for a decent amount of time.",//power 8, level 2
			"Tap or drag this ability into a specific lane to convert all spawned elements to the lane’s elemental type and gain invincibility for a decent amount of time.",//power 8, level 3
			"Tap or drag this ability into a specific lane to automatically collect all elements in that lane.",//power 9, level 1
			"Tap or drag this ability into a specific lane to automatically collect all elements in that lane and an adjacent lane.",//power 9, level 2
			"Tap or drag this ability to automatically collect all elements in all lanes."};//power 9, level 3
			
		bonusTexts = new string[TOTAL_BASE_POWERUPS*UPGRADE_LEVELS]{
			"1 Lane",//power 1, level 1
			"2 Lanes",//power 1, level 2
			"All Lanes",//power 1, level 3
			"Slow",//power 2, level 1
			"Slower",//power 2, level 2
			"Slower & All Lanes",//power 2, level 3
			"Fuel",//power 3, level 1
			"Fuel+",//power 3, level 2
			"Fuel++",//power 3, level 3
			"1 Element",//power 4, level 1
			"2 Elements",//power 4, level 2
			"4 Element",//power 4, level 3
			"2 Saves & 1 Lane",//power 5, level 1
			"4 Saves & 1 Lane",//power 5, level 2
			"4 Saves & All Lanes",//power 5, level 3
			"Tap Time",//power 6, level 1
			"Tap Time+",//power 6, level 2
			"Tap Time++",//power 6, level 3
			"Invincible Time & 1 Lane",//power 7, level 1
			"Invincible Time+ & 1 Lane",//power 7, level 2
			"Invincible Time+ & All Lanes",//power 7, level 3
			"Conversion Time",//power 8, level 1
			"Conversion Time+",//power 8, level 2
			"Conversion Time+ & Invincible",//power 8, level 3
			"1 Lane",//power 9, level 1
			"2 Lanes",//power 9, level 2
			"All Lanes"};//power 9, level 3

		elem = new string[TOTAL_BASE_POWERUPS*TIMES_UPGRADED,TYPES_OF_ELEMENTS_PER_UNLOCK]{
			{FirstPowerUpElements[0], FirstPowerUpElements[1], FirstPowerUpElements[2], FirstPowerUpElements[3]},//power 1, upgrade 1
			{"magma","cloud","mud","energy"},//power 1, upgrade 2
			{"desert","earthquake","blizzard","mountain"},//power 2, upgrade 1
			{"storm","glass","clay","brick"},//power 2, upgrade 2
			{"coal","volcano","tornado","glacier"},//power 3, upgrade 1
			{"ion","fission","ceramics","monsoon"},//power 3, upgrade 2
			{"diamond","aurora","smoke","flood"},//power 4, upgrade 1
			{"metal","grass","radiation","tsunami"},//power 4, upgrade 2
			{"meteoroid","supernova","archipelago","rust"},//power 5, upgrade 1
			{"wasteland","voltage","tide","edifice"},//power 5, upgrade 2
			{"algae","pulsar","dynamo","windmill"},//power 6, upgrade 1
			{"aliens","fusion","whirlpool","resistance"},//power 6, upgrade 2
			{"forest","crater","maze","apocalypse"},//power 7, upgrade 1
			{"dreams","balance","coincidence","fate"},//power 7, upgrade 2
			{"valley","fruit","sorcery","prophecy"},//power 8, upgrade 1
			{"inertia","morality","method","insight"},//power 8, upgrade 2
			{"emotion","chaos","deception","reason"},//power 9, upgrade 1
			{"information","chronology","absurd","motivation"}};//power 9, upgrade 2
		cost = new int[TOTAL_BASE_POWERUPS*TIMES_UPGRADED,TYPES_OF_ELEMENTS_PER_UNLOCK]{
			{50, 50, 50, 50},//power 1, upgrade 1
			{99,99,99,99},//power 1, upgrade 2
			{50,50,50,50},//power 2, upgrade 1
			{99,99,99,99},//power 2, upgrade 2
			{50,50,50,50},//power 3, upgrade 1
			{99,99,99,99},//power 3, upgrade 2
			{50,50,50,50},//power 4, upgrade 1
			{99,99,99,99},//power 4, upgrade 2
			{50,50,50,50},//power 5, upgrade 1
			{99,99,99,99},//power 5, upgrade 2
			{50,50,50,50},//power 6, upgrade 1
			{99,99,99,99},//power 6, upgrade 2
			{50,50,50,50},//power 7, upgrade 1
			{99,99,99,99},//power 7, upgrade 2
			{50,50,50,50},//power 8, upgrade 1
			{99,99,99,99},//power 8, upgrade 2
			{50,50,50,50},//power 9, upgrade 1
			{99,99,99,99}};//power 9, upgrade 2


		notEnoughColor = new Color(255.0f/255.0f,135.0f/255.0f,126.0f/255.0f);
	}


	
	// Update is called once per frame
	void Update () {
	}

	public void createPowerList(){
		if (OnTogglePowerUpUpgradeScreen != null) {
			OnTogglePowerUpUpgradeScreen(true);
		}

		PowerUp.CheckIndexDictionary();
		for (int power = 0; power < TOTAL_BASE_POWERUPS; power++) {

			GameObject newPower = Instantiate(powerUpButton)as GameObject;
			newPower.transform.SetParent(contentList.transform);
			newPower.transform.localScale = buttonScale;
			currentPower = PowerUps[power];
			Debug.Log(currentPower.name);

			newPower.SetActive(true);

			//for tutorial
			if (currentPower.name == "LaneConversion") {
				newPower.AddComponent<CraftingTutorialComponent>().Reinitialize(MainMenuController.Tutorial.UpgradePowerup);
			}

			newPower.transform.FindChild ("Name/Description").GetComponent<Text>().text = "To unlock this ability discover all level "+(power+1).ToString() +" elements.";	

			if (PowerUp.PowerUpUnlocked(currentPower)){
				myPowerLevel = PowerUp.GetPowerUpLevel(currentPower);
				newPower.transform.FindChild ("Name").GetComponent<Text>().text = powerNames[power];
				newPower.transform.FindChild ("Name/Description").GetComponent<Text>().text = powerDescriptions[(power*UPGRADE_LEVELS)+(myPowerLevel-1)];
				newPower.transform.FindChild ("Bonus/BonusText").GetComponent<Text>().text = bonusTexts[(power*UPGRADE_LEVELS)+(myPowerLevel-1)];
				Debug.Log("___"+bonusTexts[(power*UPGRADE_LEVELS)+myPowerLevel]);

				newPower.transform.FindChild ("UpgradeBars").GetComponent<Image>().sprite = upgradeBarImages[myPowerLevel];
				newPower.transform.FindChild ("PowerUpIcon").GetComponent<Image>().sprite = upgradeCardImages[myPowerLevel];
				newPower.transform.FindChild ("PowerUpIcon/SpecificIcon").GetComponent<Image>().sprite = GlobalVars.POWERUP_SPRITES[power];

				if (myPowerLevel==PowerUp.MAX_LEVEL){
					newPower.transform.FindChild ("UpgradeBars/Upgrade/Text").GetComponent<Text>().text = "MAX";
				}else{
					int indexOffset = myPowerLevel==2?1:0; 
					int index = power*TIMES_UPGRADED+indexOffset;
					newPower.transform.FindChild ("UpgradeBars/Upgrade/Text").GetComponent<Text>().text = "UPGRADE";

					elem1 = elem[index,0];
					elem2 = elem[index,1];
					elem3 = elem[index,2];
					elem4 = elem[index,3];
					cost1 = cost[index,0];
					cost2 = cost[index,1];
					cost3 =	cost[index,2];
					cost4 = cost[index,3];

					newPower.transform.FindChild ("UpgradeCost/cost1").GetComponent<Text>().text = cost1.ToString();
					newPower.transform.FindChild ("UpgradeCost/elem1").GetComponent<Image>().sprite = GlobalVars.ELEMENT_SPRITES[elem1];
					newPower.transform.FindChild ("UpgradeCost/elem1/Name").GetComponent<Text>().text = elem1;
					newPower.transform.FindChild ("UpgradeCost/myAmount1").GetComponent<Text>().text = PlayerPrefs.GetInt(elem1).ToString();
					if (PlayerPrefs.GetInt(elem1) < cost1){
						newPower.transform.FindChild ("UpgradeCost/myAmount1").GetComponent<Text>().color = notEnoughColor;
					}
					Debug.Log("hih"+notEnoughColor);

					newPower.transform.FindChild ("UpgradeCost/cost2").GetComponent<Text>().text = cost2.ToString();
					newPower.transform.FindChild ("UpgradeCost/elem2").GetComponent<Image>().sprite = GlobalVars.ELEMENT_SPRITES[elem2];
					newPower.transform.FindChild ("UpgradeCost/elem2/Name").GetComponent<Text>().text = elem2;
					newPower.transform.FindChild ("UpgradeCost/myAmount2").GetComponent<Text>().text = PlayerPrefs.GetInt(elem2).ToString();
					if (PlayerPrefs.GetInt(elem2) < cost2){
						newPower.transform.FindChild ("UpgradeCost/myAmount2").GetComponent<Text>().color = notEnoughColor;
					}

					newPower.transform.FindChild ("UpgradeCost/cost3").GetComponent<Text>().text = cost3.ToString();
					newPower.transform.FindChild ("UpgradeCost/elem3").GetComponent<Image>().sprite = GlobalVars.ELEMENT_SPRITES[elem3];
					newPower.transform.FindChild ("UpgradeCost/elem3/Name").GetComponent<Text>().text = elem3;
					newPower.transform.FindChild ("UpgradeCost/myAmount3").GetComponent<Text>().text = PlayerPrefs.GetInt(elem3).ToString();
					if (PlayerPrefs.GetInt(elem3) < cost3){
						newPower.transform.FindChild ("UpgradeCost/myAmount3").GetComponent<Text>().color = notEnoughColor;
					}

					newPower.transform.FindChild ("UpgradeCost/cost4").GetComponent<Text>().text = cost4.ToString();
					newPower.transform.FindChild ("UpgradeCost/elem4").GetComponent<Image>().sprite = GlobalVars.ELEMENT_SPRITES[elem4];
					newPower.transform.FindChild ("UpgradeCost/elem4/Name").GetComponent<Text>().text = elem4;
					newPower.transform.FindChild ("UpgradeCost/myAmount4").GetComponent<Text>().text = PlayerPrefs.GetInt(elem4).ToString();
					if (PlayerPrefs.GetInt(elem4) < cost4){
						newPower.transform.FindChild ("UpgradeCost/myAmount4").GetComponent<Text>().color = notEnoughColor;
					}

					upgrader = newPower.transform.FindChild ("UpgradeBars/Upgrade").gameObject;
					upgrader.AddComponent<BuyUpgrade>();
					upgradeScript = upgrader.GetComponent<BuyUpgrade>();
					upgradeScript.setPower(currentPower);
					upgradeScript.setCost(cost1,1);
					upgradeScript.setCost(cost2,2);
					upgradeScript.setCost(cost3,3);
					upgradeScript.setCost(cost4,4);
					upgradeScript.setElem(elem1,1);
					upgradeScript.setElem(elem2,2);
					upgradeScript.setElem(elem3,3);
					upgradeScript.setElem(elem4,4);


					upgrader.GetComponent<Button>().onClick.AddListener(upgradeScript.purchaseUpgrade);

				}

			}
		}
/*
		//recalculates the powerup tutorial to account for the newly created components
		CraftingTutorialComponent TutorialComponent = powerScreen.GetComponent<CraftingTutorialComponent>();
		TutorialComponent.Reinitialize(MainMenuController.Tutorial.UpgradePowerup);
		TutorialComponent.ActivateComponent();

*/
	}

	public void fullClearPowerList(){
		foreach (Transform child in contentList.transform) {
			Destroy(child.gameObject);
		}
		resetScrollPosition();
	}

	public void clearPowerList(){
		foreach (Transform child in contentList.transform) {
			Destroy(child.gameObject);
		}
	}

	public void resetScrollPosition(){
		scrollView.GetComponent<ScrollRect> ().verticalNormalizedPosition = 1;
	}

	public void refreshList(){
		clearPowerList ();
		createPowerList ();
	}

	public void ExitPowerUpUpgradeScreen () {
		if (OnTogglePowerUpUpgradeScreen != null) {
			OnTogglePowerUpUpgradeScreen(false);
		}
	}

}
