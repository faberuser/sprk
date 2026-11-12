using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PayShopDiscountGroupKeyFormatter : IMessagePackFormatter<PayShopDiscountGroupKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PayShopDiscountGroupKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.GroupIndex);
		writer.Write(value.Step);
	}

	public PayShopDiscountGroupKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int groupIndex = 0;
		int step = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				groupIndex = reader.ReadInt32();
				break;
			case 1:
				step = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		PayShopDiscountGroupKey result = new PayShopDiscountGroupKey(groupIndex, step);
		reader.Depth--;
		return result;
	}
}
