using NShared.NJit;

namespace NShared;

public class HeroVoiceDataContainer : JitDataContainerDefault<HeroVoiceDataKey, HeroVoiceData>
{
	public const string TABLENAME = "HeroVoiceTable";

	public override TablePathDefine PathDefine => new TablePathDefine("HeroVoiceTable");

	public HeroVoiceData GetData(int heroIndex, string voiceCode)
	{
		return GetData(new HeroVoiceDataKey(heroIndex, voiceCode));
	}
}
