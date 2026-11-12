using NShared.NJit;

namespace NShared;

public class ValanceEnchantOptionCountDataContainer : JitDataContainerDefault<ValanceEnchantOptionCountDataKey, ValanceEnchantOptionCountData>
{
	public const string TABLENAME = "ValanceEnchantOptionCountTable";

	public override TablePathDefine PathDefine => new TablePathDefine("ValanceEnchantOptionCountTable");

	public int GetMinOptionCount(string itemCode)
	{
		int num = 4;
		foreach (ValanceEnchantOptionCountData allDatum in GetAllData())
		{
			if (allDatum.ItemCode == itemCode && allDatum.OptionCount < num)
			{
				num = allDatum.OptionCount;
			}
		}
		return num;
	}

	public int GetMaxOptionCount(string itemCode)
	{
		int num = 1;
		foreach (ValanceEnchantOptionCountData allDatum in GetAllData())
		{
			if (allDatum.ItemCode == itemCode && allDatum.OptionCount > num)
			{
				num = allDatum.OptionCount;
			}
		}
		return num;
	}
}
