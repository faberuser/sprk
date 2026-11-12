using System.Collections.Generic;
using UnityEngine;

namespace NVespa.NNGUIExtension.NAnimation;

public class RotationUpdater : BaseUpdater<RotationData>
{
	private List<Transform> _transList = new List<Transform>();

	private List<Vector3> _endAngleList = new List<Vector3>();

	private Transform _trans;

	private Vector3 _endAngle;

	public override bool Setup(RotationData data, GameObject[] objs, float delay, bool isReverse)
	{
		if (!base.Setup(data, objs, delay, isReverse))
		{
			return false;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(objs); i++)
		{
			GameObject gameObject = objs[i];
			if (!(gameObject == null))
			{
				_trans = gameObject.transform;
				_endAngle = _trans.localEulerAngles;
				_transList.Add(_trans);
				_endAngleList.Add(_endAngle);
			}
		}
		float ratio = GetRatio();
		float animationCurveWeight = GetAnimationCurveWeight(ratio);
		Vector3 eulerAngles = _data.startAngle * animationCurveWeight;
		SetEulerAngles(eulerAngles);
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
			Vector3 eulerAngles = _data.startAngle * animationCurveWeight;
			SetEulerAngles(eulerAngles);
		}
	}

	public override void SetEnd()
	{
		float ratio = GetRatio();
		float animationCurveWeight = GetAnimationCurveWeight(ratio);
		animationCurveWeight = Mathf.Clamp01(animationCurveWeight);
		Vector3 eulerAngles = _data.startAngle * animationCurveWeight;
		SetEulerAngles(eulerAngles);
	}

	public override void Release()
	{
		SetEulerAngles(Vector3.zero);
		base.Release();
		_transList.Clear();
		_endAngleList.Clear();
		_trans = null;
		_endAngle = Vector3.zero;
	}

	private void SetEulerAngles(Vector3 weightAngle)
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
				_endAngle = _endAngleList[i];
				_endAngle += weightAngle;
				_trans.localEulerAngles = _endAngle;
			}
		}
	}
}
