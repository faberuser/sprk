using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MatchBanPickRewardDataFormatter : IMessagePackFormatter<MatchBanPickRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MatchBanPickRewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(6);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<MatchRewardType>().Serialize(ref writer, value.Type, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Name, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Desc, options);
		writer.Write(value.RewardIndex);
		resolver.GetFormatterWithVerify<MatchSeasonType[]>().Serialize(ref writer, value.Season, options);
	}

	public MatchBanPickRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		MatchBanPickRewardData matchBanPickRewardData = new MatchBanPickRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				matchBanPickRewardData.Index = reader.ReadInt32();
				break;
			case 1:
				matchBanPickRewardData.Type = resolver.GetFormatterWithVerify<MatchRewardType>().Deserialize(ref reader, options);
				break;
			case 2:
				matchBanPickRewardData.Name = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				matchBanPickRewardData.Desc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				matchBanPickRewardData.RewardIndex = reader.ReadInt32();
				break;
			case 5:
				matchBanPickRewardData.Season = resolver.GetFormatterWithVerify<MatchSeasonType[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return matchBanPickRewardData;
	}
}
