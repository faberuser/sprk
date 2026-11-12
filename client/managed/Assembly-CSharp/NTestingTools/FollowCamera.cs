using System;
using NVespa;
using NVespa.NUtil;
using UnityEngine;

namespace NTestingTools;

public class FollowCamera : MonoBehaviour
{
	public VirtualPad m_virtualPad;

	public Transform m_target;

	public Transform m_camera;

	public float m_maxDistance = 1f;

	public float m_angleMaxSpeed = 30f;

	private float _smoothRate;

	private void Start()
	{
	}

	private void LateUpdate()
	{
		if (!(m_target == null))
		{
			if (m_virtualPad == null)
			{
				_smoothRate = 1f;
			}
			else
			{
				_smoothRate = Mathf.MoveTowards(target: (!(m_virtualPad.m_padDirection.magnitude < Mathf.Epsilon)) ? 1f : 0f, current: _smoothRate, maxDelta: MainTimer.deltaTime);
			}
			UpdatePosition();
			UpdateRotation();
		}
	}

	private void UpdatePosition()
	{
		float y = base.transform.position.y;
		float num = Mathf.Max(Mathf.Epsilon, m_maxDistance);
		float num2 = Mathf.Min(num, Vector2.Distance(Vespa_Unity_Vector3_Extension_Method.XZ(base.transform.position), Vespa_Unity_Vector3_Extension_Method.XZ(m_target.position))) / num;
		Vector2 vector = Vector2.Lerp(t: ((!(num2 < 0.5f)) ? Mathf.Lerp(Mathf.Clamp01(MainTimer.deltaTime), 1f, num2 * 2f - 1f) : Mathf.Clamp01(Mathf.Lerp(0f, MainTimer.deltaTime, num2 * 2f))) * _smoothRate, a: Vespa_Unity_Vector3_Extension_Method.XZ(base.transform.position), b: Vespa_Unity_Vector3_Extension_Method.XZ(m_target.position));
		base.transform.position = new Vector3(vector.x, y, vector.y);
	}

	private void UpdateRotation()
	{
		float num = Mathf.Sin(Vector3.Angle(base.transform.forward, m_target.forward) * (MathF.PI / 180f));
		base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, m_target.rotation, num * m_angleMaxSpeed * _smoothRate * MainTimer.deltaTime);
	}
}
