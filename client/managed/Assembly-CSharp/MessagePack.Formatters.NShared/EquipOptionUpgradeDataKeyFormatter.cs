using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipOptionUpgradeDataKeyFormatter : IMessagePackFormatter<EquipOptionUpgradeDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipOptionUpgradeDataKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.Tier);
		writer.Write(value.DetailIndex);
	}

	public EquipOptionUpgradeDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int tier = 0;
		int detailIndex = 0;
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
			default:
				reader.Skip();
				break;
			}
		}
		EquipOptionUpgradeDataKey result = new EquipOptionUpgradeDataKey(tier, detailIndex);
		reader.Depth--;
		return result;
	}
}
