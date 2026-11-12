using System;
using System.Collections;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class ShakmehGaugeComponent : LinkFactoryComponent
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

	public const string PrefabPath = "Prefab2/Component/ShakmehDungeon/ShakmehGaugeComponent";

	public static PrefabAuto<ShakmehGaugeComponent> PrefabAuto = new PrefabAuto<ShakmehGaugeComponent>("Prefab2/Component/ShakmehDungeon/ShakmehGaugeComponent");

	[Header("[Gauge Controller]")]
	public float FillGaugeTime;

	public LabelChangeState LabelChangeMode;

	[Header("[Gauge Group]")]
	public UISlider GaugeSlider;

	public UILabel Label_GaugeAmount;

	public UILabel Label_ExpectedGaugeAmount;

	public UISprite ExpectedGauageSprite;

	private float _currVal;

	private float _nextVal;

	private float _maxVal;

	private float _delay;

	private Action _complete;

	private GaugeState _gaugeState;

	public void Open(float currVal, float nextVal, float delay, bool autoStart, Action action = null)
	{
		_currVal = currVal;
		_nextVal = nextVal;
		_delay = delay;
		CurrencyTypeData data = GameRoot.Instance.CurrencyTypeDataContainer.GetData(CurrencyType.ShakmehMiddleBossPoint);
		if (data != null)
		{
			_maxVal = data.MaxValue;
			if (action != null)
			{
				_complete = action;
			}
			_gaugeState = GaugeState.Idle;
			Init();
			if (autoStart)
			{
				StartGagueAnimation();
			}
		}
	}

	private void Init()
	{
		Set_GaugeLabel(_currVal);
		Set_GaugeSlider(0f);
		SetExpectedGauage(0f);
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

	public void SetExpectedGauage(float val)
	{
		if (val == 0f || _currVal == val)
		{
			UIHelper.EnableGameObject(Label_ExpectedGaugeAmount, enable: false);
			UIHelper.EnableGameObject(ExpectedGauageSprite, enable: false);
			return;
		}
		UIHelper.SetLabelText(Label_GaugeAmount, "");
		UIHelper.EnableGameObject(Label_ExpectedGaugeAmount, enable: true);
		UIHelper.EnableGameObject(ExpectedGauageSprite, enable: true);
		string text = $"{val / _maxVal * 100f:0}%";
		UIHelper.SetLabelText(Label_ExpectedGaugeAmount, text);
		ExpectedGauageSprite.fillAmount = val / _maxVal;
	}

	public void StartGagueAnimation()
	{
		if (_currVal == _nextVal)
		{
			Set_GaugeLabel(_currVal);
			Set_GaugeSlider(_currVal);
		}
		else if (_gaugeState != GaugeState.Active)
		{
			ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, coFillGauge());
		}
	}

	private IEnumerator coFillGauge()
	{
		_gaugeState = GaugeState.Active;
		Set_GaugeLabel(_currVal);
		Set_GaugeSlider(0f);
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
