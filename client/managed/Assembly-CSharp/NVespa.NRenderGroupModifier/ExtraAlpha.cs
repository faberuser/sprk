using UnityEngine;

namespace NVespa.NRenderGroupModifier;

public class ExtraAlpha
{
	public const float AnimateSpeed = 10f;

	private float _valueByTimer;

	public float MinExtraAlpha => GameSharedSetting.data.Value.GetTimerEvent().ExtraAlpha;

	public float MinExtraAlphaBattleField => GameSharedSetting.data.Value.GetTimerEvent().ExtraAlphaBattleField;

	public float Final => Value * _valueByTimer;

	public float Value { get; set; }

	public ExtraAlpha()
	{
		Reset();
	}

	public void Reset()
	{
		Value = 1f;
		_valueByTimer = 1f;
	}

	public void Update(ILocalTimer localTimer, bool isBattleField = false)
	{
		switch (localTimer.TOTALEVENTFLAG().FlagMax())
		{
		case ETimerEventFlag.SkillSoloPause:
			_valueByTimer -= GlobalTimer.deltaTime * 10f;
			if (isBattleField)
			{
				_valueByTimer = Mathf.Max(_valueByTimer, MinExtraAlphaBattleField);
			}
			else
			{
				_valueByTimer = Mathf.Max(_valueByTimer, MinExtraAlpha);
			}
			break;
		case ETimerEventFlag.BossIntroPause:
			_valueByTimer -= GlobalTimer.deltaTime * 10f;
			_valueByTimer = Mathf.Max(_valueByTimer, 0f);
			break;
		default:
			_valueByTimer += GlobalTimer.deltaTime * 10f;
			_valueByTimer = Mathf.Min(_valueByTimer, 1f);
			break;
		}
		_valueByTimer = Mathf.Clamp01(_valueByTimer);
	}
}
