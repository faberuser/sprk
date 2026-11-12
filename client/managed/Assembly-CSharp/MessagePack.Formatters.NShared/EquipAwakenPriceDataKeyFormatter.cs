using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipAwakenPriceDataKeyFormatter : IMessagePackFormatter<EquipAwakenPriceDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipAwakenPriceDataKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(3);
		writer.Write(value.DetailIndex);
		writer.Write(value.Star);
		writer.Write(value.Tier);
	}

	public EquipAwakenPriceDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int detailIndex = 0;
		int star = 0;
		int tier = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				detailIndex = reader.ReadInt32();
				break;
			case 1:
				star = reader.ReadInt32();
				break;
			case 2:
				tier = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		EquipAwakenPriceDataKey result = new EquipAwakenPriceDataKey(detailIndex, star, tier);
		reader.Depth--;
		return result;
	}
}
