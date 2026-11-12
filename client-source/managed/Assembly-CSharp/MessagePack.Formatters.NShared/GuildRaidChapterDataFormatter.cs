using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildRaidChapterDataFormatter : IMessagePackFormatter<GuildRaidChapterData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildRaidChapterData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(6);
		writer.Write(value.ChapterIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Name, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Description, options);
		writer.Write(value.BonusGroup);
		writer.Write(value.IsBonusDungeon);
		writer.Write(value.IsDummyDungeon);
	}

	public GuildRaidChapterData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		GuildRaidChapterData guildRaidChapterData = new GuildRaidChapterData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				guildRaidChapterData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				guildRaidChapterData.Name = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				guildRaidChapterData.Description = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				guildRaidChapterData.BonusGroup = reader.ReadInt32();
				break;
			case 4:
				guildRaidChapterData.IsBonusDungeon = reader.ReadBoolean();
				break;
			case 5:
				guildRaidChapterData.IsDummyDungeon = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return guildRaidChapterData;
	}
}
