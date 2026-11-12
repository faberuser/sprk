using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class HeroVoiceData : IDataKeySelector<HeroVoiceDataKey>
{
	[Key(0)]
	public int HeroIndex { get; set; }

	[Key(1)]
	public string VoiceNameText { get; set; }

	[Key(2)]
	public string VoiceCode { get; set; }

	HeroVoiceDataKey IDataKeySelector<HeroVoiceDataKey>.Key => new HeroVoiceDataKey(HeroIndex, VoiceCode);
}
