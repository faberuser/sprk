using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NChatting;
using NGame2.NContents;
using NGame2.NContentsDefine;
using NGame2.NDispatchBattle;
using NGame2.NDropBonusEvent;
using NGame2.NEventTrigger;
using NGame2.NNotify;
using NGame2.NParty;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper.NAffix;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NAffix;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NChatting;
using NShared;
using NShared.BeginCampaign;
using NShared.CreateRaidRoom;
using NShared.NEvent;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NShared.NParty;
using NShared.NRaid;
using NShared.SearchFriend;
using NShared.SearchRaidRoom;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NManager.NLobby;

public class RaidManagement : BaseManagement<RaidManagement>, RaidList.Listener, RaidInfoBase.Listener, RaidPartySetting.Listener, BattlePartySetting.Listener, RaidSinglePartySetting.Listener, RaidInviteList.Listener, RaidLevelSelect.Listener, RaidRoomList.Listener, MultiSelect.Listener, ChallengeRaidRanking.Listener, PartyManager.INewPartyListener, PartyManager.IPartyListener, IChatListener, AffixSelectPopup.IListener, BaseAffixPanel.IListener, IUserObserver<DropBonusEventManager.DropBonusEventChangeInfo>, IUserObserver
{
	public const int MINI_GUIDE_CHAPTER_INDEX = 900;

	public const int MINI_GUIDE_DUNGEON_INDEX = 1;

	private RaidData _raidData;

	private float _lastGuildRaidPingTime;

	private Dictionary<int, bool> _requestedGuildRaidMemberInfo = new Dictionary<int, bool>();

	private Dictionary<int, bool> _requestedGuildRaidRewardItem = new Dictionary<int, bool>();

	protected ChallengeRaidWrapper _challengeRaid = new ChallengeRaidWrapper();

	private float _battleRequestedTime;

	public int RaidIndex
	{
		get
		{
			if (_raidData == null)
			{
				return 0;
			}
			return _raidData.Index;
		}
	}

