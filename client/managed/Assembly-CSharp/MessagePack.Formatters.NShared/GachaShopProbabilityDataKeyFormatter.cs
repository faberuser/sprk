using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GachaShopProbabilityDataKeyFormatter : IMessagePackFormatter<GachaShopProbabilityDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GachaShopProbabilityDataKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		writer.Write(value.GachaIndex);
		resolver.GetFormatterWithVerify<HighGachaCategoryType>().Serialize(ref writer, value.GachaType, options);
	}

	public GachaShopProbabilityDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		int gachaIndex = 0;
		HighGachaCategoryType gachaType = HighGachaCategoryType.None;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				gachaIndex = reader.ReadInt32();
				break;
			case 1:
				gachaType = resolver.GetFormatterWithVerify<HighGachaCategoryType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		GachaShopProbabilityDataKey result = new GachaShopProbabilityDataKey(gachaIndex, gachaType);
		reader.Depth--;
		return result;
	}
}
