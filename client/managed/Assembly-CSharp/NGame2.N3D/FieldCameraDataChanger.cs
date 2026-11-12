using NGameAsset.NBattleFieldMap2;
using NVespa;
using UnityEngine;

namespace NGame2.N3D;

public class FieldCameraDataChanger
{
	private CameraStat.CameraStock _prevCameraStock;

	private CameraStat.CameraStock _cachedStock;

	private FieldCameraData _cachedCameraData;

	private float _weight;

	public bool NeedUpdate { get; private set; }

	public void ChangeFieldCameraData(string customFieldCameraDataName, bool immediately)
	{
		if (String_Extension_Method.IsNullOrEmpty(customFieldCameraDataName) && String_Extension_Method.IsNullOrEmpty(CameraStat.Basic.NextCameraName))
		{
			return;
		}
		NeedUpdate = !immediately;
		if (NeedUpdate)
		{
			_prevCameraStock = CameraStat.Basic;
			_weight = 0f;
			if (_cachedCameraData == null)
			{
				_cachedCameraData = new FieldCameraData();
				_cachedCameraData.FitToField = false;
			}
			if (_cachedStock == null)
			{
				_cachedStock = new CameraStat.CameraStock(_cachedCameraData);
			}
		}
		string text = ((!String_Extension_Method.IsNullOrEmpty(customFieldCameraDataName)) ? customFieldCameraDataName : CameraStat.Basic.NextCameraName);
		CameraStat.ResetCameraStock(String_Extension_Method.IsNullOrEmpty(text) ? null : GameSetting.data.BattleView.GetBattleCamera(text));
	}

	public void ChangeFieldCameraData(FieldCameraData fieldCameraData, bool immediately)
	{
		if (fieldCameraData == null)
		{
			return;
		}
		NeedUpdate = !immediately;
		if (NeedUpdate)
		{
			_prevCameraStock = CameraStat.Basic;
			_weight = 0f;
			if (_cachedCameraData == null)
			{
				_cachedCameraData = new FieldCameraData();
				_cachedCameraData.FitToField = false;
			}
			if (_cachedStock == null)
			{
				_cachedStock = new CameraStat.CameraStock(_cachedCameraData);
			}
		}
		CameraStat.ResetCameraStock(fieldCameraData);
	}

	public CameraStat.CameraStock UpdateChangeFieldCameraData()
	{
		if (!NeedUpdate)
		{
			return CameraStat.Basic;
		}
		_weight += (float)GlobalTimer.DefaultLocal.deltaTick * 0.001f;
		if (_weight >= 1f)
		{
			_prevCameraStock = null;
			NeedUpdate = false;
			_weight = 1f;
			return CameraStat.Basic;
		}
		return LerpCameraStock(_prevCameraStock, CameraStat.Basic, _weight);
	}

	private CameraStat.CameraStock LerpCameraStock(CameraStat.CameraStock from, CameraStat.CameraStock to, float weight)
	{
		_cachedCameraData.FieldOfView = Mathf.LerpUnclamped(from.FieldOfView, to.FieldOfView, weight);
		_cachedCameraData.HorizonAngle = Mathf.LerpUnclamped(from.HorizonAngle, to.HorizonAngle, weight);
		_cachedCameraData.LookAngle = Mathf.LerpUnclamped(from.LookAngle, to.LookAngle, weight);
		_cachedCameraData.MinHeight = Mathf.LerpUnclamped(from.MinHeight, to.MinHeight, weight);
		_cachedCameraData.BoundsMargin = Vector3.LerpUnclamped(from.BoundsMargin, to.BoundsMargin, weight);
		_cachedCameraData.BoundsOffset = Vector3.LerpUnclamped(from.BoundsOffset, to.BoundsOffset, weight);
		_cachedCameraData.FitToField = to.FitToField;
		Quaternion cameraRotation = Quaternion.LerpUnclamped(from.CameraRotation, to.CameraRotation, weight);
		_cachedStock.ResetCameraValues(cameraRotation);
		return _cachedStock;
	}
}
