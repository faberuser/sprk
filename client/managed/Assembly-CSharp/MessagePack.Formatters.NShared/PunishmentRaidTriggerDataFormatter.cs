using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PunishmentRaidTriggerDataFormatter : IMessagePackFormatter<PunishmentRaidTriggerData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PunishmentRaidTriggerData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		writer.Write(value.TriggerIndex);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Condition, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.AffixIndices, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.IconColor, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.IconName, options);
	}

	public PunishmentRaidTriggerData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PunishmentRaidTriggerData punishmentRaidTriggerData = new PunishmentRaidTriggerData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				punishmentRaidTriggerData.TriggerIndex = reader.ReadInt32();
				break;
			case 1:
				punishmentRaidTriggerData.Condition = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 2:
				punishmentRaidTriggerData.AffixIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 3:
				punishmentRaidTriggerData.IconColor = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				punishmentRaidTriggerData.IconName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return punishmentRaidTriggerData;
	}
}
