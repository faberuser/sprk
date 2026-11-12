using NShared.NJit;

namespace NShared;

public class OrdealArenaRewardRatingDataContainer : JitDataContainerDefault<int, OrdealArenaRewardRatingData>
{
	public const string TABLENAME = "OrdealArenaRewardRatingTable";

	public override TablePathDefine PathDefine => new TablePathDefine("OrdealArenaRewardRatingTable");
}
