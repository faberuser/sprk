using System;
using System.Text;
using NGame2.NAccount;
using NGame2.NContents;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUtil;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI;

internal static class Common
{
	public static void ShowEnterClassBanMessage(RaidBanType banType, string messageKey, int enterCount)
	{
		if (banType == RaidBanType.ClassEnterPermission)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect(messageKey, enterCount));
		}
	}

	public static void SetupSkillTypeInfo(UILabel labelCoolTime, HeroSkillDesc.SkillManaUiGroup skillManaUi, HeroInfo heroInfo, Helper.SkillDetailInfo skillInfo)
	{
		int num = ((skillInfo.BasicInfo.TableData != null) ? skillInfo.BasicInfo.TableData.CoolTimeMs : 0);
		if (num != 0)
		{
			int num2 = num / 1000;
			int num3 = (num - num2 * 1000) / 100;
			string text = ((num3 == 0) ? num2.ToString() : (num2 + "." + num3));
			UIHelper.EnableGameObject(labelCoolTime, enable: true);
			UIHelper.SetLabelText(labelCoolTime, LocalizationManager.GetString(HeroSkill.SKILL_COOLTIME_DESC, text));
		}
		else
		{
			UIHelper.EnableGameObject(labelCoolTime, enable: false);
		}
		skillManaUi.Setup(heroInfo, skillInfo);
	}

	public static bool CheckContentsOpenCondition(string[] openCondition)
	{
		if (openCondition == null || openCondition.Length == 0)
		{
			return true;
		}
		string text = openCondition[0].Trim();
		if (text.Equals("System"))
		{
			string text2 = openCondition[1].Trim();
			if (text2.Equals("MenuOpenCheckManager"))
			{
				string text3 = openCondition[2].Trim();
				ERightMenuButton o_output = ERightMenuButton.None;
				Vespa_String_Extension_Method.TryToEnum<ERightMenuButton>(text3, out o_output);
				return SingletonAuto<MenuOpenCheckManager>.instance.IsOpen(o_output);
			}
			if (text2.Equals("MatchManagement"))
			{
				openCondition[2].Trim();
				int[] heroIndices;
				return MatchManagement.CheckBanPickCondition(out heroIndices);
			}
			if (text2.Equals("CanGoDungeon"))
			{
				int chapterindex = Convert.ToInt32(Vespa_String_Extension_Method.TRIM(openCondition[2]));
				int dungeonIndex = Convert.ToInt32(Vespa_String_Extension_Method.TRIM(openCondition[3]));
				return LobbyManagement.CanGoDungeon(chapterindex, dungeonIndex);
			}
		}
		if (text.Equals("Table"))
		{
			string text4 = openCondition[1].Trim();
			if (text4.Equals("NPCTable"))
			{
				string key = openCondition[2].Trim();
				NPCData data = GameRoot.Instance.NPCDataContainer.GetData(key);
				if (data == null)
				{
					return false;
				}
				string text5 = openCondition[3].Trim();
				if (text5.Equals("EntryMenuCondition1"))
				{
					return NGame2.NUI.NWindow.EntryMenu.CheckEntryMenuCondition(null, data.EntryMenuCondition1);
				}
				if (text5.Equals("EntryMenuCondition2"))
				{
					return NGame2.NUI.NWindow.EntryMenu.CheckEntryMenuCondition(null, data.EntryMenuCondition2);
				}
				if (text5.Equals("EntryMenuCondition3"))
				{
					return NGame2.NUI.NWindow.EntryMenu.CheckEntryMenuCondition(null, data.EntryMenuCondition3);
				}
			}
			if (text4.Equals("EventTriggerTable"))
			{
				short key2 = Convert.ToInt16(openCondition[2].Trim());
				EventTriggerData data2 = GameRoot.Instance.EventTriggerDataContainer.GetData(key2);
				if (data2 == null)
				{
					return false;
				}
				return SingletonAuto<ConditionChecker>.instance.Check(data2.CurrentCondition);
			}
		}
		return false;
	}

	public static bool IsClearTutorial(AdventureNoticeData adventureNoticeData)
	{
		if (adventureNoticeData == null)
		{
			return true;
		}
		if (adventureNoticeData.TutorialIndex == 0)
		{
			return true;
		}
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance || GlobalAutoBehaviour<AccountManager>.instance.TutorialManager == null)
		{
			return true;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.TutorialManager.IsComplete(adventureNoticeData.TutorialIndex);
	}

	public static bool CheckFirstOpen(AdventureNoticeData adventureNoticeData)
	{
		return new LocalAccountBool(i_default: false, typeof(PortalComponent2).FullName + "_AdventureNotice_" + adventureNoticeData.Code).Value;
	}

	public static void SetFirstOpen(bool opened, AdventureNoticeData adventureNoticeData)
	{
		new LocalAccountBool(i_default: false, typeof(PortalComponent2).FullName + "_AdventureNotice_" + adventureNoticeData.Code).Value = opened;
	}

	public static bool CheckContentsStatus(AdventureNoticeData adventureNoticeData)
	{
		if (adventureNoticeData == null)
		{
			return false;
		}
		if (adventureNoticeData.StaminaType == StaminaType.Sword)
		{
			return SingletonAuto<ContentsStatusManager>.instance.GetContentsStatus(ContentsStatusType.Arena) == ContentsStatus.On;
		}
		if (adventureNoticeData.StaminaType == StaminaType.Sword2)
		{
			return SingletonAuto<ContentsStatusManager>.instance.GetContentsStatus(ContentsStatusType.BanPick) == ContentsStatus.On;
		}
		return true;
	}

	public static bool IsShowBoosterInfoList(EventBonusType bonusType)
	{
		switch (bonusType)
		{
		case EventBonusType.BonusExp:
		case EventBonusType.BonusItem:
		case EventBonusType.BonusGold:
		case EventBonusType.BonusStamina:
		case EventBonusType.BonusEventDungeonCount:
		case EventBonusType.BonusFriendlyPoint:
		case EventBonusType.DiscountPurify:
		case EventBonusType.DiscountKeepRune:
		case EventBonusType.DiscountAwakeEquip:
		case EventBonusType.BonusRaidPoint:
		case EventBonusType.OpenAllPrison:
		case EventBonusType.DiscountUpgradeOption:
		case EventBonusType.BonusWorldBossCurrency:
		case EventBonusType.BonusHideoutDungeonCount:
		case EventBonusType.DiscountEquipItemOptionRenew:
		case EventBonusType.DiscountEquipItemOptionUpgrade:
		case EventBonusType.BonusRaidReward:
		case EventBonusType.BonusDailyMissionReward:
		case EventBonusType.BonusEquipSellReward:
		case EventBonusType.BonusEquipBreakReward:
		case EventBonusType.NpcBuff:
		case EventBonusType.SaveStamina:
		case EventBonusType.ClassBuff:
		case EventBonusType.PetBuff:
		case EventBonusType.SaveDispatchStamina:
			return true;
		default:
			return false;
		}
	}

	public static Enum GetDayOfWeekKey(DayOfWeek dow)
	{
		return dow switch
		{
			DayOfWeek.Monday => EventDungeon.DAY_OF_WEEK_MON, 
			DayOfWeek.Tuesday => EventDungeon.DAY_OF_WEEK_TUE, 
			DayOfWeek.Wednesday => EventDungeon.DAY_OF_WEEK_WED, 
			DayOfWeek.Thursday => EventDungeon.DAY_OF_WEEK_THU, 
			DayOfWeek.Friday => EventDungeon.DAY_OF_WEEK_FRI, 
			DayOfWeek.Saturday => EventDungeon.DAY_OF_WEEK_SAT, 
			DayOfWeek.Sunday => EventDungeon.DAY_OF_WEEK_SUN, 
			_ => EventDungeon.INVALID_DOW_VALUE, 
		};
	}

	public static TimeSpan GetRemainTime(string completeTimeStr)
	{
		Vespa_String_Extension_Method2.TryParseNetworkTime2(completeTimeStr, out var o_dateTime);
		return o_dateTime - NGame2.NUI.NWindow.Common.GetUTCNow();
	}

	public static TimeSpan GetRemainTime(DateTime completeTime)
	{
		return completeTime - NGame2.NUI.NWindow.Common.GetUTCNow();
	}

	public static int GetAdminRequireLevel()
	{
		int defaultValue = 2;
		return GameRoot.Instance.ConstantDataContainer.GetValueInt("AdminRequireLevel", defaultValue);
	}

	public static Enum GetOrdinalizeDayFormatKey(int day)
	{
		SystemLanguage language = SingletonAuto<LocalizationManager>.instance.Language;
		NShared.NLocalization.System system = NShared.NLocalization.System.ORDINAL_DAY_DEFAULT;
		switch (day)
		{
		case 1:
			system = NShared.NLocalization.System.ORDINAL_DAY_1ST;
			break;
		case 21:
		case 31:
			system = ((language != SystemLanguage.English) ? NShared.NLocalization.System.ORDINAL_DAY_DEFAULT : NShared.NLocalization.System.ORDINAL_DAY_1ST);
			break;
		case 2:
		case 22:
			system = NShared.NLocalization.System.ORDINAL_DAY_2ND;
			break;
		case 3:
		case 23:
			system = NShared.NLocalization.System.ORDINAL_DAY_3RD;
			break;
		default:
			system = NShared.NLocalization.System.ORDINAL_DAY_DEFAULT;
			break;
		}
		return system;
	}

	public static string GetOrdinalizeDay(int day)
	{
		return LocalizationManager.GetString(GetOrdinalizeDayFormatKey(day), day);
	}

	public static string GetLocalMonthName(int month)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("SYSTEM.MONTH_NAME_");
		stringBuilder.Append(month);
		return LocalizationManager.GetStringDirect(stringBuilder.ToString());
	}
}
