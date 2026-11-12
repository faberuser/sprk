using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class TeamLevelBonusBuffOptionDataFormatter : IMessagePackFormatter<TeamLevelBonusBuffOptionData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, TeamLevelBonusBuffOptionData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(12);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<EventBonusType>().Serialize(ref writer, value.BonusType, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.BonusValue, options);
		resolver.GetFormatterWithVerify<CurrencyType>().Serialize(ref writer, value.ReinforcePriceType, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ReinforcePrice, options);
		resolver.GetFormatterWithVerify<TeamLevelBuffOptionConditionType>().Serialize(ref writer, value.ConditionType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ConditionValue, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ContentsOptionDescKey, options);
		writer.WriteNil();
		resolver.GetFormatterWithVerify<TeamLevelGrade>().Serialize(ref writer, value.Grade, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BonusNameKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BonusDescKey, options);
	}

	public TeamLevelBonusBuffOptionData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		TeamLevelBonusBuffOptionData teamLevelBonusBuffOptionData = new TeamLevelBonusBuffOptionData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				teamLevelBonusBuffOptionData.Index = reader.ReadInt32();
				break;
			case 1:
				teamLevelBonusBuffOptionData.BonusType = resolver.GetFormatterWithVerify<EventBonusType>().Deserialize(ref reader, options);
				break;
			case 2:
				teamLevelBonusBuffOptionData.BonusValue = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 3:
				teamLevelBonusBuffOptionData.ReinforcePriceType = resolver.GetFormatterWithVerify<CurrencyType>().Deserialize(ref reader, options);
				break;
			case 4:
				teamLevelBonusBuffOptionData.ReinforcePrice = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 5:
				teamLevelBonusBuffOptionData.ConditionType = resolver.GetFormatterWithVerify<TeamLevelBuffOptionConditionType>().Deserialize(ref reader, options);
				break;
			case 6:
				teamLevelBonusBuffOptionData.ConditionValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 7:
				teamLevelBonusBuffOptionData.ContentsOptionDescKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				teamLevelBonusBuffOptionData.Grade = resolver.GetFormatterWithVerify<TeamLevelGrade>().Deserialize(ref reader, options);
				break;
			case 10:
				teamLevelBonusBuffOptionData.BonusNameKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				teamLevelBonusBuffOptionData.BonusDescKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return teamLevelBonusBuffOptionData;
	}
}
