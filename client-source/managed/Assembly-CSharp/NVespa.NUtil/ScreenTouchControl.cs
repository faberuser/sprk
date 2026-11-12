using System;
using UnityEngine;

namespace NVespa.NUtil;

public class ScreenTouchControl : MonoBehaviour
{
	public Func<Vector2, bool> m_isInside;

	public Action<Vector2> m_beginTouchDelegate;

	public Action<Vector2, Vector2> m_moveTouchDelegate;

	public Action m_releaseTouchDelegate;

	private int _touchFingerId = -1;

	private Vector2 _prevTouchPos;

	private bool MOUSE_Drag;

	private void Update()
	{
		UpdateTouchControl();
	}

	private void UpdateMouseControl()
	{
		if (VespaInput.GetMouseButtonDown(0))
		{
			if (IsInside(VespaInput.mousePosition))
			{
				MOUSE_Drag = true;
				BeginTouch(VespaInput.mousePosition);
			}
		}
		else if (MOUSE_Drag)
		{
			if (!VespaInput.GetMouseButton(0))
			{
				MoveTouch(VespaInput.mousePosition);
				EndTouch();
				MOUSE_Drag = false;
			}
			else
			{
				MoveTouch(VespaInput.mousePosition);
			}
		}
	}

	private void UpdateTouchControl()
	{
		for (int i = 0; i < VespaInput.touchCount; i++)
		{
			Touch touch = VespaInput.GetTouch(i);
			if (touch.phase == TouchPhase.Began && IsInside(touch.position))
			{
				_touchFingerId = touch.fingerId;
				BeginTouch(touch.position);
				return;
			}
		}
		if (_touchFingerId == -1)
		{
			return;
		}
		bool flag = false;
		Touch touch2 = default(Touch);
		for (int j = 0; j < VespaInput.touchCount; j++)
		{
			Touch touch3 = VespaInput.GetTouch(j);
			if (touch3.fingerId == _touchFingerId)
			{
				flag = true;
				touch2 = touch3;
			}
		}
		if (!flag)
		{
			EndTouch();
			_touchFingerId = -1;
		}
		else if (touch2.phase == TouchPhase.Canceled || touch2.phase == TouchPhase.Ended)
		{
			MoveTouch(touch2.position);
			EndTouch();
			_touchFingerId = -1;
		}
		else
		{
			MoveTouch(touch2.position);
		}
	}

	private bool IsInside(Vector2 i_pos)
	{
		if (m_isInside == null)
		{
			return true;
		}
		return m_isInside(i_pos);
	}

	private void BeginTouch(Vector2 i_position)
	{
		_prevTouchPos = i_position;
		Vespa_System_Action_Extension_Method.fire(m_beginTouchDelegate, i_position);
	}

	private void MoveTouch(Vector2 i_position)
	{
		Vector2 i_argB = i_position - _prevTouchPos;
		_prevTouchPos = i_position;
		Vespa_System_Action_Extension_Method.fire(m_moveTouchDelegate, i_position, i_argB);
	}

	private void EndTouch()
	{
		Vespa_System_Action_Extension_Method.fire(m_releaseTouchDelegate);
	}
}
