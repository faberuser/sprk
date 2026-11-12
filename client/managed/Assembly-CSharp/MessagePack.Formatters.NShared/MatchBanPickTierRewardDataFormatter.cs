using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MatchBanPickTierRewardDataFormatter : IMessagePackFormatter<MatchBanPickTierRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MatchBanPickTierRewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(20);
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
		writer.Write(value.WinReward);
		writer.Write(value.AttendanceReward);
		writer.Write(value.WeeklyReward);
		writer.Write(value.WeeklyBoxReward);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ServerBuff, options);
		writer.Write(value.ShowRewardIndex);
	}

	public MatchBanPickTierRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		MatchBanPickTierRewardData matchBanPickTierRewardData = new MatchBanPickTierRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				matchBanPickTierRewardData.TierIndex = reader.ReadInt32();
				break;
			case 1:
				matchBanPickTierRewardData.MinRank = reader.ReadInt32();
				break;
			case 2:
				matchBanPickTierRewardData.MaxRank = reader.ReadInt32();
				break;
			case 3:
				matchBanPickTierRewardData.DailyReward = reader.ReadInt32();
				break;
			case 4:
				matchBanPickTierRewardData.SeasonReward = reader.ReadInt32();
				break;
			case 5:
				matchBanPickTierRewardData.FirstReward = reader.ReadInt32();
				break;
			case 6:
				matchBanPickTierRewardData.ShowMinRank = reader.ReadInt32();
				break;
			case 7:
				matchBanPickTierRewardData.ShowMaxRank = reader.ReadInt32();
				break;
			case 8:
				matchBanPickTierRewardData.DailyRewardExpItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				matchBanPickTierRewardData.DailyRewardExpItemCount = reader.ReadInt32();
				break;
			case 10:
				matchBanPickTierRewardData.WeeklyRewardExpItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				matchBanPickTierRewardData.WeeklyRewardExpItemCount = reader.ReadInt32();
				break;
			case 12:
				matchBanPickTierRewardData.SeasonRewardExpItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 13:
				matchBanPickTierRewardData.SeasonRewardExpItemCount = reader.ReadInt32();
				break;
			case 14:
				matchBanPickTierRewardData.WinReward = reader.ReadInt32();
				break;
			case 15:
				matchBanPickTierRewardData.AttendanceReward = reader.ReadInt32();
				break;
			case 16:
				matchBanPickTierRewardData.WeeklyReward = reader.ReadInt32();
				break;
			case 17:
				matchBanPickTierRewardData.WeeklyBoxReward = reader.ReadInt32();
				break;
			case 18:
				matchBanPickTierRewardData.ServerBuff = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 19:
				matchBanPickTierRewardData.ShowRewardIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return matchBanPickTierRewardData;
	}
}
