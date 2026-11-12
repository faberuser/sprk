using NShared.NJit;

namespace NShared;

public class EquipAwakenPriceDataContainer : JitDataContainerDefault<EquipAwakenPriceDataKey, EquipAwakenPriceData>
{
	public const string TABLENAME = "EquipAwakenPriceTable";

	public override TablePathDefine PathDefine => new TablePathDefine("EquipAwakenPriceTable");

	public int GetReqGold(int detailIndex, int star, int tier)
	{
		return GetData(new EquipAwakenPriceDataKey(detailIndex, star, tier))?.ReqGold ?? 0;
	}
}
