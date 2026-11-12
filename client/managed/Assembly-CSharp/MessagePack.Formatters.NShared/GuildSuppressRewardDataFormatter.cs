using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildSuppressRewardDataFormatter : IMessagePackFormatter<GuildSuppressRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildSuppressRewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(9);
		writer.Write(value.Index);
		writer.Write(value.MinRank);
		writer.Write(value.MaxRank);
		writer.Write(value.Percentage);
		writer.Write(value.RewardIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DescKey, options);
		resolver.GetFormatterWithVerify<GuildSuppressServerRewardType>().Serialize(ref writer, value.RewardServerBuffType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.RewardServerBuffValue, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.RewardServerBuffCode, options);
	}

	public GuildSuppressRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		GuildSuppressRewardData guildSuppressRewardData = new GuildSuppressRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				guildSuppressRewardData.Index = reader.ReadInt32();
				break;
			case 1:
				guildSuppressRewardData.MinRank = reader.ReadInt32();
				break;
			case 2:
				guildSuppressRewardData.MaxRank = reader.ReadInt32();
				break;
			case 3:
				guildSuppressRewardData.Percentage = reader.ReadInt32();
				break;
			case 4:
				guildSuppressRewardData.RewardIndex = reader.ReadInt32();
				break;
			case 5:
				guildSuppressRewardData.DescKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				guildSuppressRewardData.RewardServerBuffType = resolver.GetFormatterWithVerify<GuildSuppressServerRewardType>().Deserialize(ref reader, options);
				break;
			case 7:
				guildSuppressRewardData.RewardServerBuffValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 8:
				guildSuppressRewardData.RewardServerBuffCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return guildSuppressRewardData;
	}
}
