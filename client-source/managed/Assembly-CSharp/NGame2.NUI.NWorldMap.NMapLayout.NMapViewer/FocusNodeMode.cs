using System;
using NGame2.NUI.NManager;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout.NMapViewer;

public class FocusNodeMode
{
	protected float _orthoSize;

	protected Vector3 _viewPosition;

	protected Mediator _mediator;

	protected float _modeWeight;

	protected Action _onComplete;

	public bool IsPlaying { get; protected set; }

	public bool IsEnter { get; protected set; }

	protected Quaternion _viewRotation => Quaternion.identity;

	public void Open(Mediator mediator, float orthoSize, int dungeonIndex, Action onComplete)
	{
		_mediator = mediator;
		IsPlaying = true;
		IsEnter = true;
		_onComplete = onComplete;
		_orthoSize = orthoSize;
		_viewPosition = _mediator.NodeGraph.GetNodePositionByDungeonIndex(dungeonIndex);
		Rect cameraArea = GetCameraArea();
		_viewPosition.x = Mathf.Clamp(_viewPosition.x, cameraArea.xMin, cameraArea.xMax);
		_viewPosition.y = Mathf.Clamp(_viewPosition.y, cameraArea.yMin, cameraArea.yMax);
	}

	public void OpenByBuilding(Mediator mediator, float orthoSize, int buildingIndex, Action onComplete)
	{
		_mediator = mediator;
		IsPlaying = true;
		IsEnter = true;
		_onComplete = onComplete;
		_orthoSize = orthoSize;
		MapLayout instance = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
		_viewPosition = instance.BuildingManager.GetPositionByIndex(buildingIndex);
		Rect cameraArea = GetCameraArea();
		_viewPosition.x = Mathf.Clamp(_viewPosition.x, cameraArea.xMin, cameraArea.xMax);
		_viewPosition.y = Mathf.Clamp(_viewPosition.y, cameraArea.yMin, cameraArea.yMax);
	}

	public void Leave(Action onComplete)
	{
		IsPlaying = true;
		IsEnter = false;
		_onComplete = onComplete;
	}

	protected virtual float GetBlendTime()
	{
		return Vespa_Float_Extension_Method.NoneZero(GameSetting.data.WorldMap.EnterNodeMenu.TimeForCameraBlend);
	}

	public void ChainUpdate()
	{
		if (_mediator == null || !IsPlaying)
		{
			return;
		}
		float blendTime = GetBlendTime();
		if (IsEnter)
		{
			_modeWeight += Time.deltaTime / blendTime;
			if (_modeWeight >= 1f)
			{
				_modeWeight = 1f;
				Vespa_System_Action_Extension_Method.fire(_onComplete);
				_onComplete = null;
			}
		}
		else
		{
			_modeWeight -= Time.deltaTime / blendTime;
			if (_modeWeight <= 0f)
			{
				_modeWeight = 0f;
				Vespa_System_Action_Extension_Method.fire(_onComplete);
				_onComplete = null;
			}
		}
	}

	public void AdjustCameraInfo(ref float orthoSize, ref Vector3 cameraPos, ref Quaternion cameraRot)
	{
		if (!(_modeWeight < Mathf.Epsilon))
		{
			float t = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(GameSetting.data.WorldMap.EnterNodeMenu.CurveForZoom, _modeWeight, _modeWeight);
			float t2 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(GameSetting.data.WorldMap.EnterNodeMenu.CurveForPosition, _modeWeight, _modeWeight);
			float t3 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(GameSetting.data.WorldMap.EnterNodeMenu.CurveForRotation, _modeWeight, _modeWeight);
			orthoSize = Mathf.Lerp(orthoSize, _orthoSize, t);
			cameraPos = Vector3.Lerp(cameraPos, _viewPosition, t2);
			cameraRot = Quaternion.Lerp(cameraRot, _viewRotation, t3);
		}
	}

	protected Rect GetCameraArea()
	{
		float num = (float)GameUIRoot.instance.UIRoot.activeHeight * _orthoSize;
		float num2 = num * (float)Screen.width / (float)Screen.height;
		if (_mediator == null)
		{
			return default(Rect);
		}
		float num3 = _mediator.MapWidth;
		float num4 = _mediator.MapHeight;
		float num5 = Mathf.Max(0f, num3 - num2);
		float num6 = Mathf.Max(0f, num4 - num);
		return new Rect((0f - num5) * 0.5f, (0f - num6) * 0.5f, num5, num6);
	}
}
