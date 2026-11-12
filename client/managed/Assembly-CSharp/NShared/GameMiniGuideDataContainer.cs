using NShared.NJit;

namespace NShared;

public class GameMiniGuideDataContainer : JitDataContainerDefault<int, GameMiniGuideData>
{
	public const string TABLENAME = "GameMiniGuideTable";

	public override TablePathDefine PathDefine => new TablePathDefine("GameMiniGuideTable");
}
