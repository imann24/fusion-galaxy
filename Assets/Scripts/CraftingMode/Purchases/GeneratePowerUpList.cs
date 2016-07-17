using UnityEngine;
using System.Linq;
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

		PowerUps = PowerUp.GetAll();

		powerDescriptions = ArrayUtil.Concat (
			LaneConversion.DESCRIPTIONS,
			SlowFall.DESCRIPTIONS,
			Fuel.DESCRIPTIONS,
			Multiply.DESCRIPTIONS,
			BucketShield.DESCRIPTIONS,
			TapToCollect.DESCRIPTIONS,
			Invincible.DESCRIPTIONS,
			TotalConversion.DESCRIPTIONS,
			CollectAll.DESCRIPTIONS
		);
			
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

		string upgradeButtonText = BuyUpgrade.UpgradeButtonText;
		for (int power = 0; power < TOTAL_BASE_POWERUPS; power++) {

			GameObject newPower = Instantiate(powerUpButton)as GameObject;
			newPower.transform.SetParent(contentList.transform);
			newPower.transform.localScale = buttonScale;
			newPower.transform.position += Vector3.forward * 100;
			currentPower = PowerUps[power];
			newPower.SetActive(true);

			//for tutorial
			if (currentPower.name == "LaneConversion") {
				newPower.AddComponent<CraftingTutorialComponent>().Reinitialize(TutorialType.UpgradePowerup);
			}

			newPower.transform.FindChild ("Name/Description").GetComponent<Text>().text = "To unlock this ability discover all level "+(power+1).ToString() +" elements.";	

			if (PowerUp.PowerUpUnlocked(currentPower)){
				myPowerLevel = PowerUp.GetPowerUpLevel(currentPower);
				newPower.transform.FindChild ("Name").GetComponent<Text>().text = powerNames[power];
				newPower.transform.FindChild ("Name/Description").GetComponent<Text>().text = powerDescriptions[(power*UPGRADE_LEVELS)+(myPowerLevel)];
				newPower.transform.FindChild ("Bonus/BonusText").GetComponent<Text>().text = bonusTexts[(power*UPGRADE_LEVELS)+(myPowerLevel-1)];
				newPower.transform.FindChild (BuyUpgrade.UpgradeBars).GetComponent<Image>().sprite = upgradeBarImages[myPowerLevel];
				newPower.transform.FindChild ("PowerUpIcon").GetComponent<Image>().sprite = upgradeCardImages[myPowerLevel];
				newPower.transform.FindChild ("PowerUpIcon/SpecificIcon").GetComponent<Image>().sprite = currentPower.GetSprite();
				// newPower.transform.FindChild ("PowerUpIcon/SpecificIcon").GetComponent<Image>().sprite = GlobalVars.POWERUP_SPRITES[power];

				if (myPowerLevel==PowerUp.MAX_LEVEL){
					newPower.transform.FindChild (upgradeButtonText).GetComponent<Text>().text = "MAX";
				}else{
					int indexOffset = myPowerLevel==2?1:0; 
					int index = power*TIMES_UPGRADED+indexOffset;
					newPower.transform.FindChild (upgradeButtonText).GetComponent<Text>().text = "UPGRADE";

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

					upgrader = newPower.transform.FindChild ("UpgradeButton").gameObject;
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
