using System;
using NGame2.NQuest;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NDescription;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;

namespace NGame2.NAchievement;

public class AchievementTextCombine : SingletonAuto<AchievementTextCombine>, AchievementComponent.ITextProvider, AchievementClearedPopup.ITextProvider
{
	string AchievementComponent.ITextProvider.GetAchievementName(AchievementData achievementData)
	{
		return GetAchievementName(achievementData);
	}

	string AchievementComponent.ITextProvider.GetAchievementDesc(AchievementData achievementData)
	{
		return GetAchievementDesc(achievementData);
	}

	string AchievementComponent.ITextProvider.GetAchievementRequirement(AchievementData achievementData, AchievementInfo achievementInfo)
	{
		return GetAchievementRequirement(achievementData, achievementInfo);
	}

	string AchievementComponent.ITextProvider.GetAchievementGoalRequirement(AchievementData achievementData, AchievementInfo achievementInfo)
	{
		return GetAchievementGoalRequirement(achievementData, achievementInfo);
	}

	string AchievementClearedPopup.ITextProvider.GetAchievementName(AchievementData achievementData)
	{
		return GetAchievementName(achievementData);
	}

	string AchievementClearedPopup.ITextProvider.GetAchievementDesc(AchievementData achievementData)
	{
		return GetAchievementDesc(achievementData);
	}

	private string GetAchievementName(AchievementData achievementData)
	{
		if (achievementData == null)
		{
			return string.Empty;
		}
		return achievementData.Type switch
		{
			AchievementType.Daily => GetDailyAchievementName(achievementData.Index), 
			AchievementType.Hero => GetHeroAchievementName(achievementData), 
			_ => GetDefaultAchievementName(achievementData), 
		};
	}

	private string GetDailyAchievementName(int AchievementIndex)
	{
		AchievementTextData data = GameRoot.Instance.AchievementTextDataContainer.GetData(AchievementIndex);
		if (data == null)
		{
			return string.Empty;
		}
		return LocalizationManager.GetStringDirect(data.AchievementNameTextKey);
	}

	private string GetHeroAchievementName(AchievementData achievementData)
	{
		int key = Vespa_String_Extension_Method.TOINT(achievementData.ReqValue, 0);
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(key);
		if (data == null)
		{
			return string.Empty;
		}
		string name = data.Name;
		AchievementTextData achievementTextData = null;
		achievementTextData = ((achievementData.ReqSub1Type != AchievementReqSubType.Level && achievementData.ReqSub1Type != AchievementReqSubType.Star && achievementData.ReqSub1Type != AchievementReqSubType.Transcendence) ? GameRoot.Instance.AchievementTextDataContainer.GetData(achievementData.Index) : GameRoot.Instance.AchievementTextDataContainer.GetData(achievementData.Index, achievementData.Step));
		if (achievementTextData == null)
		{
			return string.Empty;
		}
		switch (achievementData.ReqSub1Type)
		{
		case AchievementReqSubType.Grade:
		case AchievementReqSubType.MasterSkillExtendSlot:
			return LocalizationManager.GetStringDirect(achievementTextData.AchievementNameTextKey, achievementTextData.DisplayValue);
		case AchievementReqSubType.ExcavateCount:
			return LocalizationManager.GetStringDirect(achievementTextData.AchievementNameTextKey, name, achievementData.Step);
		case AchievementReqSubType.Level:
		case AchievementReqSubType.Star:
		case AchievementReqSubType.Transcendence:
			return LocalizationManager.GetStringDirect(achievementTextData.AchievementNameTextKey, name, achievementTextData.DisplayValue);
		case AchievementReqSubType.AllMasterSkillExtend:
			return LocalizationManager.GetStringDirect(achievementTextData.AchievementNameTextKey, name);
		default:
			return string.Empty;
		}
	}

