using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipEnchantableConditionKeyFormatter : IMessagePackFormatter<EquipEnchantableConditionKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipEnchantableConditionKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.TierIndex);
		writer.Write(value.DetailIndex);
	}

	public EquipEnchantableConditionKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int tierIndex = 0;
		int detailIndex = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				tierIndex = reader.ReadInt32();
				break;
			case 1:
				detailIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		EquipEnchantableConditionKey result = new EquipEnchantableConditionKey(tierIndex, detailIndex);
		reader.Depth--;
		return result;
	}
}
