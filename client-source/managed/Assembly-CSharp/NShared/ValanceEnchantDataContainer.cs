using NShared.NJit;

namespace NShared;

public class ValanceEnchantDataContainer : JitDataContainerDefault<ValanceEnchantDataKey, ValanceEnchantData>
{
	public const string TABLENAME = "ValanceEnchantTable";

	public override TablePathDefine PathDefine => new TablePathDefine("ValanceEnchantTable");

	public int GetReqGold(string itemCode, string partType, string subType)
	{
		return GetData(new ValanceEnchantDataKey(itemCode, partType, subType))?.ReqGold ?? 0;
	}
}
