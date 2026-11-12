using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class WorldBossDataFormatter : IMessagePackFormatter<WorldBossData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, WorldBossData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(19);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Name, options);
		writer.Write(value.OpenDow);
		writer.Write(value.ReqMaxHeroLevel);
		writer.Write(value.DealBattleDuration);
		writer.Write(value.NormalBattleDuration);
		writer.Write(value.CoolTimeDuration);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Tactic, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.RecommendedHeroIndex, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BossInfoImage, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PreviewBg, options);
		writer.Write(value.KillRewardIndex);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ShowInfoSkill, options);
		writer.Write(value.IsGlobal);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PortalImagePath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.AdventureNoticeImagePath, options);
		writer.Write(value.NextIndex);
	}

	public WorldBossData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		WorldBossData worldBossData = new WorldBossData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				worldBossData.Index = reader.ReadInt32();
				break;
			case 1:
				worldBossData.Name = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				worldBossData.OpenDow = reader.ReadInt32();
				break;
			case 3:
				worldBossData.ReqMaxHeroLevel = reader.ReadInt32();
				break;
			case 4:
				worldBossData.DealBattleDuration = reader.ReadInt32();
				break;
			case 5:
				worldBossData.NormalBattleDuration = reader.ReadInt32();
				break;
			case 6:
				worldBossData.CoolTimeDuration = reader.ReadInt32();
				break;
			case 7:
				worldBossData.ChapterIndex = reader.ReadInt32();
				break;
			case 8:
				worldBossData.DungeonIndex = reader.ReadInt32();
				break;
			case 9:
				worldBossData.Tactic = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				worldBossData.RecommendedHeroIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 11:
				worldBossData.BossInfoImage = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 12:
				worldBossData.PreviewBg = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 13:
				worldBossData.KillRewardIndex = reader.ReadInt32();
				break;
			case 14:
				worldBossData.ShowInfoSkill = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 15:
				worldBossData.IsGlobal = reader.ReadBoolean();
				break;
			case 16:
				worldBossData.PortalImagePath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 17:
				worldBossData.AdventureNoticeImagePath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 18:
				worldBossData.NextIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return worldBossData;
	}
}
