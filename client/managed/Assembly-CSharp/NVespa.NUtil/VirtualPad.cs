using System;
using UnityEngine;

namespace NVespa.NUtil;

public class VirtualPad : MonoBehaviour
{
	public const float MaxDistance = 64f;

	public Camera m_camera;

	public Collider m_hitCollider;

	public MultiTouchControl m_touchControl;

	public UIWidget m_controller;

	public Vector2 m_padDirection;

	private void Start()
	{
		if (m_touchControl != null)
		{
			MultiTouchControl touchControl = m_touchControl;
			touchControl.m_isInside = (Func<Vector2, bool>)Delegate.Combine(touchControl.m_isInside, new Func<Vector2, bool>(IsInside));
			MultiTouchControl touchControl2 = m_touchControl;
			touchControl2.m_beginTouchDelegate = (Action<MultiTouchControl.SingleTouch>)Delegate.Combine(touchControl2.m_beginTouchDelegate, new Action<MultiTouchControl.SingleTouch>(BeginTouch));
			MultiTouchControl touchControl3 = m_touchControl;
			touchControl3.m_moveTouchDelegate = (Action<MultiTouchControl.SingleTouch>)Delegate.Combine(touchControl3.m_moveTouchDelegate, new Action<MultiTouchControl.SingleTouch>(MoveTouch));
			MultiTouchControl touchControl4 = m_touchControl;
			touchControl4.m_endTouchDelegate = (Action<MultiTouchControl.SingleTouch>)Delegate.Combine(touchControl4.m_endTouchDelegate, new Action<MultiTouchControl.SingleTouch>(EndTouch));
		}
	}

	public static float DistanceToLine(Ray ray, Vector3 point)
	{
		return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
	}

	private bool IsInside(Vector2 i_pos)
	{
		if (m_camera == null || m_hitCollider == null || m_camera == null)
		{
			return false;
		}
		Ray ray = Vespa_Unity_Camera_Extension_Method.VespaScreenPointToRay(m_camera, i_pos);
		if (!m_hitCollider.Raycast(ray, out var _, 1000f))
		{
			return false;
		}
		return true;
	}

	private void BeginTouch(MultiTouchControl.SingleTouch i_touch)
	{
		TouchCollider(i_touch);
	}

	private void MoveTouch(MultiTouchControl.SingleTouch i_touch)
	{
		TouchCollider(i_touch);
	}

	private void EndTouch(MultiTouchControl.SingleTouch i_touch)
	{
		UpdateDirection(Vector2.zero);
	}

	private void TouchCollider(MultiTouchControl.SingleTouch i_touch)
	{
		Ray ray = Vespa_Unity_Camera_Extension_Method.VespaScreenPointToRay(m_camera, i_touch.m_curTouch);
		Vector2 vector = base.transform.worldToLocalMatrix.MultiplyPoint(ray.origin);
		Vector2 vector2 = m_hitCollider.transform.localPosition;
		if (Vector2.Distance(vector, vector2) < 64f)
		{
			UpdateDirection(Vector2.zero);
		}
		else
		{
			UpdateDirection((vector - vector2).normalized);
		}
	}

	private void UpdateDirection(Vector2 i_dir)
	{
		m_padDirection = i_dir;
		m_controller.transform.localPosition = i_dir * 64f;
	}
}
