using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CreatureTranscendOptionDataFormatter : IMessagePackFormatter<CreatureTranscendOptionData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CreatureTranscendOptionData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(10);
		writer.Write(value.TranscendLevel);
		writer.Write(value.SlotIndex);
		writer.Write(value.ReqTranscendPoint);
		writer.Write(value.MaxLevel);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.IconPath, options);
		resolver.GetFormatterWithVerify<OperationType>().Serialize(ref writer, value.OptionType1, options);
		writer.Write(value.OptionValue1);
		resolver.GetFormatterWithVerify<OperationType>().Serialize(ref writer, value.OptionType2, options);
		writer.Write(value.OptionValue2);
		writer.Write(value.LegacySkillIndex);
	}

	public CreatureTranscendOptionData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CreatureTranscendOptionData creatureTranscendOptionData = new CreatureTranscendOptionData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				creatureTranscendOptionData.TranscendLevel = reader.ReadInt32();
				break;
			case 1:
				creatureTranscendOptionData.SlotIndex = reader.ReadInt32();
				break;
			case 2:
				creatureTranscendOptionData.ReqTranscendPoint = reader.ReadInt32();
				break;
			case 3:
				creatureTranscendOptionData.MaxLevel = reader.ReadInt32();
				break;
			case 4:
				creatureTranscendOptionData.IconPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				creatureTranscendOptionData.OptionType1 = resolver.GetFormatterWithVerify<OperationType>().Deserialize(ref reader, options);
				break;
			case 6:
				creatureTranscendOptionData.OptionValue1 = reader.ReadInt32();
				break;
			case 7:
				creatureTranscendOptionData.OptionType2 = resolver.GetFormatterWithVerify<OperationType>().Deserialize(ref reader, options);
				break;
			case 8:
				creatureTranscendOptionData.OptionValue2 = reader.ReadInt32();
				break;
			case 9:
				creatureTranscendOptionData.LegacySkillIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return creatureTranscendOptionData;
	}
}
