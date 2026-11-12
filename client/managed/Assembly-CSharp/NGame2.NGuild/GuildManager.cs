using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NSocket;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NodeFunctionType;
using NGame2.NUI.NWindow.NodeFunctionType.Condition;
using NShared;
using NShared.GiveRewardGuildMember;
using NShared.NLocalization;
using NShared.OpenGuildBuilding;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NGuild;

public class GuildManager : GlobalAutoBehaviour<GuildManager>
{
	private static PenaltyChecker PenaltyChecker = new PenaltyChecker();

	private static DateTime MyGuildInfoUpdatedTime = DateTime.MinValue;

	private static DateTime MemberInfoUpdatedTime = DateTime.MinValue;

	private static DateTime JoinRequestedPlayerUpdatedTime = DateTime.MinValue;

	public GuildInfo SearchedGuildInfo;

	private LocalAccountString _guildAttendanceLastCheckTimeLocalAccount = new LocalAccountString(string.Empty, "Guild_Attendance_Time");

	public UserGuildManager MyGuildManager { get; private set; }

	public RequestedGuildManager RequestedGuildManager { get; private set; }

	public GuildMemberManager MemberManager { get; private set; }

	public JoinRequestedGuildManager JoinRequestedGuildManager { get; private set; }

	public GuildJoinRequestPlayerManager JoinRequestPlayerManager { get; private set; }

	public GuildRaidBootyEquipItemManager GuildRaidBootyEquipItemManager { get; private set; }

	public GuildRaidBootyItemManager GuildRaidBootyItemManager { get; private set; }

	public GuildAttendanceInfo GuildAttendanceInfo { get; private set; }

	public GuildMemberAttendanceInfo GuildMemberAttendanceInfo { get; private set; }

	public long JoinedGuildId { get; private set; }

	public bool CheckIsGuild
	{
		get
		{
			if (MyGuildManager != null)
			{
				return MyGuildManager.GetGuildId() > 0;
			}
			return false;
		}
	}

	public Dictionary<int, NShared.GuildRaidDungeonInfo> GuildRaidDungeonInfoDic
	{
		get
		{
			if (MyGuildManager == null)
			{
				return null;
			}
			if (MyGuildManager.GuildRaidManager == null)
			{
				return null;
			}
			return MyGuildManager.GuildRaidManager.GuildRaidDungeonInfoDic;
		}
	}

	public List<int> GuildAttendanceDayList
	{
		get
		{
			if (GuildMemberAttendanceInfo == null)
			{
				return null;
			}
			List<int> list = new List<int>();
			list.Clear();
			list.Add(GuildMemberAttendanceInfo.Day1);
			list.Add(GuildMemberAttendanceInfo.Day2);
			list.Add(GuildMemberAttendanceInfo.Day3);
			list.Add(GuildMemberAttendanceInfo.Day4);
			list.Add(GuildMemberAttendanceInfo.Day5);
			list.Add(GuildMemberAttendanceInfo.Day6);
			list.Add(GuildMemberAttendanceInfo.Day7);
			return list;
		}
	}

	private int LastAttendanceDay
	{
		get
		{
			List<int> guildAttendanceDayList = GuildAttendanceDayList;
			if (__Extension_Method_ICollection__.IsNullOrEmpty(guildAttendanceDayList))
			{
				return -1;
			}
			int result = -1;
			for (int num = guildAttendanceDayList.Count - 1; num >= 0; num--)
			{
				if (guildAttendanceDayList[num] == 1)
				{
					result = num;
					break;
				}
			}
			return result;
		}
	}

