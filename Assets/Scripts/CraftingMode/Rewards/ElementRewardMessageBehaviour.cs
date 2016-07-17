/*
 * Author(s): Isaiah Mann
 * Description: Controls an element reward message
 */


using UnityEngine;
using UnityEngine.UI;

public class ElementRewardMessageBehaviour : MonoBehaviour {
	[Header("References")]
	public Text RewardAmount;
	public Text RewardTitle;
	public GameObject BaseElementsPanel;
	public GameObject ElementIconPrefab;
	public Transform DiscoveredElementParent;

	[Header("Tunable")]
	public string DisplayTitleText = "Element Discovery Bonus";

	Image[] discovereDelements;
	string rewardAmountMessage = "+{0})";
	public void Init (string[] elementNames, int rewardAmount, bool includeBaseElements) {
		SetDiscoveredElements(elementNames);
		SetRewardAmount(rewardAmount);
		ToggleBaseElements(includeBaseElements);
		SetTitle(DisplayTitleText);
	}

	public void Close () {
		MessageController.Instance.CloseCurrentMessage();
	}

	void SetDiscoveredElements (string[] elementNames) {
		discovereDelements = new Image[elementNames.Length];
		for (int i = 0; i < discovereDelements.Length; i++) {
			GameObject elementIcon = (GameObject) Instantiate(ElementIconPrefab);
			elementIcon.transform.SetParent(DiscoveredElementParent);
			Image icon = elementIcon.GetComponent<Image>();
			if (icon != null) {
				icon.sprite = GlobalVars.ELEMENT_SPRITES[elementNames[i]];
				icon.transform.localScale = Vector2.one;
			}
		}
	}
	
	void SetRewardAmount (int rewardAmount) {
		RewardAmount.text = string.Format(rewardAmountMessage, rewardAmount);
	}

	void ToggleBaseElements (bool areVisible) {
		BaseElementsPanel.SetActive(areVisible);
	}

	void SetTitle (string message) {
		RewardTitle.text = message;
	}
}
