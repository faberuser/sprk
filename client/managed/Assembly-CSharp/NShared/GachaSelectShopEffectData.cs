using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class GachaSelectShopEffectData : IDataKeySelector<GachaSelectShopEffectDataKey>
{
	[Key(0)]
	public string ItemGroupCode { get; set; }

	[Key(1)]
	public string ItemCode { get; set; }

	[Key(2)]
	public int ItemCountMin { get; set; }

	GachaSelectShopEffectDataKey IDataKeySelector<GachaSelectShopEffectDataKey>.Key => new GachaSelectShopEffectDataKey(ItemGroupCode, ItemCode);
}
