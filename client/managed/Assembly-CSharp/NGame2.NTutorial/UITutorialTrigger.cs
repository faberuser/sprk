using System.Collections.Generic;
using NGame2.NLobby;
using UnityEngine;

namespace NGame2.NTutorial;

[AddComponentMenu("NGUI/Interaction/Tutorial Trigger (Vespa)")]
public class UITutorialTrigger : MonoBehaviour, UICamera.IOnPressListener, UICamera.IOnClickListener, UICamera.IOnDoubleClickListener, UICamera.IOnDragStartListener, UICamera.IOnDragEndListener, UICamera.IOnDragListener, UICamera.IOnDragOverListener, UICamera.IOnDragOutListener
{
	private enum ELongTouchState
	{
		None,
		Waiting,
		Show
	}

	public static UITutorialTrigger current;

	public float LongTouchDelay = 1f;

	public List<EventDelegate> onPress = new List<EventDelegate>();

	public List<EventDelegate> onRelease = new List<EventDelegate>();

	public List<EventDelegate> onClick = new List<EventDelegate>();

	public List<EventDelegate> onDoubleClick = new List<EventDelegate>();

	public List<EventDelegate> onDragStart = new List<EventDelegate>();

	public List<EventDelegate> onDragEnd = new List<EventDelegate>();

	public List<EventDelegate> onDragOver = new List<EventDelegate>();

	public List<EventDelegate> onDragOut = new List<EventDelegate>();

	public List<EventDelegate> onDrag = new List<EventDelegate>();

	public List<EventDelegate> onLongTouch = new List<EventDelegate>();

	private ELongTouchState _longTouchState;

	private float _beginWaitTime;

	private bool _ignoreClick;

	protected virtual void Update()
	{
		if (_longTouchState == ELongTouchState.Waiting && Time.realtimeSinceStartup - _beginWaitTime > LongTouchDelay)
		{
			OnLongTouch();
		}
	}

	public virtual void OnPress(bool pressed)
	{
		if (!(current != null))
		{
			current = this;
			if (pressed)
			{
				EventDelegate.Execute(onPress);
				_longTouchState = ELongTouchState.Waiting;
				_beginWaitTime = Time.realtimeSinceStartup;
				_ignoreClick = false;
			}
			else
			{
				EventDelegate.Execute(onRelease);
				ReleaseLongTouch();
			}
			ActivityManager.RequestStop();
			current = null;
		}
	}

	public virtual void OnClick()
	{
		if (_ignoreClick)
		{
			_ignoreClick = false;
		}
		else if (!(current != null))
		{
			current = this;
			EventDelegate.Execute(onClick);
			ActivityManager.RequestStop();
			current = null;
		}
	}

	public virtual void OnDoubleClick()
	{
		if (!(current != null))
		{
			current = this;
			EventDelegate.Execute(onDoubleClick);
			ActivityManager.RequestStop();
			current = null;
		}
	}

	public virtual void OnDragStart()
	{
		if (!(current != null))
		{
			current = this;
			EventDelegate.Execute(onDragStart);
			current = null;
		}
	}

	public virtual void OnDragEnd()
	{
		if (!(current != null))
		{
			current = this;
			EventDelegate.Execute(onDragEnd);
			ReleaseLongTouch();
			current = null;
		}
	}

	public virtual void OnDragOver(GameObject go)
	{
		if (!(current != null))
		{
			current = this;
			EventDelegate.Execute(onDragOver);
			current = null;
		}
	}

	public virtual void OnDragOut(GameObject go)
	{
		if (!(current != null))
		{
			current = this;
			EventDelegate.Execute(onDragOut);
			ReleaseLongTouch();
			current = null;
		}
	}

	public virtual void OnDrag(Vector2 delta)
	{
		if (!(current != null))
		{
			current = this;
			EventDelegate.Execute(onDrag);
			current = null;
			_beginWaitTime = Time.realtimeSinceStartup;
		}
	}

	private void OnLongTouch()
	{
		if (!(current != null) && __Extension_Method_ICollection__.COUNT(onLongTouch) != 0)
		{
			current = this;
			EventDelegate.Execute(onLongTouch);
			_longTouchState = ELongTouchState.Show;
			ActivityManager.RequestStop();
			current = null;
			_ignoreClick = true;
		}
	}

	public void ReleaseLongTouch()
	{
		_longTouchState = ELongTouchState.None;
	}
}
