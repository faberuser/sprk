namespace NVespa;

public interface IAudioVolumeListener
{
	float ConvertVolume(float volume);

	bool CheckConvertVolume(EAudioMixerFlag audioMixerFlag);
}
