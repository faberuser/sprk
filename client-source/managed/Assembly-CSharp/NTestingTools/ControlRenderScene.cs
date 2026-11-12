using System;
using System.Collections.Generic;
using NVespa.NGui;
using NVespa.NScene;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NTestingTools;

public class ControlRenderScene : MonoBehaviour
{
	[Serializable]
	public class TargetInfo
	{
		public Transform m_transform;

		[NonSerialized]
		public Quaternion m_initRotate;
	}

	private enum EMultiTouchMode
	{
		None,
		Slide,
		Zoom
	}

	public Transform m_cameraTarget;

	public Camera m_camera;

	[HideInInspector]
	public List<TargetInfo> m_listTarget = new List<TargetInfo>();

	[NonSerialized]
	public int m_targetIndex;

	public Transform m_pickedTarget;

	public bool m_rotateHorizonOnly = true;

	public float m_rotateAmount;

	public MultiTouchControl m_touchControl;

	private EMultiTouchMode _multiTouchMode;

	private void Start()
	{
		for (int i = 0; i < m_listTarget.Count; i++)
		{
			TargetInfo targetInfo = m_listTarget[i];
			if (targetInfo.m_transform == null)
			{
				m_listTarget.RemoveAt(i);
				i--;
			}
			else
			{
				targetInfo.m_initRotate = targetInfo.m_transform.localRotation;
			}
		}
		SceneAutoBehaviour<GuiManager>.instance.Add("RenderScene", this, DrawGUI);
		if (m_touchControl != null)
		{
			MultiTouchControl touchControl = m_touchControl;
			touchControl.m_beginTouchDelegate = (Action<MultiTouchControl.SingleTouch>)Delegate.Combine(touchControl.m_beginTouchDelegate, new Action<MultiTouchControl.SingleTouch>(BeginTouch));
			MultiTouchControl touchControl2 = m_touchControl;
			touchControl2.m_moveTouchDelegate = (Action<MultiTouchControl.SingleTouch>)Delegate.Combine(touchControl2.m_moveTouchDelegate, new Action<MultiTouchControl.SingleTouch>(MoveTouch));
			MultiTouchControl touchControl3 = m_touchControl;
			touchControl3.m_endTouchDelegate = (Action<MultiTouchControl.SingleTouch>)Delegate.Combine(touchControl3.m_endTouchDelegate, new Action<MultiTouchControl.SingleTouch>(EndTouch));
		}
	}

