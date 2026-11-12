using NGameAsset;
using UnityEngine;

namespace NVespa.NUtil;

public class CameraTester : MonoBehaviour, IInspectorGuiDrawer
{
	public Camera TargetCamera;

	public float OrthoToFovScale = 45f;

	public float CameraAnimationTime = 10f;

	[HideInInspector]
	public CameraData CameraData = new CameraData();

	private float _lastFrame;

	public void SetAnimationFrame(float frame)
	{
		if (!(TargetCamera == null))
		{
			Vector3 vector = CameraData.PositionCurve.Evaluate(frame);
			Vector3 vector2 = CameraData.TargetCurve.Evaluate(frame);
			float fieldOfView = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(CameraData.OrthoCurve, frame, 1f) * OrthoToFovScale;
			TargetCamera.transform.position = vector;
			TargetCamera.transform.forward = (vector2 - vector).normalized;
			TargetCamera.fieldOfView = fieldOfView;
			_lastFrame = frame;
		}
	}

	public void SetAnimationTime(float time)
	{
		float animationFrame = Mathf.Clamp01(time / Vespa_Float_Extension_Method.NoneZero(CameraAnimationTime));
		SetAnimationFrame(animationFrame);
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
