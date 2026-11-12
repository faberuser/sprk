using NGame2.NUI.NRender3D;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class PurifyCamera : IPreviewCamera
{
	private Vector3 _offset = Vector3.up * 1.5f;

	private Vector3 _lookDir = (Vector3.left * 2f + Vector3.down + Vector3.forward * 2f).normalized;

	private float _lookDistance = 5f;

	private bool _playAwake;

	private float _awakeBeginTime;

	public HeroPreview Preview { get; private set; }

	public void Open(Vector3 offset, Vector3 lookDir, float lookDistance)
	{
		_offset = offset;
		_lookDir = lookDir;
		_lookDistance = lookDistance;
		_playAwake = false;
	}

	public void PlayAwake()
	{
		_playAwake = true;
		_awakeBeginTime = GlobalTimer.time;
	}

	void IPreviewCamera.AssignPreview(HeroPreview preview)
	{
		Preview = preview;
		Preview.MainCamera.orthographic = false;
		Preview.MainCamera.nearClipPlane = 0.1f;
		Preview.MainCamera.farClipPlane = 50f;
	}

	void IPreviewCamera.UpdateCamera()
	{
		if (_playAwake)
		{
			float num = Mathf.Clamp01((GlobalTimer.time - _awakeBeginTime) / GameSetting.data.Ui.PurifyHero_AwakeCameraTimeLength);
			float num2 = GameSetting.data.Ui.PurifyHero_AwakeCameraRotateSpeed * Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(GameSetting.data.Ui.PurifyHero_AwakeCameraRotate, num, num);
			_lookDir = Quaternion.AngleAxis(num2 * GlobalTimer.deltaTime, Vector3.up) * _lookDir;
		}
		Preview.MainCamera.fieldOfView = GameSetting.data.Ui.PurifyHero_SceneFOV;
		Preview.MainCamera.transform.position = _offset - _lookDir * _lookDistance;
		Preview.MainCamera.transform.forward = _lookDir;
	}
}
