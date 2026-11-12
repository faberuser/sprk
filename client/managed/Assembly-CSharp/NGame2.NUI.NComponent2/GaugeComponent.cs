using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class GaugeComponent : MonoBehaviour
{
	public interface IListener
	{
		void ResetGauge(int next);
	}

	public UIProgressBar CurrentValueProgressBar;

	public UIProgressBar AddValueProgressBar;

	public UILabel GaugeValueLabel;

	public UILabel AddValueLabel;

	private IListener _iListener;

	private Coroutine _coroutIneIncreaseAnimation;

	private int _currentValue;

	private int _addValue;

	private int _maxValue;

	public int AddValue => _addValue;

	public void Init(int currentValue, int maxValue)
	{
		_currentValue = currentValue;
		_maxValue = maxValue;
		_addValue = 0;
		CurrentValueProgressBar.value = 0f;
		AddValueProgressBar.value = 0f;
		SetCurrentGauge();
		SetAddValueGauge(_addValue);
	}

	public void SetCurrentGauge()
	{
		if (!(CurrentValueProgressBar == null))
		{
			CurrentValueProgressBar.value = (float)_currentValue / (float)_maxValue;
			SetGaugeValueLabel(_currentValue, _maxValue);
		}
	}

	public void SetAddValueGauge(int value)
	{
		if (!(AddValueProgressBar == null))
		{
			_addValue += value;
			AddValueProgressBar.value = (float)(_addValue + _currentValue) / (float)_maxValue;
			SetGaugeValueLabel(_addValue + _currentValue, _maxValue);
			SetAddValueLabel();
		}
	}

	private void SetAddValueLabel()
	{
		if (_addValue == 0)
		{
			UIHelper.SetLabelText(AddValueLabel, "");
		}
		else
		{
			UIHelper.SetLabelText(AddValueLabel, "+" + _addValue);
		}
	}

	public void SetGaugeValueLabel(int value, int maXValue)
	{
		if (!(GaugeValueLabel == null))
		{
			string text = $"{value} / {maXValue}";
			UIHelper.SetLabelText(GaugeValueLabel, text);
		}
	}
}
