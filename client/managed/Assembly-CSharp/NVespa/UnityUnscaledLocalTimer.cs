namespace NVespa;

public class UnityUnscaledLocalTimer : ILocalTimer
{
	public static UnityUnscaledLocalTimer Default = new UnityUnscaledLocalTimer();

	int ILocalTimer.deltaTick => UnitySystemTimer.unscaledDeltaTick;

	int ILocalTimer.playbackSpeed => 1000;

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
