using NShared.NJit;

namespace NShared;

public class ChallengeRaidDataContainer : JitDataContainerDefault<int, ChallengeRaidData>
{
	public const string TABLENAME = "ChallengeRaidTable";

	public override TablePathDefine PathDefine => new TablePathDefine("ChallengeRaidTable");
}
