using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CampaignDungeonDataFormatter : IMessagePackFormatter<CampaignDungeonData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CampaignDungeonData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(76);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		resolver.GetFormatterWithVerify<HashString>().Serialize(ref writer, value.BattleField, options);
		writer.Write(value.IsExceededEntryChapter);
		resolver.GetFormatterWithVerify<BattleType>().Serialize(ref writer, value.BattleType, options);
		resolver.GetFormatterWithVerify<HashString>().Serialize(ref writer, value.BattleDefineCode, options);
		resolver.GetFormatterWithVerify<HashString>().Serialize(ref writer, value.BgmCode, options);
		writer.Write(value.Scenario);
		writer.Write(value.ScenarioEndWave);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ReqScenarioDungeon, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Constant, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ForcedHeroIndex, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ScenarioForcedHeroIndex, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.RandomSkillProjectileGroup, options);
		writer.Write(value.FirstRewardIndex);
		resolver.GetFormatterWithVerify<StaminaType>().Serialize(ref writer, value.ReqStaminaType, options);
		writer.Write(value.ReqStamina);
		writer.Write(value.AdvancedReqStamina);
		writer.WriteNil();
		writer.Write(value.MainSquardCount);
		writer.Write(value.SubSquardCount);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.LocalStorageKey, options);
		writer.Write(value.FirstDungeonIndex);
		writer.Write(value.NextDungeonIndex);
		writer.Write(value.DropRewardIndex_Easy);
		writer.Write(value.CreatureExp_Easy);
		writer.Write(value.FirstClearRewardIndex_Easy);
		writer.Write(value.FirstClearGem_Easy);
		writer.Write(value.ReqStamina_Easy);
		resolver.GetFormatterWithVerify<DispatchType>().Serialize(ref writer, value.DispatchType_Easy, options);
		writer.Write(value.DropRewardIndex_Normal);
		writer.Write(value.CreatureExp_Normal);
		writer.Write(value.FirstClearRewardIndex_Normal);
		writer.Write(value.FirstClearGem_Normal);
		writer.Write(value.ReqStamina_Normal);
		resolver.GetFormatterWithVerify<DispatchType>().Serialize(ref writer, value.DispatchType_Normal, options);
		writer.Write(value.DropRewardIndex_Hard);
		writer.Write(value.CreatureExp_Hard);
		writer.Write(value.FirstClearRewardIndex_Hard);
		writer.Write(value.FirstClearGem_Hard);
		writer.Write(value.ReqStamina_Hard);
		resolver.GetFormatterWithVerify<DispatchType>().Serialize(ref writer, value.DispatchType_Hard, options);
		writer.Write(value.DropRewardIndex_Hell);
		writer.Write(value.CreatureExp_Hell);
		writer.Write(value.FirstClearRewardIndex_Hell);
		writer.Write(value.FirstClearGem_Hell);
		writer.Write(value.ReqStamina_Hell);
		resolver.GetFormatterWithVerify<DispatchType>().Serialize(ref writer, value.DispatchType_Hell, options);
		writer.Write(value.CheckClearChapter);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.Value, options);
		writer.Write(value.OpenDifficulty);
		writer.Write(value.IsBranch);
		writer.Write(value.NoDifficulty);
		resolver.GetFormatterWithVerify<EGuildFlagDisplayType>().Serialize(ref writer, value.EGuildFlagDisplayType, options);
		writer.Write(value.DispatchDeckCount);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.DungeonSkillIndexList, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ScenarioBattleDefine, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.DungeonPassiveEnvironmentIndices, options);
		writer.Write(value.IsNotInItDailyCount);
		resolver.GetFormatterWithVerify<SkillAttrType[]>().Serialize(ref writer, value.DeckAttributeCondition, options);
		writer.WriteNil();
		writer.Write(value.AffixGroupIndex_Easy);
		writer.Write(value.AffixGroupIndex_Normal);
		writer.Write(value.AffixGroupIndex_Hard);
		writer.Write(value.AffixGroupIndex_Hell);
		writer.Write(value.AffixMultiple_Easy);
		writer.Write(value.AffixMultiple_Normal);
		writer.Write(value.AffixMultiple_Hard);
		writer.Write(value.AffixMultiple_Hell);
		writer.Write(value.ClearPoint_Easy);
		writer.Write(value.ClearPoint_Normal);
		writer.Write(value.ClearPoint_Hard);
		writer.Write(value.ClearPoint_Hell);
		resolver.GetFormatterWithVerify<SweepDungeonType>().Serialize(ref writer, value.SweepDungeonType, options);
		writer.Write(value.SweepTicketCount);
		writer.Write(value.SweepTicketMaxCount);
	}

	public CampaignDungeonData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CampaignDungeonData campaignDungeonData = new CampaignDungeonData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				campaignDungeonData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				campaignDungeonData.DungeonIndex = reader.ReadInt32();
				break;
			case 2:
				campaignDungeonData.BattleField = resolver.GetFormatterWithVerify<HashString>().Deserialize(ref reader, options);
				break;
			case 3:
				campaignDungeonData.IsExceededEntryChapter = reader.ReadBoolean();
				break;
			case 4:
				campaignDungeonData.BattleType = resolver.GetFormatterWithVerify<BattleType>().Deserialize(ref reader, options);
				break;
			case 5:
				campaignDungeonData.BattleDefineCode = resolver.GetFormatterWithVerify<HashString>().Deserialize(ref reader, options);
				break;
			case 6:
				campaignDungeonData.BgmCode = resolver.GetFormatterWithVerify<HashString>().Deserialize(ref reader, options);
				break;
			case 7:
				campaignDungeonData.Scenario = reader.ReadBoolean();
				break;
			case 8:
				campaignDungeonData.ScenarioEndWave = reader.ReadInt32();
				break;
			case 9:
				campaignDungeonData.ReqScenarioDungeon = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 10:
				campaignDungeonData.Constant = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 11:
				campaignDungeonData.ForcedHeroIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 12:
				campaignDungeonData.ScenarioForcedHeroIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 13:
				campaignDungeonData.RandomSkillProjectileGroup = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 14:
				campaignDungeonData.FirstRewardIndex = reader.ReadInt32();
				break;
			case 15:
				campaignDungeonData.ReqStaminaType = resolver.GetFormatterWithVerify<StaminaType>().Deserialize(ref reader, options);
				break;
			case 16:
				campaignDungeonData.ReqStamina = reader.ReadInt32();
				break;
			case 17:
				campaignDungeonData.AdvancedReqStamina = reader.ReadInt32();
				break;
			case 19:
				campaignDungeonData.MainSquardCount = reader.ReadSByte();
				break;
			case 20:
				campaignDungeonData.SubSquardCount = reader.ReadSByte();
				break;
			case 21:
				campaignDungeonData.LocalStorageKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 22:
				campaignDungeonData.FirstDungeonIndex = reader.ReadInt32();
				break;
			case 23:
				campaignDungeonData.NextDungeonIndex = reader.ReadInt32();
				break;
			case 24:
				campaignDungeonData.DropRewardIndex_Easy = reader.ReadInt32();
				break;
			case 25:
				campaignDungeonData.CreatureExp_Easy = reader.ReadInt32();
				break;
			case 26:
				campaignDungeonData.FirstClearRewardIndex_Easy = reader.ReadInt32();
				break;
			case 27:
				campaignDungeonData.FirstClearGem_Easy = reader.ReadInt32();
				break;
			case 28:
				campaignDungeonData.ReqStamina_Easy = reader.ReadInt32();
				break;
			case 29:
				campaignDungeonData.DispatchType_Easy = resolver.GetFormatterWithVerify<DispatchType>().Deserialize(ref reader, options);
				break;
			case 30:
				campaignDungeonData.DropRewardIndex_Normal = reader.ReadInt32();
				break;
			case 31:
				campaignDungeonData.CreatureExp_Normal = reader.ReadInt32();
				break;
			case 32:
				campaignDungeonData.FirstClearRewardIndex_Normal = reader.ReadInt32();
				break;
			case 33:
				campaignDungeonData.FirstClearGem_Normal = reader.ReadInt32();
				break;
			case 34:
				campaignDungeonData.ReqStamina_Normal = reader.ReadInt32();
				break;
			case 35:
				campaignDungeonData.DispatchType_Normal = resolver.GetFormatterWithVerify<DispatchType>().Deserialize(ref reader, options);
				break;
			case 36:
				campaignDungeonData.DropRewardIndex_Hard = reader.ReadInt32();
				break;
			case 37:
				campaignDungeonData.CreatureExp_Hard = reader.ReadInt32();
				break;
			case 38:
				campaignDungeonData.FirstClearRewardIndex_Hard = reader.ReadInt32();
				break;
			case 39:
				campaignDungeonData.FirstClearGem_Hard = reader.ReadInt32();
				break;
			case 40:
				campaignDungeonData.ReqStamina_Hard = reader.ReadInt32();
				break;
			case 41:
				campaignDungeonData.DispatchType_Hard = resolver.GetFormatterWithVerify<DispatchType>().Deserialize(ref reader, options);
				break;
			case 42:
				campaignDungeonData.DropRewardIndex_Hell = reader.ReadInt32();
				break;
			case 43:
				campaignDungeonData.CreatureExp_Hell = reader.ReadInt32();
				break;
			case 44:
				campaignDungeonData.FirstClearRewardIndex_Hell = reader.ReadInt32();
				break;
			case 45:
				campaignDungeonData.FirstClearGem_Hell = reader.ReadInt32();
				break;
			case 46:
				campaignDungeonData.ReqStamina_Hell = reader.ReadInt32();
				break;
			case 47:
				campaignDungeonData.DispatchType_Hell = resolver.GetFormatterWithVerify<DispatchType>().Deserialize(ref reader, options);
				break;
			case 48:
				campaignDungeonData.CheckClearChapter = reader.ReadByte();
				break;
			case 49:
				campaignDungeonData.Value = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 50:
				campaignDungeonData.OpenDifficulty = reader.ReadBoolean();
				break;
			case 51:
				campaignDungeonData.IsBranch = reader.ReadBoolean();
				break;
			case 52:
				campaignDungeonData.NoDifficulty = reader.ReadBoolean();
				break;
			case 53:
				campaignDungeonData.EGuildFlagDisplayType = resolver.GetFormatterWithVerify<EGuildFlagDisplayType>().Deserialize(ref reader, options);
				break;
			case 54:
				campaignDungeonData.DispatchDeckCount = reader.ReadInt32();
				break;
			case 55:
				campaignDungeonData.DungeonSkillIndexList = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 56:
				campaignDungeonData.ScenarioBattleDefine = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 57:
				campaignDungeonData.DungeonPassiveEnvironmentIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 58:
				campaignDungeonData.IsNotInItDailyCount = reader.ReadBoolean();
				break;
			case 59:
				campaignDungeonData.DeckAttributeCondition = resolver.GetFormatterWithVerify<SkillAttrType[]>().Deserialize(ref reader, options);
				break;
			case 61:
				campaignDungeonData.AffixGroupIndex_Easy = reader.ReadInt32();
				break;
			case 62:
				campaignDungeonData.AffixGroupIndex_Normal = reader.ReadInt32();
				break;
			case 63:
				campaignDungeonData.AffixGroupIndex_Hard = reader.ReadInt32();
				break;
			case 64:
				campaignDungeonData.AffixGroupIndex_Hell = reader.ReadInt32();
				break;
			case 65:
				campaignDungeonData.AffixMultiple_Easy = reader.ReadInt32();
				break;
			case 66:
				campaignDungeonData.AffixMultiple_Normal = reader.ReadInt32();
				break;
			case 67:
				campaignDungeonData.AffixMultiple_Hard = reader.ReadInt32();
				break;
			case 68:
				campaignDungeonData.AffixMultiple_Hell = reader.ReadInt32();
				break;
			case 69:
				campaignDungeonData.ClearPoint_Easy = reader.ReadInt32();
				break;
			case 70:
				campaignDungeonData.ClearPoint_Normal = reader.ReadInt32();
				break;
			case 71:
				campaignDungeonData.ClearPoint_Hard = reader.ReadInt32();
				break;
			case 72:
				campaignDungeonData.ClearPoint_Hell = reader.ReadInt32();
				break;
			case 73:
				campaignDungeonData.SweepDungeonType = resolver.GetFormatterWithVerify<SweepDungeonType>().Deserialize(ref reader, options);
				break;
			case 74:
				campaignDungeonData.SweepTicketCount = reader.ReadInt32();
				break;
			case 75:
				campaignDungeonData.SweepTicketMaxCount = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return campaignDungeonData;
	}
}
