using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class KarmaGaugeExponentialDataFormatter : IMessagePackFormatter<KarmaGaugeExponentialData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, KarmaGaugeExponentialData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(3);
		writer.Write(value.PunishmentGroupIndex);
		writer.Write(value.Wave);
		writer.Write(value.ExponentialValue);
	}

	public KarmaGaugeExponentialData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		KarmaGaugeExponentialData karmaGaugeExponentialData = new KarmaGaugeExponentialData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				karmaGaugeExponentialData.PunishmentGroupIndex = reader.ReadInt32();
				break;
			case 1:
				karmaGaugeExponentialData.Wave = reader.ReadInt32();
				break;
			case 2:
				karmaGaugeExponentialData.ExponentialValue = reader.ReadSingle();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return karmaGaugeExponentialData;
	}
}
