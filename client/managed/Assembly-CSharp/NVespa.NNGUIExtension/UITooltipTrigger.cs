using System.Collections.Generic;
using NGame2.NLobby;
using UnityEngine;

namespace NVespa.NNGUIExtension;

[AddComponentMenu("NGUI/Interaction/Tooltip Trigger (Vespa)")]
public class UITooltipTrigger : MonoBehaviour, UICamera.IOnPressListener, UICamera.IOnClickListener, UICamera.IOnDoubleClickListener, UICamera.IOnDragStartListener, UICamera.IOnDragEndListener, UICamera.IOnDragListener, UICamera.IOnDragOverListener, UICamera.IOnDragOutListener
{
	private enum ETooltipState
	{
		None,
		Waiting,
		Show
	}

	public enum ETooltipTriggerMode
	{
		ClickAndTooltip,
		ClickOnly,
		TooltipOnly
	}

	public static UITooltipTrigger working;

	public static UITooltipTrigger current;

	public float TooltipDelay = 1f;

	public List<EventDelegate> onPress = new List<EventDelegate>();

	public List<EventDelegate> onRelease = new List<EventDelegate>();

	public List<EventDelegate> onClick = new List<EventDelegate>();

	public List<EventDelegate> onDoubleClick = new List<EventDelegate>();

	public List<EventDelegate> onDragStart = new List<EventDelegate>();

	public List<EventDelegate> onDragEnd = new List<EventDelegate>();

	public List<EventDelegate> onDragOver = new List<EventDelegate>();

	public List<EventDelegate> onDragOut = new List<EventDelegate>();

	public List<EventDelegate> onDrag = new List<EventDelegate>();

	public List<EventDelegate> onShowTooltip = new List<EventDelegate>();

	public List<EventDelegate> onHideTooltip = new List<EventDelegate>();

	private ETooltipState _tooltipState;

	private float _beginWaitTime;

	private bool _ignoreClick;

	protected virtual void Update()
	{
		if (_tooltipState == ETooltipState.Waiting && Time.realtimeSinceStartup - _beginWaitTime > TooltipDelay)
		{
			ShowTooltip();
		}
	}

	private void InitTooltipDelay()
	{
		TooltipPrintInfo componentInParent = GetComponentInParent<TooltipPrintInfo>();
		if (!(componentInParent == null))
		{
			switch (componentInParent.Mode)
			{
			case ETooltipTriggerMode.ClickAndTooltip:
				TooltipDelay = 0.3f;
				break;
			case ETooltipTriggerMode.TooltipOnly:
				TooltipDelay = 0f;
				break;
			}
		}
	}

	public virtual void OnPress(bool pressed)
	{
		if (current != null)
		{
			return;
		}
		current = this;
		if (pressed)
		{
			if (working != null)
			{
				working.ReleaseTooltip();
			}
			working = this;
			EventDelegate.Execute(onPress);
			_tooltipState = ETooltipState.Waiting;
			_beginWaitTime = Time.realtimeSinceStartup;
			_ignoreClick = false;
			InitTooltipDelay();
		}
		else
		{
			EventDelegate.Execute(onRelease);
			ReleaseTooltip();
		}
		ActivityManager.RequestStop();
		current = null;
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
			ReleaseTooltip();
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
			ReleaseTooltip();
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

	private void ShowTooltip()
	{
		if (!(current != null) && __Extension_Method_ICollection__.COUNT(onShowTooltip) != 0)
		{
			current = this;
			EventDelegate.Execute(onShowTooltip);
			_tooltipState = ETooltipState.Show;
			current = null;
			_ignoreClick = true;
		}
	}

	public void ReleaseTooltip()
	{
		if (_tooltipState == ETooltipState.Show)
		{
			EventDelegate.Execute(onHideTooltip);
		}
		_tooltipState = ETooltipState.None;
		if (working == this)
		{
			working = null;
		}
	}
}
