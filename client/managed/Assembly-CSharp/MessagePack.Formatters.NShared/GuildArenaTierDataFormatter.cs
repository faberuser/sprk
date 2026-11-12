using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildArenaTierDataFormatter : IMessagePackFormatter<GuildArenaTierData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildArenaTierData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(13);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<GuildArenaTierType>().Serialize(ref writer, value.Tier, options);
		writer.Write(value.InTopTier);
		writer.Write(value.Level);
		writer.Write(value.MinScore);
		writer.Write(value.MaxScore);
		writer.Write(value.SoftResetScore);
		writer.Write(value.SoftResetTierIndex);
		writer.Write(value.MinRank);
		writer.Write(value.MaxRank);
		writer.Write(value.MatchScoreRate);
		writer.Write(value.DefenseDeckCount);
		writer.Write(value.AttackCount);
	}

	public GuildArenaTierData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		GuildArenaTierData guildArenaTierData = new GuildArenaTierData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				guildArenaTierData.Index = reader.ReadInt32();
				break;
			case 1:
				guildArenaTierData.Tier = resolver.GetFormatterWithVerify<GuildArenaTierType>().Deserialize(ref reader, options);
				break;
			case 2:
				guildArenaTierData.InTopTier = reader.ReadBoolean();
				break;
			case 3:
				guildArenaTierData.Level = reader.ReadInt32();
				break;
			case 4:
				guildArenaTierData.MinScore = reader.ReadInt32();
				break;
			case 5:
				guildArenaTierData.MaxScore = reader.ReadInt32();
				break;
			case 6:
				guildArenaTierData.SoftResetScore = reader.ReadInt32();
				break;
			case 7:
				guildArenaTierData.SoftResetTierIndex = reader.ReadInt32();
				break;
			case 8:
				guildArenaTierData.MinRank = reader.ReadInt32();
				break;
			case 9:
				guildArenaTierData.MaxRank = reader.ReadInt32();
				break;
			case 10:
				guildArenaTierData.MatchScoreRate = reader.ReadInt32();
				break;
			case 11:
				guildArenaTierData.DefenseDeckCount = reader.ReadInt32();
				break;
			case 12:
				guildArenaTierData.AttackCount = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return guildArenaTierData;
	}
}
