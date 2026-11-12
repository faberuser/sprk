using NShared.NJit;

namespace NShared;

public class ItemEffectWidgetDataContainer : JitDataContainerDefault<ItemEffectWidgetDataKey, ItemEffectWidgetData>
{
	public const string TABLENAME = "ItemEffectWidgetTable";

	public override TablePathDefine PathDefine => new TablePathDefine("ItemEffectWidgetTable");

	public ItemEffectWidgetData GetData(ItemData.GetEffectType effectType, int star)
	{
		return GetData(new ItemEffectWidgetDataKey(effectType, star));
	}
}
