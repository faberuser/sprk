using NShared.NJit;

namespace NShared;

public class OrdealArenaBuffDataContainer : JitDataContainerDefault<int, OrdealArenaBuffData>
{
	public const string TABLENAME = "OrdealArenaBuffTable";

	public override TablePathDefine PathDefine => new TablePathDefine("OrdealArenaBuffTable");
}
