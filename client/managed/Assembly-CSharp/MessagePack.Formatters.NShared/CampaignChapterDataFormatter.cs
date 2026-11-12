using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CampaignChapterDataFormatter : IMessagePackFormatter<CampaignChapterData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CampaignChapterData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(56);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<ChapterType>().Serialize(ref writer, value.Type, options);
		writer.Write(value.DailyEnterMaxCount);
		writer.Write(value.MainField);
		writer.Write(value.NodeOpenEffect);
		writer.Write(value.IsOpen);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Name, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Map, options);
		writer.Write(value.MapSizeX);
		writer.Write(value.MapSizeY);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.ShadowDirection, options);
		writer.Write(value.StartNodeIndex);
		writer.Write(value.LabelScale);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.UserStatType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.UserStatTypeIconSprite, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ResultInfoIconSprite, options);
		resolver.GetFormatterWithVerify<StaminaType>().Serialize(ref writer, value.StaminaType, options);
		writer.Write(value.ReqStamina);
		resolver.GetFormatterWithVerify<StaminaType>().Serialize(ref writer, value.SubStaminaType, options);
		writer.Write(value.OccurRaid);
		writer.Write(value.CreatureExp);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BgmCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Description, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BasePortalImageName, options);
		writer.Write(value.ReqChapterIndex);
		writer.Write(value.ReqDungeonIndex);
		resolver.GetFormatterWithVerify<DungeonDifficultyType>().Serialize(ref writer, value.ReqDungeonDifficulty, options);
		writer.Write(value.ForcedAvatarIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BeforeEnterMap, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.EnterCheckerName, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Values, options);
		resolver.GetFormatterWithVerify<RewardType>().Serialize(ref writer, value.EasyRewardType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.EasyRewardCode, options);
		writer.Write(value.EasyRewardCount);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.EasyChapterHiddenReward, options);
		resolver.GetFormatterWithVerify<RewardType>().Serialize(ref writer, value.NormalRewardType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.NormalRewardCode, options);
		writer.Write(value.NormalRewardCount);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.NormalChapterHiddenReward, options);
		resolver.GetFormatterWithVerify<RewardType>().Serialize(ref writer, value.HardRewardType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.HardRewardCode, options);
		writer.Write(value.HardRewardCount);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.HardChapterHiddenReward, options);
		resolver.GetFormatterWithVerify<RewardType>().Serialize(ref writer, value.HellRewardType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.HellRewardCode, options);
		writer.Write(value.HellRewardCount);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.HellChapterHiddenReward, options);
		writer.Write(value.HellMode_ReqChapterIndex);
		writer.Write(value.HellMode_ReqDungeonIndex);
		resolver.GetFormatterWithVerify<DungeonDifficultyType>().Serialize(ref writer, value.HellMode_ReqDungeonDifficulty, options);
		resolver.GetFormatterWithVerify<ERightMenuButton[]>().Serialize(ref writer, value.ShowMenuButtons, options);
		resolver.GetFormatterWithVerify<DungeonDifficultyType>().Serialize(ref writer, value.MaxDifficulty, options);
		writer.Write(value.AvatarScale);
		resolver.GetFormatterWithVerify<GuildConstructionResourceTopmostMenuType>().Serialize(ref writer, value.GuildResourceMenuType, options);
		writer.Write(value.MultiAvatarUse);
		resolver.GetFormatterWithVerify<EMapArrangement>().Serialize(ref writer, value.MapArrangement, options);
	}

	public CampaignChapterData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CampaignChapterData campaignChapterData = new CampaignChapterData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				campaignChapterData.Index = reader.ReadInt32();
				break;
			case 1:
				campaignChapterData.Type = resolver.GetFormatterWithVerify<ChapterType>().Deserialize(ref reader, options);
				break;
			case 2:
				campaignChapterData.DailyEnterMaxCount = reader.ReadInt32();
				break;
			case 3:
				campaignChapterData.MainField = reader.ReadBoolean();
				break;
			case 4:
				campaignChapterData.NodeOpenEffect = reader.ReadBoolean();
				break;
			case 5:
				campaignChapterData.IsOpen = reader.ReadBoolean();
				break;
			case 6:
				campaignChapterData.Name = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				campaignChapterData.Map = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				campaignChapterData.MapSizeX = reader.ReadInt32();
				break;
			case 9:
				campaignChapterData.MapSizeY = reader.ReadInt32();
				break;
			case 10:
				campaignChapterData.ShadowDirection = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 11:
				campaignChapterData.StartNodeIndex = reader.ReadInt32();
				break;
			case 12:
				campaignChapterData.LabelScale = reader.ReadInt32();
				break;
			case 13:
				campaignChapterData.UserStatType = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 14:
				campaignChapterData.UserStatTypeIconSprite = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 15:
				campaignChapterData.ResultInfoIconSprite = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 16:
				campaignChapterData.StaminaType = resolver.GetFormatterWithVerify<StaminaType>().Deserialize(ref reader, options);
				break;
			case 17:
				campaignChapterData.ReqStamina = reader.ReadInt32();
				break;
			case 18:
				campaignChapterData.SubStaminaType = resolver.GetFormatterWithVerify<StaminaType>().Deserialize(ref reader, options);
				break;
			case 19:
				campaignChapterData.OccurRaid = reader.ReadBoolean();
				break;
			case 20:
				campaignChapterData.CreatureExp = reader.ReadInt32();
				break;
			case 21:
				campaignChapterData.BgmCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 22:
				campaignChapterData.Description = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 23:
				campaignChapterData.BasePortalImageName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 24:
				campaignChapterData.ReqChapterIndex = reader.ReadInt32();
				break;
			case 25:
				campaignChapterData.ReqDungeonIndex = reader.ReadInt32();
				break;
			case 26:
				campaignChapterData.ReqDungeonDifficulty = resolver.GetFormatterWithVerify<DungeonDifficultyType>().Deserialize(ref reader, options);
				break;
			case 27:
				campaignChapterData.ForcedAvatarIndex = reader.ReadInt32();
				break;
			case 28:
				campaignChapterData.BeforeEnterMap = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 29:
				campaignChapterData.EnterCheckerName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 30:
				campaignChapterData.Values = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 31:
				campaignChapterData.EasyRewardType = resolver.GetFormatterWithVerify<RewardType>().Deserialize(ref reader, options);
				break;
			case 32:
				campaignChapterData.EasyRewardCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 33:
				campaignChapterData.EasyRewardCount = reader.ReadInt32();
				break;
			case 34:
				campaignChapterData.EasyChapterHiddenReward = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 35:
				campaignChapterData.NormalRewardType = resolver.GetFormatterWithVerify<RewardType>().Deserialize(ref reader, options);
				break;
			case 36:
				campaignChapterData.NormalRewardCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 37:
				campaignChapterData.NormalRewardCount = reader.ReadInt32();
				break;
			case 38:
				campaignChapterData.NormalChapterHiddenReward = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 39:
				campaignChapterData.HardRewardType = resolver.GetFormatterWithVerify<RewardType>().Deserialize(ref reader, options);
				break;
			case 40:
				campaignChapterData.HardRewardCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 41:
				campaignChapterData.HardRewardCount = reader.ReadInt32();
				break;
			case 42:
				campaignChapterData.HardChapterHiddenReward = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 43:
				campaignChapterData.HellRewardType = resolver.GetFormatterWithVerify<RewardType>().Deserialize(ref reader, options);
				break;
			case 44:
				campaignChapterData.HellRewardCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 45:
				campaignChapterData.HellRewardCount = reader.ReadInt32();
				break;
			case 46:
				campaignChapterData.HellChapterHiddenReward = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 47:
				campaignChapterData.HellMode_ReqChapterIndex = reader.ReadInt32();
				break;
			case 48:
				campaignChapterData.HellMode_ReqDungeonIndex = reader.ReadInt32();
				break;
			case 49:
				campaignChapterData.HellMode_ReqDungeonDifficulty = resolver.GetFormatterWithVerify<DungeonDifficultyType>().Deserialize(ref reader, options);
				break;
			case 50:
				campaignChapterData.ShowMenuButtons = resolver.GetFormatterWithVerify<ERightMenuButton[]>().Deserialize(ref reader, options);
				break;
			case 51:
				campaignChapterData.MaxDifficulty = resolver.GetFormatterWithVerify<DungeonDifficultyType>().Deserialize(ref reader, options);
				break;
			case 52:
				campaignChapterData.AvatarScale = reader.ReadInt32();
				break;
			case 53:
				campaignChapterData.GuildResourceMenuType = resolver.GetFormatterWithVerify<GuildConstructionResourceTopmostMenuType>().Deserialize(ref reader, options);
				break;
			case 54:
				campaignChapterData.MultiAvatarUse = reader.ReadBoolean();
				break;
			case 55:
				campaignChapterData.MapArrangement = resolver.GetFormatterWithVerify<EMapArrangement>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return campaignChapterData;
	}
}
