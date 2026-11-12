namespace NVespa;

public interface ILocalTimer
{
	int deltaTick { get; }

	int playbackSpeed { get; }

	bool pause { get; }

	ETimerEventFlag TotalEventFlag { get; }

	EAudioMixerFlag TotalAudioMixerFlag { get; }

	int GetPlaybackSpeedRecursively(int depth);

	bool GetPauseRecursively(int depth);
}
