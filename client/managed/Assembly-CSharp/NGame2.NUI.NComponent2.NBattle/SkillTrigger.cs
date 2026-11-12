using NVespa.NNGUIExtension;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NBattle;

public class SkillTrigger : UITooltipTrigger
{
	public interface IListener
	{
		void OnRelease();

		void OnDrag(Vector3 posOnViewport);
	}

	private int _touchId;

	private bool _dragOut;

	public IListener Listener { get; set; }

	public override void OnPress(bool pressed)
	{
		base.OnPress(pressed);
		if (pressed)
		{
			_touchId = UICamera.currentTouchID;
			return;
		}
		_touchId = int.MinValue;
		if (_dragOut)
		{
			if (Listener != null)
			{
				Listener.OnRelease();
			}
			_dragOut = false;
		}
	}

	public override void OnDragOut(GameObject go)
	{
		base.OnDragOut(go);
		if (_touchId == UICamera.currentTouchID)
		{
			Vector3 posOnViewport = Vespa_Unity_Camera_Extension_Method.VespaScreenToViewportPoint(UICamera.currentCamera, UICamera.currentTouch.pos);
			_dragOut = true;
			if (Listener != null)
			{
				Listener.OnDrag(posOnViewport);
			}
		}
	}

	public override void OnDrag(Vector2 delta)
	{
		base.OnDrag(delta);
		if (_touchId == UICamera.currentTouchID && _dragOut)
		{
			Vector3 posOnViewport = Vespa_Unity_Camera_Extension_Method.VespaScreenToViewportPoint(UICamera.currentCamera, UICamera.currentTouch.pos);
			if (Listener != null)
			{
				Listener.OnDrag(posOnViewport);
			}
		}
	}
}
