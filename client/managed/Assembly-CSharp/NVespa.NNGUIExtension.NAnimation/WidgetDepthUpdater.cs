using System.Collections.Generic;
using UnityEngine;

namespace NVespa.NNGUIExtension.NAnimation;

public class WidgetDepthUpdater : BaseUpdater<WidgetDepthData>
{
	private List<UIWidget> _widgetList = new List<UIWidget>();

	private List<int> _originWidgetDepthList = new List<int>();

	private UIWidget _widget;

	private int _depth;

	private int _depthValue;

	public override bool Setup(WidgetDepthData data, GameObject[] objs, float value, bool isReverse)
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
				UIWidget[] componentsInChildren = gameObject.GetComponentsInChildren<UIWidget>(includeInactive: true);
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(componentsInChildren))
				{
					_widgetList.AddRange(componentsInChildren);
				}
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_widgetList); j++)
		{
			_widget = _widgetList[j];
			if (_widget == null)
			{
				_originWidgetDepthList.Add(0);
			}
			else
			{
				_originWidgetDepthList.Add(_widget.depth);
			}
		}
		float ratio = GetRatio();
		float animationCurveWeight = GetAnimationCurveWeight(ratio);
		SetDepths(animationCurveWeight);
		return true;
	}

	public override void Refresh()
	{
		SetDepths(0f);
		_widgetList.Clear();
		_originWidgetDepthList.Clear();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_objs); i++)
		{
			GameObject gameObject = _objs[i];
			if (!(gameObject == null))
			{
				UIWidget[] componentsInChildren = gameObject.GetComponentsInChildren<UIWidget>(includeInactive: true);
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(componentsInChildren))
				{
					_widgetList.AddRange(componentsInChildren);
				}
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_widgetList); j++)
		{
			_widget = _widgetList[j];
			if (_widget == null)
			{
				_originWidgetDepthList.Add(0);
			}
			else
			{
				_originWidgetDepthList.Add(_widget.depth);
			}
		}
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
			SetDepths(animationCurveWeight);
		}
	}

	public override void SetEnd()
	{
		float ratio = GetRatio();
		float animationCurveWeight = GetAnimationCurveWeight(ratio);
		animationCurveWeight = Mathf.Clamp01(animationCurveWeight);
		SetDepths(Mathf.Clamp01(animationCurveWeight));
	}

	public override void Release()
	{
		SetDepths(0f);
		base.Release();
		_widgetList.Clear();
		_originWidgetDepthList.Clear();
		_widget = null;
		_depth = 0;
	}

	private void SetDepths(float weight)
	{
		_depthValue = _data.GetDepthValue(weight);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_widgetList); i++)
		{
			_widget = _widgetList[i];
			if (!(_widget == null))
			{
				_depth = _originWidgetDepthList[i] + _depthValue;
				if (_widget.depth != _depth)
				{
					_widget.depth = _depth;
				}
			}
		}
	}
}
