using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PunishmentRuneDataContainer_PunishmentRuneKeyFormatter : IMessagePackFormatter<PunishmentRuneDataContainer.PunishmentRuneKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PunishmentRuneDataContainer.PunishmentRuneKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.VoidRuneIndex);
		writer.Write(value.LegendRuneIndex);
	}

	public PunishmentRuneDataContainer.PunishmentRuneKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int voidRuneIndex = 0;
		int legendRuneIndex = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				voidRuneIndex = reader.ReadInt32();
				break;
			case 1:
				legendRuneIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		PunishmentRuneDataContainer.PunishmentRuneKey result = new PunishmentRuneDataContainer.PunishmentRuneKey(voidRuneIndex, legendRuneIndex);
		reader.Depth--;
		return result;
	}
}
