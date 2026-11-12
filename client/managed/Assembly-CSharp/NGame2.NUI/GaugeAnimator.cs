using System;
using System.Collections.Generic;
using NVespa;
using UnityEngine;

namespace NGame2.NUI;

public class GaugeAnimator
{
	private float _startPercentage;

	private float _targetPercentage;

	private float _currentPercentage;

	private int _startValue;

	private int _currentValue;

	private int _targetValue;

	private bool _isMaxTarget;

	private AnimationCurve _curve;

	private UISprite[] _gauges;

	private UILabel _displayLabel;

	private UILabel _displayLabel2;

	private UIButton _displayMaxButton;

	private Func<float, string> _displayFormat;

	private Func<int, string> _displayFormat2;

	private List<Action<float>> _CurPercentageActionList = new List<Action<float>>();

	private bool _isInitialized;

	private bool _isCompleted;

	private float _elapsedTime;

	public float StartPercentage => _startPercentage;

	public float TargetPercentage => _targetPercentage;

	public float CurrentPercentage => _currentPercentage;

	public bool IsCompleted => _isCompleted;

	public float TimeLength { get; set; }

	public int StartValue => _startValue;

	public int CurrentValue => _currentValue;

	public int TargetValue => _targetValue;

	public void Init(float startPercentage, float targetPercentage, bool isMaxTarget)
	{
		_startPercentage = startPercentage;
		_targetPercentage = targetPercentage;
		_currentPercentage = startPercentage;
		_isMaxTarget = isMaxTarget;
		_isCompleted = false;
		_elapsedTime = 0f;
		_CurPercentageActionList.Clear();
		_isInitialized = true;
	}

	public void InitDisplayValue(int startValue, int targetValue)
	{
		_startValue = startValue;
		_targetValue = targetValue;
		_currentValue = startValue;
	}

	public bool IsInitialized()
	{
		return _isInitialized;
	}

	public void SetCurve(AnimationCurve curve)
	{
		_curve = curve;
	}

	public void SetGaugeSprite(UISprite[] gauges)
	{
		_gauges = gauges;
		UpdateGauge();
	}

	public void SetLabel(UILabel label, Func<float, string> format)
	{
		_displayLabel = label;
		_displayFormat = format;
	}

	public void SetLabel2(UILabel label, Func<int, string> format)
	{
		_displayLabel2 = label;
		_displayFormat2 = format;
	}

	public void SetMaxButton(UIButton maxButton)
	{
		_displayMaxButton = maxButton;
	}

	public void AddCurPercentageActions(Action<float>[] onCurPercentageChanged)
	{
		if (onCurPercentageChanged != null)
		{
			_CurPercentageActionList.AddRange(onCurPercentageChanged);
		}
	}

	public void Update(float ratio)
	{
		if (!_isCompleted)
		{
			ratio = Mathf.Clamp01(ratio);
			float t = ((_curve != null) ? _curve.Evaluate(ratio) : ratio);
			_currentPercentage = Mathf.Lerp(_startPercentage, _targetPercentage, t);
			_isCompleted = Mathf.Abs(_currentPercentage - _targetPercentage) < Mathf.Epsilon;
			UpdateLabel();
			UpdateGauge();
			DoCurPercentageAction();
		}
	}

	public void Update()
	{
		if (!_isCompleted && _isInitialized)
		{
			_elapsedTime += (float)UnitySystemTimer.unscaledDeltaTick * 0.001f;
			float t = Mathf.Clamp01(_elapsedTime / TimeLength);
			_currentPercentage = Mathf.Lerp(_startPercentage, _targetPercentage, t);
			_currentValue = Vespa_Int_Extension_Method.Lerp(_startValue, _targetValue, t);
			_isCompleted = Mathf.Abs(_currentPercentage - _targetPercentage) < Mathf.Epsilon;
			UpdateLabel();
			UpdateGauge();
		}
	}

	private void DoCurPercentageAction()
	{
		_CurPercentageActionList.ForEach(delegate(Action<float> action)
		{
			Vespa_System_Action_Extension_Method.fire(action, _currentPercentage);
		});
	}

	private void UpdateLabel()
	{
		UpdatePercentLabel();
		UpdateDisplayLabel();
	}

	private void UpdatePercentLabel()
	{
		if (!(_displayLabel == null) && _displayFormat != null)
		{
			UIHelper.SetLabelText(_displayLabel, _displayFormat(_currentPercentage));
		}
	}

	private void UpdateDisplayLabel()
	{
		if (!(_displayLabel2 == null) && _displayFormat2 != null)
		{
			UIHelper.SetLabelText(_displayLabel2, _displayFormat2(_currentValue));
		}
	}

	private void UpdateGauge()
	{
		int num = (int)(_currentPercentage / 100f);
		bool flag = _isCompleted && _isMaxTarget && num > 0;
		if (flag)
		{
			num--;
		}
		int num2 = Mathf.Min(num, __Extension_Method_ICollection__.COUNT(_gauges) - 1);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_gauges); i++)
		{
			if (i < num2)
			{
				UIHelper.SetSpriteFillAmount(_gauges[i], 1f);
				continue;
			}
			if (i > num2)
			{
				UIHelper.SetSpriteFillAmount(_gauges[i], 0f);
				continue;
			}
			float num3 = _currentPercentage % 100f;
			if (flag)
			{
				num3 = 100f;
				UIHelper.EnableButton(_displayMaxButton, enable: false);
			}
			UIHelper.SetSpriteFillAmount(_gauges[i], num3 / 100f);
		}
	}
}
