using NShared.NJit;

namespace NShared;

public class KingsPassSeasonDataContainer : JitDataContainerDefault<int, KingsPassSeasonData>
{
	public const string TABLENAME = "KingsPassSeasonTable";

	public override TablePathDefine PathDefine => new TablePathDefine("KingsPassSeasonTable");
}
