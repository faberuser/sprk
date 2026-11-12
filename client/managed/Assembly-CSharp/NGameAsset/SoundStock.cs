using NVespa;
using UnityEngine;

namespace NGameAsset;

public struct SoundStock
{
	public string Code;

	public int AddVolume;

	public ILocalTimer LocalTimer;

	public EAudioMixerFlag MixerFlag;

	public SystemLanguage Language;
}