	private string GetDefaultAchievementName(AchievementData achievementData)
	{
		AchievementTextData data = GameRoot.Instance.AchievementTextDataContainer.GetData(achievementData.Index);
		if (data == null)
		{
			return string.Empty;
		}
		string text = string.Empty;
		if (achievementData.ReqType == AchievementReqType.ClearAllNodeOfChapter)
		{
			int key = Vespa_String_Extension_Method.TOINT(achievementData.ReqValue, 0);
			CampaignChapterData data2 = GameRoot.Instance.CampaignChapterDataContainer.GetData(key);
			if (data2 == null)
			{
				return string.Empty;
			}
			string stringDirect = LocalizationManager.GetStringDirect(data2.Name);
			text = LocalizationManager.GetStringDirect(data.AchievementNameTextKey, stringDirect);
		}
		else if (achievementData.ReqType == AchievementReqType.GetItemType)
		{
			if (achievementData.ReqSub1Type == AchievementReqSubType.Grade)
			{
				string gradeText = GetGradeText(achievementData.ReqSub1Value);
				text = LocalizationManager.GetStringDirect(data.AchievementNameTextKey, gradeText, achievementData.Step);
			}
		}
		else if (achievementData.ReqType == AchievementReqType.FirstGetItem || achievementData.ReqType == AchievementReqType.HeroCountToEquipGrade || achievementData.ReqType == AchievementReqType.UndergroundPrisonFirstClear || achievementData.ReqType == AchievementReqType.EquipLevelUp)
		{
			text = LocalizationManager.GetStringDirect(data.AchievementNameTextKey, data.DisplayValue);
		}
		else if (achievementData.ReqType == AchievementReqType.GetStarEquipment)
		{
			text = LocalizationManager.GetStringDirect(data.AchievementNameTextKey, achievementData.ReqValue, achievementData.Step);
		}
		else if (achievementData.ReqType == AchievementReqType.MatchGrade || achievementData.ReqType == AchievementReqType.MatchBanPickGrade || achievementData.ReqType == AchievementReqType.GlobalMatchBanPickGrade || achievementData.ReqType == AchievementReqType.TranscendHero || achievementData.ReqType == AchievementReqType.GetSpecialEquipItem)
		{
			data = GameRoot.Instance.AchievementTextDataContainer.GetData(achievementData.Index, achievementData.Step);
		}
		if (String_Extension_Method.IsNullOrEmpty(text))
		{
			text = LocalizationManager.GetStringDirect(data.AchievementNameTextKey, achievementData.Step);
		}
		return "[CCFFFF]" + text + "[-]";
	}

	public string GetAchievementRequirement(AchievementData achievementData, AchievementInfo achievementInfo)
	{
		if (GameRoot.Instance.AchievementTextDataContainer.GetData(achievementData.Index) == null)
		{
			return string.Empty;
		}
		string achievementDesc = GetAchievementDesc(achievementData);
		ulong num = (ulong)(achievementInfo?.Achievement ?? 0);
		if (achievementData.ReqType == AchievementReqType.AllClearUndergroundPrison)
		{
			int value = (int)((achievementInfo != null) ? achievementInfo.Achievement : 0);
			value = GetCountOfBit(value);
			int countOfBit = GetCountOfBit(Vespa_String_Extension_Method.TOINT(achievementData.ReqValue, 0));
			return achievementDesc + " (" + value + "/" + countOfBit + ")";
		}
		if (achievementData.ReqSub2Type != AchievementReqSubType.None)
		{
			return achievementDesc + " (" + num + "/" + achievementData.ReqSub2Value + ")";
		}
		if (achievementData.ReqSub1Type != AchievementReqSubType.None)
		{
			return achievementDesc + " (" + num + "/" + achievementData.ReqSub1Value + ")";
		}
		return achievementDesc + " (" + num + "/" + achievementData.ReqValue + ")";
	}

	public string GetAchievementGoalRequirement(AchievementData achievementData, AchievementInfo achievementInfo)
	{
		if (GameRoot.Instance.AchievementTextDataContainer.GetData(achievementData.Index) == null)
		{
			return string.Empty;
		}
		string localizeMissionText = SingletonUpdater<QuestManager>.instance.GetLocalizeMissionText(achievementData.Goal, achievementData.Variable3);
		ulong num = (ulong)(achievementInfo?.Achievement ?? 0);
		if (achievementData.ReqType == AchievementReqType.AllClearUndergroundPrison)
		{
			int value = (int)((achievementInfo != null) ? achievementInfo.Achievement : 0);
			value = GetCountOfBit(value);
			int countOfBit = GetCountOfBit(Vespa_String_Extension_Method.TOINT(achievementData.ReqValue, 0));
			return localizeMissionText + " (" + value + "/" + countOfBit + ")";
		}
		if (achievementData.ReqSub2Type != AchievementReqSubType.None)
		{
			return localizeMissionText + " (" + num + "/" + achievementData.ReqSub2Value + ")";
		}
		if (achievementData.ReqSub1Type != AchievementReqSubType.None)
		{
			return localizeMissionText + " (" + num + "/" + achievementData.ReqSub1Value + ")";
		}
		return localizeMissionText + " (" + num + "/" + achievementData.ReqValue + ")";
	}

