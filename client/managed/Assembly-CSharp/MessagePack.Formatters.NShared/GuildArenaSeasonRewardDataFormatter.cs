using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildArenaSeasonRewardDataFormatter : IMessagePackFormatter<GuildArenaSeasonRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildArenaSeasonRewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(13);
		writer.Write(value.Index);
		writer.Write(value.TierIndex);
		resolver.GetFormatterWithVerify<GuildArenaTierType>().Serialize(ref writer, value.ShowTierUI, options);
		writer.Write(value.ShowScoreUI);
		writer.Write(value.MinRank);
		writer.Write(value.MaxRank);
		writer.Write(value.SeasonPointReward);
		writer.Write(value.SeasonRubyReward);
		writer.Write(value.FreeSeasonPointReward);
		writer.Write(value.FreeSeasonRubyReward);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.FlagCode, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.RewardItemIndices, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.RewardItemCounts, options);
	}

	public GuildArenaSeasonRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		GuildArenaSeasonRewardData guildArenaSeasonRewardData = new GuildArenaSeasonRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				guildArenaSeasonRewardData.Index = reader.ReadInt32();
				break;
			case 1:
				guildArenaSeasonRewardData.TierIndex = reader.ReadInt32();
				break;
			case 2:
				guildArenaSeasonRewardData.ShowTierUI = resolver.GetFormatterWithVerify<GuildArenaTierType>().Deserialize(ref reader, options);
				break;
			case 3:
				guildArenaSeasonRewardData.ShowScoreUI = reader.ReadInt32();
				break;
			case 4:
				guildArenaSeasonRewardData.MinRank = reader.ReadInt32();
				break;
			case 5:
				guildArenaSeasonRewardData.MaxRank = reader.ReadInt32();
				break;
			case 6:
				guildArenaSeasonRewardData.SeasonPointReward = reader.ReadInt32();
				break;
			case 7:
				guildArenaSeasonRewardData.SeasonRubyReward = reader.ReadInt32();
				break;
			case 8:
				guildArenaSeasonRewardData.FreeSeasonPointReward = reader.ReadInt32();
				break;
			case 9:
				guildArenaSeasonRewardData.FreeSeasonRubyReward = reader.ReadInt32();
				break;
			case 10:
				guildArenaSeasonRewardData.FlagCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				guildArenaSeasonRewardData.RewardItemIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 12:
				guildArenaSeasonRewardData.RewardItemCounts = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return guildArenaSeasonRewardData;
	}
}
