using System;
using NGame2.NAccount;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class TeamLevelGauge : LinkFactoryComponent, InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/Reward/TeamLevelGauge";

	public static PrefabAuto<TeamLevelGauge> PrefabAuto = new PrefabAuto<TeamLevelGauge>("Resources/Prefab2/Component/Reward/TeamLevelGauge");

	public UILabel TeamLevelLabel;

	public UILabel TeamExpLabel;

	public UISprite GaugeSprite;

	public AnimationCurve GaugeCurve;

	public float GaugeSpeed = 1f;

	private GaugeAnimator _gaugeAnimator;

	private float _startTime;

	private int _startLevel;

	private Action _onEnd;

	void InitOnCreate.OnCreate()
	{
		Close();
	}

	void InitOnCreate.OnRelease()
	{
		Close();
	}

	public void Open(ExpResultInfo expResult, Action onEnd)
	{
		if (expResult != null)
		{
			Open(expResult.OldLevel, expResult.OldValue, expResult.NewLevel, expResult.NewValue, onEnd);
		}
	}

	public void Open(int currentLevel, long currentExp, int targetLevel, long targetExp, Action onEnd)
	{
		float expPercentage = GetExpPercentage(currentLevel, currentExp);
		float expPercentage2 = GetExpPercentage(targetLevel, targetExp);
		UIHelper.SetLabelText(TeamLevelLabel, currentLevel);
		UIHelper.SetLabelText(TeamExpLabel, ConvertRatio(expPercentage));
		bool isMaxLevel = Helper.GetTeamLevelInfo(targetLevel, (int)targetExp).IsMaxLevel;
		_gaugeAnimator = new GaugeAnimator();
		_gaugeAnimator.SetCurve(GaugeCurve);
		_gaugeAnimator.SetLabel(TeamExpLabel, ConvertRatio);
		_gaugeAnimator.SetGaugeSprite(new UISprite[1] { GaugeSprite });
		_gaugeAnimator.Init(expPercentage, (float)((targetLevel - currentLevel) * 100) + expPercentage2, isMaxLevel);
		_startTime = Time.time;
		_onEnd = onEnd;
		_startLevel = currentLevel;
	}

	public void OpenMax(int currentLevel)
	{
		UIHelper.SetLabelText(TeamLevelLabel, currentLevel);
		UIHelper.SetLabelText(TeamExpLabel, ConvertRatio(0f));
		UIHelper.SetSpriteFillAmount(GaugeSprite, 0f);
	}

	private float GetExpPercentage(int level, long exp)
	{
		Helper.LevelInfo teamLevelInfo = Helper.GetTeamLevelInfo(level, (int)exp);
		if (teamLevelInfo.IsMaxLevel)
		{
			return 100f;
		}
		return Mathf.Min(Mathf.Max(teamLevelInfo.ExpPercent * 100f, 0f), 100f);
	}

	private string ConvertRatio(float ratio)
	{
		return Vespa_Float_Extension_Method.ToExpString(ratio % 100f);
	}

	private void Close()
	{
		_gaugeAnimator = null;
		_startTime = 0f;
	}

	private void Update()
	{
		if (_gaugeAnimator != null)
		{
			float ratio = (Time.time - _startTime) * GaugeSpeed;
			_gaugeAnimator.Update(ratio);
			float currentPercentage = _gaugeAnimator.CurrentPercentage;
			int num = _startLevel + (int)(currentPercentage / 100f);
			UIHelper.SetLabelText(TeamLevelLabel, num);
			if (_gaugeAnimator.IsCompleted)
			{
				_gaugeAnimator = null;
				Vespa_System_Action_Extension_Method.fire(_onEnd);
			}
		}
	}
}
