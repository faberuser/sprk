using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class KarmaGaugeExponentialKeyFormatter : IMessagePackFormatter<KarmaGaugeExponentialKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, KarmaGaugeExponentialKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.PunishmentGroupIndex);
		writer.Write(value.Wave);
	}

	public KarmaGaugeExponentialKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int punishmentGroupIndex = 0;
		int wave = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				punishmentGroupIndex = reader.ReadInt32();
				break;
			case 1:
				wave = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		KarmaGaugeExponentialKey result = new KarmaGaugeExponentialKey(punishmentGroupIndex, wave);
		reader.Depth--;
		return result;
	}
}
