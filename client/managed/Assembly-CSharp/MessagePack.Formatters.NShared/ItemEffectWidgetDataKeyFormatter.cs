using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ItemEffectWidgetDataKeyFormatter : IMessagePackFormatter<ItemEffectWidgetDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ItemEffectWidgetDataKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		resolver.GetFormatterWithVerify<ItemData.GetEffectType>().Serialize(ref writer, value.EffectType, options);
		writer.Write(value.Star);
	}

	public ItemEffectWidgetDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ItemData.GetEffectType effectType = ItemData.GetEffectType.Normal;
		int star = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				effectType = resolver.GetFormatterWithVerify<ItemData.GetEffectType>().Deserialize(ref reader, options);
				break;
			case 1:
				star = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		ItemEffectWidgetDataKey result = new ItemEffectWidgetDataKey(effectType, star);
		reader.Depth--;
		return result;
	}
}
