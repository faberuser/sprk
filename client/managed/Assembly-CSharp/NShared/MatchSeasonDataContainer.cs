using NShared.NJit;

namespace NShared;

public class MatchSeasonDataContainer : JitDataContainerDefault<int, MatchSeasonData>
{
	public const string TABLENAME = "MatchSeasonTable";

	public override TablePathDefine PathDefine => new TablePathDefine("MatchSeasonTable");
}
