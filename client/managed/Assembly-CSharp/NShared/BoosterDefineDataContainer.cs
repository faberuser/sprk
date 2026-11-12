using NShared.NJit;

namespace NShared;

public class BoosterDefineDataContainer : JitDataContainerDefault<string, BoosterDefineData>
{
	public const string TABLENAME = "BoosterDefineTable";

	public override TablePathDefine PathDefine => new TablePathDefine("BoosterDefineTable");
}
