using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class TeamLevelBuffOptionDataFormatter : IMessagePackFormatter<TeamLevelBuffOptionData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, TeamLevelBuffOptionData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(9);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<OperationType>().Serialize(ref writer, value.OptionType, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.OptionValue, options);
		resolver.GetFormatterWithVerify<CurrencyType>().Serialize(ref writer, value.ReinforcePriceType, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ReinforcePrice, options);
		resolver.GetFormatterWithVerify<TeamLevelBuffOptionConditionType>().Serialize(ref writer, value.ConditionType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ConditionValue, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ContentsOptionDescKey, options);
		resolver.GetFormatterWithVerify<TeamLevelGrade>().Serialize(ref writer, value.Grade, options);
	}

	public TeamLevelBuffOptionData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		TeamLevelBuffOptionData teamLevelBuffOptionData = new TeamLevelBuffOptionData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				teamLevelBuffOptionData.Index = reader.ReadInt32();
				break;
			case 1:
				teamLevelBuffOptionData.OptionType = resolver.GetFormatterWithVerify<OperationType>().Deserialize(ref reader, options);
				break;
			case 2:
				teamLevelBuffOptionData.OptionValue = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 3:
				teamLevelBuffOptionData.ReinforcePriceType = resolver.GetFormatterWithVerify<CurrencyType>().Deserialize(ref reader, options);
				break;
			case 4:
				teamLevelBuffOptionData.ReinforcePrice = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 5:
				teamLevelBuffOptionData.ConditionType = resolver.GetFormatterWithVerify<TeamLevelBuffOptionConditionType>().Deserialize(ref reader, options);
				break;
			case 6:
				teamLevelBuffOptionData.ConditionValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 7:
				teamLevelBuffOptionData.ContentsOptionDescKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				teamLevelBuffOptionData.Grade = resolver.GetFormatterWithVerify<TeamLevelGrade>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return teamLevelBuffOptionData;
	}
}
