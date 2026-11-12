using NShared.NJit;

namespace NShared;

public class LuckyArenaDeckPresetDataContainer : JitDataContainerDefault<int, LuckyArenaDeckPresetData>
{
	public const string TABLENAME = "LuckyArenaDeckPresetTable";

	public override TablePathDefine PathDefine => new TablePathDefine("LuckyArenaDeckPresetTable");
}
