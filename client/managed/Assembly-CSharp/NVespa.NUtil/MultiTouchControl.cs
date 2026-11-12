using System;
using System.Collections.Generic;
using UnityEngine;

namespace NVespa.NUtil;

public class MultiTouchControl : MonoBehaviour
{
	public class SingleTouch
	{
		public Vector2 m_dragPosition
		{
			get
			{
				if (m_draging)
				{
					return m_curTouch;
				}
				return m_initTouch;
			}
		}

		public int m_touchIndex { get; private set; }

		public Vector2 m_initTouch { get; private set; }

		public Vector2 m_prevTouch { get; private set; }

		public Vector2 m_curTouch { get; private set; }

		public bool m_draging { get; private set; }

		public SingleTouch(int i_touchIndex, Vector2 i_touchPos)
		{
			m_touchIndex = i_touchIndex;
			m_initTouch = i_touchPos;
			m_prevTouch = i_touchPos;
			m_curTouch = i_touchPos;
			m_draging = false;
		}

		public void SetNextPosition(Vector2 i_touchPos)
		{
			m_prevTouch = m_curTouch;
			m_curTouch = i_touchPos;
			if (Vector2.Distance(m_curTouch, m_initTouch) > 5f)
			{
				m_draging = true;
			}
		}
	}

	public Dictionary<int, SingleTouch> m_dicTouch = new Dictionary<int, SingleTouch>();

	public Func<Vector2, bool> m_isInside;

	public Action<SingleTouch> m_beginTouchDelegate;

	public Action<SingleTouch> m_moveTouchDelegate;

	public Action<SingleTouch> m_endTouchDelegate;

	private void Start()
	{
	}

	private void Update()
	{
		UpdateTouchControl();
	}

	private void UpdateTouchControl()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < VespaInput.touchCount; i++)
		{
			Touch touch = VespaInput.GetTouch(i);
			SingleTouch vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(m_dicTouch, touch.fingerId, null);
			if (vALUE != null)
			{
				vALUE.SetNextPosition(touch.position);
				MoveTouch(vALUE);
				if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended)
				{
					EndTouch(vALUE);
					list.Add(vALUE.m_touchIndex);
				}
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			int key = list[j];
			if (m_dicTouch.ContainsKey(key))
			{
				m_dicTouch.Remove(key);
			}
		}
		for (int k = 0; k < VespaInput.touchCount; k++)
		{
			Touch touch2 = VespaInput.GetTouch(k);
			if (touch2.phase == TouchPhase.Began && !m_dicTouch.ContainsKey(touch2.fingerId) && IsInside(touch2.position))
			{
				SingleTouch singleTouch = new SingleTouch(touch2.fingerId, touch2.position);
				m_dicTouch[touch2.fingerId] = singleTouch;
				BeginTouch(singleTouch);
			}
		}
	}

	private void UpdateMouseControl()
	{
		List<int> list = new List<int>();
		foreach (SingleTouch value in m_dicTouch.Values)
		{
			value.SetNextPosition(VespaInput.mousePosition);
			MoveTouch(value);
			if (!VespaInput.GetMouseButton(value.m_touchIndex))
			{
				EndTouch(value);
				list.Add(value.m_touchIndex);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			int key = list[i];
			if (m_dicTouch.ContainsKey(key))
			{
				m_dicTouch.Remove(key);
			}
		}
		for (int j = 0; j < 2; j++)
		{
			if (VespaInput.GetMouseButtonDown(j) && !m_dicTouch.ContainsKey(j) && IsInside(VespaInput.mousePosition))
			{
				SingleTouch singleTouch = new SingleTouch(j, VespaInput.mousePosition);
				m_dicTouch[j] = singleTouch;
				BeginTouch(singleTouch);
			}
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

	private void BeginTouch(SingleTouch i_newTouch)
	{
		Vespa_System_Action_Extension_Method.fire(m_beginTouchDelegate, i_newTouch);
	}

	private void MoveTouch(SingleTouch i_touch)
	{
		Vespa_System_Action_Extension_Method.fire(m_moveTouchDelegate, i_touch);
	}

	private void EndTouch(SingleTouch i_touch)
	{
		Vespa_System_Action_Extension_Method.fire(m_endTouchDelegate, i_touch);
	}
}
