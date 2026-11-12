using System;
using System.Collections.Generic;
using NGame2.NNodeTree.NSceneContainer;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NRender3D.NRenderTargetCinema;

public class CinemaCamera : ICamera
{
	public class CameraInfoData
	{
		public Vector3 TargetPos;

		public Vector3 LookDir;

		public float TargetDistance;

		public float FieldOfView;

		public float DefaultDistance => 8f;

		public CameraInfoData()
		{
			TargetPos = Vector3.up;
			LookDir = Vector3.forward;
			TargetDistance = DefaultDistance;
			FieldOfView = GameSetting.data.Ui.HeroDetailView.FieldOfView;
		}
	}

	public CameraInfoData CameraInfo = new CameraInfoData();

	private Camera _camera;

	private ShakeMaker _cameraShake = new ShakeMaker();

	private List<IEventCameraEntry> _entryList = new List<IEventCameraEntry>();

	private Camera MainCamera => _camera;

	public CinemaCamera(Camera camera)
	{
		_camera = camera;
	}

	Camera ICamera.GetUnityCamera()
	{
		return MainCamera;
	}

	ShakeMaker ICamera.GetCameraShake()
	{
		return _cameraShake;
	}

	void ICamera.SetAlphaToggleEntry(AlphaToggleEntry entry)
	{
	}

	void ICamera.SetAlphaToggleEntry2(AlphaToggleEntry2 entry)
	{
	}

	void ICamera.SetFocusEntry(IFocusCameraEntry entry)
	{
	}

	void ICamera.SetEventEntry(IEventCameraEntry entry)
	{
		_entryList.Add(entry);
	}

	void ICamera.ReleaseEventEntry(IEventCameraEntry entry)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_entryList) && entry != null && _entryList.Remove(entry))
		{
			_ = _entryList.Count;
			_ = 0;
		}
	}

	public void UpdateCamera()
	{
		Vector3 cameraTargetPos = CameraInfo.TargetPos;
		Quaternion cameraRot = Quaternion.LookRotation(CameraInfo.LookDir);
		float cameraDistance = CameraInfo.TargetDistance;
		float cameraFov = CameraInfo.FieldOfView;
		UpdateEntry(ref cameraTargetPos, ref cameraRot, ref cameraDistance, ref cameraFov);
		Vector3 shakeTarget = cameraTargetPos + cameraRot * Vector3.back * cameraDistance;
		_cameraShake.ApplyShake(ref shakeTarget, CalcShakeWeight());
		MainCamera.transform.position = shakeTarget;
		MainCamera.transform.rotation = cameraRot;
		MainCamera.fieldOfView = cameraFov;
		CameraInfo.TargetPos = cameraTargetPos;
		CameraInfo.LookDir = cameraRot * Vector3.forward;
		CameraInfo.TargetDistance = cameraDistance;
		CameraInfo.FieldOfView = cameraFov;
	}

	public void ChainUpdate()
	{
		UpdateCamera();
	}

	private float CalcShakeWeight()
	{
		float num = 1f;
		for (int i = 0; i < _entryList.Count; i++)
		{
			IEventCameraEntry eventCameraEntry = _entryList[i];
			num = Mathf.Min(num, eventCameraEntry.ShakeWeight);
		}
		return num;
	}

	private void UpdateEntry(ref Vector3 cameraTargetPos, ref Quaternion cameraRot, ref float cameraDistance, ref float cameraFov)
	{
		for (int num = _entryList.Count - 1; num >= 1; num--)
		{
			if (_entryList[num].FadeInEnd)
			{
				_entryList.RemoveRange(0, num);
				break;
			}
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_entryList))
		{
			for (int i = 0; i < _entryList.Count; i++)
			{
				_entryList[i].UpdateCamera(ref cameraTargetPos, ref cameraRot, ref cameraDistance, ref cameraFov);
			}
		}
	}

	public void OrthographicMode(bool isOn)
	{
		MainCamera.orthographic = isOn;
		float num = Convert.ToSingle(Screen.height);
		float num2 = Convert.ToSingle(Screen.width);
		float orthographicSize = num / num2;
		MainCamera.orthographicSize = orthographicSize;
	}

	public void AllowHDR(bool isOn)
	{
		MainCamera.allowHDR = isOn;
	}
}
