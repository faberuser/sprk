using NShared.NJit;

namespace NShared;

public class HonoraryChamberMenuDataContainer : JitDataContainerDefault<int, HonoraryChamberMenuData>
{
	public const string TABLENAME = "HonoraryChamberMenuTable";

	public override TablePathDefine PathDefine => new TablePathDefine("HonoraryChamberMenuTable");
}
