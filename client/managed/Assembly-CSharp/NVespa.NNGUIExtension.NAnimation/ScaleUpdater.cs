using System.Collections.Generic;
using UnityEngine;

namespace NVespa.NNGUIExtension.NAnimation;

public class ScaleUpdater : BaseUpdater<ScaleData>
{
	private List<Transform> _transList = new List<Transform>();

	private List<Vector3> _endScaleList = new List<Vector3>();

	private Transform _trans;

	private Vector3 _endScale;

	public override bool Setup(ScaleData data, GameObject[] objs, float value, bool isReverse)
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
				_endScale = _trans.localScale;
				_transList.Add(_trans);
				_endScaleList.Add(_endScale);
			}
		}
		float ratio = GetRatio();
		float animationCurveWeight = GetAnimationCurveWeight(ratio);
		Vector3 scales = data.ScaleOffset * animationCurveWeight;
		SetScales(scales);
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
			Vector3 scales = _data.ScaleOffset * animationCurveWeight;
			SetScales(scales);
		}
	}

	public override void SetEnd()
	{
		float ratio = GetRatio();
		float animationCurveWeight = GetAnimationCurveWeight(ratio);
		animationCurveWeight = Mathf.Clamp01(animationCurveWeight);
		Vector3 scales = _data.ScaleOffset * animationCurveWeight;
		SetScales(scales);
	}

	public override void Release()
	{
		SetScales(Vector3.zero);
		base.Release();
		_transList.Clear();
		_endScaleList.Clear();
		_trans = null;
		_endScale = Vector3.zero;
	}

	private void SetScales(Vector3 weightScale)
	{
		if (!IsValid())
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_endScaleList); i++)
		{
			_trans = _transList[i];
			if (!(_trans == null))
			{
				_endScale = _endScaleList[i];
				_endScale += weightScale;
				SetScale(_endScale);
			}
		}
	}

	private void SetScale(Vector3 scale)
	{
		if (scale.x == 0f)
		{
			scale.x = 0.01f;
		}
		if (scale.y == 0f)
		{
			scale.y = 0.01f;
		}
		_trans.localScale = scale;
	}
}
