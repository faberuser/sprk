using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildRaidDungeonDataFormatter : IMessagePackFormatter<GuildRaidDungeonData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildRaidDungeonData value, MessagePackSerializerOptions options)
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

	public GuildRaidDungeonData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		GuildRaidDungeonData guildRaidDungeonData = new GuildRaidDungeonData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				guildRaidDungeonData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				guildRaidDungeonData.DungeonIndex = reader.ReadInt32();
				break;
			case 2:
				guildRaidDungeonData.Step = reader.ReadInt32();
				break;
			case 3:
				guildRaidDungeonData.BonusDungeonRate = reader.ReadInt32();
				break;
			case 4:
				guildRaidDungeonData.KillRewardIndex = reader.ReadInt32();
				break;
			case 5:
				guildRaidDungeonData.KillActivityPoint = reader.ReadInt32();
				break;
			case 6:
				guildRaidDungeonData.BuyableRewardIndex = reader.ReadInt32();
				break;
			case 7:
				guildRaidDungeonData.Tactic = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				guildRaidDungeonData.RecommendedHeroIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 9:
				guildRaidDungeonData.PortraitPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				guildRaidDungeonData.PreviewBg = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				guildRaidDungeonData.FinishModeRatio = reader.ReadInt64();
				break;
			case 12:
				guildRaidDungeonData.ShowInfoSkill = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return guildRaidDungeonData;
	}
}
