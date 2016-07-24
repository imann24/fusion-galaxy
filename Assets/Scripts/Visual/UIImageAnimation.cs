/*
 * Author: Isaiah Mann
 * Description: Family of classes to handle animating UI Images in the Unity Canvas system
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(MaskableGraphic))]
public class UIImageAnimation : MonoBehaviour {

	static Dictionary<string, UIImageAnimation> animationsByID = new Dictionary<string, UIImageAnimation>();

	public TutorialType PlayOnTutorial = TutorialType.Any;

	public bool PlayOnAwake;
	public bool PlayOnStart;
	public float AnimationTime = 1f;
	protected bool playing;
	protected bool shouldPlayOnEnable;
	bool _hidden = false;
	public bool Hidden {
		get {
			return _hidden;
		}
	}

	[Header("Animation ID is optional. Add if you want static variable control over animation. All IDs must be unique")]
	public string AnimationID;

	RectTransform _transform2D = null;
	protected RectTransform transform2D {
		get {
			if (_transform2D == null) {
				_transform2D = GetComponent<RectTransform>();
			}
			return _transform2D;
		}
		set {
			_transform2D = value;
		}
	}

	Vector3 initialPosition;
	Vector3 initialScale;

	MaskableGraphic _image = null;
	protected MaskableGraphic image {
		get {
			if (_image == null) {
				_image = GetComponent<Image>();
			}
			return _image;
		}
		set {
			_image = value;
		}
	}

	public virtual void Play () {
		playing = true;
	}

	public virtual void Stop () {
		playing = false;
	}

	public void Hide () {
		_hidden = true;
		image.enabled = false;
		Stop();
	}

	public void Show (bool shouldPlay = true) {
		_hidden = false;
		image.enabled = true;
		if (shouldPlay) {
			Play();
		}
	}

	protected virtual void Awake () {
		setInitialPosition();
		checkForID();
		if (PlayOnAwake && !playing) {
			Play ();
		}
	}

	protected virtual void Start () {
		if (PlayOnStart && !playing) {
			Play ();
		}
	}

	protected virtual void OnEnable () {
		if (shouldPlayOnEnable) {
			Play();
		}
	}

	void setInitialPosition () {
		initialPosition = transform.localPosition;
	}

	void setInitialScale () {
		initialScale = transform.localScale;
	}

	void checkForID () {
		if (!string.IsNullOrEmpty(AnimationID)) {
			addSelfToAnimationsByID(AnimationID);
		}
	}
	void addSelfToAnimationsByID (string id) {
		if (!animationsByID.ContainsKey(id)) {
			animationsByID.Add(id, this);
		}
	}

	protected void resetPosition () {
		transform.localPosition = initialPosition;
	}

	protected void resetScale () {
		transform.localScale = initialScale;
	}

	public static bool TryGetByAnimation (string id, out UIImageAnimation animation) {
		if (animationsByID.TryGetValue(id, out animation)) {
			return true;
		} else {
			return false;
		}
	}
}
