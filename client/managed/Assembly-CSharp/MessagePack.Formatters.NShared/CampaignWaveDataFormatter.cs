using System.Collections.Generic;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CampaignWaveDataFormatter : IMessagePackFormatter<CampaignWaveData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CampaignWaveData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(32);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		resolver.GetFormatterWithVerify<DungeonDifficultyType>().Serialize(ref writer, value.Difficulty, options);
		writer.Write(value.Scenario);
		writer.Write(value.WaveIndex);
		writer.Write(value.Star);
		writer.Write(value.Level);
		writer.Write(value.Tier);
		writer.Write(value.SkillOpen);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BgmCode, options);
		resolver.GetFormatterWithVerify<WinDeciderType>().Serialize(ref writer, value.WinDeciderType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.WinDeciderValue, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.AddHeroExtraSkill, options);
		writer.Write(value.AreaGroupIndex);
		writer.Write(value.AreaPositionMx);
		writer.Write(value.AreaPositionIsRelative);
		writer.Write(value.AreaSizeMx);
		writer.Write(value.RightTeamOffsetMx);
		writer.Write(value.AreaGroupIndex2);
		writer.Write(value.AreaPositionMx2);
		writer.Write(value.AreaPositionIsRelative2);
		writer.Write(value.AreaSizeMx2);
		writer.Write(value.RightTeamOffsetMx2);
		resolver.GetFormatterWithVerify<DelayedSpawnType>().Serialize(ref writer, value.DelayedSpawnType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EnvironmentProjectile1, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EnvironmentProjectile2, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EnvironmentProjectile3, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EnvironmentProjectile4, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EnvironmentProjectile5, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SoundName, options);
		resolver.GetFormatterWithVerify<List<CampaignWaveData.CreatureDetailWritable>>().Serialize(ref writer, value.BossList, options);
		resolver.GetFormatterWithVerify<List<CampaignWaveData.CreatureDetailWritable>>().Serialize(ref writer, value.CreatureList, options);
	}

	public CampaignWaveData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CampaignWaveData campaignWaveData = new CampaignWaveData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				campaignWaveData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				campaignWaveData.DungeonIndex = reader.ReadInt32();
				break;
			case 2:
				campaignWaveData.Difficulty = resolver.GetFormatterWithVerify<DungeonDifficultyType>().Deserialize(ref reader, options);
				break;
			case 3:
				campaignWaveData.Scenario = reader.ReadBoolean();
				break;
			case 4:
				campaignWaveData.WaveIndex = reader.ReadInt32();
				break;
			case 5:
				campaignWaveData.Star = reader.ReadSByte();
				break;
			case 6:
				campaignWaveData.Level = reader.ReadInt16();
				break;
			case 7:
				campaignWaveData.Tier = reader.ReadInt32();
				break;
			case 8:
				campaignWaveData.SkillOpen = reader.ReadSByte();
				break;
			case 9:
				campaignWaveData.BgmCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				campaignWaveData.WinDeciderType = resolver.GetFormatterWithVerify<WinDeciderType>().Deserialize(ref reader, options);
				break;
			case 11:
				campaignWaveData.WinDeciderValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 12:
				campaignWaveData.AddHeroExtraSkill = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 13:
				campaignWaveData.AreaGroupIndex = reader.ReadInt32();
				break;
			case 14:
				campaignWaveData.AreaPositionMx = reader.ReadInt32();
				break;
			case 15:
				campaignWaveData.AreaPositionIsRelative = reader.ReadBoolean();
				break;
			case 16:
				campaignWaveData.AreaSizeMx = reader.ReadInt32();
				break;
			case 17:
				campaignWaveData.RightTeamOffsetMx = reader.ReadInt32();
				break;
			case 18:
				campaignWaveData.AreaGroupIndex2 = reader.ReadInt32();
				break;
			case 19:
				campaignWaveData.AreaPositionMx2 = reader.ReadInt32();
				break;
			case 20:
				campaignWaveData.AreaPositionIsRelative2 = reader.ReadBoolean();
				break;
			case 21:
				campaignWaveData.AreaSizeMx2 = reader.ReadInt32();
				break;
			case 22:
				campaignWaveData.RightTeamOffsetMx2 = reader.ReadInt32();
				break;
			case 23:
				campaignWaveData.DelayedSpawnType = resolver.GetFormatterWithVerify<DelayedSpawnType>().Deserialize(ref reader, options);
				break;
			case 24:
				campaignWaveData.EnvironmentProjectile1 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 25:
				campaignWaveData.EnvironmentProjectile2 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 26:
				campaignWaveData.EnvironmentProjectile3 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 27:
				campaignWaveData.EnvironmentProjectile4 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 28:
				campaignWaveData.EnvironmentProjectile5 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 29:
				campaignWaveData.SoundName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 30:
				campaignWaveData.BossList = resolver.GetFormatterWithVerify<List<CampaignWaveData.CreatureDetailWritable>>().Deserialize(ref reader, options);
				break;
			case 31:
				campaignWaveData.CreatureList = resolver.GetFormatterWithVerify<List<CampaignWaveData.CreatureDetailWritable>>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return campaignWaveData;
	}
}
