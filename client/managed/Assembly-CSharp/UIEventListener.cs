using UnityEngine;

[AddComponentMenu("NGUI/Internal/Event Listener")]
public class UIEventListener : MonoBehaviour, UICamera.IOnPressListener, UICamera.IOnClickListener, UICamera.IOnDoubleClickListener, UICamera.IOnDragStartListener, UICamera.IOnDragEndListener, UICamera.IOnDragListener, UICamera.IOnDragOverNoArgListener, UICamera.IOnDragOutNoArgListener, UICamera.IOnDropListener
{
	public delegate void VoidDelegate(GameObject go);

	public delegate void BoolDelegate(GameObject go, bool state);

	public delegate void FloatDelegate(GameObject go, float delta);

	public delegate void VectorDelegate(GameObject go, Vector2 delta);

	public delegate void ObjectDelegate(GameObject go, GameObject obj);

	public delegate void KeyCodeDelegate(GameObject go, KeyCode key);

	public object parameter;

	public VoidDelegate onSubmit;

	public VoidDelegate onClick;

	public VoidDelegate onDoubleClick;

	public BoolDelegate onHover;

	public BoolDelegate onPress;

	public BoolDelegate onSelect;

	public FloatDelegate onScroll;

	public VoidDelegate onDragStart;

	public VectorDelegate onDrag;

	public VoidDelegate onDragOver;

	public VoidDelegate onDragOut;

	public VoidDelegate onDragEnd;

	public ObjectDelegate onDrop;

	public KeyCodeDelegate onKey;

	public BoolDelegate onTooltip;

	private void OnSubmit()
	{
		if (onSubmit != null)
		{
			onSubmit(base.gameObject);
		}
	}

	public void OnClick()
	{
		if (onClick != null)
		{
			onClick(base.gameObject);
		}
	}

	public void OnDoubleClick()
	{
		if (onDoubleClick != null)
		{
			onDoubleClick(base.gameObject);
		}
	}

	private void OnHover(bool isOver)
	{
		if (onHover != null)
		{
			onHover(base.gameObject, isOver);
		}
	}

	public void OnPress(bool isPressed)
	{
		if (onPress != null)
		{
			onPress(base.gameObject, isPressed);
		}
	}

	private void OnSelect(bool selected)
	{
		if (onSelect != null)
		{
			onSelect(base.gameObject, selected);
		}
	}

	private void OnScroll(float delta)
	{
		if (onScroll != null)
		{
			onScroll(base.gameObject, delta);
		}
	}

	public void OnDragStart()
	{
		if (onDragStart != null)
		{
			onDragStart(base.gameObject);
		}
	}

	public void OnDrag(Vector2 delta)
	{
		if (onDrag != null)
		{
			onDrag(base.gameObject, delta);
		}
	}

	public void OnDragOver()
	{
		if (onDragOver != null)
		{
			onDragOver(base.gameObject);
		}
	}

	public void OnDragOut()
	{
		if (onDragOut != null)
		{
			onDragOut(base.gameObject);
		}
	}

	public void OnDragEnd()
	{
		if (onDragEnd != null)
		{
			onDragEnd(base.gameObject);
		}
	}

	public void OnDrop(GameObject go)
	{
		if (onDrop != null)
		{
			onDrop(base.gameObject, go);
		}
	}

	private void OnKey(KeyCode key)
	{
		if (onKey != null)
		{
			onKey(base.gameObject, key);
		}
	}

	private void OnTooltip(bool show)
	{
		if (onTooltip != null)
		{
			onTooltip(base.gameObject, show);
		}
	}

	public static UIEventListener Get(GameObject go)
	{
		UIEventListener uIEventListener = go.GetComponent<UIEventListener>();
		if (uIEventListener == null)
		{
			uIEventListener = go.AddComponent<UIEventListener>();
		}
		return uIEventListener;
	}
}
