using NShared.NJit;

namespace NShared;

public class ShopDataContainer : JitDataContainerDefault<int, ShopData>
{
	public const string TABLENAME = "ShopTable";

	public override TablePathDefine PathDefine => new TablePathDefine("ShopTable");

	public ShopData GetData(string code)
	{
		foreach (ShopData allDatum in GetAllData())
		{
			if (!string.IsNullOrEmpty(allDatum.Code) && allDatum.Code == code)
			{
				return allDatum;
			}
		}
		return null;
	}
}
