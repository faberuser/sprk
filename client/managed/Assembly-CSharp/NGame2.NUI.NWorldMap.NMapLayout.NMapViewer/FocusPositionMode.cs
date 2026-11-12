using System;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout.NMapViewer;

public class FocusPositionMode : FocusNodeMode
{
	protected float _blendTime;

	public void Open(Mediator mediator, float orthoSize, Vector2 postion, float blendTime, Action onComplete)
	{
		_mediator = mediator;
		base.IsPlaying = true;
		base.IsEnter = true;
		_onComplete = onComplete;
		_blendTime = blendTime;
		_orthoSize = orthoSize;
		_viewPosition = Vespa_Unity_Vector2_Extension_Method.ToVec3(postion);
		Rect cameraArea = GetCameraArea();
		_viewPosition.x = Mathf.Clamp(_viewPosition.x, cameraArea.xMin, cameraArea.xMax);
		_viewPosition.y = Mathf.Clamp(_viewPosition.y, cameraArea.yMin, cameraArea.yMax);
	}

	protected override float GetBlendTime()
	{
		return _blendTime;
	}
}
