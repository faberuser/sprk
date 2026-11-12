using System;
using NShared;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout.NMapViewer;

public class NodeMenuMode
{
	private Mediator _mediator;

	private EntryMenuSettingData _menuAppearance;

	private bool _isPlaying;

	private bool _isEnter;

	private float _modeWeight;

	private Action _onComplete;

	public float ModeWeight { get; private set; }

	private float _orthoSize => _menuAppearance.CameraZoomScale;

	private Vector3 _viewPosition => _mediator.Avatar.GetLocalPosition() + new Vector3(_menuAppearance.CameraOffsetX, _menuAppearance.CameraOffsetY, _menuAppearance.CameraOffsetZ);

	private Quaternion _viewRotation => Quaternion.LookRotation(new Vector3(_menuAppearance.CameraDirectionX, _menuAppearance.CameraDirectionY, _menuAppearance.CameraDirectionZ));

	public void Open(Mediator mediator)
	{
		_mediator = mediator;
	}

	public void Enter(EntryMenuSettingData menuAppearance, bool skipBlend, Action onComplete)
	{
		_menuAppearance = menuAppearance;
		_isPlaying = true;
		_isEnter = true;
		ModeWeight = (skipBlend ? 1f : 0f);
		_onComplete = onComplete;
	}

	public void Leave(Action onComplete)
	{
		if (!_isEnter)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return;
		}
		_isPlaying = true;
		_isEnter = false;
		_onComplete = onComplete;
	}

	public void ChainUpdate()
	{
		if (_mediator == null || !_isPlaying)
		{
			return;
		}
		float num = Vespa_Float_Extension_Method.NoneZero(GameSetting.data.WorldMap.EnterNodeMenu.TimeForCameraBlend);
		if (_isEnter)
		{
			ModeWeight += Time.deltaTime / num;
			if (ModeWeight >= 1f)
			{
				ModeWeight = 1f;
				Vespa_System_Action_Extension_Method.fire(_onComplete);
				_onComplete = null;
			}
		}
		else
		{
			ModeWeight -= Time.deltaTime / num;
			if (ModeWeight <= 0f)
			{
				ModeWeight = 0f;
				Vespa_System_Action_Extension_Method.fire(_onComplete);
				_onComplete = null;
			}
		}
	}

	public void AdjustCameraInfo(ref float orthoSize, ref Vector3 cameraPos, ref Quaternion cameraRot)
	{
		if (!(ModeWeight < Mathf.Epsilon))
		{
			float t = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(GameSetting.data.WorldMap.EnterNodeMenu.CurveForZoom, ModeWeight, ModeWeight);
			float t2 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(GameSetting.data.WorldMap.EnterNodeMenu.CurveForPosition, ModeWeight, ModeWeight);
			float t3 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(GameSetting.data.WorldMap.EnterNodeMenu.CurveForRotation, ModeWeight, ModeWeight);
			orthoSize = Mathf.Lerp(orthoSize, _orthoSize, t);
			cameraPos = Vector3.Lerp(cameraPos, _viewPosition, t2);
			cameraRot = Quaternion.Lerp(cameraRot, _viewRotation, t3);
		}
	}
}