	private int GetCountOfBit(int value)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i <= 6; i++)
		{
			num = 1 << i;
			if ((value & num) == num)
			{
				num2++;
			}
		}
		return num2;
	}

	private string GetAchievementDesc(AchievementData achievementData)
	{
		switch (achievementData.Type)
		{
		case AchievementType.Daily:
		case AchievementType.Weekly:
			return GetDailyAchievementDesc(achievementData);
		case AchievementType.Hero:
			return GetHeroAchievementDesc(achievementData);
		case AchievementType.DailyEvent:
			return GetDailyEventAchievementDesc(achievementData);
		default:
			return GetDefaultAchievementDesc(achievementData);
		}
	}

	private string GetDailyAchievementDesc(AchievementData achievementData)
	{
		AchievementTextData data = GameRoot.Instance.AchievementTextDataContainer.GetData(achievementData.Index);
		if (data == null)
		{
			return string.Empty;
		}
		return LocalizationManager.GetStringDirect(data.AchievementDescTextKey, achievementData.ReqValue);
	}

	private string GetDailyEventAchievementDesc(AchievementData achievementData)
	{
		AchievementTextData data = GameRoot.Instance.AchievementTextDataContainer.GetData(achievementData.Index);
		if (data == null)
		{
			return string.Empty;
		}
		if (achievementData.ReqType == AchievementReqType.LoginOrConnectedTime)
		{
			string text = $"{achievementData.ReqSub1Value}:00";
			string text2 = $"{achievementData.ReqSub2Value}:00";
			return LocalizationManager.GetStringDirect(data.AchievementDescTextKey, text, text2);
		}
		return GetDefaultAchievementDesc(achievementData);
	}

	private string GetHeroAchievementDesc(AchievementData achievementData)
	{
		int key = Vespa_String_Extension_Method.TOINT(achievementData.ReqValue, 0);
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(key);
		if (data == null)
		{
			return string.Empty;
		}
		string name = data.Name;
		AchievementTextData data2 = GameRoot.Instance.AchievementTextDataContainer.GetData(achievementData.Index);
		if (data2 == null)
		{
			return string.Empty;
		}
		switch (achievementData.ReqSub1Type)
		{
		case AchievementReqSubType.Star:
		{
			string starNumberText = GetStarNumberText(achievementData.ReqSub1Value);
			return LocalizationManager.GetStringDirect(data2.AchievementDescTextKey, name, starNumberText);
		}
		case AchievementReqSubType.Grade:
		{
			string gradeText = GetGradeText(achievementData.ReqSub1Value);
			return LocalizationManager.GetStringDirect(data2.AchievementDescTextKey, name, gradeText);
		}
		case AchievementReqSubType.Level:
		case AchievementReqSubType.ExcavateCount:
		case AchievementReqSubType.EmployCount:
		case AchievementReqSubType.JoinMember:
		case AchievementReqSubType.Transcendence:
			return LocalizationManager.GetStringDirect(data2.AchievementDescTextKey, name, achievementData.ReqSub1Value);
		case AchievementReqSubType.MasterSkillExtendSlot:
		{
			long reqSub1Value = achievementData.ReqSub1Value;
			int skillIndexBySlot = data.GetSkillIndexBySlot((int)reqSub1Value);
			if (skillIndexBySlot != 0)
			{
				SkillData data3 = GameRoot.Instance.SkillDataContainer.GetData(skillIndexBySlot);
				if (data3 != null)
				{
					string skillName = SkillDescContainer.GetSkillName(data3);
					return LocalizationManager.GetStringDirect(data2.AchievementDescTextKey, skillName);
				}
			}
			break;
		}
		case AchievementReqSubType.AllMasterSkillExtend:
			return LocalizationManager.GetStringDirect(data2.AchievementDescTextKey);
		}
		return string.Empty;
	}

	private string GetDefaultAchievementDesc(AchievementData achievementData)
	{
		AchievementTextData data = GameRoot.Instance.AchievementTextDataContainer.GetData(achievementData.Index);
		if (data == null)
		{
			return string.Empty;
		}
		switch (achievementData.ReqType)
		{
		case AchievementReqType.GetItemType:
			if (achievementData.ReqSub1Type == AchievementReqSubType.Count)
			{
				return LocalizationManager.GetStringDirect(data.AchievementDescTextKey, achievementData.ReqSub1Value);
			}
			if (achievementData.ReqSub1Type == AchievementReqSubType.Grade)
			{
				string gradeText = GetGradeText(achievementData.ReqSub1Value);
				return LocalizationManager.GetStringDirect(data.AchievementDescTextKey, gradeText, achievementData.ReqSub2Value);
			}
			break;
		case AchievementReqType.HeroCountToEquipGrade:
			return LocalizationManager.GetStringDirect(data.AchievementDescTextKey, GetGradeText(achievementData.ReqSub1Value));
		case AchievementReqType.GetStarEquipment:
			return LocalizationManager.GetStringDirect(data.AchievementDescTextKey, achievementData.ReqValue, achievementData.ReqSub1Value);
		case AchievementReqType.ExecEquipGacha:
		case AchievementReqType.GetGradeEquipment:
		case AchievementReqType.ConsumeStamina:
		case AchievementReqType.ConsumePackageItem:
		case AchievementReqType.ClearTower:
			return LocalizationManager.GetStringDirect(data.AchievementDescTextKey, achievementData.ReqSub1Value);
		case AchievementReqType.FirstGetItem:
			if (Enum.IsDefined(typeof(ItemType), achievementData.ReqValue))
			{
				if (EnumTools.Parse(achievementData.ReqValue, ItemType.None) == ItemType.Equip)
				{
					switch (achievementData.ReqSub1Type)
					{
					case AchievementReqSubType.Star:
						return LocalizationManager.GetStringDirect(data.AchievementDescTextKey, GetStarNumberText(achievementData.ReqSub1Value));
					case AchievementReqSubType.Grade:
						return LocalizationManager.GetStringDirect(data.AchievementDescTextKey, GetGradeText(achievementData.ReqSub1Value));
					}
				}
			}
			else
			{
				VespaDebug.LogError("invalid item type value = " + achievementData.ReqValue);
			}
			break;
		case AchievementReqType.MatchGrade:
		case AchievementReqType.MatchBanPickGrade:
		case AchievementReqType.GlobalMatchBanPickGrade:
			data = GameRoot.Instance.AchievementTextDataContainer.GetData(achievementData.Index, achievementData.Step);
			return LocalizationManager.GetStringDirect(data.AchievementDescTextKey);
		case AchievementReqType.UndergroundPrisonFirstClear:
			return LocalizationManager.GetStringDirect(data.AchievementDescTextKey, data.DisplayValue);
		case AchievementReqType.GetItemIndex:
		{
			int result = 0;
			if (!int.TryParse(achievementData.ReqValue, out result))
			{
				return string.Empty;
			}
			ItemData data2 = GameRoot.Instance.ItemDataContainer.GetData(result);
			if (data2 == null)
			{
				return string.Empty;
			}
			string name = data2.Name;
			return LocalizationManager.GetStringDirect(data.AchievementDescTextKey, name, achievementData.ReqSub1Value);
		}
		case AchievementReqType.RecvRewardFriendlyPoint:
			return LocalizationManager.GetStringDirect(data.AchievementDescTextKey, achievementData.ReqSub1Value);
		case AchievementReqType.ClearChapterDungeon:
			if (achievementData.ReqSub2Value > 0)
			{
				return LocalizationManager.GetStringDirect(data.AchievementDescTextKey, achievementData.ReqSub2Value);
			}
			return LocalizationManager.GetStringDirect(data.AchievementDescTextKey, achievementData.ReqValue);
		case AchievementReqType.LimitedMazeClearRewardClearCheck:
		case AchievementReqType.TranscendHero:
		case AchievementReqType.GetSpecialEquipItem:
			data = GameRoot.Instance.AchievementTextDataContainer.GetData(achievementData.Index, achievementData.Step);
			return LocalizationManager.GetStringDirect(data.AchievementDescTextKey);
		default:
			return LocalizationManager.GetStringDirect(data.AchievementDescTextKey, achievementData.ReqValue);
		}
		return string.Empty;
	}

	private string GetGradeText(long Grade)
	{
		long num = Grade - 1;
		if ((ulong)num <= 4uL)
		{
			switch (num)
			{
			case 0L:
				return LocalizationManager.GetString(Achievement.GRADE_WHITE2);
			case 1L:
				return LocalizationManager.GetString(Achievement.GRADE_GREEN2);
			case 2L:
				return LocalizationManager.GetString(Achievement.GRADE_BLUE2);
			case 3L:
				return LocalizationManager.GetString(Achievement.GRADE_PURPLE2);
			case 4L:
				return LocalizationManager.GetString(Achievement.GRADE_ORANGE2);
			}
		}
		return string.Empty;
	}

	private string GetStarNumberText(long Star)
	{
		return "★" + Star;
	}

	private string GetStarText(long Star)
	{
		string text = string.Empty;
		for (int i = 1; i <= Star; i++)
		{
			text += "★";
		}
		return text;
	}
}
