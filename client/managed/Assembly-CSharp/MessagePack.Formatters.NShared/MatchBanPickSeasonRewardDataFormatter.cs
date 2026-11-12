using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MatchBanPickSeasonRewardDataFormatter : IMessagePackFormatter<MatchBanPickSeasonRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MatchBanPickSeasonRewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(5);
		writer.Write(value.Index);
		writer.Write(value.MinRank);
		writer.Write(value.MaxRank);
		writer.Write(value.Percentage);
		writer.Write(value.RewardIndex);
	}

	public MatchBanPickSeasonRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		MatchBanPickSeasonRewardData matchBanPickSeasonRewardData = new MatchBanPickSeasonRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				matchBanPickSeasonRewardData.Index = reader.ReadInt32();
				break;
			case 1:
				matchBanPickSeasonRewardData.MinRank = reader.ReadInt32();
				break;
			case 2:
				matchBanPickSeasonRewardData.MaxRank = reader.ReadInt32();
				break;
			case 3:
				matchBanPickSeasonRewardData.Percentage = reader.ReadInt32();
				break;
			case 4:
				matchBanPickSeasonRewardData.RewardIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return matchBanPickSeasonRewardData;
	}
}
