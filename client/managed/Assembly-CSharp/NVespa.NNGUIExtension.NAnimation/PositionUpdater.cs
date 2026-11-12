using System.Collections.Generic;
using UnityEngine;

namespace NVespa.NNGUIExtension.NAnimation;

public class PositionUpdater : BaseUpdater<PositionData>
{
	private List<Transform> _transList = new List<Transform>();

	private List<Vector3> _endPosList = new List<Vector3>();

	private Transform _trans;

	private Vector3 _endPos;

	public override bool Setup(PositionData data, GameObject[] objs, float value, bool isReverse)
	{
		if (!base.Setup(data, objs, value, isReverse))
		{
			return false;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(objs); i++)
		{
			GameObject gameObject = objs[i];
			if (!(gameObject == null))
			{
				_trans = gameObject.transform;
				_endPos = (_data.IsWorldPosition ? _trans.position : _trans.localPosition);
				_transList.Add(_trans);
				_endPosList.Add(_endPos);
			}
		}
		float ratio = GetRatio();
		float animationCurveWeight = GetAnimationCurveWeight(ratio);
		Vector3 positions = _data.PositionOffset * animationCurveWeight;
		SetPositions(positions);
		return true;
	}

	public override void OnUpdate(float delta)
	{
		if (CanUpdateAnimation(delta))
		{
			if (IsEnd())
			{
				SetEnd();
				return;
			}
			float ratio = GetRatio();
			float animationCurveWeight = GetAnimationCurveWeight(ratio);
			Vector3 positions = _data.PositionOffset * animationCurveWeight;
			SetPositions(positions);
		}
	}

	public override void SetEnd()
	{
		float ratio = GetRatio();
		float animationCurveWeight = GetAnimationCurveWeight(ratio);
		animationCurveWeight = Mathf.Clamp01(animationCurveWeight);
		Vector3 positions = _data.PositionOffset * animationCurveWeight;
		SetPositions(positions);
	}

	public override void Release()
	{
		SetPositions(Vector3.zero);
		base.Release();
		_transList.Clear();
		_endPosList.Clear();
		_trans = null;
		_endPos = Vector3.zero;
	}

	private void SetPositions(Vector3 weightPos)
	{
		if (!IsValid())
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_transList); i++)
		{
			_trans = _transList[i];
			if (!(_trans == null))
			{
				_endPos = _endPosList[i];
				_endPos += weightPos;
				SetPosition(_endPos);
			}
		}
	}

	protected void SetPosition(Vector3 position)
	{
		if (_data.IsWorldPosition)
		{
			_trans.position = position;
		}
		else
		{
			_trans.localPosition = position;
		}
	}
}
