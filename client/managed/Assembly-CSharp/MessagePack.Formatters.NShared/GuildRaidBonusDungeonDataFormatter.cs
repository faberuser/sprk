using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildRaidBonusDungeonDataFormatter : IMessagePackFormatter<GuildRaidBonusDungeonData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildRaidBonusDungeonData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(22);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		writer.Write(value.Step);
		writer.Write(value.BonusDungeonRate);
		writer.Write(value.KillRewardIndex);
		writer.Write(value.KillActivityPoint);
		writer.Write(value.BuyableRewardIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Tactic, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.RecommendedHeroIndex, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PortraitPath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PreviewBg, options);
		writer.Write(value.FinishModeRatio);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ShowInfoSkill, options);
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.Write(value.BonusGroup);
		writer.Write(value.Rate);
	}

	public GuildRaidBonusDungeonData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		GuildRaidBonusDungeonData guildRaidBonusDungeonData = new GuildRaidBonusDungeonData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				guildRaidBonusDungeonData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				guildRaidBonusDungeonData.DungeonIndex = reader.ReadInt32();
				break;
			case 2:
				guildRaidBonusDungeonData.Step = reader.ReadInt32();
				break;
			case 3:
				guildRaidBonusDungeonData.BonusDungeonRate = reader.ReadInt32();
				break;
			case 4:
				guildRaidBonusDungeonData.KillRewardIndex = reader.ReadInt32();
				break;
			case 5:
				guildRaidBonusDungeonData.KillActivityPoint = reader.ReadInt32();
				break;
			case 6:
				guildRaidBonusDungeonData.BuyableRewardIndex = reader.ReadInt32();
				break;
			case 7:
				guildRaidBonusDungeonData.Tactic = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				guildRaidBonusDungeonData.RecommendedHeroIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 9:
				guildRaidBonusDungeonData.PortraitPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				guildRaidBonusDungeonData.PreviewBg = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				guildRaidBonusDungeonData.FinishModeRatio = reader.ReadInt64();
				break;
			case 12:
				guildRaidBonusDungeonData.ShowInfoSkill = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 20:
				guildRaidBonusDungeonData.BonusGroup = reader.ReadInt32();
				break;
			case 21:
				guildRaidBonusDungeonData.Rate = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return guildRaidBonusDungeonData;
	}
}
