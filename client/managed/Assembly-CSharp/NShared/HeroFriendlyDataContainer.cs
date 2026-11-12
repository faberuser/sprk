using NShared.NJit;

namespace NShared;

public class HeroFriendlyDataContainer : JitDataContainerDefault<int, HeroFriendlyData>
{
	public const string TABLENAME = "HeroFriendlyTable";

	public override TablePathDefine PathDefine => new TablePathDefine("HeroFriendlyTable");
}
