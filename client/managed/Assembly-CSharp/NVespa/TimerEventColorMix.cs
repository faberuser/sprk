using NVespa.NGameSharedSetting;
using UnityEngine;

namespace NVespa;

public class TimerEventColorMix
{
	public float AnimateSpeed = 10f;

	public bool IsBattleField;

	private float _effectAmount;

	public float MaxColorEffect
	{
		get
		{
			ValueData.TimerEventData timerEvent = GameSharedSetting.data.Value.GetTimerEvent();
			if (!IsBattleField)
			{
				return timerEvent.ColorEffect;
			}
			return timerEvent.FieldColorEffect;
		}
	}

	public static Color SoloColor => GameSharedSetting.data.Value.GetTimerEvent().SoloColor;

	public void Reset()
	{
		_effectAmount = 0f;
	}

	public Color UpdateAndCalc(ILocalTimer localTimer)
	{
		return UpdateAndCalc(localTimer, Colors.zero);
	}

	public Color UpdateAndCalc(ILocalTimer localTimer, Color colorMix)
	{
		Update(localTimer);
		return Calculate(colorMix);
	}

	public void Update(ILocalTimer localTimer)
	{
		switch (localTimer.TOTALEVENTFLAG().FlagMax())
		{
		case ETimerEventFlag.SkillSoloPause:
			_effectAmount += GlobalTimer.deltaTime * AnimateSpeed;
			_effectAmount = Mathf.Min(MaxColorEffect, _effectAmount);
			break;
		case ETimerEventFlag.BossIntroPause:
			_effectAmount += GlobalTimer.deltaTime * AnimateSpeed;
			_effectAmount = Mathf.Min(1f, _effectAmount);
			break;
		default:
			_effectAmount -= GlobalTimer.deltaTime * AnimateSpeed;
			_effectAmount = Mathf.Max(0f, _effectAmount);
			break;
		}
		_effectAmount = Mathf.Clamp01(_effectAmount);
	}

	public Color Calculate(Color colorMix)
	{
		return Color.Lerp(colorMix, SoloColor, _effectAmount);
	}
}
