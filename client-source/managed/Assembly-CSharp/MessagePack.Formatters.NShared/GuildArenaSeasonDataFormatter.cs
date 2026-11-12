using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildArenaSeasonDataFormatter : IMessagePackFormatter<GuildArenaSeasonData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildArenaSeasonData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(6);
		writer.Write(value.SeasonIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DisplaySeasonText, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DisplaySeasonLocalKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Begin, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.End, options);
		writer.Write(value.SeasonType);
	}

	public GuildArenaSeasonData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		GuildArenaSeasonData guildArenaSeasonData = new GuildArenaSeasonData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				guildArenaSeasonData.SeasonIndex = reader.ReadInt32();
				break;
			case 1:
				guildArenaSeasonData.DisplaySeasonText = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				guildArenaSeasonData.DisplaySeasonLocalKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				guildArenaSeasonData.Begin = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				guildArenaSeasonData.End = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				guildArenaSeasonData.SeasonType = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return guildArenaSeasonData;
	}
}
