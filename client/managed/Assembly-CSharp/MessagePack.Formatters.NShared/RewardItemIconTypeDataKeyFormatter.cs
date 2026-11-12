using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class RewardItemIconTypeDataKeyFormatter : IMessagePackFormatter<RewardItemIconTypeDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, RewardItemIconTypeDataKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		resolver.GetFormatterWithVerify<RewardItemIconType>().Serialize(ref writer, value.IconType, options);
		resolver.GetFormatterWithVerify<CurrencyType>().Serialize(ref writer, value.CurrencyType, options);
	}

	public RewardItemIconTypeDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		RewardItemIconType iconType = RewardItemIconType.Currency;
		CurrencyType currencyType = CurrencyType.None;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				iconType = resolver.GetFormatterWithVerify<RewardItemIconType>().Deserialize(ref reader, options);
				break;
			case 1:
				currencyType = resolver.GetFormatterWithVerify<CurrencyType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		RewardItemIconTypeDataKey result = new RewardItemIconTypeDataKey(iconType, currencyType);
		reader.Depth--;
		return result;
	}
}
