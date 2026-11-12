using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildRaidDummyDungeonDataFormatter : IMessagePackFormatter<GuildRaidDummyDungeonData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildRaidDummyDungeonData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(13);
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
	}

	public GuildRaidDummyDungeonData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		GuildRaidDummyDungeonData guildRaidDummyDungeonData = new GuildRaidDummyDungeonData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				guildRaidDummyDungeonData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				guildRaidDummyDungeonData.DungeonIndex = reader.ReadInt32();
				break;
			case 2:
				guildRaidDummyDungeonData.Step = reader.ReadInt32();
				break;
			case 3:
				guildRaidDummyDungeonData.BonusDungeonRate = reader.ReadInt32();
				break;
			case 4:
				guildRaidDummyDungeonData.KillRewardIndex = reader.ReadInt32();
				break;
			case 5:
				guildRaidDummyDungeonData.KillActivityPoint = reader.ReadInt32();
				break;
			case 6:
				guildRaidDummyDungeonData.BuyableRewardIndex = reader.ReadInt32();
				break;
			case 7:
				guildRaidDummyDungeonData.Tactic = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				guildRaidDummyDungeonData.RecommendedHeroIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 9:
				guildRaidDummyDungeonData.PortraitPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				guildRaidDummyDungeonData.PreviewBg = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				guildRaidDummyDungeonData.FinishModeRatio = reader.ReadInt64();
				break;
			case 12:
				guildRaidDummyDungeonData.ShowInfoSkill = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return guildRaidDummyDungeonData;
	}
}
