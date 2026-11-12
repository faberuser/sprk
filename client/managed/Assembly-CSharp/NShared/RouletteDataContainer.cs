using NShared.NJit;

namespace NShared;

public class RouletteDataContainer : JitDataContainerDefault<int, RouletteData>
{
	public const string TABLENAME = "RouletteTable";

	public override TablePathDefine PathDefine => new TablePathDefine("RouletteTable");
}
