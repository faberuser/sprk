namespace NVespa;

public class UnityLocalTimer : ILocalTimer
{
	public static UnityLocalTimer Default = new UnityLocalTimer();

	int ILocalTimer.deltaTick => UnitySystemTimer.deltaTick;

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
