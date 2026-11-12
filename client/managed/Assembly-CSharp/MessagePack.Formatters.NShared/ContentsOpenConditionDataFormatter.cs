using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ContentsOpenConditionDataFormatter : IMessagePackFormatter<ContentsOpenConditionData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ContentsOpenConditionData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(10);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<ContentsType>().Serialize(ref writer, value.ContentsType, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ConditionValue, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.EntryMenuCondition, options);
		writer.Write(value.OpenConditionMainMission);
		writer.Write(value.OpenText);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.OpenTextResorce, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.OpenLocalKey, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.OpenLocalKeyVariable, options);
		writer.Write(value.OpenConditionSubQuest);
	}

	public ContentsOpenConditionData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ContentsOpenConditionData contentsOpenConditionData = new ContentsOpenConditionData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				contentsOpenConditionData.Index = reader.ReadInt32();
				break;
			case 1:
				contentsOpenConditionData.ContentsType = resolver.GetFormatterWithVerify<ContentsType>().Deserialize(ref reader, options);
				break;
			case 2:
				contentsOpenConditionData.ConditionValue = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 3:
				contentsOpenConditionData.EntryMenuCondition = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				contentsOpenConditionData.OpenConditionMainMission = reader.ReadInt32();
				break;
			case 5:
				contentsOpenConditionData.OpenText = reader.ReadBoolean();
				break;
			case 6:
				contentsOpenConditionData.OpenTextResorce = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				contentsOpenConditionData.OpenLocalKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				contentsOpenConditionData.OpenLocalKeyVariable = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 9:
				contentsOpenConditionData.OpenConditionSubQuest = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return contentsOpenConditionData;
	}
}
