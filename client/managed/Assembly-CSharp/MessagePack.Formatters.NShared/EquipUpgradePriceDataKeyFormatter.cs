using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipUpgradePriceDataKeyFormatter : IMessagePackFormatter<EquipUpgradePriceDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipUpgradePriceDataKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(3);
		writer.Write(value.Tier);
		writer.Write(value.DetailIndex);
		writer.Write(value.Level);
	}

	public EquipUpgradePriceDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int tier = 0;
		int detailIndex = 0;
		int level = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				tier = reader.ReadInt32();
				break;
			case 1:
				detailIndex = reader.ReadInt32();
				break;
			case 2:
				level = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		EquipUpgradePriceDataKey result = new EquipUpgradePriceDataKey(tier, detailIndex, level);
		reader.Depth--;
		return result;
	}
}
