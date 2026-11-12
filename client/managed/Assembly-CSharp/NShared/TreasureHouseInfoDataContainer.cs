using NShared.NJit;

namespace NShared;

public class TreasureHouseInfoDataContainer : JitDataContainerDefault<int, TreasureHouseInfoData>
{
	public const string TABLENAME = "TreasureHouseInfoTable";

	public override TablePathDefine PathDefine => new TablePathDefine("TreasureHouseInfoTable");
}
