using NShared.NJit;

namespace NShared;

public class MatchBanPickSeasonDataContainer : JitDataContainerDefault<int, MatchSeasonData>
{
	public const string TABLENAME = "MatchBanPickSeasonTable";

	public override TablePathDefine PathDefine => new TablePathDefine("MatchBanPickSeasonTable");
}
