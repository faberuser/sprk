using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class AchievementDataFormatter : IMessagePackFormatter<AchievementData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, AchievementData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(44);
		writer.Write(value.Index);
		writer.Write(value.Step);
		writer.Write(value.ShowIndex);
		resolver.GetFormatterWithVerify<AchievementType>().Serialize(ref writer, value.Type, options);
		resolver.GetFormatterWithVerify<AchievementSubType>().Serialize(ref writer, value.SubType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.EventName, options);
		resolver.GetFormatterWithVerify<AchievementCategoryType>().Serialize(ref writer, value.Category, options);
		writer.Write(value.SubGroup);
		resolver.GetFormatterWithVerify<DisplayProgressType>().Serialize(ref writer, value.DisplayProgress, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.CheckType, options);
		resolver.GetFormatterWithVerify<AchievementReqType>().Serialize(ref writer, value.ReqType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ReqValue, options);
		writer.Write(value.ReqValueDecrease);
		resolver.GetFormatterWithVerify<AchievementReqSubType>().Serialize(ref writer, value.ReqSub1Type, options);
		writer.Write(value.ReqSub1Value);
		resolver.GetFormatterWithVerify<AchievementReqSubType>().Serialize(ref writer, value.ReqSub2Type, options);
		writer.Write(value.ReqSub2Value);
		resolver.GetFormatterWithVerify<AchievementRewardType>().Serialize(ref writer, value.RewardType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.RewardCode, options);
		writer.Write(value.RewardValue);
		resolver.GetFormatterWithVerify<AchievementRewardType>().Serialize(ref writer, value.Reward2Type, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Reward2Code, options);
		writer.Write(value.Reward2Value);
		writer.Write(value.ResetAchievement);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MoveTaskType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.MoveTaskValue, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.GPGSAchievementId, options);
		writer.Write(value.OnlyDisplayClearFlag);
		writer.Write(value.RepeatValue);
		writer.Write(value.MenuIndex);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.OpenCondition, options);
		writer.Write(value.TutorialIndex);
		writer.Write(value.DailyReset);
		writer.Write(value.WeeklyReset);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.MonthlyResetDate, options);
		resolver.GetFormatterWithVerify<EventBonusType>().Serialize(ref writer, value.LimitedEventMissionType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.HandlerName, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.HandlerCondition, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Title, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Variable1, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Desc, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Variable2, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Goal, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Variable3, options);
	}

	public AchievementData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		AchievementData achievementData = new AchievementData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				achievementData.Index = reader.ReadInt32();
				break;
			case 1:
				achievementData.Step = reader.ReadInt32();
				break;
			case 2:
				achievementData.ShowIndex = reader.ReadInt32();
				break;
			case 3:
				achievementData.Type = resolver.GetFormatterWithVerify<AchievementType>().Deserialize(ref reader, options);
				break;
			case 4:
				achievementData.SubType = resolver.GetFormatterWithVerify<AchievementSubType>().Deserialize(ref reader, options);
				break;
			case 5:
				achievementData.EventName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				achievementData.Category = resolver.GetFormatterWithVerify<AchievementCategoryType>().Deserialize(ref reader, options);
				break;
			case 7:
				achievementData.SubGroup = reader.ReadInt32();
				break;
			case 8:
				achievementData.DisplayProgress = resolver.GetFormatterWithVerify<DisplayProgressType>().Deserialize(ref reader, options);
				break;
			case 9:
				achievementData.CheckType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				achievementData.ReqType = resolver.GetFormatterWithVerify<AchievementReqType>().Deserialize(ref reader, options);
				break;
			case 11:
				achievementData.ReqValue = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 12:
				achievementData.ReqValueDecrease = reader.ReadBoolean();
				break;
			case 13:
				achievementData.ReqSub1Type = resolver.GetFormatterWithVerify<AchievementReqSubType>().Deserialize(ref reader, options);
				break;
			case 14:
				achievementData.ReqSub1Value = reader.ReadInt64();
				break;
			case 15:
				achievementData.ReqSub2Type = resolver.GetFormatterWithVerify<AchievementReqSubType>().Deserialize(ref reader, options);
				break;
			case 16:
				achievementData.ReqSub2Value = reader.ReadInt64();
				break;
			case 17:
				achievementData.RewardType = resolver.GetFormatterWithVerify<AchievementRewardType>().Deserialize(ref reader, options);
				break;
			case 18:
				achievementData.RewardCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 19:
				achievementData.RewardValue = reader.ReadInt32();
				break;
			case 20:
				achievementData.Reward2Type = resolver.GetFormatterWithVerify<AchievementRewardType>().Deserialize(ref reader, options);
				break;
			case 21:
				achievementData.Reward2Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 22:
				achievementData.Reward2Value = reader.ReadInt32();
				break;
			case 23:
				achievementData.ResetAchievement = reader.ReadInt32();
				break;
			case 24:
				achievementData.MoveTaskType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 25:
				achievementData.MoveTaskValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 26:
				achievementData.GPGSAchievementId = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 27:
				achievementData.OnlyDisplayClearFlag = reader.ReadBoolean();
				break;
			case 28:
				achievementData.RepeatValue = reader.ReadInt32();
				break;
			case 29:
				achievementData.MenuIndex = reader.ReadInt32();
				break;
			case 30:
				achievementData.OpenCondition = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 31:
				achievementData.TutorialIndex = reader.ReadInt32();
				break;
			case 32:
				achievementData.DailyReset = reader.ReadBoolean();
				break;
			case 33:
				achievementData.WeeklyReset = reader.ReadBoolean();
				break;
			case 34:
				achievementData.MonthlyResetDate = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 35:
				achievementData.LimitedEventMissionType = resolver.GetFormatterWithVerify<EventBonusType>().Deserialize(ref reader, options);
				break;
			case 36:
				achievementData.HandlerName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 37:
				achievementData.HandlerCondition = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 38:
				achievementData.Title = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 39:
				achievementData.Variable1 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 40:
				achievementData.Desc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 41:
				achievementData.Variable2 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 42:
				achievementData.Goal = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 43:
				achievementData.Variable3 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return achievementData;
	}
}
