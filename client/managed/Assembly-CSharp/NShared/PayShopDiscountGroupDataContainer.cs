using NShared.NJit;

namespace NShared;

public class PayShopDiscountGroupDataContainer : JitDataContainerDefault<PayShopDiscountGroupKey, PayShopDiscountGroupData>
{
	public const string TABLENAME = "PayShopDiscountGroupTable";

	public override TablePathDefine PathDefine => new TablePathDefine("PayShopDiscountGroupTable");

	public PayShopDiscountGroupData GetData(int groupIndex, int step)
	{
		return GetData(new PayShopDiscountGroupKey(groupIndex, step));
	}

	public int GetLastStep(int groupIndex)
	{
		int num = 0;
		foreach (PayShopDiscountGroupData allDatum in GetAllData())
		{
			if (allDatum.GroupIndex == groupIndex && allDatum.Step > num)
			{
				num = allDatum.Step;
			}
		}
		return num;
	}
}
