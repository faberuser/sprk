using NShared.NJit;

namespace NShared;

public class GachaSelectShopEffectDataContainer : JitDataContainerDefault<GachaSelectShopEffectDataKey, GachaSelectShopEffectData>
{
	public const string TABLENAME = "GachaSelectShopEffectTable";

	public override TablePathDefine PathDefine => new TablePathDefine("GachaSelectShopEffectTable");

	public bool IsRareItem(string itemGroupCode, string itemCode, int count)
	{
		GachaSelectShopEffectData data = GetData(new GachaSelectShopEffectDataKey(itemGroupCode, itemCode));
		if (data == null)
		{
			return false;
		}
		return data.ItemCountMin <= count;
	}
}
