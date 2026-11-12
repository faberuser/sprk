using System;
using System.Collections;
using NGame2.NScene;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class LobbyShakmehBossGauge : BaseWindow
{
	private enum GaugeState
	{
		Idle,
		Active
	}

	public enum LabelChangeState
	{
		Smooth,
		Direct,
		DelayDirect
	}

	public const string PrefabPath = "Resources/Prefab2/Window/ShakmehDungeon/LobbyShakmehBossGauge";

	[Header("[Gauge Group]")]
	public LabelChangeState LabelChangeMode;

	public float FillGaugeTime;

	public UISlider GaugeSlider;

	public UILabel Label_GaugeAmount;

	private float _currVal;

	private float _nextVal;

	private float _maxVal;

	private float _delay;

	private Action _complete;

	private GaugeState _gaugeState;

	public void Open(float currVal, float nextVal, float maxVal, float delay = 0f, Action action = null)
	{
		_currVal = currVal;
		_nextVal = nextVal;
		_maxVal = maxVal;
		_delay = delay;
		if (action != null)
		{
			_complete = action;
		}
		_gaugeState = GaugeState.Idle;
		Init();
	}

	private void Init()
	{
		Set_GaugeLabel(_currVal);
		Set_GaugeSlider(_currVal);
	}

	public void UpdateGauge(float currVal, float nextVal, float delay = 0f)
	{
		_currVal = currVal;
		_nextVal = nextVal;
		_delay = delay;
		if (_maxVal == 0f)
		{
			CurrencyTypeData data = GameRoot.Instance.CurrencyTypeDataContainer.GetData(CurrencyType.ShakmehMiddleBossPoint);
			if (data == null)
			{
				return;
			}
			_maxVal = data.MaxValue;
		}
		StartGagueAnimation();
	}

	private void Set_GaugeLabel(float val)
	{
		string text = $"{val / _maxVal * 100f:0}%";
		UIHelper.SetLabelText(Label_GaugeAmount, text);
	}

	private void Set_GaugeSlider(float val)
	{
		GaugeSlider.value = val / _maxVal;
	}

	public void StartGagueAnimation()
	{
		if (_currVal == _nextVal)
		{
			Set_GaugeLabel(_currVal);
			Set_GaugeSlider(_currVal / _maxVal);
		}
		else if (_gaugeState != GaugeState.Active)
		{
			ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, coFillGauge());
		}
	}

	private IEnumerator coFillGauge()
	{
		_gaugeState = GaugeState.Active;
		while (GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading)
		{
			yield return null;
		}
		Set_GaugeLabel(_currVal);
		Set_GaugeSlider(_currVal / _maxVal);
		yield return new WaitForSeconds(_delay);
		float elapsTime = 0f;
		if (LabelChangeMode == LabelChangeState.Direct)
		{
			Set_GaugeLabel(_nextVal);
		}
		while (true)
		{
			float num = Mathf.Lerp(_currVal, _nextVal, elapsTime / FillGaugeTime);
			Set_GaugeSlider(num);
			elapsTime += Time.deltaTime;
			if (elapsTime >= FillGaugeTime)
			{
				break;
			}
			if (LabelChangeMode == LabelChangeState.Smooth)
			{
				Set_GaugeLabel(num);
			}
			yield return null;
		}
		Set_GaugeLabel(_nextVal);
		if (_complete != null)
		{
			Vespa_System_Action_Extension_Method.fire(_complete);
		}
		_gaugeState = GaugeState.Idle;
		yield return null;
	}
}
