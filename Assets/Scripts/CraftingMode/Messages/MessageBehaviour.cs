using UnityEngine;
using System.Collections;

public abstract class MessageBehaviour : MonoBehaviour {
	public void Close () {
		MessageController.Instance.CloseMessage(this);
	}
}
