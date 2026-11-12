using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class RaidDataFormatter : IMessagePackFormatter<RaidData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, RaidData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(38);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Name, options);
		resolver.GetFormatterWithVerify<RaidType>().Serialize(ref writer, value.Type, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Step_Name, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Step_HelpDesc, options);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		writer.Write(value.OccurChapterIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PreviewBg, options);
		writer.Write(value.PlayerCount);
		writer.Write(value.DeckCount);
		writer.Write(value.DeckCountPerPlayer);
		writer.Write(value.AliveDuration);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.NodeTreePath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.NodeTreeCode, options);
		writer.Write(value.RaidPoint);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Tactic, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.RecommendedHeroIndex, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ShowInfoSkill, options);
		writer.Write(value.SingleRaidIndex);
		writer.Write(value.MainPartyCount);
		writer.WriteNil();
		writer.Write(value.SubPartyCount);
		writer.WriteNil();
		writer.Write(value.IsOnlineSingle);
		writer.Write(value.Index);
		writer.Write(value.Level);
		writer.Write(value.FilterValue);
		writer.Write(value.IsOpen);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.OpenCondition, options);
		writer.Write(value.ReqHeroLevel);
		writer.Write(value.ResetGem);
		writer.Write(value.IndividualReward);
		writer.Write(value.ClearRaidIndex);
		writer.Write(value.BanIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.RaidDesc, options);
		writer.Write(value.RaidMultiInfoGroupIndex);
		writer.Write(value.HideRaidInfoSkillTab);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.RaidInfoCustomIdle, options);
	}

	public RaidData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		RaidData raidData = new RaidData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				raidData.Name = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				raidData.Type = resolver.GetFormatterWithVerify<RaidType>().Deserialize(ref reader, options);
				break;
			case 2:
				raidData.Step_Name = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				raidData.Step_HelpDesc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				raidData.ChapterIndex = reader.ReadInt32();
				break;
			case 5:
				raidData.DungeonIndex = reader.ReadInt32();
				break;
			case 6:
				raidData.OccurChapterIndex = reader.ReadInt32();
				break;
			case 7:
				raidData.PreviewBg = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				raidData.PlayerCount = reader.ReadInt32();
				break;
			case 9:
				raidData.DeckCount = reader.ReadInt32();
				break;
			case 10:
				raidData.DeckCountPerPlayer = reader.ReadInt32();
				break;
			case 11:
				raidData.AliveDuration = reader.ReadInt32();
				break;
			case 12:
				raidData.NodeTreePath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 13:
				raidData.NodeTreeCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 14:
				raidData.RaidPoint = reader.ReadInt32();
				break;
			case 15:
				raidData.Tactic = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 16:
				raidData.RecommendedHeroIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 17:
				raidData.ShowInfoSkill = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 18:
				raidData.SingleRaidIndex = reader.ReadInt32();
				break;
			case 19:
				raidData.MainPartyCount = reader.ReadInt32();
				break;
			case 21:
				raidData.SubPartyCount = reader.ReadInt32();
				break;
			case 23:
				raidData.IsOnlineSingle = reader.ReadBoolean();
				break;
			case 24:
				raidData.Index = reader.ReadInt32();
				break;
			case 25:
				raidData.Level = reader.ReadInt32();
				break;
			case 26:
				raidData.FilterValue = reader.ReadInt32();
				break;
			case 27:
				raidData.IsOpen = reader.ReadBoolean();
				break;
			case 28:
				raidData.OpenCondition = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 29:
				raidData.ReqHeroLevel = reader.ReadInt32();
				break;
			case 30:
				raidData.ResetGem = reader.ReadInt32();
				break;
			case 31:
				raidData.IndividualReward = reader.ReadInt32();
				break;
			case 32:
				raidData.ClearRaidIndex = reader.ReadInt32();
				break;
			case 33:
				raidData.BanIndex = reader.ReadInt32();
				break;
			case 34:
				raidData.RaidDesc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 35:
				raidData.RaidMultiInfoGroupIndex = reader.ReadInt32();
				break;
			case 36:
				raidData.HideRaidInfoSkillTab = reader.ReadBoolean();
				break;
			case 37:
				raidData.RaidInfoCustomIdle = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return raidData;
	}
}
