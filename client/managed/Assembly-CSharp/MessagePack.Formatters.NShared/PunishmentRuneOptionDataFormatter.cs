using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PunishmentRuneOptionDataFormatter : IMessagePackFormatter<PunishmentRuneOptionData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PunishmentRuneOptionData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<StatType[]>().Serialize(ref writer, value.StatTypes, options);
		writer.Write(value.OptionValueMin0);
		writer.Write(value.OptionValueMax0);
	}

	public PunishmentRuneOptionData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PunishmentRuneOptionData punishmentRuneOptionData = new PunishmentRuneOptionData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				punishmentRuneOptionData.Index = reader.ReadInt32();
				break;
			case 1:
				punishmentRuneOptionData.StatTypes = resolver.GetFormatterWithVerify<StatType[]>().Deserialize(ref reader, options);
				break;
			case 2:
				punishmentRuneOptionData.OptionValueMin0 = reader.ReadInt32();
				break;
			case 3:
				punishmentRuneOptionData.OptionValueMax0 = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return punishmentRuneOptionData;
	}
}
