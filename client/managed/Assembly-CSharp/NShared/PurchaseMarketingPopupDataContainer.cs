using System.Linq;
using NShared.NJit;

namespace NShared;

public class PurchaseMarketingPopupDataContainer : JitDataContainerDefault<int, PurchaseMarketingPopupData>
{
	public const string TABLENAME = "PurchaseMarketingPopupTable";

	public override TablePathDefine PathDefine => new TablePathDefine("PurchaseMarketingPopupTable");

	public PurchaseMarketingPopupData GetPurchaseMarketingPopupData(int purchaseIndex)
	{
		return (from v in GetAllData()?.Where((PurchaseMarketingPopupData v) => v.PurchaseIndices != null)
			where TypeIterator.Contains(v.PurchaseIndices, purchaseIndex)
			select v).FirstOrDefault();
	}

	public PurchaseMarketingPopupData[] GetPurchaseMarketingPopupDatas(MarketingType type)
	{
		return GetAllData()?.Where((PurchaseMarketingPopupData v) => v.Type == type).ToArray();
	}
}
