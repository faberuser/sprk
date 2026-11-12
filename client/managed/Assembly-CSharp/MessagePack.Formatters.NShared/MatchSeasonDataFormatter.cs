using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MatchSeasonDataFormatter : IMessagePackFormatter<MatchSeasonData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MatchSeasonData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(16);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Name, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Begin, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.End, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Description, options);
		writer.Write(value.SeasonIndex);
		resolver.GetFormatterWithVerify<MatchArenaType>().Serialize(ref writer, value.ArenaType, options);
		resolver.GetFormatterWithVerify<MatchSeasonType>().Serialize(ref writer, value.MatchSeasonType, options);
		writer.Write(value.IsTakeReward);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PlayTimeBegin, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PlayTimeEnd, options);
		writer.Write(value.LimitedHeroStar);
		writer.Write(value.LimitedHeroLevel);
		writer.Write(value.LimitedHeroCount);
		writer.Write(value.ActiveTier);
		writer.Write(value.ShowSeasonIndex);
	}

	public MatchSeasonData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		MatchSeasonData matchSeasonData = new MatchSeasonData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				matchSeasonData.Index = reader.ReadInt32();
				break;
			case 1:
				matchSeasonData.Name = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				matchSeasonData.Begin = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				matchSeasonData.End = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				matchSeasonData.Description = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				matchSeasonData.SeasonIndex = reader.ReadInt32();
				break;
			case 6:
				matchSeasonData.ArenaType = resolver.GetFormatterWithVerify<MatchArenaType>().Deserialize(ref reader, options);
				break;
			case 7:
				matchSeasonData.MatchSeasonType = resolver.GetFormatterWithVerify<MatchSeasonType>().Deserialize(ref reader, options);
				break;
			case 8:
				matchSeasonData.IsTakeReward = reader.ReadBoolean();
				break;
			case 9:
				matchSeasonData.PlayTimeBegin = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				matchSeasonData.PlayTimeEnd = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				matchSeasonData.LimitedHeroStar = reader.ReadInt32();
				break;
			case 12:
				matchSeasonData.LimitedHeroLevel = reader.ReadInt32();
				break;
			case 13:
				matchSeasonData.LimitedHeroCount = reader.ReadInt32();
				break;
			case 14:
				matchSeasonData.ActiveTier = reader.ReadBoolean();
				break;
			case 15:
				matchSeasonData.ShowSeasonIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return matchSeasonData;
	}
}