	public int RaidLevel
	{
		get
		{
			if (_raidData == null)
			{
				return 0;
			}
			return _raidData.Level;
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	private bool IsChallengeRaidDropBonusType(DropBonusEventManager.DropBonusEventChangeInfo changedInfo)
	{
		if (changedInfo.bonusEvent.BonusType != EventBonusType.ChallengeRaidSeason && changedInfo.bonusEvent.BonusType != EventBonusType.ChallengeRaid)
		{
			return changedInfo.bonusEvent.BonusType == EventBonusType.ChallengeRaidNotice;
		}
		return true;
	}

	void IUserObserver<DropBonusEventManager.DropBonusEventChangeInfo>.OnChanged(DropBonusEventManager.DropBonusEventChangeInfo changedInfo)
	{
		if (changedInfo != null && changedInfo.bonusEvent != null && IsChallengeRaidDropBonusType(changedInfo))
		{
			_challengeRaid.SetChallengeRaidInfos();
			if (changedInfo.changeType == DropBonusEventManager.DropBonusEventChangeType.EventRemoved && changedInfo.bonusEvent.BonusType == EventBonusType.ChallengeRaidSeason)
			{
				OnChallengeRaidSeasonEnd();
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		UseBackgroundBlack(use: true);
		_showLobbyMenu = true;
		AddInstance<RaidList>("Resources/Prefab2/Window/Raid/RaidList");
		AddInstance<RaidRoomList>("Resources/Prefab2/Window/Raid/RaidRoomList");
		AddInstance<RaidPartySetting>("Resources/Prefab2/Window/Raid/RaidPartySetting");
		AddInstance<RaidSinglePartySetting>("Resources/Prefab2/Window/Raid/RaidSinglePartySetting");
		AddInstance<NGame2.NUI.NWindow.RaidInfo>("Resources/Prefab2/Window/Raid/RaidInfo");
		AddInstance<RaidInviteList>("Resources/Prefab2/Window/Raid/RaidInviteList");
		AddInstance<RaidLevelSelect>("Resources/Prefab2/Window/Raid/RaidLevelSelect");
		AddInstance<MultiSelect>("Resources/Prefab2/Window/Raid/MultiSelect");
		AddInstance<ChallengeRaidRanking>("Resources/Prefab2/Window/Raid/ChallengeRaidRanking");
		AddInstance<ChallengeRaidRewardInfo>("Resources/Prefab2/Window/Raid/ChallengeRaidRewardInfo");
		AddInstance<ChallengeRaidGuide>("Resources/Prefab2/Window/Raid/ChallengeRaidGuide");
		AddInstance<GuildSuppressPartySetting>("Resources/Prefab2/Window/Raid/GuildSuppressPartySetting");
		AddInstance<GuildSuppressBossInfo>("Resources/Prefab2/Window/GuildSuppress/GuildSuppressBossInfo");
		AddInstance<AttributeRaidPartySetting>("Resources/Prefab2/Window/Raid/AttributeRaidPartySetting");
		AddInstance<AffixSelectPopup>("Resources/Prefab2/Window/AffixSystem/AffixSelectPopup");
		AddInstance<PunishmentRaidWindow>("Resources/Prefab2/Window/Raid/PunishmentRaidWindow");
		AddInstance<PunishmentRaidRewardInfoPopup>("Resources/Prefab2/Window/Raid/PunishmentRaidRewardInfoPopup");
		GlobalAutoBehaviour<PartyManager>.instance.AddPartyListener(this);
		GlobalAutoBehaviour<PartyManager>.instance.AddNewPartyListener(this);
		GlobalAutoBehaviour<ChattingManager>.instance.AddChatListener(this);
		GlobalAutoBehaviour<DropBonusEventManager>.instance.ObserverManager.AddObserver(this);
		_challengeRaid.SetChallengeRaidInfos();
	}

	public ChallengeRaidWrapper GetChallengeRaidWarpper()
	{
		return _challengeRaid;
	}

	public void RefreshChallengeRaidInfo()
	{
		_challengeRaid.SetChallengeRaidInfos();
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			base.OnDestroy();
			if (GlobalAutoBehaviour<PartyManager>.isValidInstance)
			{
				GlobalAutoBehaviour<PartyManager>.instance.RemoveNewPartyListener(this);
				GlobalAutoBehaviour<PartyManager>.instance.RemovePartyListener(this);
			}
			if (GlobalAutoBehaviour<ChattingManager>.isValidInstance)
			{
				GlobalAutoBehaviour<ChattingManager>.instance.RemoveChatListener(this);
			}
			if (GlobalAutoBehaviour<DropBonusEventManager>.isValidInstance)
			{
				GlobalAutoBehaviour<DropBonusEventManager>.instance.ObserverManager.RemoveObserver(this);
			}
		}
	}

	protected override void ChainUpdate()
	{
		base.ChainUpdate();
		UpdateShowLobbyMenu();
	}

	private void UpdateShowLobbyMenu()
	{
		RaidPartySetting raidPartySetting = GetInstance<RaidPartySetting>(openedOnly: true);
		bool flag = raidPartySetting != null && raidPartySetting.IsOpen();
		_showLobbyMenu = !flag;
	}

	private bool IsEnterAble(int raidIndex, int raidLevel)
	{
		return IsEnterAble(GameRoot.Instance.RaidDataContainer.GetData(raidIndex, raidLevel));
	}

	private bool IsEnterAble(RaidData raidData)
	{
		if (raidData == null)
		{
			return false;
		}
		if (raidData.Type != RaidType.Challenge)
		{
			return true;
		}
		if (!_challengeRaid.IsAbleEnter())
		{
			return false;
		}
		if (_challengeRaid.GetEnterInfo().RaidIndex != raidData.Index)
		{
			return false;
		}
		return true;
	}

	public void ShowPopupSeasonEnd(Action okFunc = null)
	{
		_challengeRaid.ShowTimeOutPopup(isSeasonEnd: true, okFunc);
	}

	public void ShowPopupBattleEnd(Action okFunc = null)
	{
		_challengeRaid.ShowTimeOutPopup(isSeasonEnd: false, okFunc);
	}

	public ChallengeRaidInfo GetChallengeRaidSeasonInfo()
	{
		return _challengeRaid.GetSeasonInfo();
	}

	public ChallengeRaidInfo GetChallengeRaidEnterInfo()
	{
		return _challengeRaid.GetEnterInfo();
	}

	public int GetRemainTime()
	{
		Vespa_String_Extension_Method2.TryParseNetworkTime2(_challengeRaid.GetEnterInfo().EndTime, out var o_dateTime);
		return System.Math.Max((int)(o_dateTime - NGame2.NUI.NWindow.Common.GetUTCNow()).TotalSeconds, 0);
	}

	private bool isOpenChallengeRaidRanking()
	{
		return GetInstance<ChallengeRaidRanking>().IsOpen();
	}

	private bool isOpenChallengeRaidPartySetting()
	{
		if (_raidData == null)
		{
			return false;
		}
		RaidType type = _raidData.Type;
		if (type != RaidType.Challenge && type != RaidType.ChallengeSingle)
		{
			return false;
		}
		return type switch
		{
			RaidType.Challenge => GetInstance<RaidPartySetting>().IsOpen(), 
			RaidType.ChallengeSingle => GetInstance<RaidSinglePartySetting>().IsOpen(), 
			_ => false, 
		};
	}

	private void OnChallengeRaidSeasonEnd()
	{
		if (!isOpenChallengeRaidRanking() && !isOpenChallengeRaidPartySetting())
		{
			return;
		}
		ShowPopupSeasonEnd(delegate
		{
			ChallengeRaidRanking challengeRaidRanking = GetInstance<ChallengeRaidRanking>();
			if (challengeRaidRanking.IsOpen() && challengeRaidRanking.RaidData != null && (challengeRaidRanking.RaidData.Type == RaidType.Challenge || challengeRaidRanking.RaidData.Type == RaidType.ChallengeSingle))
			{
				challengeRaidRanking.Close();
			}
			UIHelper.CloseWindowsBeforeRaidPartySetting();
			if (_raidData != null)
			{
				RaidType type = _raidData.Type;
				if (type == RaidType.Challenge || type == RaidType.ChallengeSingle)
				{
					switch (type)
					{
					case RaidType.Challenge:
					{
						RaidPartySetting raidPartySetting = GetInstance<RaidPartySetting>();
						if (raidPartySetting.IsOpen() && raidPartySetting.RaidData != null && raidPartySetting.RaidData.Type == RaidType.Challenge)
						{
							raidPartySetting.Close();
						}
						break;
					}
					case RaidType.ChallengeSingle:
					{
						RaidSinglePartySetting raidSinglePartySetting = GetInstance<RaidSinglePartySetting>();
						if (raidSinglePartySetting.IsOpen() && raidSinglePartySetting.RaidData != null && raidSinglePartySetting.RaidData.Type == RaidType.ChallengeSingle)
						{
							raidSinglePartySetting.Close();
						}
						break;
					}
					}
				}
			}
		});
	}

	public bool IsAblePreSeason()
	{
		return _challengeRaid.IsAbleNotice();
	}

	public bool IsAbleSeason()
	{
		return _challengeRaid.IsAbleSeason();
	}

	public bool IsAbleEnter()
	{
		return _challengeRaid.IsAbleEnter();
	}

	public void OpenRaidList(RaidType raidType)
	{
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(GetOpenRaidTriggerEvent(raidType));
		if (raidType != RaidType.PunishmentRaid)
		{
			GetInstance<RaidList>().Open(this, raidType);
		}
	}

	private IEvent GetOpenRaidTriggerEvent(RaidType raidType)
	{
		IEvent obj = new OpenRaidSelect();
		if (SingletonAuto<EventTriggerManager>.instance.CanRaiseEvent(obj))
		{
			return obj;
		}
		if (CheckTechnoEnchantRaid(raidType))
		{
			obj = new OpenTechnoEnchantRaid();
		}
		return obj;
	}

	void MultiSelect.Listener.RefreshChallengeRaidInfo()
	{
		RefreshChallengeRaidInfo();
	}

	bool MultiSelect.Listener.IsAbleChallengeRaidNotice()
	{
		return _challengeRaid.IsAbleNotice();
	}

	ChallengeRaidInfo MultiSelect.Listener.GetChallengeRaidNoticeInfo()
	{
		return _challengeRaid.GetNoticeInfo();
	}

	bool MultiSelect.Listener.IsAbleChallengeRaidSeason()
	{
		return _challengeRaid.IsAbleSeason();
	}

	ChallengeRaidInfo MultiSelect.Listener.GetChallengeRaidSeasonInfo()
	{
		return _challengeRaid.GetSeasonInfo();
	}

	bool MultiSelect.Listener.IsAbleChallengeRaidEnter()
	{
		return _challengeRaid.IsAbleEnter();
	}

	ChallengeRaidInfo MultiSelect.Listener.GetChallengeRaidEnterInfo()
	{
		return _challengeRaid.GetEnterInfo();
	}

	void MultiSelect.Listener.OnEnterDragonRaid()
	{
		GetInstance<RaidList>().Open(this);
	}

	void MultiSelect.Listener.OnEnterDragonRaidHard()
	{
		GetInstance<RaidList>().Open(this, RaidType.DragonHard);
	}

	void MultiSelect.Listener.OnEnterChallengeRaid()
	{
		OpenChallengeRaid();
	}

	void MultiSelect.Listener.OnEnterPartyDungeon()
	{
		SceneAutoBehaviour<MultiPartyManagement>.instance.OpenPartyDungeonRoomList();
	}

	void MultiSelect.Listener.OnEnterFieldRaid()
	{
		GetInstance<RaidList>().Open(this, RaidType.FieldRaid);
	}

	void MultiSelect.Listener.OnEnterTechnoMagicRaid()
	{
		GetInstance<RaidList>().Open(this, RaidType.TechnoMagic);
	}

	public void OpenChallengeRaidRanking()
	{
		GetInstance<ChallengeRaidRanking>().Open(this, _challengeRaid.GetSeasonInfo());
	}

	public void OpenChallengeRaid()
	{
		int challengeRaidIndex = _challengeRaid.GetSeasonInfo().ChallengeRaidIndex;
		GameRoot.Instance.ChallengeRaidDataContainer.GetData(challengeRaidIndex);
		_challengeRaid.RequestChallengeRaidRankInfo(delegate(bool success)
		{
			if (success)
			{
				OpenChallengeRaidRanking();
			}
			else
			{
				VespaDebug.LogError("Get ChallengeRaid Ranker failed.");
			}
		});
	}

	bool ChallengeRaidRanking.Listener.IsAbleSeason()
	{
		return _challengeRaid.IsAbleSeason();
	}

	bool ChallengeRaidRanking.Listener.IsAbleEnter()
	{
		return _challengeRaid.IsAbleEnter();
	}

	private void OpenAffixSelectCreate(int raidIndex, int raidLevel)
	{
		AffixSelectPopup affixSelectPopup = GetInstance<AffixSelectPopup>();
		if (!(affixSelectPopup == null) && !affixSelectPopup.IsOpen())
		{
			affixSelectPopup.OpenCreate(raidIndex, raidLevel, this);
		}
	}

	private void OpenAffixSelectChallenge(int raidIndex, int raidLevel, int[] affixIndices)
	{
		AffixSelectPopup affixSelectPopup = GetInstance<AffixSelectPopup>();
		if (!(affixSelectPopup == null) && !affixSelectPopup.IsOpen())
		{
			affixSelectPopup.OpenChallenge(raidIndex, raidLevel, affixIndices, this);
		}
	}

	private void OpenAffixSelectSetting(int raidIndex, int raidLevel, int[] affixIndices, bool canChange)
	{
		AffixSelectPopup affixSelectPopup = GetInstance<AffixSelectPopup>();
		if (!(affixSelectPopup == null) && !affixSelectPopup.IsOpen())
		{
			affixSelectPopup.OpenSetting(raidIndex, raidLevel, affixIndices, canChange, this);
		}
	}

	void ChallengeRaidRanking.Listener.OnClickEnter(int raidIndex, int raidLevel)
	{
		OpenAffixSelectCreate(raidIndex, raidLevel);
	}

	void ChallengeRaidRanking.Listener.OnClickEnterRank(int raidIndex, int raidLevel)
	{
		OpenAffixSelectCreate(raidIndex, raidLevel);
	}

	void ChallengeRaidRanking.Listener.OnClickInfo(int raidIndex)
	{
		if (raidIndex == 0)
		{
			IsAbleSeason();
			return;
		}
		int raidMaxLevel = RaidHelper.GetRaidMaxLevel(raidIndex);
		RaidData data = GameRoot.Instance.RaidDataContainer.GetData(raidIndex, raidMaxLevel);
		OpenChallengeRaidInfo(data);
	}

	void ChallengeRaidRanking.Listener.OnClickGuide()
	{
		if (GetChallengeRaidSeasonInfo().ChallengeRaidIndex == 0)
		{
			IsAbleSeason();
		}
		else
		{
			GetInstance<ChallengeRaidGuide>().Open(GetChallengeRaidSeasonInfo().ChallengeRaidIndex);
		}
	}

	void ChallengeRaidRanking.Listener.OnClickRoomList(int raidIndex)
	{
		RequestRaidRoomList(raidIndex);
	}

	void ChallengeRaidRanking.Listener.OnClickRankingReward()
	{
		if (!IsAbleSeason() || !IsAbleEnter())
		{
			string stringDirect = LocalizationManager.GetStringDirect("COMMON_ERROR");
			string stringDirect2 = LocalizationManager.GetStringDirect("RESULT_TYPE.NOT_FOUND_SEASON_DATA");
			MessagePopup.Open(stringDirect, stringDirect2);
		}
		else
		{
			GetInstance<ChallengeRaidRewardInfo>().Open();
		}
	}

	void ChallengeRaidRanking.Listener.GetSingleRankerList(int page, Action<ChallengeRaidRankerInfo[]> onList)
	{
		if (!IsAbleSeason() || !IsAbleEnter())
		{
			string stringDirect = LocalizationManager.GetStringDirect("COMMON_ERROR");
			string stringDirect2 = LocalizationManager.GetStringDirect("RESULT_TYPE.NOT_FOUND_SEASON_DATA");
			MessagePopup.Open(stringDirect, stringDirect2);
			Vespa_System_Action_Extension_Method.fire(onList, null);
		}
		else
		{
			_challengeRaid.GetRankerList(page, onList);
		}
	}

	void ChallengeRaidRanking.Listener.GetMultiRankerList(Action<MultiPlayChallengeRaidRankerInfo[]> onList)
	{
		if (!IsAbleSeason() || !IsAbleEnter())
		{
			string stringDirect = LocalizationManager.GetStringDirect("COMMON_ERROR");
			string stringDirect2 = LocalizationManager.GetStringDirect("RESULT_TYPE.NOT_FOUND_SEASON_DATA");
			MessagePopup.Open(stringDirect, stringDirect2);
			Vespa_System_Action_Extension_Method.fire(onList, null);
		}
		else
		{
			_challengeRaid.GetMultiRankerList(onList);
		}
	}

	void ChallengeRaidRanking.Listener.OpenAffixSelectChallenge(int raidIndex, int raidLevel, int[] affixIndices)
	{
		OpenAffixSelectChallenge(raidIndex, raidLevel, affixIndices);
	}

	public void OpenRaidSelect()
	{
		if (!SingletonAuto<ContentsOpenChecker>.instance.CheckContentsOpen(ContentsType.Raid))
		{
			return;
		}
		MultiSelect multiSelect = GetInstance<MultiSelect>();
		if (!(multiSelect == null))
		{
			multiSelect.Open(this);
			SceneAutoBehaviour<LobbyManagement>.instance.SaveToLocalOpenMultiSelect();
			if (!SingletonAuto<EventTriggerManager>.instance.RaiseEvent(GetOpenRaidTriggerEvent((_raidData != null) ? _raidData.Type : RaidType.NONE)))
			{
				OpenCreatureIntro();
			}
		}
	}

	private void OpenCreatureIntro()
	{
		if (_challengeRaid == null || _challengeRaid.GetSeasonInfo() == null || !IsAbleSeason() || !IsAbleEnter())
		{
			return;
		}
		string text = "ChallengeRaid_BossIntro_" + _challengeRaid.GetSeasonInfo().ChallengeRaidIndex;
		CreatureIntro.LocalValueFirstView = new LocalAccountBool(i_default: false, CreatureIntro.KeyDefaultFirstView + text);
		if (CreatureIntro.LocalValueFirstView.Value)
		{
			return;
		}
		int raidIndex = _challengeRaid.GetSeasonInfo().RaidIndex;
		int raidMaxLevel = RaidHelper.GetRaidMaxLevel(raidIndex);
		RaidData data = GameRoot.Instance.RaidDataContainer.GetData(raidIndex, raidMaxLevel);
		if (data != null && (data.Type == RaidType.Challenge || data.Type == RaidType.ChallengeSingle))
		{
			int chapterIndex = data.ChapterIndex;
			int dungeonIndex = data.DungeonIndex;
			NShared.DungeonDifficultyType difficulty = (NShared.DungeonDifficultyType)Helper.GetDifficulty(chapterIndex, dungeonIndex);
			CampaignWaveData bossWaveData;
			CreatureData bossCreatureData = GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(chapterIndex, dungeonIndex, difficulty, scenario: false, out bossWaveData);
			if (bossCreatureData != null)
			{
				SceneAutoBehaviour<LobbyManagement>.instance.OpenCreatureIntro(bossCreatureData);
			}
		}
	}

	public static void CreateParty(int raidIndex, int raidLevel, bool isPublic, RaidRoomType roomType, int[] affixIndices)
	{
		int raidBossLevel = RaidHelper.GetRaidBossLevel(raidIndex, raidLevel);
		NShared.CreateRaidRoom.Request request = new NShared.CreateRaidRoom.Request
		{
			RaidIndex = raidIndex,
			RaidLevel = raidLevel,
			RaidMonsterLevel = raidBossLevel,
			GoalIndex = (byte)roomType,
			Opened = (byte)(isPublic ? 1u : 0u),
			DungeonDifficulty = 0,
			AffixIndices = affixIndices
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.CreateRaidRoom.Response response)
		{
			if (response.Result != NShared.CreateRaidRoom.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else
			{
				RaidRoomInfo roomInfo = new RaidRoomInfo
				{
					RoomNo = response.RoomNo,
					RaidLevel = raidLevel,
					RaidIndex = raidIndex,
					GoalIndex = (byte)roomType,
					MasterNick = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Nick,
					MasterAccountId = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId,
					CurPartyMember = 1,
					Opened = request.Opened,
					AffixIndices = request.AffixIndices
				};
				GlobalAutoBehaviour<PartyManager>.instance.CreateParty(raidIndex, raidLevel, roomInfo, isRaid: true);
			}
		});
	}

	public void CreateRaidParty(int raidIndex, int raidLevel, bool isPublic, RaidRoomType roomType, int[] affixIndices)
	{
		int raidMaxLevel = RaidHelper.GetRaidMaxLevel(raidIndex);
		if (raidLevel > raidMaxLevel)
		{
			VespaDebug.LogError("Raid level is too high : raidIndex=" + raidIndex + " raidLevel=" + raidLevel + " maxRaidLevel=" + raidMaxLevel);
		}
		else
		{
			CreateParty(raidIndex, raidLevel, isPublic, roomType, affixIndices);
		}
	}

	public void OpenSingleRaidPartySetting(int raidIndex, int raidLevel, int[] affixIndices, bool isReturnToBattle = false)
	{
		if (!RestoredPortal.CheckDragonHardEntryByIndex(raidIndex, raidLevel))
		{
			return;
		}
		RaidData data = GameRoot.Instance.RaidDataContainer.GetData(raidIndex, raidLevel);
		if (data == null)
		{
			VespaDebug.LogError("RaidData not found : raid=" + data);
			return;
		}
		UIHelper.CloseWindowsBeforeRaidPartySetting();
		if (IsAttributeRaidPartySetting(data))
		{
			OpenAttributeRaidPartySetting(data, isMulti: false);
		}
		else
		{
			OpenSingleRaidPartySetting(data, raidLevel, affixIndices, isReturnToBattle);
		}
		if (IsOpened<RaidRoomList>())
		{
			CloseInstance<RaidRoomList>();
		}
	}

	private void OpenSingleRaidPartySetting(RaidData raidData, int raidLevel, int[] affixIndices, bool isReturnToBattle)
	{
		if (!RestoredPortal.CheckDragonHardEntry(raidData))
		{
			return;
		}
		RaidData raidData2 = raidData;
		if (raidData2 == null)
		{
			return;
		}
		if (!isReturnToBattle)
		{
			raidData2 = GameRoot.Instance.RaidDataContainer.GetData(raidData2.SingleRaidIndex, raidLevel);
		}
		if (raidData2 == null)
		{
			VespaDebug.LogError("SingleRaidData not found : raid=" + raidData2);
			return;
		}
		RaidSinglePartySetting raidPartySetting = GetInstance<RaidSinglePartySetting>();
		if (raidPartySetting == null)
		{
			return;
		}
		SetRaidData(raidData2);
		DefaultBattlePartySettingDataProdiver dataProvider = DataProviderBuilder.Build(raidData2, affixIndices, isStorageKey: true);
		if (raidData2.Type == RaidType.GuildSuppressSingle)
		{
			int guildSuppressDungeonIndex = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.GuildSuppressDungeonIndex;
			GuildSuppressDungeonData guildSuppressDungeonData = GameRoot.Instance.GuildSuppressDungeonDataContainer.GetData(guildSuppressDungeonIndex);
			if (guildSuppressDungeonData != null)
			{
				raidPartySetting.Open(delegate
				{
					raidPartySetting.Init(this, dataProvider, raidData2, guildSuppressDungeonData);
				});
			}
		}
		else
		{
			raidPartySetting.Open(delegate
			{
				raidPartySetting.Init(this, dataProvider, raidData2);
			});
		}
	}

	public void OpenRaidPartySetting(int raidIndex, int raidLevel)
	{
		if (!RestoredPortal.CheckDragonHardEntryByIndex(raidIndex, raidLevel))
		{
			return;
		}
		if (!GlobalAutoBehaviour<PartyManager>.isValidInstance || !GlobalAutoBehaviour<PartyManager>.instance.IsValidParty())
		{
			VespaDebug.LogError("Invalid party!");
			return;
		}
		RaidData raidData = GameRoot.Instance.RaidDataContainer.GetData(raidIndex, raidLevel);
		if (raidData == null)
		{
			VespaDebug.LogError("RaidData not found : raid=" + raidData);
			return;
		}
		SetRaidData(raidData);
		int[] affixIndices = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.GetAffixIndices();
		UIHelper.CloseWindowsBeforeRaidPartySetting();
		if (IsAttributeRaidPartySetting(raidData))
		{
			OpenAttributeRaidPartySetting(raidData, isMulti: true);
		}
		else
		{
			DefaultBattlePartySettingDataProdiver dataProvider = DataProviderBuilder.Build(raidData, affixIndices);
			RaidPartySetting window = GetInstance<RaidPartySetting>();
			window.Open(delegate
			{
				window.InitForRaid(this, dataProvider, raidData);
			});
		}
		if (IsOpened<RaidRoomList>())
		{
			CloseInstance<RaidRoomList>();
		}
	}

	private void OpenGuildSuppressInfo(GuildSuppressDungeonData guildSuppressDungeonData, int level)
	{
		GetInstance<GuildSuppressBossInfo>().Open(this, guildSuppressDungeonData, level);
	}

	public void OpenRaidInfo(RaidData raidData)
	{
		GetInstance<NGame2.NUI.NWindow.RaidInfo>().Open(this, raidData);
	}

	private void OpenChallengeRaidInfo(RaidData raidData)
	{
		GetInstance<NGame2.NUI.NWindow.RaidInfo>().Open(this, raidData);
	}

	private void OpenFieldRaidRewardInfo(RaidData raidData)
	{
		int individualReward = raidData.IndividualReward;
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(individualReward);
		if (data != null)
		{
			CampaignDungeonData data2 = GameRoot.Instance.CampaignDungeonDataContainer.GetData(raidData.ChapterIndex, raidData.DungeonIndex);
			if (data2 != null)
			{
				DungeonRewardInfo.OpenGlobal(data, data2.BattleType);
			}
		}
	}

	private void OpenAttributeRaidPartySetting(RaidData raidData, bool isMulti)
	{
		if (raidData != null)
		{
			AttributeRaidPartySetting attributeRaidPartySetting = GetInstance<AttributeRaidPartySetting>();
			if (!(attributeRaidPartySetting == null))
			{
				SetRaidData(raidData);
				attributeRaidPartySetting.Open(new AttributeRaidPartySetting.AttributeRaidPartySettingInfo
				{
					IListener = this,
					RaidData = raidData,
					IsMulti = isMulti
				});
			}
		}
	}

	public void OpenGuilSuppressPartySetting(RaidData raidData, GuildSuppressDungeonData guildSuppressDungeonData)
	{
		if (raidData == null)
		{
			return;
		}
		GuildSuppressPartySetting partySetting = GetInstance<GuildSuppressPartySetting>();
		if (!(partySetting == null))
		{
			DefaultBattlePartySettingDataProdiver dataProvider = GetDefaultBattlePartySettingDataProdiverByRaidData(raidData);
			partySetting.Open(delegate
			{
				partySetting.Init(this, dataProvider, raidData, guildSuppressDungeonData);
			});
		}
	}

	private bool IsAttributeRaidPartySetting(RaidData raidData)
	{
		if (raidData == null)
		{
			return false;
		}
		if (CheckTechnoEnchantRaid(raidData.Type))
		{
			return true;
		}
		return false;
	}

	private bool CheckTechnoEnchantRaid(RaidType raidType)
	{
		if (raidType == RaidType.TechnoEnchant || raidType == RaidType.TechnoEnchantSingle)
		{
			return true;
		}
		return false;
	}

	public DefaultBattlePartySettingDataProdiver CreateDataProvider(RaidData raidData, int[] affixIndices, bool isStorageKey = false, NShared.DungeonDifficultyType difficultyType = NShared.DungeonDifficultyType.Easy)
	{
		return DataProviderBuilder.Build(raidData, affixIndices, isStorageKey, difficultyType);
	}

	public DefaultBattlePartySettingDataProdiver GetDefaultBattlePartySettingDataProdiverByRaidData(RaidData raidData, bool isStorageKey = false, NShared.DungeonDifficultyType difficultyType = NShared.DungeonDifficultyType.Easy)
	{
		if (raidData == null)
		{
			return null;
		}
		int chapterIndex = raidData.ChapterIndex;
		int dungeonIndex = raidData.DungeonIndex;
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			VespaDebug.LogError("Raid DungeonData found : raid=" + raidData?.ToString() + " chapter=" + chapterIndex + " dungeon=" + dungeonIndex);
			return null;
		}
		int staminaOfDifficulty = data.GetStaminaOfDifficulty(GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina());
		if (data.ReqStaminaType != StaminaType.Empty && staminaOfDifficulty <= 0)
		{
			VespaDebug.LogError("Raid chicken is 0 : raid=" + raidData?.ToString() + " chapter=" + chapterIndex + " dungeon=" + dungeonIndex);
			return null;
		}
		EnterDungeonData enterDungeonData = new EnterDungeonData(chapterIndex, dungeonIndex, dungeonIndex, difficultyType);
		if (enterDungeonData == null)
		{
			return null;
		}
		return new DefaultBattlePartySettingDataProdiver(isStorageKey ? data.LocalStorageKey : null, data.ReqStaminaType, staminaOfDifficulty, Battle.START_BATTLE, enterDungeonData);
	}

	private void ChangeRoomSettingAttributeSingleRaid(RaidData raidData)
	{
		AttributeRaidPartySetting attributeRaidPartySetting = GetInstance<AttributeRaidPartySetting>();
		if (!(attributeRaidPartySetting == null))
		{
			attributeRaidPartySetting.ChangeRoomSettingSingleRaid(raidData);
		}
	}

	private void ChangeRoomSettingSingleRaid(RaidData raidData)
	{
		RaidSinglePartySetting raidSinglePartySetting = GetInstance<RaidSinglePartySetting>();
		if (!(raidSinglePartySetting == null))
		{
			raidSinglePartySetting.ChangeSingleRoomSetting(raidData);
		}
	}

	private void RequestRaidRoomList(int raidIndex, int filterValue = 0, RaidRoomType type = RaidRoomType.All)
	{
		NShared.SearchRaidRoom.Request request = new NShared.SearchRaidRoom.Request
		{
			RaidIndex = raidIndex,
			StartRaidMonsterLevel = filterValue,
			GoalIndex = (byte)type
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SearchRaidRoom.Response response)
		{
			if (response.Result != NShared.SearchRaidRoom.ResultType.Success)
			{
				CenterMessage.Open(LocalizationManager.GetString(response.Result));
			}
			if (response.BaseResult != BaseResultType.Success)
			{
				CenterMessage.Open(LocalizationManager.GetString(response.BaseResult));
			}
			RaidRoomList raidRoomList = GetInstance<RaidRoomList>();
			if (!raidRoomList.IsOpen())
			{
				raidRoomList.Open(response.RoomInfos, raidIndex, this);
			}
			else
			{
				raidRoomList.RefreshList(response.RoomInfos);
			}
		});
	}

	public void RequestRaidRoomEnter(RaidRoomInfo roomInfo)
	{
		GlobalAutoBehaviour<PartyManager>.instance.SendJoinPartyRequest_Accept(roomInfo.MasterAccountId, roomInfo.RoomNo);
	}

	void PartyManager.INewPartyListener.OnPartyCreated()
	{
		PartyInfo partyInfo = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo;
		if (!partyInfo.IsValid())
		{
			VespaDebug.LogError("Party created but is invalid");
			return;
		}
		if (GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo() == null)
		{
			VespaDebug.LogError("Party created but cannot find my member info.");
			return;
		}
		RaidData data = GameRoot.Instance.RaidDataContainer.GetData(partyInfo.RaidIndex, partyInfo.RaidLevel);
		if (data == null)
		{
			return;
		}
		if (data.IsGuildSuppressContents())
		{
			GuildSuppressDungeonData data2 = GameRoot.Instance.GuildSuppressDungeonDataContainer.GetData(partyInfo.GuildSuppressDungeonIndex);
			if (data2 != null)
			{
				SetRaidData(data);
				SceneAutoBehaviour<GuildSuppressManagement>.instance.OpenGuildSuppressPartySetting(data2, data.Index, data.Level);
			}
		}
		else
		{
			OpenRaidPartySetting(partyInfo.RaidIndex, partyInfo.RaidLevel);
		}
	}

	void PartyManager.IPartyListener.OnPartyDestroy(bool isMasterLeave)
	{
		_ = RaidIndex;
		SetRaidData(null);
		if (IsOpened<AffixSelectPopup>())
		{
			AffixSelectPopup affixSelectPopup = GetInstance<AffixSelectPopup>();
			if (affixSelectPopup != null)
			{
				affixSelectPopup.Close();
			}
		}
	}

	private void SetRaidData(RaidData raidData)
	{
		_raidData = raidData;
	}

	void PartyManager.INewPartyListener.OnPartyInvited(long senderId, InvitePartyReq data)
	{
		GlobalAutoBehaviour<InviteNotifyManager>.instance.AddNotify(InviteNotifyType.Raid, data, delegate(bool accepted)
		{
			if (accepted)
			{
				accepted = InviteNotifyManager.IsOpenableNoty(senderId);
			}
			if (accepted)
			{
				GlobalAutoBehaviour<PartyManager>.instance.SendJoinPartyRequest_Accept(senderId, data.RoomNo);
			}
			else
			{
				GlobalAutoBehaviour<PartyManager>.instance.SendJoinPartyRequest_Reject(senderId);
			}
		});
	}

	void PartyManager.IPartyListener.OnPartyMemberDeckCostumeChanged(long accountId, int heroIndex, CachedHeroInfo heroInfo)
	{
	}

	void PartyManager.IPartyListener.OnPartyMemberJoin(MemberInfo memberInfo)
	{
	}

	void PartyManager.IPartyListener.OnPartyMemberDeckHeroChanged(MemberInfo memberInfo, int heroIndex, CachedHeroInfo heroInfo, bool isMain)
	{
	}

	void PartyManager.IPartyListener.OnPartyMemberReady(MemberInfo memberInfo, bool isReady)
	{
	}

	void PartyManager.IPartyListener.OnPartyMemberChangeAvatar(MemberInfo memberInfo)
	{
	}

	void PartyManager.IPartyListener.OnPartyMemberLeave(MemberInfo memberInfo)
	{
	}

	void PartyManager.IPartyListener.OnChangePartyMemberStatus(MemberInfo memberInfo)
	{
	}

	void PartyManager.IPartyListener.OnPartyRoomInfoUpdated(RaidRoomInfo raidRoomInfo, bool isViolateBanRule)
	{
	}

	void PartyManager.IPartyListener.OnPartyRoomMasterChanged(MemberInfo memberInfo)
	{
	}

	void PartyManager.IPartyListener.OnPartyMemberResults(MemberInfo memberInfo)
	{
	}

	void PartyManager.IPartyListener.OnPartyRoomSetRepeat(bool IsRepeat)
	{
	}

	void RaidList.Listener.OnClickEnter(RaidList caller, RaidListComponent component)
	{
		if (caller == null || component == null)
		{
			return;
		}
		RaidData raidData = component.RaidData;
		if (raidData != null)
		{
			RaidType type = raidData.Type;
			CurrencyMaxCap.WarningOrExecute(ContentsDefine.CurrencyMaxCap.GetCurrencyTypeOnEndBattle(type), delegate
			{
				EnterRaidInternal(raidData);
			});
		}
	}

	private void EnterRaidInternal(RaidData raidData)
	{
		if (RestoredPortal.CheckDragonHardEntry(raidData))
		{
			int index = raidData.Index;
			int raidMaxLevel = RaidHelper.GetRaidMaxLevel(index);
			GetInstance<RaidLevelSelect>().Open(this, index, raidMaxLevel);
		}
	}

	void RaidList.Listener.OnClickViewInfo(RaidList caller, RaidListComponent component)
	{
		if (component.RaidData.Type == RaidType.FieldRaid)
		{
			OpenFieldRaidRewardInfo(component.RaidData);
		}
		else
		{
			OpenRaidInfo(component.RaidData);
		}
	}

	void RaidList.Listener.OnClickRaidRoomList(RaidList caller, RaidListComponent component)
	{
		RaidData raidData = component.RaidData;
		if (raidData != null)
		{
			RaidType type = raidData.Type;
			CurrencyMaxCap.WarningOrExecute(ContentsDefine.CurrencyMaxCap.GetCurrencyTypeOnEndBattle(type), delegate
			{
				RequestRaidRoomList(component.RaidIndex);
			});
		}
	}

	void RaidList.Listener.OnClickDispatch(RaidList caller, RaidListComponent component)
	{
		if (!(component == null) && component.RaidData != null)
		{
			int raidMaxCompleteLv = DispatchHelper.GetRaidMaxCompleteLv(component.RaidData);
			RaidData data = GameRoot.Instance.RaidDataContainer.GetData(component.RaidData.Index, raidMaxCompleteLv);
			if (data != null)
			{
				ContentUIDispatchBattle.OpenGlobalRaid(data);
			}
		}
	}

	void BattlePartySetting.Listener.StartBattle(EnterDungeonData enterDungeon, BattlePartySetting.BattlePartyInfo battlePartyInfo, BattlePartySetting.ExtraOptionData extraOption)
	{
		if (RaidPartySetting.IsMultiRaid || Time.time - _battleRequestedTime < 10f || enterDungeon == null)
		{
			return;
		}
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(enterDungeon.Chapter, enterDungeon.OriginalDungeon);
		if (data == null)
		{
			return;
		}
		_battleRequestedTime = Time.time;
		NShared.BeginCampaign.Request request = new NShared.BeginCampaign.Request();
		request.ChapterIndex = enterDungeon.Chapter;
		request.DungeonIndex = enterDungeon.OriginalDungeon;
		request.RaidIndex = RaidIndex;
		request.RaidLevel = RaidLevel;
		request.LinkedDungeonIndex = enterDungeon.LinkedDungeon;
		request.DungeonDifficulty = (int)enterDungeon.Difficulty;
		request.ScenarioDungeon = enterDungeon.IsScenario;
		request.HeroIndices = battlePartyInfo.heroIndices;
		request.LeaderHeroIndex = battlePartyInfo.leaderHeroIndex;
		request.AccountIds = battlePartyInfo.accountIds;
		request.Repeat = false;
		int onetimeBoosterCount = battlePartyInfo.OnetimeBoosterCount;
		request.ItemBoosterCount = onetimeBoosterCount;
		request.DeltaHeroIndices = ((!__Extension_Method_ICollection__.IsNullOrEmpty(battlePartyInfo.DeltaHeroIndexList)) ? battlePartyInfo.DeltaHeroIndexList.ToArray() : null);
		request.ZetaHeroIndices = ((!__Extension_Method_ICollection__.IsNullOrEmpty(battlePartyInfo.ZetaHeroIndexList)) ? battlePartyInfo.ZetaHeroIndexList.ToArray() : null);
		request.AffixIndices = battlePartyInfo.AffixIndices;
		if (CampaignContext.CheckEachWaveEachTeam(data.BattleType))
		{
			request.GroupHeroIndices = battlePartyInfo.subHeroIndices;
		}
		else
		{
			request.AiHeroIndices = battlePartyInfo.subHeroIndices;
		}
		SingletonAuto<BattleContext>.instance.BeginCampaign(request, delegate
		{
			if (extraOption != null && extraOption.AutoRepeat)
			{
				NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.OpenGlobal();
			}
			LobbyScene2.ListReturnState.Add(new RaidReturnState
			{
				AffixIndices = battlePartyInfo.AffixIndices,
				_raidData = _raidData
			});
		});
	}

	bool BattlePartySetting.Listener.OnCloseRequest(BattlePartySetting closeThis)
	{
		return true;
	}

	void BattlePartySetting.Listener.OpenHeroDetailView(CachedHeroInfo heroInfo)
	{
	}

	bool BattlePartySetting.Listener.OpenAutoNextBattleSelect()
	{
		return false;
	}

	BanRuleData RaidPartySetting.Listener.GetRaidBanData(RaidData raidData)
	{
		if (raidData.Type == RaidType.GuildSuppress || raidData.Type == RaidType.GuildSuppressSingle)
		{
			int guildSuppressDungeonIndex = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.GuildSuppressDungeonIndex;
			GuildSuppressDungeonData data = GameRoot.Instance.GuildSuppressDungeonDataContainer.GetData(guildSuppressDungeonIndex);
			if (data == null)
			{
				return null;
			}
			BanRuleData data2 = GameRoot.Instance.BanRuleDataContainer.GetData(data.BanIndex);
			if (data2 == null || data2.Index == 0)
			{
				return null;
			}
			return data2;
		}
		if (raidData.Type == RaidType.TechnoMagic || raidData.Type == RaidType.TechnoMagicSingle)
		{
			BanRuleData data3 = GameRoot.Instance.BanRuleDataContainer.GetData(raidData.BanIndex);
			if (data3 == null || data3.Index == 0)
			{
				return null;
			}
			return data3;
		}
		return null;
	}

	void RaidPartySetting.Listener.OnEnterRaidPartySetting(RaidData raidData)
	{
		if (raidData.Type == RaidType.Challenge && _challengeRaid.LastStoragedRaidIndex.Value != GetChallengeRaidSeasonInfo().ChallengeRaidIndex)
		{
			_challengeRaid.LastStoragedRaidIndex.Value = GetChallengeRaidSeasonInfo().ChallengeRaidIndex;
			GetInstance<ChallengeRaidGuide>().Open(GetChallengeRaidSeasonInfo().ChallengeRaidIndex);
		}
	}

	void RaidPartySetting.Listener.OnClickPartyMemberName(MemberInfo memberInfo)
	{
		InspectUserManagement.InspectUserData inspectUser = new InspectUserManagement.InspectUserData(memberInfo, InspectUserCategory.Raid);
		SceneAutoBehaviour<InspectUserManagement>.instance.OpenMenuPopup(inspectUser);
	}

	void RaidPartySetting.Listener.OnClickInvite(RaidPartySetting caller)
	{
		long[] partyMemberIds = GlobalAutoBehaviour<PartyManager>.instance.GetPartyMemberIds();
		RaidData raidData = caller.RaidData;
		if (raidData != null)
		{
			GetInstance<RaidInviteList>().Open(this, partyMemberIds, raidData.Type != RaidType.GuildSuppress);
		}
	}

	void RaidPartySetting.Listener.OnClickRaidInfo(RaidPartySetting caller)
	{
		if (caller.RaidData.Type == RaidType.GuildSuppress)
		{
			GuildSuppressPartySetting guildSuppressPartySetting = caller as GuildSuppressPartySetting;
			OpenGuildSuppressInfo(guildSuppressPartySetting.GuildSuppressDungeonData, guildSuppressPartySetting.RaidData.Level);
		}
		else if (caller.RaidData.Type == RaidType.GuildSuppressSingle)
		{
			RaidSinglePartySetting raidSinglePartySetting = caller as RaidSinglePartySetting;
			OpenGuildSuppressInfo(raidSinglePartySetting.GuildSuppressDungeonData, raidSinglePartySetting.RaidData.Level);
		}
		else if (caller.RaidData.Type == RaidType.FieldRaid || caller.RaidData.Type == RaidType.FieldRaidSingle)
		{
			OpenFieldRaidRewardInfo(caller.RaidData);
		}
		else
		{
			OpenRaidInfo(caller.RaidData);
		}
	}

	void RaidPartySetting.Listener.OnPartyMemberJoin(MemberInfo memberInfo)
	{
		RaidInviteList raidInviteList = GetInstance<RaidInviteList>();
		if ((bool)raidInviteList && raidInviteList.IsOpen())
		{
			raidInviteList.UpdateInvited(memberInfo.AccountId, invited: true);
		}
	}

	void RaidPartySetting.Listener.OnPartyMemberLeave(long accountId)
	{
		RaidInviteList raidInviteList = GetInstance<RaidInviteList>();
		if ((bool)raidInviteList && raidInviteList.IsOpen())
		{
			raidInviteList.UpdateInvited(accountId, invited: false);
		}
		RaidPartySetting raidPartySetting = GetInstance<RaidPartySetting>();
		if (raidPartySetting.IsOpen() && accountId == GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId)
		{
			raidPartySetting.Close();
		}
	}

	void RaidPartySetting.Listener.OnAutoDestroyGuildRaidParty()
	{
		NGame2.NUI.NWindow.RaidInfo raidInfo = GetInstance<NGame2.NUI.NWindow.RaidInfo>();
		if (raidInfo != null)
		{
			raidInfo.Close();
		}
	}

	void RaidPartySetting.Listener.OnSubmitPartyChat(string chat)
	{
		SceneAutoBehaviour<ChattingManagement>.instance.SendPartyChat(chat);
	}

	List<ChatInfo> RaidPartySetting.Listener.GetPartyChat()
	{
		return GlobalAutoBehaviour<ChattingManager>.instance.GetChatList(ChatType.Party, 0L);
	}

	void RaidPartySetting.Listener.OnClickEmoticonSelect(RaidPartySetting caller)
	{
		ChattingPartyComponent chattingComponent = caller.PartyChat;
		if (!(chattingComponent == null))
		{
			ChattingEmoticonSelectPopup.Open(caller, delegate(EmoticonData emoticonData)
			{
				string value = "(" + emoticonData.Keyword + ")";
				chattingComponent.Input.value = value;
				chattingComponent.Input.selectAllTextOnFocus = false;
				chattingComponent.Input.isSelected = true;
			});
		}
	}

	bool RaidPartySetting.Listener.CanInputPartyChat()
	{
		return !IsOpened<RaidInviteList>();
	}

	void RaidPartySetting.Listener.OnClickRoomSetting(int raidIndex, int raidLevel)
	{
		GetInstance<RaidLevelSelect>().Open(this, raidIndex, raidLevel, RaidLevelSelect.WindowType.Setting);
	}

	void RaidPartySetting.Listener.OnClickSingleRoomSetting(int raidIndex, int raidLevel)
	{
		GetInstance<RaidLevelSelect>().Open(this, raidIndex, raidLevel, RaidLevelSelect.WindowType.Setting, isSingleRoom: true);
	}

	void RaidInviteList.Listener.Invite(RaidInviteList caller, RaidInviteComponent component)
	{
		GlobalAutoBehaviour<PartyManager>.instance.SendInvitePartyRequest(component.AccountId, component.Nick);
	}

	void RaidInviteList.Listener.InviteAllFriend(RaidInviteList caller)
	{
		GlobalAutoBehaviour<PartyManager>.instance.SendInvitePartyRequestToAllFriend();
	}

	void RaidInviteList.Listener.InviteAllGuild(RaidInviteList caller)
	{
		GlobalAutoBehaviour<PartyManager>.instance.SendInvitePartyRequestToAllGuild();
	}

	private void FindUserInGuildSuppress(string nick, Action<CachedUserInfo> onComplete)
	{
		CachedUserInfo userInfo = GetInstance<RaidInviteList>().GuildInvitor.GetUserInfo(nick);
		if (userInfo == null)
		{
			MessagePopup.OpenError(NShared.SearchFriend.ResultType.FriendNotFound);
		}
		Vespa_System_Action_Extension_Method.fire(onComplete, userInfo);
	}

	void RaidInviteList.Listener.FindUser(string Nick, Action<CachedUserInfo> onComplete)
	{
		if (_raidData != null && _raidData.Type == RaidType.GuildSuppress)
		{
			FindUserInGuildSuppress(Nick, onComplete);
			return;
		}
		NShared.SearchFriend.Request request = new NShared.SearchFriend.Request
		{
			Keyword = Nick
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SearchFriend.Response response)
		{
			if (response == null)
			{
				Vespa_System_Action_Extension_Method.fire(onComplete, null);
			}
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				Vespa_System_Action_Extension_Method.fire(onComplete, null);
			}
			else if (response.Result != NShared.SearchFriend.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				Vespa_System_Action_Extension_Method.fire(onComplete, null);
			}
			else if (!__Extension_Method_ICollection__.IsNullOrEmpty(response.FriendInfos) && response.FriendInfos.Length != 0)
			{
				if (string.Compare(response.FriendInfos[0].Nick, Nick) == 0)
				{
					Vespa_System_Action_Extension_Method.fire(onComplete, response.FriendInfos[0]);
				}
				else
				{
					MessagePopup.OpenError(NShared.SearchFriend.ResultType.FriendNotFound);
					Vespa_System_Action_Extension_Method.fire(onComplete, null);
				}
			}
			else
			{
				Vespa_System_Action_Extension_Method.fire(onComplete, null);
			}
		});
	}

	void RaidLevelSelect.Listener.OnSelectRaidLevelForSingleRaid(RaidLevelSelect caller, int raidIndex, int raidLevel)
	{
		caller.Close();
		RaidData data = GameRoot.Instance.RaidDataContainer.GetData(raidIndex, raidLevel);
		if (data == null)
		{
			VespaDebug.LogError("RaidData not found : raid=" + data);
			return;
		}
		UIHelper.CloseWindowsBeforeRaidPartySetting();
		if (IsAttributeRaidPartySetting(data))
		{
			data = GameRoot.Instance.RaidDataContainer.GetData(data.SingleRaidIndex, data.Level);
			OpenAttributeRaidPartySetting(data, isMulti: false);
		}
		else
		{
			OpenSingleRaidPartySetting(data, raidLevel, null, isReturnToBattle: false);
		}
	}

	void RaidLevelSelect.Listener.OnSelectRaidLevel(RaidLevelSelect caller, int raidIndex, int level)
	{
		bool isPublic = caller.IsPublic;
		RaidRoomType roomType = caller.RoomType;
		caller.Close();
		if (!IsEnterAble(raidIndex, level))
		{
			ShowPopupBattleEnd();
		}
		else
		{
			CreateRaidParty(raidIndex, level, isPublic, roomType, null);
		}
	}

	void RaidLevelSelect.Listener.OnChangeRoomSetting(RaidLevelSelect caller, int raidLevel, RaidRoomType type, bool isViolateBanRule)
	{
		caller.Close();
		GlobalAutoBehaviour<PartyManager>.instance.SendChagneRaidRoomSetting(raidLevel, (byte)type, null, isViolateBanRule);
	}

	void RaidLevelSelect.Listener.OnChangeSingleRoomSetting(RaidLevelSelect caller, RaidData raidData)
	{
		caller.Close();
		if (raidData != null)
		{
			SetRaidData(raidData);
			if (IsAttributeRaidPartySetting(raidData))
			{
				ChangeRoomSettingAttributeSingleRaid(raidData);
			}
			else
			{
				ChangeRoomSettingSingleRaid(raidData);
			}
		}
	}

	void IChatListener.OnChatAdded(ChatInfo chatInfo, bool showPreview, EmotiType emoticon)
	{
		if (chatInfo != null && chatInfo.ChatType == ChatType.Party)
		{
			RaidPartySetting raidPartySetting = null;
			if (IsOpened<RaidPartySetting>())
			{
				raidPartySetting = SceneAutoBehaviour<RaidManagement>.instance.GetInstance<RaidPartySetting>();
			}
			else if (IsOpened<AttributeRaidPartySetting>())
			{
				raidPartySetting = SceneAutoBehaviour<RaidManagement>.instance.GetInstance<AttributeRaidPartySetting>();
			}
			else if (IsOpened<GuildSuppressPartySetting>())
			{
				raidPartySetting = SceneAutoBehaviour<RaidManagement>.instance.GetInstance<GuildSuppressPartySetting>();
			}
			if (raidPartySetting != null && raidPartySetting.PartyChat != null)
			{
				raidPartySetting.PartyChat.AddChat(chatInfo);
				GlobalAutoBehaviour<ChattingManager>.instance.MakeChatChecked(chatInfo.ChatType, 0L);
				raidPartySetting.PartyChat.Reposition();
			}
		}
	}

	void IChatListener.OnWhisperUserAdded(long accountId, string userName)
	{
	}

	void IChatListener.OnWhisperUserRemoved(long accountId)
	{
	}

	void RaidRoomList.Listener.OnClickEnterRaid(RaidRoomInfo roomIndex)
	{
		RequestRaidRoomEnter(roomIndex);
	}

	void RaidRoomList.Listener.RefreshList(int raidIndex, int filterValue, RaidRoomType type)
	{
		RequestRaidRoomList(raidIndex, filterValue, type);
	}

	void RaidRoomList.Listener.OpenAffixSelectSetting(int raidindex, int raidLevel, int[] affixIndices)
	{
		OpenAffixSelectSetting(raidindex, raidLevel, affixIndices, canChange: false);
	}

	public static string GetRaidRoomTypeString(RaidRoomType type, RaidData raidData, bool isTitle)
	{
		string key = "";
		if (raidData != null && raidData.Type == RaidType.TechnoMagic)
		{
			key = GetTechnoMagicRaidTypeString(type, isTitle);
		}
		else
		{
			switch (type)
			{
			case RaidRoomType.All:
				key = (isTitle ? "" : "RAID_ROOM_SELECT_ALL");
				break;
			case RaidRoomType.Help:
				key = ((raidData != null && raidData.Type == RaidType.FieldRaid) ? (isTitle ? "RAID_ROOM_TITLE_HELP_FIELD" : "RAID_ROOM_SELECT_HELP") : ((raidData == null || raidData.Type != RaidType.TechnoEnchant) ? (isTitle ? "RAID_ROOM_TITLE_HELP" : "RAID_ROOM_SELECT_HELP") : (isTitle ? "RAID_ROOM_TITLE_HELP_TECHNO_ENCHANT" : "RAID_ROOM_SELECT_HELP")));
				break;
			case RaidRoomType.LevelUp:
				key = (isTitle ? "RAID_ROOM_TITLE_LEVELUP" : "RAID_ROOM_SELECT_LEVELUP");
				break;
			case RaidRoomType.Advice:
				key = (isTitle ? "RAID_ROOM_TITLE_ADVICE" : "RAID_ROOM_SELECT_ADVICE");
				break;
			case RaidRoomType.Physical:
				key = (isTitle ? "RAID_ROOM_TITLE_PHYSICAL" : "RAID_ROOM_SELECT_PHYSICAL");
				break;
			case RaidRoomType.Magical:
				key = (isTitle ? "RAID_ROOM_TITLE_MAGICAL" : "RAID_ROOM_SELECT_MAGICAL");
				break;
			}
		}
		return LocalizationManager.GetStringDirect(key);
	}

	private static string GetTechnoMagicRaidTypeString(RaidRoomType type, bool isTitle)
	{
		string result = "";
		switch (type)
		{
		case RaidRoomType.All:
			result = (isTitle ? "" : "RAID_ROOM_SELECT_ALL");
			break;
		case RaidRoomType.Help:
			result = (isTitle ? "TECHNO_MAGIC_RAID_ROOM_TITLE_HELP" : "RAID_ROOM_SELECT_HELP");
			break;
		case RaidRoomType.LevelUp:
			result = (isTitle ? "TECHNO_MAGIC_RAID_ROOM_TITLE_LEVEL_UP" : "RAID_ROOM_SELECT_LEVELUP");
			break;
		case RaidRoomType.Advice:
			result = (isTitle ? "TECHNO_MAGIC_RAID_ROOM_TITLE_NO_LIMIT" : "RAID_ROOM_SELECT_ADVICE");
			break;
		case RaidRoomType.Physical:
			result = (isTitle ? "TECHNO_MAGIC_RAID_ROOM_TITLE_PHYSICAL" : "RAID_ROOM_SELECT_PHYSICAL");
			break;
		case RaidRoomType.Magical:
			result = (isTitle ? "TECHNO_MAGIC_RAID_ROOM_TITLE_MAGICAL" : "RAID_ROOM_SELECT_MAGICAL");
			break;
		}
		return result;
	}

	void RaidPartySetting.Listener.OnClickOpenHero()
	{
		List<CachedHeroInfo> heroInfoList = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: true);
		CachedHeroInfo cachedHeroInfo = ((__Extension_Method_ICollection__.COUNT(heroInfoList) > 0) ? heroInfoList[0] : null);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroInfoList); i++)
		{
			if (heroInfoList[i] != null)
			{
				cachedHeroInfo = heroInfoList[i];
				break;
			}
		}
		if (cachedHeroInfo == null)
		{
			VespaDebug.LogError("Cannot select hero.");
		}
		else
		{
			SceneAutoBehaviour<MyHeroManagement>.instance.OpenMyHero(cachedHeroInfo);
		}
	}

	void RaidPartySetting.Listener.OnClickOpenInventory()
	{
		SceneAutoBehaviour<InventoryManagement>.instance.OpenInventory();
	}

	void RaidPartySetting.Listener.OnClickClose(RaidPartySetting caller)
	{
		caller.Close();
	}

	void RaidPartySetting.Listener.OnClickAffixSetting(int raidindex, int raidLevel, int[] affixIndices, bool canChange)
	{
		OpenAffixSelectSetting(raidindex, raidLevel, affixIndices, canChange);
	}

	void RaidPartySetting.Listener.OnClickStartBattle(RaidPartySetting caller)
	{
		caller.OnClickStartBattle();
	}

	void BaseAffixPanel.IListener.OnClickAlwaysSave(bool isSave)
	{
		AffixHelper.SetAlwaysSave(isSave);
	}

	void AffixSelectPopup.IListener.OnClickCreateRoom(AffixSelectPopup caller)
	{
		if (caller == null)
		{
			return;
		}
		RaidData raidData = caller.GetRaidData();
		if (raidData == null)
		{
			VespaDebug.LogError("RaidData not found : raidData");
			return;
		}
		caller.SaveOrResetAffixIndices();
		int[] affixIndices = caller.GetAffixIndices();
		if (raidData.Type == RaidType.ChallengeSingle)
		{
			SetRaidData(raidData);
			caller.Close();
			UIHelper.CloseWindowsBeforeRaidPartySetting();
			NShared.DungeonDifficultyType difficulty = (NShared.DungeonDifficultyType)Helper.GetDifficulty(raidData.ChapterIndex, raidData.DungeonIndex);
			DefaultBattlePartySettingDataProdiver dataProvider = DataProviderBuilder.Build(raidData, affixIndices, isStorageKey: true, difficulty);
			RaidSinglePartySetting window = GetInstance<RaidSinglePartySetting>();
			window.Open(delegate
			{
				window.Init(this, dataProvider, raidData);
			});
		}
		else
		{
			int index = raidData.Index;
			int level = raidData.Level;
			bool isPublic = caller.IsPublic;
			RaidRoomType roomType = caller.RoomType;
			if (!IsEnterAble(index, level))
			{
				ShowPopupBattleEnd();
				return;
			}
			caller.Close();
			CreateParty(index, level, isPublic, roomType, affixIndices);
		}
	}

	void AffixSelectPopup.IListener.OnClickMultiSettingRoom(AffixSelectPopup caller)
	{
		if (caller == null)
		{
			return;
		}
		RaidData raidData = caller.GetRaidData();
		if (raidData == null)
		{
			VespaDebug.LogError("RaidData not found : raidData");
			return;
		}
		int[] affixIndices = caller.GetAffixIndices();
		int[] affixIndices2 = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.GetAffixIndices();
		if (_raidData == raidData && AffixHelper.IsEqualAffixIndices(affixIndices2, affixIndices))
		{
			caller.Close();
			return;
		}
		SetRaidData(raidData);
		int level = raidData.Level;
		RaidRoomType roomType = caller.RoomType;
		caller.Close();
		GlobalAutoBehaviour<PartyManager>.instance.SendChagneRaidRoomSetting(level, (byte)roomType, affixIndices);
	}

	void AffixSelectPopup.IListener.OnClickSingelSettingRoom(AffixSelectPopup caller)
	{
		if (caller == null)
		{
			return;
		}
		RaidData raidData = caller.GetRaidData();
		if (raidData == null)
		{
			VespaDebug.LogError("RaidData not found : raidData");
			return;
		}
		SetRaidData(raidData);
		int[] affixIndices = caller.GetAffixIndices();
		caller.Close();
		RaidSinglePartySetting raidSinglePartySetting = GetInstance<RaidSinglePartySetting>();
		if (!(raidSinglePartySetting == null) && raidSinglePartySetting.IsOpen())
		{
			raidSinglePartySetting.ChangeSingleRoomSetting(raidData, affixIndices);
		}
	}
}
