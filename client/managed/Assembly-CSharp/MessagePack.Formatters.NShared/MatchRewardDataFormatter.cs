using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MatchRewardDataFormatter : IMessagePackFormatter<MatchRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MatchRewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(14);
		writer.Write(value.TierIndex);
		writer.Write(value.MinRank);
		writer.Write(value.MaxRank);
		writer.Write(value.DailyReward);
		writer.Write(value.SeasonReward);
		writer.Write(value.FirstReward);
		writer.Write(value.ShowMinRank);
		writer.Write(value.ShowMaxRank);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DailyRewardExpItemCode, options);
		writer.Write(value.DailyRewardExpItemCount);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.WeeklyRewardExpItemCode, options);
		writer.Write(value.WeeklyRewardExpItemCount);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SeasonRewardExpItemCode, options);
		writer.Write(value.SeasonRewardExpItemCount);
	}

	public MatchRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		MatchRewardData matchRewardData = new MatchRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				matchRewardData.TierIndex = reader.ReadInt32();
				break;
			case 1:
				matchRewardData.MinRank = reader.ReadInt32();
				break;
			case 2:
				matchRewardData.MaxRank = reader.ReadInt32();
				break;
			case 3:
				matchRewardData.DailyReward = reader.ReadInt32();
				break;
			case 4:
				matchRewardData.SeasonReward = reader.ReadInt32();
				break;
			case 5:
				matchRewardData.FirstReward = reader.ReadInt32();
				break;
			case 6:
				matchRewardData.ShowMinRank = reader.ReadInt32();
				break;
			case 7:
				matchRewardData.ShowMaxRank = reader.ReadInt32();
				break;
			case 8:
				matchRewardData.DailyRewardExpItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				matchRewardData.DailyRewardExpItemCount = reader.ReadInt32();
				break;
			case 10:
				matchRewardData.WeeklyRewardExpItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				matchRewardData.WeeklyRewardExpItemCount = reader.ReadInt32();
				break;
			case 12:
				matchRewardData.SeasonRewardExpItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 13:
				matchRewardData.SeasonRewardExpItemCount = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return matchRewardData;
	}
}
