/*
 * Author: Isaiah Mann
 * Description: Calls events on canvas groups
 * Depedencies: UnityEventType enum
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasGroup))]
public class SetCanvasGroupOnUnityEvent : MonoBehaviour {
	public CanvasGroupEvent[] Events;
		

	Dictionary <UnityEventType, List<CanvasGroupEvent>> _eventDict;
	Dictionary <UnityEventType, List<CanvasGroupEvent>> internalEventDic {
		get {
			if (_eventDict == null) {
				initEventDict();
			}
			return _eventDict;
		}
		set {
			_eventDict = value;
		}
	}

	CanvasGroup _canvasGroup;
	CanvasGroup canvasGroup {
		get {
			if (_canvasGroup == null) {
				_canvasGroup = GetComponent<CanvasGroup>();
			}
			return _canvasGroup;
		}
		set {
			_canvasGroup = value;
		}
	}

	void Awake () {
		callEventType(UnityEventType.Awake);
	}

	void Start () {
		callEventType(UnityEventType.Start);
	}

	void OnDestroy () {
		callEventType(UnityEventType.OnDestroy);
	}

	void OnEnable () {
		callEventType(UnityEventType.OnEnable);
	}

	void OnDisable () {
		callEventType(UnityEventType.OnDisable);
	}
	
	void OnLevelWasLoaded (int level) {
		callEventType(UnityEventType.OnLevelWasLoaded);
	}

	void OnMouseDown () {
		callEventType(UnityEventType.OnMouseDown);
	}

	void OnMouseUp () {
		callEventType(UnityEventType.OnMouseUp);
	}
	
	// Must be called before anything else
	void initEventDict () {
		_eventDict = new Dictionary<UnityEventType, List<CanvasGroupEvent>>();
		foreach (CanvasGroupEvent cGEvent in this.Events) {
			List<CanvasGroupEvent> eventList;
			if (!_eventDict.TryGetValue(cGEvent.Type, out eventList)) { 
				eventList = new List<CanvasGroupEvent>();
				internalEventDic[cGEvent.Type] = eventList;
			}
			eventList.Add(cGEvent);
		}
	}

	void callEventType (UnityEventType type) {
		List<CanvasGroupEvent> events;
		if (internalEventDic.TryGetValue(type, out events)) {
			callEventList(events);
		}
	}

	void callEventList (List<CanvasGroupEvent> eventList) {
		if (eventList != null) {
			foreach (CanvasGroupEvent cgEvent in eventList) {
				cgEvent.Call(canvasGroup);
			}
		}
	}
}

[System.Serializable]
public class CanvasGroupEvent {
	public UnityEventType Type;
	public float Alpha;
	public bool IsInteractable;
	public bool BlocksRaycasts;
	public bool IngnoreParentGroups;

	public void Call (CanvasGroup canvasGroup) {
		canvasGroup.alpha = Alpha;
		canvasGroup.interactable = IsInteractable;
		canvasGroup.blocksRaycasts = BlocksRaycasts;
		canvasGroup.ignoreParentGroups = IngnoreParentGroups;
	}
}