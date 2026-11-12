using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ItemEffectWidgetDataFormatter : IMessagePackFormatter<ItemEffectWidgetData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ItemEffectWidgetData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		resolver.GetFormatterWithVerify<ItemData.GetEffectType>().Serialize(ref writer, value.EffectType, options);
		writer.Write(value.Star);
		resolver.GetFormatterWithVerify<EffectWidgetType>().Serialize(ref writer, value.EffectWidgetType, options);
	}

	public ItemEffectWidgetData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ItemEffectWidgetData itemEffectWidgetData = new ItemEffectWidgetData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				itemEffectWidgetData.EffectType = resolver.GetFormatterWithVerify<ItemData.GetEffectType>().Deserialize(ref reader, options);
				break;
			case 1:
				itemEffectWidgetData.Star = reader.ReadInt32();
				break;
			case 2:
				itemEffectWidgetData.EffectWidgetType = resolver.GetFormatterWithVerify<EffectWidgetType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return itemEffectWidgetData;
	}
}
