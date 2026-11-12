using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Forward Events (Legacy)")]
public class UIForwardEvents : MonoBehaviour, UICamera.IOnPressListener, UICamera.IOnClickListener, UICamera.IOnDoubleClickListener, UICamera.IOnDragListener, UICamera.IOnDropListener
{
	public GameObject target;

	public bool onHover;

	public bool onPress;

	public bool onClick;

	public bool onDoubleClick;

	public bool onSelect;

	public bool onDrag;

	public bool onDrop;

	public bool onSubmit;

	public bool onScroll;

	private void OnHover(bool isOver)
	{
		if (onHover && target != null)
		{
			Vespa_Unity_GameObject_Extension_Method.SENDMESSAGE(target, "OnHover", isOver, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void OnPress(bool pressed)
	{
		if (onPress && target != null)
		{
			Vespa_Unity_GameObject_Extension_Method.SENDMESSAGE(target, "OnPress", pressed, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void OnClick()
	{
		if (onClick && target != null)
		{
			Vespa_Unity_GameObject_Extension_Method.SENDMESSAGE(target, "OnClick", SendMessageOptions.DontRequireReceiver);
		}
	}

	public void OnDoubleClick()
	{
		if (onDoubleClick && target != null)
		{
			Vespa_Unity_GameObject_Extension_Method.SENDMESSAGE(target, "OnDoubleClick", SendMessageOptions.DontRequireReceiver);
		}
	}

	private void OnSelect(bool selected)
	{
		if (onSelect && target != null)
		{
			Vespa_Unity_GameObject_Extension_Method.SENDMESSAGE(target, "OnSelect", selected, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void OnDrag(Vector2 delta)
	{
		if (onDrag && target != null)
		{
			Vespa_Unity_GameObject_Extension_Method.SENDMESSAGE(target, "OnDrag", delta, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void OnDrop(GameObject go)
	{
		if (onDrop && target != null)
		{
			Vespa_Unity_GameObject_Extension_Method.SENDMESSAGE(target, "OnDrop", go, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void OnSubmit()
	{
		if (onSubmit && target != null)
		{
			Vespa_Unity_GameObject_Extension_Method.SENDMESSAGE(target, "OnSubmit", SendMessageOptions.DontRequireReceiver);
		}
	}

	private void OnScroll(float delta)
	{
		if (onScroll && target != null)
		{
			Vespa_Unity_GameObject_Extension_Method.SENDMESSAGE(target, "OnScroll", delta, SendMessageOptions.DontRequireReceiver);
		}
	}
}
