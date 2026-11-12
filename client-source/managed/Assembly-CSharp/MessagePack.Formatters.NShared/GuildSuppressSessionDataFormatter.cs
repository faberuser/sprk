using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildSuppressSessionDataFormatter : IMessagePackFormatter<GuildSuppressSessionData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildSuppressSessionData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(13);
		writer.Write(value.SessionIndex);
		writer.Write(value.SeasonIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.GlobalApplyStart, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.GlobalApplyEnd, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.GlobalBattleStart, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.GlobalBattleEnd, options);
		writer.Write(value.ViewRankSeason);
		writer.Write(value.ViewSession);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.SuppressDungeonIndex, options);
		writer.Write(value.RaidIndex);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.Level, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.SingleLevel, options);
		writer.Write(value.PortalViewSession);
	}

	public GuildSuppressSessionData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		GuildSuppressSessionData guildSuppressSessionData = new GuildSuppressSessionData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				guildSuppressSessionData.SessionIndex = reader.ReadInt32();
				break;
			case 1:
				guildSuppressSessionData.SeasonIndex = reader.ReadInt32();
				break;
			case 2:
				guildSuppressSessionData.GlobalApplyStart = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				guildSuppressSessionData.GlobalApplyEnd = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				guildSuppressSessionData.GlobalBattleStart = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				guildSuppressSessionData.GlobalBattleEnd = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				guildSuppressSessionData.ViewRankSeason = reader.ReadInt32();
				break;
			case 7:
				guildSuppressSessionData.ViewSession = reader.ReadInt32();
				break;
			case 8:
				guildSuppressSessionData.SuppressDungeonIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 9:
				guildSuppressSessionData.RaidIndex = reader.ReadInt32();
				break;
			case 10:
				guildSuppressSessionData.Level = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 11:
				guildSuppressSessionData.SingleLevel = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 12:
				guildSuppressSessionData.PortalViewSession = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return guildSuppressSessionData;
	}
}
