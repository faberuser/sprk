using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_AchievementData
{
	public static AchievementData Parse(string s)
	{
		return Parse(s, new AchievementData());
	}

	public static AchievementData Parse(string s, AchievementData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static AchievementData Parse(IDictionary d)
	{
		return Parse(d, new AchievementData());
	}

	public static AchievementData Parse(IDictionary d, AchievementData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.Step = BaseJsonMarshaler.ParseValueInt(d, "Step");
		o.ShowIndex = BaseJsonMarshaler.ParseValueInt(d, "ShowIndex");
		o.Type = BaseJsonMarshaler.ParseEnum<AchievementType>(d, "Type");
		o.SubType = BaseJsonMarshaler.ParseEnum<AchievementSubType>(d, "SubType");
		o.EventName = BaseJsonMarshaler.ParseValueString(d, "EventName");
		o.Category = BaseJsonMarshaler.ParseEnum<AchievementCategoryType>(d, "Category");
		o.SubGroup = BaseJsonMarshaler.ParseValueInt(d, "SubGroup");
		o.DisplayProgress = BaseJsonMarshaler.ParseEnum<DisplayProgressType>(d, "DisplayProgress");
		o.CheckType = BaseJsonMarshaler.ParseValueString(d, "CheckType");
		o.ReqType = BaseJsonMarshaler.ParseEnum<AchievementReqType>(d, "ReqType");
		o.ReqValue = BaseJsonMarshaler.ParseValueString(d, "ReqValue");
		o.ReqValueDecrease = BaseJsonMarshaler.ParseValueBool(d, "ReqValueDecrease");
		o.ReqSub1Type = BaseJsonMarshaler.ParseEnum<AchievementReqSubType>(d, "ReqSub1Type");
		o.ReqSub1Value = BaseJsonMarshaler.ParseValueLong(d, "ReqSub1Value");
		o.ReqSub2Type = BaseJsonMarshaler.ParseEnum<AchievementReqSubType>(d, "ReqSub2Type");
		o.ReqSub2Value = BaseJsonMarshaler.ParseValueLong(d, "ReqSub2Value");
		o.RewardType = BaseJsonMarshaler.ParseEnum<AchievementRewardType>(d, "RewardType");
		o.RewardCode = BaseJsonMarshaler.ParseValueString(d, "RewardCode");
		o.RewardValue = BaseJsonMarshaler.ParseValueInt(d, "RewardValue");
		o.Reward2Type = BaseJsonMarshaler.ParseEnum<AchievementRewardType>(d, "Reward2Type");
		o.Reward2Code = BaseJsonMarshaler.ParseValueString(d, "Reward2Code");
		o.Reward2Value = BaseJsonMarshaler.ParseValueInt(d, "Reward2Value");
		o.ResetAchievement = BaseJsonMarshaler.ParseValueInt(d, "ResetAchievement");
		o.MoveTaskType = BaseJsonMarshaler.ParseValueString(d, "MoveTaskType");
		o.MoveTaskValue = BaseJsonMarshaler.ParseArrayCast(d, "MoveTaskValue", BaseJsonMarshaler.CastToString);
		o.GPGSAchievementId = BaseJsonMarshaler.ParseValueString(d, "GPGSAchievementId");
		o.OnlyDisplayClearFlag = BaseJsonMarshaler.ParseValueBool(d, "OnlyDisplayClearFlag");
		o.RepeatValue = BaseJsonMarshaler.ParseValueInt(d, "RepeatValue");
		o.MenuIndex = BaseJsonMarshaler.ParseValueInt(d, "MenuIndex");
		o.OpenCondition = BaseJsonMarshaler.ParseArrayCast(d, "OpenCondition", BaseJsonMarshaler.CastToString);
		o.TutorialIndex = BaseJsonMarshaler.ParseValueInt(d, "TutorialIndex");
		o.DailyReset = BaseJsonMarshaler.ParseValueBool(d, "DailyReset");
		o.WeeklyReset = BaseJsonMarshaler.ParseValueBool(d, "WeeklyReset");
		o.MonthlyResetDate = BaseJsonMarshaler.ParseArrayCast(d, "MonthlyResetDate", BaseJsonMarshaler.CastToInt);
		o.LimitedEventMissionType = BaseJsonMarshaler.ParseEnum<EventBonusType>(d, "LimitedEventMissionType");
		o.HandlerName = BaseJsonMarshaler.ParseValueString(d, "HandlerName");
		o.HandlerCondition = BaseJsonMarshaler.ParseArrayCast(d, "HandlerCondition", BaseJsonMarshaler.CastToString);
		o.Title = BaseJsonMarshaler.ParseValueString(d, "Title");
		o.Variable1 = BaseJsonMarshaler.ParseArrayCast(d, "Variable1", BaseJsonMarshaler.CastToString);
		o.Desc = BaseJsonMarshaler.ParseValueString(d, "Desc");
		o.Variable2 = BaseJsonMarshaler.ParseArrayCast(d, "Variable2", BaseJsonMarshaler.CastToString);
		o.Goal = BaseJsonMarshaler.ParseValueString(d, "Goal");
		o.Variable3 = BaseJsonMarshaler.ParseArrayCast(d, "Variable3", BaseJsonMarshaler.CastToString);
		return o;
	}

	public static string EncodeJsonString(AchievementData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(AchievementData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("Step", o.Step.ToString());
		dictionary.Add("ShowIndex", o.ShowIndex.ToString());
		dictionary.Add("Type", o.Type.ToString());
		dictionary.Add("SubType", o.SubType.ToString());
		if (o.EventName != null)
		{
			dictionary.Add("EventName", o.EventName);
		}
		dictionary.Add("Category", o.Category.ToString());
		dictionary.Add("SubGroup", o.SubGroup.ToString());
		dictionary.Add("DisplayProgress", o.DisplayProgress.ToString());
		if (o.CheckType != null)
		{
			dictionary.Add("CheckType", o.CheckType);
		}
		dictionary.Add("ReqType", o.ReqType.ToString());
		if (o.ReqValue != null)
		{
			dictionary.Add("ReqValue", o.ReqValue);
		}
		dictionary.Add("ReqValueDecrease", o.ReqValueDecrease.ToString());
		dictionary.Add("ReqSub1Type", o.ReqSub1Type.ToString());
		dictionary.Add("ReqSub1Value", o.ReqSub1Value.ToString());
		dictionary.Add("ReqSub2Type", o.ReqSub2Type.ToString());
		dictionary.Add("ReqSub2Value", o.ReqSub2Value.ToString());
		dictionary.Add("RewardType", o.RewardType.ToString());
		if (o.RewardCode != null)
		{
			dictionary.Add("RewardCode", o.RewardCode);
		}
		dictionary.Add("RewardValue", o.RewardValue.ToString());
		dictionary.Add("Reward2Type", o.Reward2Type.ToString());
		if (o.Reward2Code != null)
		{
			dictionary.Add("Reward2Code", o.Reward2Code);
		}
		dictionary.Add("Reward2Value", o.Reward2Value.ToString());
		dictionary.Add("ResetAchievement", o.ResetAchievement.ToString());
		if (o.MoveTaskType != null)
		{
			dictionary.Add("MoveTaskType", o.MoveTaskType);
		}
		dictionary.Add("MoveTaskValue", BaseJsonMarshaler.EncodeArray(o.MoveTaskValue));
		if (o.GPGSAchievementId != null)
		{
			dictionary.Add("GPGSAchievementId", o.GPGSAchievementId);
		}
		dictionary.Add("OnlyDisplayClearFlag", o.OnlyDisplayClearFlag.ToString());
		dictionary.Add("RepeatValue", o.RepeatValue.ToString());
		dictionary.Add("MenuIndex", o.MenuIndex.ToString());
		dictionary.Add("OpenCondition", BaseJsonMarshaler.EncodeArray(o.OpenCondition));
		dictionary.Add("TutorialIndex", o.TutorialIndex.ToString());
		dictionary.Add("DailyReset", o.DailyReset.ToString());
		dictionary.Add("WeeklyReset", o.WeeklyReset.ToString());
		dictionary.Add("MonthlyResetDate", BaseJsonMarshaler.EncodeArray(o.MonthlyResetDate));
		dictionary.Add("LimitedEventMissionType", o.LimitedEventMissionType.ToString());
		if (o.HandlerName != null)
		{
			dictionary.Add("HandlerName", o.HandlerName);
		}
		dictionary.Add("HandlerCondition", BaseJsonMarshaler.EncodeArray(o.HandlerCondition));
		if (o.Title != null)
		{
			dictionary.Add("Title", o.Title);
		}
		dictionary.Add("Variable1", BaseJsonMarshaler.EncodeArray(o.Variable1));
		if (o.Desc != null)
		{
			dictionary.Add("Desc", o.Desc);
		}
		dictionary.Add("Variable2", BaseJsonMarshaler.EncodeArray(o.Variable2));
		if (o.Goal != null)
		{
			dictionary.Add("Goal", o.Goal);
		}
		dictionary.Add("Variable3", BaseJsonMarshaler.EncodeArray(o.Variable3));
		return dictionary;
	}
}
