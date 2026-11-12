using NShared.NJit;

namespace NShared;

public class MatchBanPickStateDataContainer : JitDataContainerDefault<MatchBanPickState, MatchBanPickStateData>
{
	public const string TABLENAME = "MatchBanPickState";

	public override TablePathDefine PathDefine => new TablePathDefine("MatchBanPickState");
}
