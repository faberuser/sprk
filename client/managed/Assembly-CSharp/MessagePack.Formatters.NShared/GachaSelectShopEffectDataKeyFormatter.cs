using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GachaSelectShopEffectDataKeyFormatter : IMessagePackFormatter<GachaSelectShopEffectDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GachaSelectShopEffectDataKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemGroupCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
	}

	public GachaSelectShopEffectDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		string itemGroupCode = null;
		string itemCode = null;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				itemGroupCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				itemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		GachaSelectShopEffectDataKey result = new GachaSelectShopEffectDataKey(itemGroupCode, itemCode);
		reader.Depth--;
		return result;
	}
}
