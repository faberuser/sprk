using NVespa;

public static class Vespa_ILocalTimer_ExtensionMethod
{
	public static ETimerEventFlag TOTALEVENTFLAG(this ILocalTimer localTimer)
	{
		return localTimer?.TotalEventFlag ?? MainTimer.EventFlag;
	}

	public static EAudioMixerFlag AUDIOMIXERFLAG(this ILocalTimer localTimer)
	{
		return localTimer?.TotalAudioMixerFlag ?? EAudioMixerFlag.None;
	}

	public static ETimerEventFlag FlagMax(this ETimerEventFlag flag)
	{
		if (flag == (ETimerEventFlag)0)
		{
			return (ETimerEventFlag)0;
		}
		for (int num = TimerEventAttribute._dic.Count - 1; num >= 0; num--)
		{
			ETimerEventFlag keyByIndex = TimerEventAttribute._dic.GetKeyByIndex(num);
			if ((flag & keyByIndex) != 0)
			{
				return keyByIndex;
			}
		}
		return (ETimerEventFlag)0;
	}

	public static ETimerEventFlag PauseFlagMax(this ETimerEventFlag flag)
	{
		if (flag == (ETimerEventFlag)0)
		{
			return (ETimerEventFlag)0;
		}
		for (int num = TimerEventAttribute._dic.Count - 1; num >= 0; num--)
		{
			ETimerEventFlag keyByIndex = TimerEventAttribute._dic.GetKeyByIndex(num);
			if ((flag & keyByIndex) != 0 && TimerEventAttribute._dic.GetValueByIndex(num).PauseType != TimerEventAttribute.EPauseType.None)
			{
				return keyByIndex;
			}
		}
		return (ETimerEventFlag)0;
	}

	public static bool IsEventPause(this ETimerEventFlag flag)
	{
		if (flag == (ETimerEventFlag)0)
		{
			return false;
		}
		for (int num = TimerEventAttribute._dic.Count - 1; num >= 0; num--)
		{
			ETimerEventFlag keyByIndex = TimerEventAttribute._dic.GetKeyByIndex(num);
			if ((flag & keyByIndex) != 0)
			{
				TimerEventAttribute valueByIndex = TimerEventAttribute._dic.GetValueByIndex(num);
				if (valueByIndex.PauseType != TimerEventAttribute.EPauseType.None)
				{
					return valueByIndex.PauseType == TimerEventAttribute.EPauseType.Pause;
				}
			}
		}
		return false;
	}
}
