using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MonthlyHeroDataFormatter : IMessagePackFormatter<MonthlyHeroData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MonthlyHeroData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(9);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.OpenDate, options);
		resolver.GetFormatterWithVerify<CalendarCutSceneType>().Serialize(ref writer, value.Type, options);
		writer.Write(value.HeroIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.CutScenePath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.IllustPath, options);
		writer.Write(value.RewardIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Title, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Desc, options);
	}

	public MonthlyHeroData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		MonthlyHeroData monthlyHeroData = new MonthlyHeroData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				monthlyHeroData.Index = reader.ReadInt32();
				break;
			case 1:
				monthlyHeroData.OpenDate = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				monthlyHeroData.Type = resolver.GetFormatterWithVerify<CalendarCutSceneType>().Deserialize(ref reader, options);
				break;
			case 3:
				monthlyHeroData.HeroIndex = reader.ReadInt32();
				break;
			case 4:
				monthlyHeroData.CutScenePath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				monthlyHeroData.IllustPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				monthlyHeroData.RewardIndex = reader.ReadInt32();
				break;
			case 7:
				monthlyHeroData.Title = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				monthlyHeroData.Desc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return monthlyHeroData;
	}
}
