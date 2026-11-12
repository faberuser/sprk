using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct ItemEffectWidgetDataKey
{
	[Key(0)]
	public ItemData.GetEffectType EffectType;

	[Key(1)]
	public int Star;

	public ItemEffectWidgetDataKey(ItemData.GetEffectType effectType, int star)
	{
		EffectType = effectType;
		Star = star;
	}

	public bool Equals(ItemEffectWidgetDataKey other)
	{
		if (EffectType == other.EffectType)
		{
			return Star == other.Star;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(EffectType, Star);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({EffectType}, {Star})";
	}
}
