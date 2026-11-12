using NShared.NJit;

namespace NShared;

public class GachaShopProbabilityDataContainer : JitDataContainerDefault<GachaShopProbabilityDataKey, GachaShopProbabilityData>
{
	public const string TABLENAME = "GachaShopProbabilityTable";

	public override TablePathDefine PathDefine => new TablePathDefine("GachaShopProbabilityTable");

	public GachaShopProbabilityData GetData(int gachaIndex, HighGachaCategoryType gachaType)
	{
		return GetData(new GachaShopProbabilityDataKey(gachaIndex, gachaType));
	}
}
