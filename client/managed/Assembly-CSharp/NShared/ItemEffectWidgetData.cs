using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ItemEffectWidgetData : IDataKeySelector<ItemEffectWidgetDataKey>
{
	[Key(0)]
	public ItemData.GetEffectType EffectType { get; set; }

	[Key(1)]
	public int Star { get; set; }

	[Key(2)]
	public EffectWidgetType EffectWidgetType { get; set; }

	ItemEffectWidgetDataKey IDataKeySelector<ItemEffectWidgetDataKey>.Key => new ItemEffectWidgetDataKey(EffectType, Star);

	public override string ToString()
	{
		return "EffectType=" + EffectType.ToString() + "Star" + Star;
	}
}
