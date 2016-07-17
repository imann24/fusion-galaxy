/*
 * Author(s): Isaiah Mann
 * Description: Displays UI Alerts and Messages to the player
 * Tech Debt Warning: This script was added late in the development cycle, many messages are not handled through it
 */

using UnityEngine;
using System.Collections;

public class MessageController : MonoBehaviour {
	public static MessageController Instance;

	public GameObject ElementRewardMessagePrefab;
	public GameObject PowerupUnlockedMessagePrefab;
	GameObject currentMessage;
	bool initializedAsSingleton;

	void Awake () {
		if (SingletonUtil.TryInit(ref Instance, this, gameObject)) {
			Subscribe();
			initializedAsSingleton = true;
		}
	}

	void OnDestroy () {
		if (initializedAsSingleton) {
			Unsubscribe();
		}
	}

	GameObject CreateMessage (MessageType messageType) {
		GameObject message;
		switch(messageType) {
		case MessageType.ElementDiscoveryReward:
			message = (GameObject) Instantiate(ElementRewardMessagePrefab);
			break;
		case MessageType.PowerUpUnlocked:
			message = (GameObject) Instantiate(PowerupUnlockedMessagePrefab);
			break;
		default:
			return null;
		}
		currentMessage = message;
		message.transform.SetParent(transform);
		message.transform.localPosition = Vector2.zero;
		message.transform.localScale = Vector2.one;
		return message;
	}

	public void ShowElementRewardMessage (string [] elementNames, 
	                                      int rewardAmount, 
	                                      bool includeBaseElements) {
		GameObject rewardMessage = CreateMessage(MessageType.ElementDiscoveryReward);
		ElementRewardMessageBehaviour behaviour = rewardMessage.GetComponent<ElementRewardMessageBehaviour>();
		if (behaviour != null) {
			behaviour.Init(elementNames, rewardAmount, includeBaseElements);
		}
	}

	public void ShowPowerupsUnlockedMessage (PowerUp powerUp) {
		GameObject rewardMessage = CreateMessage(MessageType.PowerUpUnlocked);
		PowerupUnlockedMessageBehaviour behaviour = rewardMessage.GetComponent<PowerupUnlockedMessageBehaviour>();
		if (behaviour != null) {
			behaviour.Init(powerUp);
		}
	}

	public void CloseCurrentMessage () {
		if (currentMessage != null) {
			Destroy(currentMessage);
		}
	}

	void Subscribe () {
		PowerUp.OnUnlockPowerUp += ShowPowerupsUnlockedMessage;
	}

	void Unsubscribe () {
		PowerUp.OnUnlockPowerUp -= ShowPowerupsUnlockedMessage;
	}
}
