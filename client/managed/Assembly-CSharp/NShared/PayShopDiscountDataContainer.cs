using NShared.NJit;

namespace NShared;

public class PayShopDiscountDataContainer : JitDataContainerDefault<int, PayShopDiscountData>
{
	public const string TABLENAME = "PayShopDiscountTable";

	public override TablePathDefine PathDefine => new TablePathDefine("PayShopDiscountTable");
}
