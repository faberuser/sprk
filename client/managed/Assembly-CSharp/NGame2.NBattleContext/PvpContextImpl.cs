using NGame2.NBattleField.NManager.NPlay;
using NGame2.NUI.NWindow;
using UnityEngine;

namespace NGame2.NBattleContext;

public class PvpContextImpl
{
	public const float CountdownTime = 3f;

	private float _countdownStartedTime;

	public BattleStartEffectData GetBattleStartEffectData()
	{
		return new BattleStartEffectData
		{
			ScreenEffectCode = null,
			SoundEffectCode = null
		};
	}

	public void StartCountdown()
	{
		_countdownStartedTime = Time.time;
		ScreenEffect.PlayEffect("UI.Arena.Count", null, null);
	}

	public bool CheckCountdownEnd()
	{
		if (Mathf.Approximately(_countdownStartedTime, 0f))
		{
			return false;
		}
		if (Time.time - _countdownStartedTime < 3f)
		{
			return false;
		}
		return true;
	}
}
