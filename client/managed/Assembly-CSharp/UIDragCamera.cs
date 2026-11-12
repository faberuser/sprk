using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Drag Camera")]
public class UIDragCamera : MonoBehaviour, UICamera.IOnPressListener, UICamera.IOnDragListener
{
	public UIDraggableCamera draggableCamera;

	private void Awake()
	{
		if (draggableCamera == null)
		{
			draggableCamera = NGUITools.FindInParents<UIDraggableCamera>(base.gameObject);
		}
	}

	public void OnPress(bool isPressed)
	{
		if (base.enabled && NGUITools.GetActive(base.gameObject) && draggableCamera != null)
		{
			draggableCamera.Press(isPressed);
		}
	}

	public void OnDrag(Vector2 delta)
	{
		if (base.enabled && NGUITools.GetActive(base.gameObject) && draggableCamera != null)
		{
			draggableCamera.Drag(delta);
		}
	}

	private void OnScroll(float delta)
	{
		if (base.enabled && NGUITools.GetActive(base.gameObject) && draggableCamera != null)
		{
			draggableCamera.Scroll(delta);
		}
	}
}