	private void DrawGUI()
	{
		string text = (m_rotateHorizonOnly ? "Rotate Horizon Only" : "Free Rotate");
		m_rotateHorizonOnly = GUILayout.Toggle(m_rotateHorizonOnly, text, GUI.skin.button);
		for (int i = 0; i < m_listTarget.Count; i++)
		{
			TargetInfo targetInfo = m_listTarget[i];
			using (new BlockManager().Enable(i != m_targetIndex))
			{
				if (GUILayout.Button(targetInfo.m_transform.name, GUILayout.Height(20f)))
				{
					m_targetIndex = i;
				}
			}
		}
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(m_listTarget, m_targetIndex))
		{
			GUILayout.Label("Picked");
		}
		if (GUILayout.Button("Reset", GUILayout.Height(30f)))
		{
			if (Vespa_Generic_IList_Extension_Method.IsValidIndex(m_listTarget, m_targetIndex))
			{
				TargetInfo targetInfo2 = m_listTarget[m_targetIndex];
				targetInfo2.m_transform.localRotation = targetInfo2.m_initRotate;
			}
			else
			{
				m_pickedTarget.localRotation = Quaternion.identity;
			}
		}
	}

	private void BeginTouch(MultiTouchControl.SingleTouch i_touch)
	{
		_multiTouchMode = EMultiTouchMode.None;
	}

	private void MoveTouch(MultiTouchControl.SingleTouch i_touch)
	{
		if (m_touchControl.m_dicTouch.Count != 2)
		{
			_multiTouchMode = EMultiTouchMode.None;
		}
		if (i_touch.m_draging && m_touchControl.m_dicTouch.Count == 1)
		{
			RotateTarget(i_touch);
		}
		else if (m_touchControl.m_dicTouch.Count == 2)
		{
			if (_multiTouchMode == EMultiTouchMode.None)
			{
				SelectMultiTouchMode();
			}
			else if (_multiTouchMode == EMultiTouchMode.Slide)
			{
				SlideCamera(i_touch);
			}
			else if (_multiTouchMode == EMultiTouchMode.Zoom)
			{
				ZoomCamera(i_touch);
			}
		}
	}

	private void SelectMultiTouchMode()
	{
		MultiTouchControl.SingleTouch[] array = new MultiTouchControl.SingleTouch[m_touchControl.m_dicTouch.Values.Count];
		m_touchControl.m_dicTouch.Values.CopyTo(array, 0);
		if (!array[0].m_draging || !array[1].m_draging)
		{
			return;
		}
		Vector2 vector = array[0].m_curTouch - array[0].m_initTouch;
		Vector2 to = array[1].m_curTouch - array[1].m_initTouch;
		if (!(vector.magnitude < Mathf.Epsilon) && !(to.magnitude < Mathf.Epsilon))
		{
			if (Vector2.Angle(vector, to) < 90f)
			{
				_multiTouchMode = EMultiTouchMode.Slide;
			}
			else
			{
				_multiTouchMode = EMultiTouchMode.Zoom;
			}
		}
	}

	private void SlideCamera(MultiTouchControl.SingleTouch i_touch)
	{
		Ray ray = Vespa_Unity_Camera_Extension_Method.VespaScreenPointToRay(m_camera, i_touch.m_prevTouch);
		Vector3 vector = Vespa_Unity_Camera_Extension_Method.VespaScreenPointToRay(m_camera, i_touch.m_curTouch).origin - ray.origin;
		m_cameraTarget.Translate(-vector * 0.5f, Space.World);
	}

	private void ZoomCamera(MultiTouchControl.SingleTouch i_touch)
	{
		MultiTouchControl.SingleTouch singleTouch = null;
		foreach (MultiTouchControl.SingleTouch value in m_touchControl.m_dicTouch.Values)
		{
			if (value.m_touchIndex != i_touch.m_touchIndex)
			{
				singleTouch = value;
				break;
			}
		}
		if (singleTouch != null)
		{
			float num = Vector2.Distance(singleTouch.m_initTouch, i_touch.m_prevTouch);
			float num2 = Vector2.Distance(singleTouch.m_initTouch, i_touch.m_curTouch);
			float num3 = Mathf.Pow(2f, (num - num2) / (float)Screen.height);
			m_camera.orthographicSize *= num3;
		}
	}

	private void RotateTarget(MultiTouchControl.SingleTouch i_touch)
	{
		Transform transform = ((!Vespa_Generic_IList_Extension_Method.IsValidIndex(m_listTarget, m_targetIndex)) ? m_pickedTarget : m_listTarget[m_targetIndex].m_transform);
		if (!(transform == null))
		{
			Vector2 vector = i_touch.m_curTouch - i_touch.m_prevTouch;
			float num = vector.x / (float)Screen.width * m_rotateAmount;
			float angle = vector.y / (float)Screen.height * m_rotateAmount;
			if (m_rotateHorizonOnly)
			{
				angle = 0f;
			}
			Quaternion localRotation = transform.localRotation;
			localRotation = Quaternion.AngleAxis(0f - num, Vector3.up) * localRotation * Quaternion.AngleAxis(angle, Vector3.right);
			transform.localRotation = localRotation;
		}
	}

	private void EndTouch(MultiTouchControl.SingleTouch i_touch)
	{
		_multiTouchMode = EMultiTouchMode.None;
		if (!i_touch.m_draging && m_touchControl.m_dicTouch.Count == 1 && Physics.Raycast(Vespa_Unity_Camera_Extension_Method.VespaScreenPointToRay(m_camera, i_touch.m_initTouch), out var hitInfo))
		{
			m_pickedTarget = hitInfo.transform;
			m_targetIndex = -1;
		}
	}

	private void Update()
	{
	}
}
