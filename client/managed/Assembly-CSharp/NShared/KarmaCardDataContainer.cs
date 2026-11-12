using NShared.NJit;

namespace NShared;

public class KarmaCardDataContainer : JitDataContainerDefault<int, KarmaCardData>
{
	public const string TABLENAME = "KarmaCardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("KarmaCardTable");
}
