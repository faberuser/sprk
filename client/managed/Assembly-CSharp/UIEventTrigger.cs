using System.Collections.Generic;
using NGame2.NLobby;
using NGame2.NUI;
using NGame2.NUI.NManager;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Event Trigger")]
public class UIEventTrigger : MonoBehaviour, UICamera.IOnPressListener, UICamera.IOnClickListener, UICamera.IOnDoubleClickListener, UICamera.IOnDragStartListener, UICamera.IOnDragEndListener, UICamera.IOnDragListener, UICamera.IOnDragOverListener, UICamera.IOnDragOutListener
{
	public static UIEventTrigger current;

	public List<EventDelegate> onHoverOver = new List<EventDelegate>();

	public List<EventDelegate> onHoverOut = new List<EventDelegate>();

	public List<EventDelegate> onPress = new List<EventDelegate>();

	public List<EventDelegate> onRelease = new List<EventDelegate>();

	public List<EventDelegate> onSelect = new List<EventDelegate>();

	public List<EventDelegate> onDeselect = new List<EventDelegate>();

	public List<EventDelegate> onClick = new List<EventDelegate>();

	public List<EventDelegate> onDoubleClick = new List<EventDelegate>();

	public List<EventDelegate> onDragStart = new List<EventDelegate>();

	public List<EventDelegate> onDragEnd = new List<EventDelegate>();

	public List<EventDelegate> onDragOver = new List<EventDelegate>();

	public List<EventDelegate> onDragOut = new List<EventDelegate>();

	public List<EventDelegate> onDrag = new List<EventDelegate>();

	private void OnHover(bool isOver)
	{
		if (!(current != null))
		{
			current = this;
			if (isOver)
			{
				EventDelegate.Execute(onHoverOver);
			}
			else
			{
				EventDelegate.Execute(onHoverOut);
			}
			current = null;
		}
	}

	public void OnPress(bool pressed)
	{
		if (!(current != null))
		{
			current = this;
			if (pressed)
			{
				EventDelegate.Execute(onPress);
			}
			else
			{
				EventDelegate.Execute(onRelease);
			}
			ActivityManager.RequestStop();
			current = null;
		}
	}

	private void OnSelect(bool selected)
	{
		if (!(current != null))
		{
			current = this;
			if (selected)
			{
				EventDelegate.Execute(onSelect);
			}
			else
			{
				EventDelegate.Execute(onDeselect);
			}
			current = null;
		}
	}

	public void OnClick()
	{
		if (!(current != null))
		{
			current = this;
			bool flag = UIEventAttribute.IsEnableEvent(onClick);
			if (!ClientValueManager.ActiveBlockingDuringBackgoundPatchDownload || flag)
			{
				EventDelegate.Execute(onClick);
			}
			ActivityManager.RequestStop();
			current = null;
		}
	}

	public void OnDoubleClick()
	{
		if (!(current != null))
		{
			current = this;
			EventDelegate.Execute(onDoubleClick);
			ActivityManager.RequestStop();
			current = null;
		}
	}

	public void OnDragStart()
	{
		if (!(current != null))
		{
			current = this;
			EventDelegate.Execute(onDragStart);
			current = null;
		}
	}

	public void OnDragEnd()
	{
		if (!(current != null))
		{
			current = this;
			EventDelegate.Execute(onDragEnd);
			current = null;
		}
	}

	public void OnDragOver(GameObject go)
	{
		if (!(current != null))
		{
			current = this;
			EventDelegate.Execute(onDragOver);
			current = null;
		}
	}

	public void OnDragOut(GameObject go)
	{
		if (!(current != null))
		{
			current = this;
			EventDelegate.Execute(onDragOut);
			current = null;
		}
	}

	public void OnDrag(Vector2 delta)
	{
		if (!(current != null))
		{
			current = this;
			EventDelegate.Execute(onDrag);
			current = null;
		}
	}
}
