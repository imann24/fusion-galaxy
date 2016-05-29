#define DEBUG
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//from the unity asset store: https://www.assetstore.unity3d.com/en/#!/content/25468
[RequireComponent(typeof(Image))]
public class DragMe : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerDownHandler
{
	public bool dragOnSurfaces = true;


	private GameObject m_DraggingIcon;
	private RectTransform m_DraggingPlane;
	private bool isDropZone;
	private bool isCompiler;
	private static Vector3 zoneScale =  new Vector3(0.3f, 0.3f);
	private CaptureScript zoneController;
	private string elementName; 
	void Start () {
		if (transform.parent.name.Contains ("Result")) {
			isCompiler = true;
		}

		if (transform.parent.name.Contains("Zone") || isCompiler) { //prevents elements in the drop zone from being scaled

			isDropZone = isCompiler ? false:true;
			
			//script reference to capture zone controller
			zoneController = isCompiler ? GetComponent<CaptureScript>():transform.parent.GetComponent<CaptureScript>();

			//changes the sort order to make sure the zone can receive button clicks
			GetComponent<Canvas>().sortingOrder = transform.parent.GetComponent<Canvas>().sortingOrder+1;
		} else {
			//makes the element slightly smaller
			transform.localScale = new Vector3 (0.9f, 0.9f);
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		var canvas = FindInParents<Canvas>(gameObject);
		if (canvas == null)
			return;

		// We have clicked something that can be dragged.
		// What we want to do is create an icon for this.
		m_DraggingIcon = new GameObject("icon");

		m_DraggingIcon.transform.SetParent (canvas.transform, false);
		m_DraggingIcon.transform.SetAsLastSibling();
		var image = m_DraggingIcon.AddComponent<Image>();
		// The icon will be under the cursor.

		// We want it to be ignored by the event system.
		CanvasGroup group = m_DraggingIcon.AddComponent<CanvasGroup>();
		group.blocksRaycasts = false;

		//makes element render in front of everything else
		Canvas iconCanvas = m_DraggingIcon.AddComponent<Canvas>();
		m_DraggingIcon.GetComponent<Canvas> ().overrideSorting = true;
		m_DraggingIcon.GetComponent<Canvas> ().sortingOrder = 1000;

		//adds a collider to the elemnt so it can tell when it hits a drop zone
		m_DraggingIcon.AddComponent<Rigidbody2D>().isKinematic = true;;
		m_DraggingIcon.AddComponent<CircleCollider2D>().isTrigger = true;
		m_DraggingIcon.tag = GlobalVars.ELEMENT_TAG;

		//add a script that enables the element to override another one if dropped on top
		m_DraggingIcon.AddComponent<CaptureMe>();

		image.sprite = GetComponent<Image>().sprite;
		image.SetNativeSize();

		//scales the elements dragged from the drop zone 
		if (isDropZone || isCompiler) {
			m_DraggingIcon.transform.localScale = zoneScale;
		}

		if (dragOnSurfaces)
			m_DraggingPlane = transform as RectTransform;
		else
			m_DraggingPlane = canvas.transform as RectTransform;
		
		SetDraggedPosition(eventData);
	}

	public void OnDrag(PointerEventData data)
	{
		if (m_DraggingIcon != null)
			SetDraggedPosition (data);
	}

	private void SetDraggedPosition(PointerEventData data)
	{
		if (dragOnSurfaces && data.pointerEnter != null && data.pointerEnter.transform as RectTransform != null)
			m_DraggingPlane = data.pointerEnter.transform as RectTransform;
		
		var rt = m_DraggingIcon.GetComponent<RectTransform>();
		Vector3 globalMousePos;
		if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_DraggingPlane, data.position, data.pressEventCamera, out globalMousePos))
		{
			rt.position = globalMousePos;
			rt.rotation = m_DraggingPlane.rotation;
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (m_DraggingIcon != null) {
			Destroy(m_DraggingIcon);
			//clears the element from the zone
			if (isDropZone || isCompiler) {
				zoneController.OnMouseDown();
			}

			if (isCompiler) {
				GlobalVars.CRAFTER.clearDropzZones();
			}
		}
	}

	public void OnPointerClick (PointerEventData eventData) {
		//gets the name of the element the script is on
		elementName = GetComponent<Image>().sprite.name;

		if (isDropZone) {
			//clears the element
			zoneController.OnMouseDown();
		} else if (isCompiler) {
			//crafts the element
			GlobalVars.CRAFTER.OnMouseDown();
		} 
	}

	//empty method, if this interface is not present then clicks are not recognized at all
	public void OnPointerDown (PointerEventData eventData) {

	}

	static public T FindInParents<T>(GameObject go) where T : Component
	{
		if (go == null) return null;
		var comp = go.GetComponent<T>();

		if (comp != null)
			return comp;
		
		Transform t = go.transform.parent;
		while (t != null && comp == null)
		{
			comp = t.gameObject.GetComponent<T>();
			t = t.parent;
		}
		return comp;
	}
}
