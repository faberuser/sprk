using System.Collections.Generic;
using UnityEngine;

namespace NVespa.NNGUIExtension.NAnimation;

public class AlphaUpdater : BaseUpdater<AlphaData>
{
	private List<UIWidget> _widgetList = new List<UIWidget>();

	private List<float> _originAlphaList = new List<float>();

	private UIWidget _widget;

	private float _finalAlpha;

	private bool _allowZero;

	public override bool Setup(AlphaData data, GameObject[] objs, float value, bool isReverse)
	{
		if (!base.Setup(data, objs, value, isReverse))
		{
			return false;
		}
		_allowZero = data.AllowAlphaZero;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(objs); i++)
		{
			GameObject gameObject = objs[i];
			if (!(gameObject == null))
			{
				_widget = SingletonAuto<LinkedTransform<UIWidget>>.instance.Get(gameObject.transform);
				if (!(_widget == null))
				{
					_widgetList.Add(_widget);
					_originAlphaList.Add(_widget.alpha);
				}
			}
		}
		float ratio = GetRatio();
		float animationCurveWeight = GetAnimationCurveWeight(ratio);
		SetAlphas(animationCurveWeight);
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
			SetAlphas(animationCurveWeight);
		}
	}

	public override void SetEnd()
	{
		float ratio = GetRatio();
		float animationCurveWeight = GetAnimationCurveWeight(ratio);
		animationCurveWeight = Mathf.Clamp01(animationCurveWeight);
		SetAlphas(Mathf.Clamp01(animationCurveWeight));
	}

	public override void Release()
	{
		SetAlphas(1f);
		base.Release();
		_widgetList.Clear();
		_originAlphaList.Clear();
		_widget = null;
		_finalAlpha = 0f;
	}

	private void SetAlphas(float weight)
	{
		if (!IsValid())
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_widgetList); i++)
		{
			_widget = _widgetList[i];
			if (!(_widget == null))
			{
				_finalAlpha = _originAlphaList[i];
				_finalAlpha *= weight;
				SetAlpha(_finalAlpha);
			}
		}
	}

	private void SetAlpha(float alpha)
	{
		if (alpha == 0f && !_allowZero)
		{
			alpha = 0.01f;
		}
		_widget.alpha = alpha;
	}
}
