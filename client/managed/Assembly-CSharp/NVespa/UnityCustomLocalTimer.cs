using System;

namespace NVespa;

public class UnityCustomLocalTimer : ILocalTimer
{
	public Func<int> PlaybackSpeedFunc;

	int ILocalTimer.deltaTick => UnitySystemTimer.deltaTick;

	int ILocalTimer.playbackSpeed
	{
		get
		{
			if (PlaybackSpeedFunc == null)
			{
				return 1000;
			}
			return Vespa_System_Action_Extension_Method.fire(PlaybackSpeedFunc);
		}
	}

	bool ILocalTimer.pause => false;

	ETimerEventFlag ILocalTimer.TotalEventFlag => (ETimerEventFlag)0;

	EAudioMixerFlag ILocalTimer.TotalAudioMixerFlag => EAudioMixerFlag.None;

	int ILocalTimer.GetPlaybackSpeedRecursively(int depth)
	{
		return 1000;
	}

	bool ILocalTimer.GetPauseRecursively(int depth)
	{
		return false;
	}
}
