using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildRaidChapterRewardDataFormatter : IMessagePackFormatter<GuildRaidChapterRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildRaidChapterRewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(7);
		writer.Write(value.ChapterIndex);
		writer.Write(value.Difficulty);
		writer.Write(value.TotalDamage);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Credit, options);
		writer.Write(value.RewardIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MailTitleKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MailContentKey, options);
	}

	public GuildRaidChapterRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		GuildRaidChapterRewardData guildRaidChapterRewardData = new GuildRaidChapterRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				guildRaidChapterRewardData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				guildRaidChapterRewardData.Difficulty = reader.ReadInt32();
				break;
			case 2:
				guildRaidChapterRewardData.TotalDamage = reader.ReadInt64();
				break;
			case 3:
				guildRaidChapterRewardData.Credit = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				guildRaidChapterRewardData.RewardIndex = reader.ReadInt32();
				break;
			case 5:
				guildRaidChapterRewardData.MailTitleKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				guildRaidChapterRewardData.MailContentKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return guildRaidChapterRewardData;
	}
}
