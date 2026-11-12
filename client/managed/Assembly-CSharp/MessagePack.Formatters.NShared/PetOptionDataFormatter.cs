using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PetOptionDataFormatter : IMessagePackFormatter<PetOptionData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PetOptionData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(11);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<OperationType>().Serialize(ref writer, value.OptionType, options);
		writer.Write(value.OptionValue0);
		writer.Write(value.OptionValue1);
		writer.Write(value.OptionValue2);
		writer.Write(value.OptionValue3);
		writer.Write(value.OptionValue4);
		writer.Write(value.OptionValue5);
		resolver.GetFormatterWithVerify<PetOptionConditionType>().Serialize(ref writer, value.ConditionType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ConditionValue, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ContentsOptionDescKey, options);
	}

	public PetOptionData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PetOptionData petOptionData = new PetOptionData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				petOptionData.Index = reader.ReadInt32();
				break;
			case 1:
				petOptionData.OptionType = resolver.GetFormatterWithVerify<OperationType>().Deserialize(ref reader, options);
				break;
			case 2:
				petOptionData.OptionValue0 = reader.ReadInt32();
				break;
			case 3:
				petOptionData.OptionValue1 = reader.ReadInt32();
				break;
			case 4:
				petOptionData.OptionValue2 = reader.ReadInt32();
				break;
			case 5:
				petOptionData.OptionValue3 = reader.ReadInt32();
				break;
			case 6:
				petOptionData.OptionValue4 = reader.ReadInt32();
				break;
			case 7:
				petOptionData.OptionValue5 = reader.ReadInt32();
				break;
			case 8:
				petOptionData.ConditionType = resolver.GetFormatterWithVerify<PetOptionConditionType>().Deserialize(ref reader, options);
				break;
			case 9:
				petOptionData.ConditionValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 10:
				petOptionData.ContentsOptionDescKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return petOptionData;
	}
}
