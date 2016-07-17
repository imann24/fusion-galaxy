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
	GameObject currentMessage;

	void Awake () {
		SingletonUtil.TryInit(ref Instance, this, gameObject);
	}

	public void ShowElementRewardMessage (string [] elementNames, 
	                                      int rewardAmount, 
	                                      bool includeBaseElements) {
		GameObject rewardMessage = (GameObject) Instantiate(ElementRewardMessagePrefab);
		currentMessage = rewardMessage;
		rewardMessage.transform.SetParent(transform);
		rewardMessage.transform.localPosition = Vector2.zero;
		rewardMessage.transform.localScale = Vector2.one;
		ElementRewardMessageBehaviour behaviour = rewardMessage.GetComponent<ElementRewardMessageBehaviour>();
		if (behaviour != null) {
			behaviour.Init(elementNames, rewardAmount, includeBaseElements);
		}
	}

	public void CloseCurrentMessage () {
		if (currentMessage != null) {
			Destroy(currentMessage);
		}
	}
}
