using System;
using System.Collections.Generic;
using NGame2.N3D;

namespace NGame2.NBattleView.NCamera;

public class CameraGroup
{
	public static Comparison<CameraBase> _cameraLessWeight;

	private List<CameraBase> _cameraList = new List<CameraBase>();

	public CameraBase MainCamera { get; private set; }

	public CameraGroup()
	{
		if (_cameraLessWeight == null)
		{
			_cameraLessWeight = (CameraBase lhs, CameraBase rhs) => lhs.Weight.Value.CompareTo(rhs.Weight.Value);
		}
	}

	public void UpdateCamera()
	{
		if (MainCamera == null)
		{
			return;
		}
		float num = (float)MainCamera.Timer.deltaTick * 0.001f * MainCamera.CameraBlendSpeed;
		MainCamera.ChainUpdate();
		MainCamera.Weight.SetByFrame(MainCamera.Weight.Frame + num);
		if (MainCamera.Weight.Value >= 1f)
		{
			_cameraList.Clear();
			return;
		}
		float num2 = 1f - MainCamera.Weight.Value;
		_cameraList.Sort(_cameraLessWeight);
		float num3 = 0f;
		for (int i = 0; i < _cameraList.Count; i++)
		{
			num3 += _cameraList[i].Weight.Value;
		}
		float num4 = num3 - num2;
		if (num4 < 0f)
		{
			return;
		}
		for (int j = 0; j < _cameraList.Count; j++)
		{
			float num5 = num4 / (float)(_cameraList.Count - j);
			CameraBase cameraBase = _cameraList[j];
			cameraBase.ChainUpdate();
			if (cameraBase.Weight.Value < num5)
			{
				num4 -= cameraBase.Weight.Value;
				cameraBase.Weight.Set(0f);
				_cameraList.RemoveAt(j);
				j--;
			}
			else
			{
				cameraBase.Weight.Set(cameraBase.Weight.Value - num5);
				num4 -= num5;
			}
		}
	}

	public void SetCam(CameraBase cam, float initWeightFrame)
	{
		if (MainCamera != cam)
		{
			_cameraList.Remove(cam);
			if (MainCamera != null)
			{
				_cameraList.Add(MainCamera);
			}
			MainCamera = cam;
			MainCamera.Weight.SetByFrame(initWeightFrame);
			MainCamera.Reset();
		}
	}

	public CameraStat GetCameraInfo(CameraStat.CameraStock cameraStock)
	{
		if (MainCamera == null)
		{
			return null;
		}
		float num = MainCamera.Weight.Value;
		for (int i = 0; i < _cameraList.Count; i++)
		{
			num += _cameraList[i].Weight.Value;
		}
		if (num < 0.1f)
		{
			return null;
		}
		CameraStat cameraInfo = MainCamera.GetCameraInfo(cameraStock);
		if (cameraInfo == null)
		{
			return null;
		}
		CameraStat cameraStat = new CameraStat();
		cameraStat.WeightAdd(cameraInfo, MainCamera.Weight.Value / num);
		for (int j = 0; j < _cameraList.Count; j++)
		{
			CameraBase cameraBase = _cameraList[j];
			float weight = cameraBase.Weight.Value / num;
			CameraStat cameraInfo2 = cameraBase.GetCameraInfo(cameraStock);
			if (cameraInfo2 != null)
			{
				cameraStat.WeightAdd(cameraInfo2, weight);
			}
		}
		return cameraStat;
	}
}
