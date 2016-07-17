/*
 * Author(s): Isaiah Mann
 * Description: Gives the player a set amount of the elements discovered
 * Notes: Optional bool also includes an increase to base elements
 */

using UnityEngine;

public class ElementDiscoveryReward {
	string[] discoveredElements;
	int rewardAmountPerElement;
	bool alsoGiveBaseElements;

	public ElementDiscoveryReward (string[] discoveredElements, 
	                               int rewardAmountPerElement, 
	                               bool alsoGiveBaseElements = false) {
		this.discoveredElements = discoveredElements;
		this.rewardAmountPerElement = rewardAmountPerElement;
		this.alsoGiveBaseElements = alsoGiveBaseElements;
	}

	public void Give () {
		for (int i = 0; i < discoveredElements.Length; i++) {
			Utility.IncreasePlayerPrefValue(
				discoveredElements[i],
				rewardAmountPerElement
			);
		}
		if (alsoGiveBaseElements) {
			foreach (string elementName in GlobalVars.BASE_ELEMENT_NAMES) {
				Utility.IncreasePlayerPrefValue(
					elementName,
					rewardAmountPerElement
				);
			}
		}
		// Makes sure numbers update of inventory counts
		GlobalVars.CRAFTING_CONTROLLER.UpdatePanels();
	}
}
