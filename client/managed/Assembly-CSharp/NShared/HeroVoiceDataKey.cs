using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct HeroVoiceDataKey
{
	[Key(0)]
	public int HeroIndex;

	[Key(1)]
	public string VoiceCode;

	public HeroVoiceDataKey(int heroIndex, string voiceCode)
	{
		HeroIndex = heroIndex;
		VoiceCode = voiceCode;
	}

	public bool Equals(HeroVoiceDataKey other)
	{
		if (HeroIndex == other.HeroIndex)
		{
			return VoiceCode == other.VoiceCode;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(HeroIndex, VoiceCode);
	}

	public override string ToString()
	{
		return $"HeroIndex={HeroIndex} VoiceCode={VoiceCode}";
	}
}