	public int GuildAttendanceToday
	{
		get
		{
			if (!CheckIsGuild)
			{
				return -1;
			}
			if (GuildMemberAttendanceInfo == null)
			{
				return -1;
			}
			int num = LastAttendanceDay;
			if (num <= -1)
			{
				num = 0;
			}
			DateTime o_dateTime;
			if (String_Extension_Method.IsNullOrEmpty(GuildMemberAttendanceInfo.LastWeekFirstAttendanceTime))
			{
				if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(GuildMemberAttendanceInfo.UpdatedTime, out o_dateTime))
				{
					return -1;
				}
			}
			else if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(GuildMemberAttendanceInfo.LastAttendanceTime, out o_dateTime))
			{
				return -1;
			}
			DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
			DateTime dateTime = GlobalAutoBehaviour<AccountManager>.instance.ServerUTCToLocal(o_dateTime);
			int days = (currentServerLocalTime.Date - dateTime.Date).Days;
			return num + days;
		}
	}

	private bool CheckPerfectAttendanceRewardable
	{
		get
		{
			if (GuildMemberAttendanceInfo == null)
			{
				return false;
			}
			int successive = GuildMemberAttendanceInfo.Successive;
			GuildAttendanceRewardData perfectAttendanceRewardDataByStep = GameRoot.Instance.GuildAttendanceRewardDataContainer.GetPerfectAttendanceRewardDataByStep(successive);
			if (perfectAttendanceRewardDataByStep == null)
			{
				return false;
			}
			if (perfectAttendanceRewardDataByStep.Day > GuildMemberAttendanceInfo.SuccessiveCount)
			{
				return false;
			}
			return true;
		}
	}

	public bool CheckGuildAttendanceRewardable
	{
		get
		{
			if (!CheckIsGuild)
			{
				return false;
			}
			if (CheckPerfectAttendanceRewardable)
			{
				return true;
			}
			if (String_Extension_Method.IsNullOrEmpty(_guildAttendanceLastCheckTimeLocalAccount.Value))
			{
				return true;
			}
			if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(_guildAttendanceLastCheckTimeLocalAccount.Value, out var o_dateTime))
			{
				return false;
			}
			if ((GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime().Date - o_dateTime.Date).Days <= 0)
			{
				return false;
			}
			return true;
		}
	}

	public int DailyCollaborationCout
	{
		get
		{
			int num = 0;
			if (GuildAttendanceInfo != null)
			{
				num += GuildAttendanceInfo.Daily;
			}
			return num;
		}
	}

	public int WeeklyCollaborationCout
	{
		get
		{
			int num = 0;
			if (GuildAttendanceInfo != null)
			{
				num += GuildAttendanceInfo.Weekly;
			}
			return num;
		}
	}

	private void Update()
	{
		UpdateGuildRaidBootyItems();
	}

	public void SetGuildId(long GuildId)
	{
		JoinedGuildId = GuildId;
	}

	public void SetMyGuildName(string name)
	{
		if (MyGuildManager != null)
		{
			MyGuildManager.SetName(name);
		}
	}

	public static void RequestMyGuildInfo(bool bIgnoreCool = false)
	{
		DateTime utcNow = DateTime.UtcNow;
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("GuildBaseInfoRequestCoolTime");
		TimeSpan timeSpan = utcNow - MyGuildInfoUpdatedTime.AddSeconds(valueInt);
		if (bIgnoreCool || timeSpan.TotalSeconds > 0.0)
		{
			MyGuildInfoUpdatedTime = utcNow;
			SceneAutoBehaviour<GuildManagement2>.instance.GetGuildBasicInfo(delegate
			{
				SceneAutoBehaviour<GuildManagement2>.instance.UpdateGuildManageUI();
			});
		}
	}

	public void SetMyGuildInfo(MyGuildInfo info)
	{
		if (MyGuildManager == null)
		{
			MyGuildManager = new UserGuildManager();
		}
		MyGuildManager.Set(info);
		long guildId = (JoinedGuildId = MyGuildManager.GetGuildId());
		GlobalAutoBehaviour<SocketServiceManager>.instance.MessageConnector.ChangeGuild(guildId);
		RefreshGuildSkillBoosterInfo();
	}

	public GuildSkillLevelData[] GetGuildSkillDataList()
	{
		if (MyGuildManager == null)
		{
			return null;
		}
		List<GuildSkillLevelData> guildSkillLevelDataList = MyGuildManager.GetGuildSkillLevelDataList();
		if (guildSkillLevelDataList == null || guildSkillLevelDataList.Count == 0)
		{
			return null;
		}
		return guildSkillLevelDataList.ToArray();
	}

	public static void RequestGuildInfo()
	{
		RequestMyGuildInfo();
		RequestMemberInfo(delegate(bool InfoUpdated)
		{
			if (InfoUpdated)
			{
				SceneAutoBehaviour<GuildManagement2>.instance.UpdateGuildManageList(GuildManage.ListType.GuildMember);
			}
		});
		RequestJoinRequestInfo();
	}

	public static void RequestMemberInfo(Action<bool> onComplete, bool bIgnoreCool = false)
	{
		DateTime utcNow = DateTime.UtcNow;
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("GuildMemberInfoRequestCoolTime");
		TimeSpan timeSpan = utcNow - MemberInfoUpdatedTime.AddSeconds(valueInt);
		if (bIgnoreCool || timeSpan.TotalSeconds > 0.0)
		{
			MemberInfoUpdatedTime = utcNow;
			SceneAutoBehaviour<GuildManagement2>.instance.GetAllGuildMemberInfo(onComplete);
		}
		else
		{
			Vespa_System_Action_Extension_Method.fire(onComplete, i_argA: false);
		}
	}

	public void InitGuildMembers(GuildMemberInfo[] infos)
	{
		if (MemberManager == null)
		{
			MemberManager = new GuildMemberManager();
		}
		MemberManager.Apply(infos);
	}

	public static void RequestJoinRequestInfo(bool bIgnoreCool = false)
	{
		GuildManage guildManage = SceneAutoBehaviour<GuildManagement2>.instance.GetInstance<GuildManage>(openedOnly: true);
		if (guildManage == null || !guildManage.IsWaitingPanelEnabled())
		{
			return;
		}
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("GuildJoinRequestInfoRequestCoolTime");
		TimeSpan timeSpan = DateTime.UtcNow - JoinRequestedPlayerUpdatedTime.AddSeconds(valueInt);
		if (bIgnoreCool || timeSpan.TotalSeconds > 0.0)
		{
			JoinRequestedPlayerUpdatedTime = DateTime.UtcNow;
			SceneAutoBehaviour<GuildManagement2>.instance.GetJoinRequestPlayers(delegate
			{
				SceneAutoBehaviour<GuildManagement2>.instance.UpdateGuildManageList(GuildManage.ListType.Waiting);
			});
		}
	}

	public void InitGuildJoinRequestPlayers(GuildJoinRequestPlayerInfo[] infos)
	{
		if (JoinRequestPlayerManager == null)
		{
			JoinRequestPlayerManager = new GuildJoinRequestPlayerManager();
		}
		JoinRequestPlayerManager.Reset(infos);
	}

	public void SetJoinRequestGuilds(GuildInfo[] guildInfos)
	{
		if (JoinRequestedGuildManager == null)
		{
			JoinRequestedGuildManager = new JoinRequestedGuildManager();
		}
		JoinRequestedGuildManager.Reset(guildInfos);
	}

	public void SetRequestedGuildInfo(GuildInfo[] guildInfos)
	{
		if (RequestedGuildManager == null)
		{
			RequestedGuildManager = new RequestedGuildManager();
		}
		RequestedGuildManager.Reset(guildInfos);
	}

	public void RemoveMyGuild()
	{
		if (MyGuildManager != null)
		{
			MyGuildManager.Set(null);
		}
		RemoveAllGuildMember();
		RemoveAllGuildJoinRequestPlayer();
		GlobalAutoBehaviour<SocketServiceManager>.instance.MessageConnector.ChangeGuild(0L);
	}

	public void AddGuildMember(GuildMemberInfo info)
	{
		if (MemberManager != null)
		{
			MemberManager.AddMember(info);
		}
	}

	public void SetGuildMemberNick(long accountId, string nick)
	{
		if (MemberManager != null)
		{
			MemberManager.SetNick(accountId, nick);
		}
	}

	public void SetGuildMemberAvatarHero(long accountId, int avatarHeroIndex)
	{
		if (MemberManager != null)
		{
			MemberManager.SetAvatarHero(accountId, avatarHeroIndex);
		}
	}

	public void RemoveJoinRequestPlayer(long AccountId)
	{
		if (JoinRequestPlayerManager != null)
		{
			JoinRequestPlayerManager.Remove(AccountId);
		}
	}

	private void RemoveAllGuildMember()
	{
		if (MemberManager != null)
		{
			MemberManager.RemoveAll();
			MemberManager = null;
		}
	}

	private void RemoveAllGuildJoinRequestPlayer()
	{
		if (JoinRequestPlayerManager != null)
		{
			JoinRequestPlayerManager.RemoveAll();
			JoinRequestPlayerManager = null;
		}
	}

	public void ResetSkillLevelUpRequest()
	{
		if (MyGuildManager != null && MyGuildManager.Info != null && MemberManager != null)
		{
			MyGuildManager.Info.ResetSkillLevelUpReq();
			GuildMemberInfo myMemberInfo = MemberManager.GetMyMemberInfo();
			if (myMemberInfo != null)
			{
				myMemberInfo.ReqGuildSkill = 0;
			}
			RefreshGuildSkillBoosterInfo();
		}
	}

	private void UpdateGuildRaidBootyItems()
	{
		if (GuildRaidBootyEquipItemManager != null)
		{
			GuildRaidBootyEquipItemManager.UpdateItemDestroyTime();
		}
		if (GuildRaidBootyItemManager != null)
		{
			GuildRaidBootyItemManager.UpdateItemDestroyTime();
		}
	}

	public void InitGuildRaidBootyEquipItems(GuildRaidBootyEquipItemInfo[] EquipItemInfos)
	{
		if (GuildRaidBootyEquipItemManager == null)
		{
			GuildRaidBootyEquipItemManager = new GuildRaidBootyEquipItemManager();
		}
		GuildRaidBootyEquipItemManager.Set(EquipItemInfos);
	}

	public void InitGuildRaidBootyItems(GuildRaidBootyItemInfo[] ItemInfos)
	{
		if (GuildRaidBootyItemManager == null)
		{
			GuildRaidBootyItemManager = new GuildRaidBootyItemManager();
		}
		GuildRaidBootyItemManager.Set(ItemInfos);
	}

	public void RemoveAllGuildRaidBootyItems()
	{
		if (GuildRaidBootyEquipItemManager != null)
		{
			GuildRaidBootyEquipItemManager.RemoveAll();
		}
		if (GuildRaidBootyItemManager != null)
		{
			GuildRaidBootyItemManager.RemoveAll();
		}
	}

	public void ApplyAddedBootyItem(GuildRaidBootyEquipItemInfo[] bootyEquipItemInfos, GuildRaidBootyItemInfo[] bootyItemInfos)
	{
		if (GuildRaidBootyEquipItemManager != null)
		{
			GuildRaidBootyEquipItemManager.Apply(bootyEquipItemInfos);
		}
		if (GuildRaidBootyItemManager != null)
		{
			GuildRaidBootyItemManager.Apply(bootyItemInfos);
		}
	}

	public NShared.GuildRaidDungeonInfo GetGuildRaidDungeonInfo(int chapterIndex)
	{
		Dictionary<int, NShared.GuildRaidDungeonInfo> guildRaidDungeonInfoDic = GuildRaidDungeonInfoDic;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(guildRaidDungeonInfoDic))
		{
			return null;
		}
		NShared.GuildRaidDungeonInfo value = null;
		guildRaidDungeonInfoDic.TryGetValue(chapterIndex, out value);
		return value;
	}

	public static void GetGuildSuppressHpInfo(List<MonsterHeroInfo> monsterInfoList, out long bossMaxHp)
	{
		bossMaxHp = 0L;
		foreach (MonsterHeroInfo monsterInfo in monsterInfoList)
		{
			if (monsterInfo.HeroInfo != null)
			{
				int waveSlotIndex = monsterInfo.WaveSlotIndex;
				long maxHp = monsterInfo.GetMaxHp();
				if (waveSlotIndex == 0)
				{
					bossMaxHp = maxHp;
				}
			}
		}
	}

	public static void GetGuildRaidHpInfo(List<MonsterHeroInfo> monsterInfoList, NShared.GuildRaidDungeonInfo guildRaidDungeonInfo, out long totalMaxHp, out long totalRemainHp, out long bossMaxHp, out long bossRemainHp)
	{
		totalMaxHp = 0L;
		totalRemainHp = 0L;
		bossMaxHp = 0L;
		bossRemainHp = 0L;
		foreach (MonsterHeroInfo monsterInfo in monsterInfoList)
		{
			if (monsterInfo.HeroInfo != null)
			{
				int waveSlotIndex = monsterInfo.WaveSlotIndex;
				long maxHp = monsterInfo.GetMaxHp();
				long monsterRealHp = guildRaidDungeonInfo.GetMonsterRealHp(waveSlotIndex, maxHp);
				totalMaxHp += maxHp;
				totalRemainHp += monsterRealHp;
				if (waveSlotIndex == 0)
				{
					bossMaxHp = maxHp;
					bossRemainHp = monsterRealHp;
				}
			}
		}
	}

	private void RefreshGuildSkillBoosterInfo()
	{
		if (SceneAutoBehaviour<LobbyManagement>.isValidInstance)
		{
			GlobalAutoBehaviour<BoosterManager>.instance.InitAllEffectiveBooster(BattleType.None, includeCash: true);
			GlobalAutoBehaviour<BoosterManager>.instance.AnnounceChangedBoosterInfoList();
		}
	}

	public void RemoveGuildAdventureNotice()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.AdventureNoticeManager != null)
		{
			GlobalAutoBehaviour<AccountManager>.instance.AdventureNoticeManager.RemoveEndTime(BattleType.GuildRaid);
			GlobalAutoBehaviour<AccountManager>.instance.AdventureNoticeManager.RemoveEndTime(BattleType.GuildSuppress);
		}
	}

	public static GuildPenaltyType GuildValidGuildPenaltyType()
	{
		return GetValidGuildPenalty()?.GetPenaltyType() ?? GuildPenaltyType.None;
	}

	public static IGuildPenalty GetValidGuildPenalty()
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return null;
		}
		return PenaltyChecker.GetValidPenalty();
	}

	public static bool InGuildPenalty(GuildContents checkContents = GuildContents.Raid)
	{
		switch (GuildValidGuildPenaltyType())
		{
		case GuildPenaltyType.None:
			return false;
		case GuildPenaltyType.WithdrawPenalty:
		{
			int guildWithdrawCount = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.GuildWithdrawCount;
			int restrictHour = GameRoot.Instance.GuildPenaltyDataContainer.GetRestrictHour(guildWithdrawCount);
			MessagePopup.OpenInform(GetPenalyMessage(checkContents, restrictHour));
			return true;
		}
		case GuildPenaltyType.ContentsRestricted:
			MessagePopup.OpenError(GuildRaid.GUILDRAID_CANNOT_ENTER_RESTRICTED);
			return true;
		default:
			return true;
		}
	}

	private static string GetPenalyMessage(GuildContents checkContents, int restrictHour)
	{
		string result = LocalizationManager.GetString(GuildRaid.GUILDRAID_CANNOT_ENTER_WITHDRAW_PENALTY, restrictHour);
		switch (checkContents)
		{
		case GuildContents.Suppress:
			result = LocalizationManager.GetStringDirect("GUILDSUPPRESS_CANNOT_ENTER_WITHDRAW_PENALTY", restrictHour);
			break;
		case GuildContents.Arena:
			result = LocalizationManager.GetStringDirect("GUILDARENA_CANNOT_ENTER_WITHDRAW_PENALTY", restrictHour);
			break;
		}
		return result;
	}

	public void EnterGuildChapter()
	{
		SceneAutoBehaviour<LobbyManagement>.instance.ExecuteCurrentNode(null);
	}

	public void ExistGuildChpater()
	{
		int chpaterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int dunjeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(chpaterIndex);
		if (data != null && !string.IsNullOrEmpty(data.EnterCheckerName))
		{
			ICondition condition = SingletonAuto<SwitchingContainer>.instance.GetCondition(data.EnterCheckerName);
			if (condition != null && !condition.EnterConditionCheck(data.Values, out chpaterIndex, out dunjeonIndex))
			{
				SceneAutoBehaviour<LobbyManagement>.instance.MoveMap(chpaterIndex, dunjeonIndex);
			}
		}
	}

	public void RequestOpenGuildBuilding(bool showUpgradeAnim, Action action)
	{
		NShared.OpenGuildBuilding.Request request = new NShared.OpenGuildBuilding.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.OpenGuildBuilding.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				Vespa_System_Action_Extension_Method.fire(action);
			}
			else if (response.Result != NShared.OpenGuildBuilding.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				Vespa_System_Action_Extension_Method.fire(action);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.GuildMemberRewardedTime = response.GuildMemberRewardedTime;
				MyGuildManager.SetBuildingInfo(response.GuildBuildingInfos);
				if (showUpgradeAnim)
				{
					MyGuildManager.GetReservedUpgradeBuildingInfo(GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.OpenGuildTime);
				}
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetOpenGuildTime(response.OpenGuildTime);
				Vespa_System_Action_Extension_Method.fire(action);
			}
		});
	}

	public void RequestGiveRewardGuildMember(int index, Action complete)
	{
		NShared.GiveRewardGuildMember.Request request = new NShared.GiveRewardGuildMember.Request
		{
			ChapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex,
			Index = index
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GiveRewardGuildMember.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				_ = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.GuildMemberRewardedTime;
				GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
				Vespa_System_Action_Extension_Method.fire(complete);
			}
			else if (response.Result != NShared.GiveRewardGuildMember.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				_ = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.GuildMemberRewardedTime;
				GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
				Vespa_System_Action_Extension_Method.fire(complete);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.GuildMemberRewardedTime = response.GuildMemberRewardedTime;
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.EquipItemResults);
				string stringDirect = LocalizationManager.GetStringDirect("CRAFT_ITEMCOMPONENT_DOCRAFT_GET");
				BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(stringDirect).SetCurrencyResultInfos3(response.CurrencyResults).SetItemResultInfos(response.ItemResults)
					.SetEquipResultInfos(response.EquipItemResults), isGlobal: false);
				Vespa_System_Action_Extension_Method.fire(complete);
			}
		});
	}

	public void Apply(GuildAttendanceInfo info)
	{
		if (info != null)
		{
			GuildAttendanceInfo = info;
		}
	}

	public void Apply(GuildMemberAttendanceInfo info)
	{
		if (info != null)
		{
			if (String_Extension_Method.IsNullOrEmpty(info.LastAttendanceTime))
			{
				_guildAttendanceLastCheckTimeLocalAccount.Value = string.Empty;
			}
			SetGuildAttendanceLastCheckTime(info);
			GuildMemberAttendanceInfo = info;
		}
	}

	private void SetGuildAttendanceLastCheckTime(GuildMemberAttendanceInfo info)
	{
		if (info != null && _guildAttendanceLastCheckTimeLocalAccount != null && Vespa_String_Extension_Method2.TryParseNetworkTime2(info.LastAttendanceTime, out var o_dateTime))
		{
			DateTime dateTime = GlobalAutoBehaviour<AccountManager>.instance.ServerUTCToLocal(o_dateTime);
			DateTime o_dateTime2;
			if (String_Extension_Method.IsNullOrEmpty(_guildAttendanceLastCheckTimeLocalAccount.Value))
			{
				SetGuildAttendanceRewardLastTime(dateTime);
			}
			else if (Vespa_String_Extension_Method2.TryParseNetworkTime2(_guildAttendanceLastCheckTimeLocalAccount.Value, out o_dateTime2) && dateTime > o_dateTime2)
			{
				SetGuildAttendanceRewardLastTime(dateTime);
			}
		}
	}

	public void SetGuildAttendanceRewardLastTime(DateTime time)
	{
		string value = time.ToString("yyyy-MM-dd") + " " + time.ToString("HH:mm:ss");
		_guildAttendanceLastCheckTimeLocalAccount.Value = value;
	}
}
