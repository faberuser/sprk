using System;
using System.Collections;
using NGame2.NAccount;
using NGame2.NContentsInfo;
using NGame2.NDropBonusEvent;
using NGame2.NGuild;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NPayShop;
using NGame2.NUI.NWindow.NTower;
using NGame2.NUtil;
using NGameAsset;
using NShared;
using NShared.NLocalization;
using NShared.NWorldBoss;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NHelper.NPortal;

public static class PortalHelper
{
	private static bool CheckGuildRaidOpenTime
	{
		get
		{
			if (!GlobalAutoBehaviour<GuildManager>.isValidInstance)
			{
				return false;
			}
			if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager == null)
			{
				return false;
			}
			return GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildRaidManager?.CheckOpenTime ?? false;
		}
	}

	public static PortalRenewal.NoticeState GetContentsNoticeState(PortalRenewalData portalRenewalData, bool updateState = true)
	{
		PortalRenewal.NoticeState noticeState = PortalRenewal.NoticeState.NONE;
		bool flag = CheckIsNewContents(portalRenewalData, updateState);
		bool flag2 = CheckIsNoticeContents(portalRenewalData);
		if (flag && flag2)
		{
			noticeState = PortalRenewal.NoticeState.UNCONFIRMED_AND_NEW;
		}
		else if (flag)
		{
			noticeState = PortalRenewal.NoticeState.NEW;
		}
		else if (flag2)
		{
			noticeState = PortalRenewal.NoticeState.UNCONFIRMED;
		}
		if (portalRenewalData.Category == PortalRenewalCategory.AdventureNotice)
		{
			switch (noticeState)
			{
			case PortalRenewal.NoticeState.UNCONFIRMED_AND_NEW:
				noticeState = PortalRenewal.NoticeState.NEW;
				break;
			case PortalRenewal.NoticeState.UNCONFIRMED:
				noticeState = PortalRenewal.NoticeState.NONE;
				break;
			}
		}
		return noticeState;
	}

	public static bool CheckIsNewContents(PortalRenewalData portalRenewalData, bool updateState = true)
	{
		if (portalRenewalData.Category == PortalRenewalCategory.Chapter)
		{
			return false;
		}
		if (portalRenewalData.Category == PortalRenewalCategory.Shop && (portalRenewalData.SubCategory == PortalRenewalSubCategory.TotalShop || portalRenewalData.SubCategory == PortalRenewalSubCategory.MayGemShop))
		{
			DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
			ShopData data = GameRoot.Instance.ShopDataContainer.GetData(portalRenewalData.ShopIndex);
			ShopRestockTimeInfo shopRestockTimeInfo = GlobalAutoBehaviour<AccountManager>.instance.ShopRestockTimeManager.Get(data.Index);
			if (shopRestockTimeInfo != null)
			{
				DateTime o_dateTime = DateTime.MinValue;
				if (Vespa_String_Extension_Method2.TryParseNetworkTime2(shopRestockTimeInfo.RestockTime, out o_dateTime))
				{
					int num = ((data.RestockSecond > 0) ? data.RestockSecond : 3600);
					if (currentServerUTCTime > o_dateTime.AddSeconds(num))
					{
						return true;
					}
				}
				return false;
			}
			return true;
		}
		LocalAccountBool localAccountBool = new LocalAccountBool(i_default: false, $"PortalRenewal_NewState_{portalRenewalData.Category}_{portalRenewalData.SubCategory}");
		if (!localAccountBool.Value)
		{
			if (updateState)
			{
				localAccountBool.Value = true;
			}
			return true;
		}
		return false;
	}

	public static bool CheckIsNoticeContents(PortalRenewalData portalData)
	{
		switch (portalData.SubCategory)
		{
		case PortalRenewalSubCategory.ChallengeTower:
		case PortalRenewalSubCategory.GodkingTrial:
		case PortalRenewalSubCategory.Eclipse:
			return IsFullChargeCurrency(portalData);
		case PortalRenewalSubCategory.Recovery:
			return IsEnoughResourceItem(portalData);
		case PortalRenewalSubCategory.GodkingSanctum:
			return IsAnyNoticeSoulWeapon();
		case PortalRenewalSubCategory.TrialTower:
			return IsNoticeTower(portalData);
		case PortalRenewalSubCategory.ShakmehMiddle:
			return IsNoticeShakmehMiddle();
		case PortalRenewalSubCategory.ShakmehFinal:
			return IsNoticeShakmehFinal();
		case PortalRenewalSubCategory.LeagueOfVictory:
		case PortalRenewalSubCategory.LeagueOfGlory:
			VespaCoroutine.Start(coNeedNetworkContents(portalData));
			break;
		case PortalRenewalSubCategory.GuildArena:
		case PortalRenewalSubCategory.GuildSuppress:
		case PortalRenewalSubCategory.GuildRaid:
			if (GuildManager.GuildValidGuildPenaltyType() != GuildPenaltyType.None)
			{
				return false;
			}
			VespaCoroutine.Start(coNeedNetworkContents(portalData));
			break;
		case PortalRenewalSubCategory.GuildHeadquarter:
			return IsNoticeGuildAttendance();
		case PortalRenewalSubCategory.TreasureHouse:
			return CheckTreasureHouseEnterDungeon();
		}
		return false;
	}

	private static bool CheckTreasureHouseEnterDungeon()
	{
		if (TreasureHouseManagement.CheckTreasureHouseDungeonCleard())
		{
			return false;
		}
		CurrencyTypeData data = GameRoot.Instance.CurrencyTypeDataContainer.GetData(CurrencyType.TreasureHousePoint);
		if (data == null)
		{
			return false;
		}
		long userStat = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(UserManager.UserStatType.TreasureHousePoint);
		int maxValue = data.MaxValue;
		return userStat >= maxValue;
	}

	private static bool GuildArenaNoticeExceptionChecker(int sessionIndex)
	{
		string i_key = $"PortalRenewal_GuildArenaCompareKey";
		if (new LocalAccountInt(0, i_key).Value >= sessionIndex)
		{
			return false;
		}
		string i_key2 = $"PortalRenewal_GuildArena_{sessionIndex}";
		if (new LocalAccountBool(i_default: false, i_key2).Value)
		{
			return false;
		}
		return true;
	}

	public static void UpdateGuildArenaEnterInfo()
	{
		GuildArenaSessionInfo guildArenaSessionInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetGuildArenaSessionInfo();
		if (guildArenaSessionInfo == null || !Vespa_String_Extension_Method2.TryParseNetworkTime2(guildArenaSessionInfo.BattleBeginTime, out var o_dateTime))
		{
			return;
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		bool num = guildArenaSessionInfo.State == GuildArenaSessionState.Battle;
		bool flag = currentServerUTCTime >= o_dateTime;
		if (!num || !flag)
		{
			return;
		}
		string i_key = $"PortalRenewal_GuildArenaCompareKey";
		LocalAccountInt localAccountInt = new LocalAccountInt(0, i_key);
		int sessionIndex = guildArenaSessionInfo.SessionIndex;
		localAccountInt.Value = sessionIndex;
		string i_key2 = $"PortalRenewal_GuildArena_{guildArenaSessionInfo.SessionIndex}";
		new LocalAccountBool(i_default: false, i_key2).Value = true;
		PortalRenewal instance = SceneAutoBehaviour<PortalManagement>.instance.GetInstance<PortalRenewal>(openedOnly: true);
		if (!(instance == null))
		{
			PortalContentsComponent contentsComponentBySubCategory = instance.GetContentsComponentBySubCategory(PortalRenewalSubCategory.GuildArena);
			if (contentsComponentBySubCategory != null)
			{
				contentsComponentBySubCategory.ChangeNoticeState(PortalRenewal.NoticeState.NONE);
			}
			ContentsMenuComponent menuCompByCategory = instance.GetMenuCompByCategory(PortalRenewalCategory.Guild);
			if (menuCompByCategory != null)
			{
				menuCompByCategory.CheckChildNoticeState();
			}
		}
	}

	private static IEnumerator coNeedNetworkContents(PortalRenewalData portalData)
	{
		if (portalData.Category == PortalRenewalCategory.AdventureNotice)
		{
			yield break;
		}
		yield return null;
		switch (portalData.SubCategory)
		{
		case PortalRenewalSubCategory.LeagueOfVictory:
			SingletonAuto<MatchArena>.instance.RequestSeasonInfo(MatchPvpType.Normal, delegate(MatchArena.MatchSeasonInfo seasonInfo)
			{
				if (seasonInfo != null && IsFullChargeCurrency(portalData))
				{
					ForceAddUnConfimedNotice(portalData);
				}
			});
			break;
		case PortalRenewalSubCategory.LeagueOfGlory:
			SingletonAuto<MatchArena>.instance.RequestSeasonInfo(MatchPvpType.BanPick, delegate(MatchArena.MatchSeasonInfo seasonInfo)
			{
				if (seasonInfo != null && IsFullChargeCurrency(portalData))
				{
					ForceAddUnConfimedNotice(portalData);
				}
			});
			break;
		case PortalRenewalSubCategory.GuildArena:
			if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info == null || GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Id <= 0)
			{
				break;
			}
			GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.RequestGuildArenaSessionInfo(delegate
			{
				GuildArenaSessionInfo guildArenaSessionInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetGuildArenaSessionInfo();
				GuildArenaSessionState state = guildArenaSessionInfo.State;
				bool isMatched = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.IsMatched;
				if (state == GuildArenaSessionState.Battle && isMatched && Vespa_String_Extension_Method2.TryParseNetworkTime2(guildArenaSessionInfo.BattleEndTime, out var o_dateTime))
				{
					DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
					if (!((o_dateTime - currentServerUTCTime).TotalSeconds <= 0.0) && GuildArenaNoticeExceptionChecker(guildArenaSessionInfo.SessionIndex))
					{
						ForceAddUnConfimedNotice(portalData);
					}
				}
			}, enableErrorPopup: false);
			break;
		case PortalRenewalSubCategory.GuildSuppress:
			if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info == null || GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Id <= 0)
			{
				break;
			}
			SceneAutoBehaviour<GuildSuppressManagement>.instance.InternalRequestGuildSuppressSessionInfo(delegate
			{
				GuildSuppressSessionInfo guildSuppressSessionInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildSuppressManager.GetGuildSuppressSessionInfo();
				if (guildSuppressSessionInfo != null)
				{
					GuildSuppressSessionState state = guildSuppressSessionInfo.State;
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(StaminaType.GuildSuppressKey, out var currentValue, out var maxValue);
					bool applied = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildSuppressManager.Applied;
					if (state == GuildSuppressSessionState.Battle && applied && currentValue >= maxValue && Vespa_String_Extension_Method2.TryParseNetworkTime2(guildSuppressSessionInfo.BattleEndTime, out var o_dateTime))
					{
						DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
						if (!((o_dateTime - currentServerUTCTime).TotalSeconds <= 0.0))
						{
							ForceAddUnConfimedNotice(portalData);
						}
					}
				}
			}, enableErrorPopup: false);
			break;
		case PortalRenewalSubCategory.GuildRaid:
			SceneAutoBehaviour<GuildManagement2>.instance.RequestGuildRaidInfo(delegate
			{
				if (CheckGuildRaidOpenTime)
				{
					ForceAddUnConfimedNotice(portalData);
				}
			});
			break;
		}
	}

	private static void ForceAddUnConfimedNotice(PortalRenewalData data)
	{
		ContentsMenuComponent menuCompByCategory = GetMenuCompByCategory(data);
		PortalContentsComponent contentsComponentByPortalData = GetContentsComponentByPortalData(data);
		if (menuCompByCategory != null)
		{
			PortalRenewal.NoticeState noticeState = menuCompByCategory.NoticeState;
			switch (noticeState)
			{
			case PortalRenewal.NoticeState.NONE:
				noticeState = PortalRenewal.NoticeState.UNCONFIRMED;
				break;
			case PortalRenewal.NoticeState.NEW:
				noticeState = PortalRenewal.NoticeState.UNCONFIRMED_AND_NEW;
				break;
			}
			menuCompByCategory.ChangeNoticeState(noticeState);
		}
		if (contentsComponentByPortalData != null)
		{
			PortalRenewal.NoticeState noticeState2 = contentsComponentByPortalData.NoticeState;
			switch (noticeState2)
			{
			case PortalRenewal.NoticeState.NONE:
				noticeState2 = PortalRenewal.NoticeState.UNCONFIRMED;
				break;
			case PortalRenewal.NoticeState.NEW:
				noticeState2 = PortalRenewal.NoticeState.UNCONFIRMED_AND_NEW;
				break;
			}
			contentsComponentByPortalData.ChangeNoticeState(noticeState2);
		}
	}

	public static bool IsFullChargeCurrency(PortalRenewalData portalRenewalData)
	{
		CurrencyTypeData data = GameRoot.Instance.CurrencyTypeDataContainer.GetData(portalRenewalData.CurrencyType);
		if (data == null || String_Extension_Method.IsNullOrEmpty(data.StaminaType))
		{
			return false;
		}
		EnumTools.TryParse<StaminaType>(data.StaminaType, out var o_value);
		return IsFullChargeCurrency(o_value);
	}

	public static bool IsFullChargeCurrency(StaminaType staminaType)
	{
		if (staminaType <= StaminaType.None)
		{
			return false;
		}
		GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(staminaType, out var currentValue, out var maxValue);
		if (currentValue >= maxValue)
		{
			return true;
		}
		return false;
	}

	public static bool IsEnoughResourceItem(PortalRenewalData portalRenewalData)
	{
		int valueInt = ArgParser.GetValueInt(portalRenewalData.CurrencyParam, 0);
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(valueInt);
		if (data == null)
		{
			return false;
		}
		int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(data.Code);
		int valueInt2 = ArgParser.GetValueInt(portalRenewalData.CurrencyParam, 1);
		if (itemCount >= valueInt2)
		{
			return true;
		}
		return false;
	}

	public static bool IsAnyNoticeSoulWeapon()
	{
		if (!AccountManager.IsLogin())
		{
			return false;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponNoticeManager.AnyNoticeStateOn())
		{
			return true;
		}
		return false;
	}

	public static bool IsNoticeTower(PortalRenewalData portalRenewalData)
	{
		int num = 0;
		int valueInt = ArgParser.GetValueInt(portalRenewalData.CurrencyParam, 0);
		BaseTower.GetMaxFloor(valueInt);
		TowerInfo towerInfo = GlobalAutoBehaviour<AccountManager>.instance.TowerManager.Get(valueInt);
		if (towerInfo != null)
		{
			num = towerInfo.CompletedFloor;
		}
		if (num <= 0)
		{
			return true;
		}
		return false;
	}

	public static bool IsNoticeShakmehMiddle()
	{
		GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(StaminaType.ShakmehMiddleBossKey, out var currentValue, out var maxValue);
		if (currentValue >= maxValue)
		{
			return true;
		}
		return false;
	}

	public static bool IsNoticeShakmehFinal()
	{
		return true;
	}

	private static bool IsNoticeGuildAttendance()
	{
		if (!GlobalAutoBehaviour<GuildManager>.isValidInstance)
		{
			return false;
		}
		return GlobalAutoBehaviour<GuildManager>.instance.CheckGuildAttendanceRewardable;
	}

	public static void OpenContentsMovePopup(int chapterIndex, int dungeonIndex, Action onYes, Action onNo = null)
	{
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
		if (data != null)
		{
			string nodeCode = GetNodeCode(chapterIndex, dungeonIndex, combineChapterIndex: false);
			if (!String_Extension_Method.IsNullOrEmpty(nodeCode))
			{
				string stringDirect = LocalizationManager.GetStringDirect(data.Name);
				OpenContentsMovePopup(LocalizationManager.GetStringDirect("PORTAL_LAST_AREA_NAME", stringDirect, nodeCode), onYes, onNo);
			}
		}
	}

	public static string GetNodeCode(int chapterIndex, int dungeonIndex, bool combineChapterIndex = true, string visualChapterIndex = "")
	{
		string empty = string.Empty;
		if (GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex) == null)
		{
			return empty;
		}
		CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			return empty;
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(data.Code) && !String_Extension_Method.IsNullOrEmpty(data.Code[0]) && data.Code[0].Contains("-"))
		{
			empty = data.Code[0];
			return empty.Replace("-", " - ");
		}
		empty = dungeonIndex.ToString();
		if (combineChapterIndex)
		{
			string text = ((!String_Extension_Method.IsNullOrEmpty(visualChapterIndex)) ? visualChapterIndex : chapterIndex.ToString());
			empty = LocalizationManager.GetStringDirect("MAZE_ARER_ALL_TITLE", text, empty);
		}
		return empty;
	}

	public static void OpenContentsMovePopup(string contentsName, Action onYes, Action onNo = null)
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect("PORTAL_POPUP_TEXT_SHIFT_CONFIRM");
		PayShopMessage3ButtonPopup.OpenPortalStyle(title, contentsName, stringDirect, onYes, onNo);
	}

	public static AssetTexture GetNewPortalBackground(string code)
	{
		return UIHelper.GetPlainTexture(GetNewPortalBackgroundPath(code));
	}

	private static string GetNewPortalBackgroundPath(string code)
	{
		return "PortalRenewal/Image/" + code;
	}

	public static PortalRenewal.LockedState GetLockedReason(PortalRenewalData data)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(data.OpenCondition) && !SingletonAuto<ConditionChecker>.instance.Check(data.OpenCondition))
		{
			return PortalRenewal.LockedState.CONDITION_LOCKED;
		}
		if (data.TutorialIndex > 0 && !SprkAdventureUnlocks.PortalTutorial(GlobalAutoBehaviour<AccountManager>.instance.TutorialManager, data.TutorialIndex))
		{
			return PortalRenewal.LockedState.TUTORIAL_LOCKED;
		}
		return PortalRenewal.LockedState.NONE;
	}

	public static bool CheckEventCondition(PortalRenewalData data)
	{
		return CheckEventCondition(data.EventBuffCondition);
	}

	public static bool CheckEventCondition(string[] conditions)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(conditions); i += 2)
		{
			EventBonusType valueEnum = ArgParser.GetValueEnum(conditions, i, EventBonusType.None);
			if (valueEnum != EventBonusType.None)
			{
				string valueString = ArgParser.GetValueString(conditions, i + 1);
				if (GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(valueEnum, valueString) != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static string GetExceptionTitle(PortalRenewalData data)
	{
		if (data.SubCategory == PortalRenewalSubCategory.WorldBoss)
		{
			WorldBossInfo worldBossInfo = SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.WorldBossInfo;
			if (worldBossInfo == null)
			{
				return string.Empty;
			}
			WorldBossData data2 = GameRoot.Instance.WorldBossDataContainer.GetData(worldBossInfo.Index);
			if (data2 == null)
			{
				return string.Empty;
			}
			string stringDirect = LocalizationManager.GetStringDirect(data2.Name);
			return LocalizationManager.GetStringDirect("PORTAL_ADVENTURE_WORLDBOSS", stringDirect);
		}
		return string.Empty;
	}

	public static ContentsMenuComponent GetMenuCompByCategory(PortalRenewalData data)
	{
		return GetMenuCompByCategory(data.Category);
	}

	public static ContentsMenuComponent GetMenuCompByCategory(PortalRenewalCategory category)
	{
		PortalRenewal instance = SceneAutoBehaviour<PortalManagement>.instance.GetInstance<PortalRenewal>(openedOnly: true);
		if (instance == null)
		{
			return null;
		}
		return instance.GetMenuCompByCategory(category);
	}

	public static PortalContentsComponent GetContentsComponentByPortalData(PortalRenewalData data)
	{
		PortalRenewal instance = SceneAutoBehaviour<PortalManagement>.instance.GetInstance<PortalRenewal>(openedOnly: true);
		if (instance == null)
		{
			return null;
		}
		return instance.GetContentsComponentByPortalData(data);
	}

	public static float GetOrthographicSize()
	{
		float num = 1.77f;
		float num2 = (float)Screen.width / (float)Screen.height;
		float result = 1f;
		if (num2 < num)
		{
			result = (float)System.Math.Truncate(num2 / num * 100f) / 100f;
		}
		return result;
	}
}
