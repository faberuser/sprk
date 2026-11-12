using System.Linq;
using NShared.NJit;

namespace NShared;

public class PurchaseMarketingDataContainer : JitDataContainerDefault<int, PurchaseMarketingData>
{
	public const string TABLENAME = "PurchaseMarketingTable";

	public override TablePathDefine PathDefine => new TablePathDefine("PurchaseMarketingTable");

	public PurchaseMarketingData GetPurchaseMarketingData(int productIndex)
	{
		return GetAllData().FirstOrDefault((PurchaseMarketingData v) => v.ProductIndex == productIndex);
	}
}
