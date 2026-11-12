using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NEventTrigger;
using NGame2.NGuild;
using NGame2.NMessageLog;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NGuildAttendance;
using NGame2.NUI.NManager.NGuilRaid;
using NGame2.NUI.NManager.NGuildAttendance;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NGuildAttendance;
using NGame2.NUtil;
using NShared;
using NShared.AcceptJoinRequest;
using NShared.ApplyGuildSettingChange;
using NShared.BeginCampaign;
using NShared.CancelRequestJoinGuild;
using NShared.CheckGuildName;
using NShared.ContributeGuild;
using NShared.CreateGuild;
using NShared.DelegateMaster;
using NShared.DestroyGuild;
using NShared.GetAllGuildMemberInfo;
using NShared.GetGuildBasicInfo;
using NShared.GetGuildList;
using NShared.GetGuildRaidList;
using NShared.GetGuildRankingBoardRankInfo;
using NShared.GetGuildRankingBoardRankerList;
using NShared.GetGuildRankingBoardServerTotalRankInfo;
using NShared.GetGuildRankingBoardServerTotalRankingList;
using NShared.GetGuildRankingBoardViewSeasonInfo;
using NShared.GetJoinRequestPlayers;
using NShared.GetJoinRequestedGuilds;
using NShared.InitGuildSkill;
using NShared.InvestGuildBuilding;
using NShared.KickGuildMember;
using NShared.LevelUpGuild;
using NShared.LevelUpGuildBuilding;
using NShared.LevelUpGuildSkill;
using NShared.NEvent;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NShared.RejectJoinRequest;
using NShared.RequestJoinGuild;
using NShared.RequestLevelUpGuildSkill;
using NShared.SearchGuild;
using NShared.SendGuildNotice;
using NShared.SetGuildAdminRank;
using NShared.WithdrawGuild;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NManager;

public class GuildManagement2 : BaseManagement<GuildManagement2>, CreateGuild2.Listener, GuildList2.Listener, GuildManage.Listener, GuildConstruction.Listener, ManageMenu.Listener, GuildRaidRanking.Listener, GuildRaidDungeonList.Listener, RaidInfoBase.Listener, BattlePartySetting.Listener, GuildRankingBoardSuppress.Listener, GuildTrainingCenter.Listener, GuildNoticePopup.Listener, SelectCountryFlag.Listener, GuildLogoEditPopup.Listener, GuildAttendancePopup.IListener, GuildAttendanceComponent.IListener
{
	public enum GuildSettingIdx
	{
		ReqLevel,
		Introduce,
		Notice,
		JoinWay,
		Logo
	}

	private DateTime _lastGuildListRequestTime = DateTime.UtcNow;

	private float _battleRequestedTime;

	private float _cachedRankerTime;

	private GuildRankingBoardRankerInfo _myGuildBoardRankInfo;

	private GuildRankingBoardServerTotalRankingInfo _myGuildBoardServerTotalRankInfo;

	private Dictionary<int, GuildRankingBoardRankerInfo[]> _cachedRankerDic = new Dictionary<int, GuildRankingBoardRankerInfo[]>();

	private Dictionary<int, GuildRankingBoardServerTotalRankingInfo[]> _cachedServerTotalRankingDic = new Dictionary<int, GuildRankingBoardServerTotalRankingInfo[]>();

	private GuildRankingBoardType _curRankingBoardType;

	private GuildRankingType _curRankingType;

	private GuildRaidWebManager _guildRaidWebManager = new GuildRaidWebManager();

	private GuildAttendanceWebManager _guildAttendanceWebManager;

	private bool CheckIsGuild
	{
		get
		{
			if (!GlobalAutoBehaviour<GuildManager>.isValidInstance)
			{
				return false;
			}
			return GlobalAutoBehaviour<GuildManager>.instance.CheckIsGuild;
		}
	}

	private bool CheckOpenPossible
	{
		get
		{
			if (SceneAutoBehaviour<GuildSuppressManagement>.isValidInstance && SceneAutoBehaviour<GuildSuppressManagement>.instance.IsOpenedAny())
			{
				return false;
			}
			if (SceneAutoBehaviour<GuildArenaManagement>.isValidInstance && SceneAutoBehaviour<GuildArenaManagement>.instance.IsOpenedAny())
			{
				return false;
			}
			return true;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		UseBackgroundBlack(use: true);
		AddInstance<CreateGuild2>("Resources/Prefab2/Window/Guild/CreateGuild2");
		AddInstance<GuildList2>("Resources/Prefab2/Window/Guild/GuildList2");
		AddInstance<GuildManage>("Resources/Prefab2/Window/Guild/GuildManage");
		AddInstance<ManageMenu>("Resources/Prefab2/Window/Guild/ManageMenu");
		AddInstance<GuildRaidRanking>("Resources/Prefab2/Window/GuildRaid/GuildRaidRanking");
		AddInstance<GuildRaidDungeonList>("Resources/Prefab2/Window/GuildRaid/GuildRaidDungeonList");
		AddInstance<NGame2.NUI.NWindow.GuildRaidDungeonInfo>("Resources/Prefab2/Window/GuildRaid/GuildRaidDungeonInfo");
		AddInstance<BattlePartySetting>("Resources/Prefab2/Window/BattlePartySetting/BattlePartySetting");
		AddInstance<GuildRaidGuildRanking>("Resources/Prefab2/Window/GuildRaid/GuildRaidGuildRanking");
		AddInstance<GuildConstruction>("Resources/Prefab2/Window/Guild/GuildConstruction");
		AddInstance<GuildRankingBoardSuppress>("Resources/Prefab2/Window/GuildRankingBoard/GuildRankingBoardSuppress");
		AddInstance<GuildTrainingCenter>("Resources/Prefab2/Window/Guild/GuildTrainingCenter");
		AddInstance<SelectCountryFlag>("Prefab2/Window/GameOption/SelectCountryFlag");
		AddInstance<GuildLogoEditPopup>("Resources/Prefab2/Window/Guild/GuildLogoEditPopup");
		AddInstance<GuildAttendancePopup>("Prefab2/Window/Guild/GuildAttendancePopup");
		AddInstance<GuildAttendanceRewardListPopup>("Prefab2/Window/Guild/GuildAttendanceRewardListPopup");
	}

	public void Init(Action onInit)
	{
	}

	public void OpenCreateGuild()
	{
		GuildCreateMessagePopup.Open(delegate
		{
			new GroupTask().ADDTASK(base.TaskFactory.Run((CreateGuild2 window) => window.OpenAsync(immediately: true))).ADDTASK(base.TaskFactory.Run(delegate(CreateGuild2 window)
			{
				window.Init(this);
			})).Commit();
		});
	}

	public void OpenGuildList()
	{
		GlobalAutoBehaviour<GuildManager>.instance.SearchedGuildInfo = null;
		GetGuildList(delegate
		{
			GetJoinRequestedGuilds(delegate
			{
				OpenGulidListAsync();
			});
		});
	}

	private void OpenGulidListAsync()
	{
		new GroupTask().ADDTASK(base.TaskFactory.Run((GuildList2 window) => window.OpenAsync(immediately: true))).ADDTASK(base.TaskFactory.Run(delegate(GuildList2 window)
		{
			window.Init(this);
		})).Commit();
	}

	public void OpenGuildConstruction(Action openComplete)
	{
		GetInstance<GuildConstruction>().Open(this, openComplete);
	}

	public void OpenGuildRankingBoard(GuildRankingBoardType boardType)
	{
		_cachedRankerDic = null;
		_cachedServerTotalRankingDic = null;
		_curRankingBoardType = boardType;
		if (_curRankingBoardType != GuildRankingBoardType.SuppressRanking)
		{
			return;
		}
		RequestGuildSuppressSessionInfo(delegate(int viewSeasonIndex)
		{
			GetInstance<GuildRankingBoardSuppress>().Open(this, viewSeasonIndex, delegate
			{
				SceneAutoBehaviour<ContentsNoticePopupManagement>.instance.OpenContentsResultPopup(ContentsNoticeType.GuildSuppress);
			});
		});
	}

	public void OpenGuildManage()
	{
		if (GlobalAutoBehaviour<GuildManager>.instance.MemberManager == null && GlobalAutoBehaviour<GuildManager>.instance.JoinRequestPlayerManager == null)
		{
			GetAllGuildMemberInfo(delegate
			{
				GetJoinRequestPlayers(delegate
				{
					OpenGuildManageAsync();
				});
			});
		}
		else if (GlobalAutoBehaviour<GuildManager>.instance.MemberManager == null)
		{
			GetAllGuildMemberInfo(delegate
			{
				OpenGuildManageAsync();
			});
		}
		else if (GlobalAutoBehaviour<GuildManager>.instance.JoinRequestPlayerManager == null)
		{
			GetJoinRequestPlayers(delegate
			{
				OpenGuildManageAsync();
			});
		}
		else
		{
			OpenGuildManageAsync();
		}
	}

	private void OpenGuildManageAsync()
	{
		new GroupTask().ADDTASK(base.TaskFactory.Run((GuildManage window) => window.OpenAsync(immediately: true))).ADDTASK(base.TaskFactory.Run(delegate(GuildManage window)
		{
			window.Init(this);
		})).Commit();
	}

	public void OpenGuildTrainingCenterAsync()
	{
		new GroupTask().ADDTASK(base.TaskFactory.Run((GuildTrainingCenter window) => window.OpenAsync(immediately: true))).ADDTASK(base.TaskFactory.Run(delegate(GuildTrainingCenter window)
		{
			window.Init(this);
		})).Commit();
		OpenGuildTrainingCenter triggerEvent = new OpenGuildTrainingCenter();
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
	}

	public void OpenGuildRaid(Action onOpen = null)
	{
		if (GuildManager.InGuildPenalty())
		{
			Vespa_System_Action_Extension_Method.fire(onOpen);
			return;
		}
		RequestGuildRaidInfo(delegate
		{
			GetInstance<GuildRaidRanking>().Open(this);
			Vespa_System_Action_Extension_Method.fire(onOpen);
			SetGuildRaidOpenTime();
		});
	}

	private void SetGuildRaidOpenTime()
	{
		if (GlobalAutoBehaviour<GuildManager>.isValidInstance && GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager != null)
		{
			GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildRaidManager?.SetOpenTime();
		}
	}

	public void OpenGuildRaidDungeonList(NShared.GuildRaidDungeonInfo beforeInfo)
	{
		Action action = delegate
		{
			OpenGuildRaidDungeonListWindow(beforeInfo, null);
		};
		GuildRaidRanking guildRaidRanking = GetInstance<GuildRaidRanking>();
		if (guildRaidRanking.IsOpen())
		{
			Vespa_System_Action_Extension_Method.fire(action);
		}
		else
		{
			guildRaidRanking.Open(this, action);
		}
	}

	private void OpenGuildRaidDungeonClearPopup(NShared.GuildRaidDungeonInfo info)
	{
		if (CheckOpenGuildRaidDungeonClearPopup(info))
		{
			GuildRaidDungeonClearedPopup.Open(info.ChapterIndex, info.DungeonIndex, info.Step, null);
		}
	}

	public void RequestGuildRaidInfo(Action onRequest)
	{
		Action requestAction = delegate
		{
			RequestGuildRaidList(delegate
			{
				RequestGuildRaidMemberScoreList(delegate
				{
					Vespa_System_Action_Extension_Method.fire(onRequest);
				});
			});
		};
		if (GlobalAutoBehaviour<GuildManager>.instance.MemberManager == null)
		{
			GetAllGuildMemberInfo(delegate
			{
				requestAction();
			});
		}
		else
		{
			requestAction();
		}
	}

	private void RequestGuildRaidList(Action<NShared.GetGuildRaidList.Response> successAction, Action<NShared.GetGuildRaidList.Response> failAction = null)
	{
		if (_guildRaidWebManager == null)
		{
			_guildRaidWebManager = new GuildRaidWebManager();
		}
		_guildRaidWebManager.RequestGetGuildRaidList(successAction, failAction);
	}

	private void RequestGuildRaidMemberScoreList(Action successAction)
	{
		if (_guildRaidWebManager == null)
		{
			_guildRaidWebManager = new GuildRaidWebManager();
		}
		_guildRaidWebManager.RequestGuildRaidMemberScoreList(successAction);
	}

	private void OpenGuildRaidDungeonListWindow(NShared.GuildRaidDungeonInfo beforeInfo, Action successAction)
	{
		RequestGuildRaidList(delegate(NShared.GetGuildRaidList.Response successResponse)
		{
			if (GlobalAutoBehaviour<GuildManager>.isValidInstance && GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager != null)
			{
				GuildRaidManager guildRaidManager = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildRaidManager;
				if (guildRaidManager != null)
				{
					NShared.GuildRaidDungeonInfo[] guildRaidInfos = successResponse.GuildRaidInfos;
					if (__Extension_Method_ICollection__.IsNullOrEmpty(guildRaidInfos))
					{
						MessagePopup.OpenError(NShared.GetGuildRaidList.ResultType.GuildRaidInitializing);
					}
					else
					{
						guildRaidManager.Apply(guildRaidInfos);
						GuildRaidDungeonList guildRaidDungeonList = GetInstance<GuildRaidDungeonList>();
						if (!(guildRaidDungeonList == null))
						{
							guildRaidDungeonList.Open(this, delegate
							{
								OpenGuildRaidDungeonClearPopup(beforeInfo);
							});
							if (successAction != null)
							{
								Vespa_System_Action_Extension_Method.fire(successAction);
							}
						}
					}
				}
			}
		}, delegate(NShared.GetGuildRaidList.Response failResponse)
		{
			if (failResponse.Result == NShared.GetGuildRaidList.ResultType.GuildRaidInitializing)
			{
				CloseGuildRaidDungeonListWindow();
			}
		});
	}

	private void CloseGuildRaidDungeonListWindow()
	{
		GuildRaidDungeonList guildRaidDungeonList = GetInstance<GuildRaidDungeonList>();
		if (!(guildRaidDungeonList == null))
		{
			guildRaidDungeonList.Close();
		}
	}

	private bool CheckOpenGuildRaidDungeonClearPopup(NShared.GuildRaidDungeonInfo beforeInfo)
	{
		if (beforeInfo == null)
		{
			return false;
		}
		if (beforeInfo.CheckDummyDungeon)
		{
			return false;
		}
		NShared.GuildRaidDungeonInfo guildRaidDungeonInfo = GetGuildRaidDungeonInfo(beforeInfo.ChapterIndex);
		if (guildRaidDungeonInfo == null)
		{
			return false;
		}
		if (beforeInfo.Step >= guildRaidDungeonInfo.Step)
		{
			int chapterLastStep = GameRoot.Instance.GuildRaidDungeonDataContainer.GetChapterLastStep(beforeInfo.ChapterIndex);
			if (beforeInfo.Step < chapterLastStep)
			{
				return false;
			}
			if (!guildRaidDungeonInfo.IsDead())
			{
				return false;
			}
		}
		if (guildRaidDungeonInfo.ClearMemberId != GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId)
		{
			return false;
		}
		return true;
	}

	void GuildRaidRanking.Listener.OnClickOpenGuildRaid(GuildRaidRanking caller)
	{
		if (!(caller == null))
		{
			OpenGuildRaidDungeonListWindow(null, delegate
			{
				caller.Init();
				SendOpenGuildRaidDungeon();
			});
		}
	}

	private void SendOpenGuildRaidDungeon()
	{
		if (!GlobalAutoBehaviour<GuildManager>.isValidInstance || GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager == null)
		{
			return;
		}
		GuildRaidManager guildRaidManager = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildRaidManager;
		if (guildRaidManager != null)
		{
			NShared.GuildRaidDungeonInfo normalGuildRaidDungeonInfo = guildRaidManager.NormalGuildRaidDungeonInfo;
			if (guildRaidManager.CheckOpenGuildRaidDungeon(normalGuildRaidDungeonInfo))
			{
				SingletonAuto<MessageLogHandler>.instance.SendToGuild(new NShared.NMessage.NProtocol.GuildRaidOpen
				{
					Nick = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Nick,
					OpenedGuildRaidInfo = normalGuildRaidDungeonInfo
				});
			}
		}
	}

	void GuildRaidDungeonList.Listener.OnClickGuildRaidDungeonInfo(GuildRaidDungeonList caller, GuildRaidDungeonData dungeonData, CreatureData bossCreatureData)
	{
		GetInstance<NGame2.NUI.NWindow.GuildRaidDungeonInfo>().Open(this, dungeonData, bossCreatureData);
	}

	void GuildRaidDungeonList.Listener.OnClickEnterGuildRaidDungeon(GuildRaidDungeonList caller, NShared.GuildRaidDungeonInfo guildRaidDungeonInfo)
	{
		if (!GlobalAutoBehaviour<GuildManager>.isValidInstance)
		{
			return;
		}
		int currentChapterIndex = guildRaidDungeonInfo.CurrentChapterIndex;
		int currentDungeonIndex = guildRaidDungeonInfo.CurrentDungeonIndex;
		NShared.DungeonDifficultyType dungeonDifficultyType = NShared.DungeonDifficultyType.Normal;
		EnterDungeonData enterDungeonData = new EnterDungeonData(currentChapterIndex, currentDungeonIndex, currentDungeonIndex, dungeonDifficultyType);
		if (enterDungeonData == null)
		{
			return;
		}
		enterDungeonData.InitialHeroInfos = guildRaidDungeonInfo.CreateResultCreatureInfos();
		enterDungeonData.BattleStartConfirmMessage = LocalizationManager.GetString(GuildRaid.GUILDRAID_ASK_BATTLE_START_MESSAGE);
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(currentChapterIndex, currentDungeonIndex);
		if (data != null)
		{
			StaminaType reqStaminaType = data.ReqStaminaType;
			int staminaCount = data.GetStaminaOfDifficulty(GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina(), dungeonDifficultyType);
			string localStorageKey = data.LocalStorageKey;
			if (guildRaidDungeonInfo.FinishMode != 0)
			{
				staminaCount = 0;
			}
			DefaultBattlePartySettingDataProdiver dataProvider = new DefaultBattlePartySettingDataProdiver(localStorageKey, reqStaminaType, staminaCount, Battle.START_BATTLE, enterDungeonData);
			GetInstance<BattlePartySetting>().Open(this, dataProvider);
		}
	}

	void BattlePartySetting.Listener.StartBattle(EnterDungeonData enterDungeon, BattlePartySetting.BattlePartyInfo battlePartyInfo, BattlePartySetting.ExtraOptionData extraOption)
	{
		if (!(Time.time - _battleRequestedTime < 10f))
		{
			Action action = delegate
			{
				_battleRequestedTime = Time.time;
				RequestBeginGuildRaid(enterDungeon, battlePartyInfo, extraOption);
			};
			string battleStartConfirmMessage = enterDungeon.BattleStartConfirmMessage;
			if (String_Extension_Method.IsNullOrEmpty(battleStartConfirmMessage))
			{
				Vespa_System_Action_Extension_Method.fire(action);
			}
			else
			{
				MessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), battleStartConfirmMessage, action, null);
			}
		}
	}

	private void RequestBeginGuildRaid(EnterDungeonData enterDungeon, BattlePartySetting.BattlePartyInfo battlePartyInfo, BattlePartySetting.ExtraOptionData extraOption)
	{
		NShared.BeginCampaign.Request obj = new NShared.BeginCampaign.Request
		{
			ChapterIndex = enterDungeon.Chapter,
			DungeonIndex = enterDungeon.OriginalDungeon,
			LinkedDungeonIndex = enterDungeon.LinkedDungeon,
			DungeonDifficulty = (int)enterDungeon.Difficulty,
			ScenarioDungeon = enterDungeon.IsScenario,
			HeroIndices = battlePartyInfo.heroIndices,
			LeaderHeroIndex = battlePartyInfo.leaderHeroIndex,
			AccountIds = battlePartyInfo.accountIds,
			Repeat = false
		};
		int onetimeBoosterCount = battlePartyInfo.OnetimeBoosterCount;
		obj.ItemBoosterCount = onetimeBoosterCount;
		obj.AffixIndices = battlePartyInfo.AffixIndices;
		NShared.BeginCampaign.Request request = obj;
		SingletonAuto<BattleContext>.instance.BeginCampaign(request, delegate
		{
			if (extraOption != null && extraOption.AutoRepeat)
			{
				NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.OpenGlobal();
			}
			NShared.GuildRaidDungeonInfo guildRaidDungeonInfo = GetGuildRaidDungeonInfo(enterDungeon.Chapter);
			LobbyScene2.ListReturnState.Add(new GuildRaidReturnState(enterDungeon, guildRaidDungeonInfo));
		}, delegate(NShared.BeginCampaign.Response errorResponse)
		{
			_battleRequestedTime = 0f;
			ProgressBeginCampaignResultTypeInitializing(errorResponse.Result);
		});
	}

	private void ProgressBeginCampaignResultTypeInitializing(NShared.BeginCampaign.ResultType resultType)
	{
		if (resultType != NShared.BeginCampaign.ResultType.GuildRaidInitializing)
		{
			return;
		}
		MessagePopup.OpenError(resultType, delegate
		{
			BattlePartySetting battlePartySetting = GetInstance<BattlePartySetting>();
			if (battlePartySetting != null)
			{
				battlePartySetting.Close();
			}
			CloseGuildRaidDungeonListWindow();
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

	public void UpdateGuildManageUI()
	{
		if (InvalidGuildInfo())
		{
			CloseAllActiveWindow();
			return;
		}
		GuildManage guildManage = GetInstance<GuildManage>(openedOnly: true);
		if (guildManage != null && guildManage.IsOpen())
		{
			guildManage.RefreshBaseUI();
		}
		GuildConstruction guildConstruction = GetInstance<GuildConstruction>(openedOnly: true);
		if (guildConstruction != null && guildConstruction.IsOpen())
		{
			guildConstruction.RefreshUI();
		}
	}

	private bool InvalidGuildInfo()
	{
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager != null)
		{
			return GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GetGuildId() == 0;
		}
		return true;
	}

	public void UpdateGuildManageList(GuildManage.ListType listType)
	{
		GuildManage guildManage = GetInstance<GuildManage>(openedOnly: true);
		if (!(guildManage == null) && guildManage.IsOpen())
		{
			guildManage.RefreshCurList(listType);
		}
	}

	public void UpdateGuildTrainningCenterUI()
	{
		if (GetInstance<GuildTrainingCenter>().IsOpen())
		{
			if (InvalidGuildInfo())
			{
				CloseAllActiveWindow();
			}
			else
			{
				GetInstance<GuildTrainingCenter>().RefreshGuildSkillComponents();
			}
		}
	}

	public void RefreshGuildConstructionWindow()
	{
		if (GetInstance<GuildConstruction>().IsOpen())
		{
			GetInstance<GuildConstruction>().RefreshUI();
		}
	}

	public void GetGuildList(Action onComplete, bool bRefresh = false)
	{
		if (!bRefresh && GlobalAutoBehaviour<GuildManager>.instance.RequestedGuildManager != null && (DateTime.UtcNow - _lastGuildListRequestTime).TotalSeconds < 600.0)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return;
		}
		NShared.GetGuildList.Request request = new NShared.GetGuildList.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetGuildList.Response response)
		{
			_lastGuildListRequestTime = DateTime.UtcNow;
			if (response.Result != NShared.GetGuildList.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<GuildManager>.instance.SetRequestedGuildInfo(response.guildInfos);
				Vespa_System_Action_Extension_Method.fire(onComplete);
			}
		});
	}

	public void GetJoinRequestedGuilds(Action onComplete)
	{
		if (GlobalAutoBehaviour<GuildManager>.instance.JoinRequestedGuildManager != null)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return;
		}
		NShared.GetJoinRequestedGuilds.Request request = new NShared.GetJoinRequestedGuilds.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetJoinRequestedGuilds.Response response)
		{
			if (response.Result != NShared.GetJoinRequestedGuilds.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<GuildManager>.instance.SetJoinRequestGuilds(response.guildInfos);
				Vespa_System_Action_Extension_Method.fire(onComplete);
			}
		});
	}

	public void GetGuildBasicInfo(Action onComplete)
	{
		NShared.GetGuildBasicInfo.Request request = new NShared.GetGuildBasicInfo.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetGuildBasicInfo.Response response)
		{
			if (response.Result == NShared.GetGuildBasicInfo.ResultType.Success)
			{
				GlobalAutoBehaviour<GuildManager>.instance.SetMyGuildInfo(response.info);
				Vespa_System_Action_Extension_Method.fire(onComplete);
			}
			else if (response.Result == NShared.GetGuildBasicInfo.ResultType.NoGuild)
			{
				GlobalAutoBehaviour<GuildManager>.instance.SetMyGuildInfo(null);
				GlobalAutoBehaviour<GuildManager>.instance.InitGuildMembers(null);
				GlobalAutoBehaviour<GuildManager>.instance.InitGuildJoinRequestPlayers(null);
				Vespa_System_Action_Extension_Method.fire(onComplete);
			}
		});
	}

	public void GetAllGuildMemberInfo(Action<bool> onComplete)
	{
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GetGuildId() == 0L)
		{
			return;
		}
		NShared.GetAllGuildMemberInfo.Request request = new NShared.GetAllGuildMemberInfo.Request
		{
			GuildId = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GetGuildId()
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetAllGuildMemberInfo.Response response)
		{
			if (response.Result != NShared.GetAllGuildMemberInfo.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				Vespa_System_Action_Extension_Method.fire(onComplete, i_argA: false);
			}
			else
			{
				GlobalAutoBehaviour<GuildManager>.instance.InitGuildMembers(response.MemberInfos);
				Vespa_System_Action_Extension_Method.fire(onComplete, i_argA: true);
			}
		});
	}

	public void GetJoinRequestPlayers(Action onComplete)
	{
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GetGuildId() == 0L || GlobalAutoBehaviour<GuildManager>.instance.MemberManager == null)
		{
			return;
		}
		GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
		if (myMemberInfo == null || myMemberInfo.Rank > 2)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return;
		}
		NShared.GetJoinRequestPlayers.Request request = new NShared.GetJoinRequestPlayers.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetJoinRequestPlayers.Response response)
		{
			if (response.Result != NShared.GetJoinRequestPlayers.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<GuildManager>.instance.InitGuildJoinRequestPlayers(response.players);
				Vespa_System_Action_Extension_Method.fire(onComplete);
			}
		});
	}

	public void CreateGuild(string name, int guildLogo, int guildLogoBackground, EGuildJoinWay joinWay, int joinLevel, string countryCode, string introduce, Action onComplete)
	{
		NShared.CreateGuild.Request request = new NShared.CreateGuild.Request
		{
			name = name,
			introduce = introduce,
			joinWay = (byte)joinWay,
			reqTeamLevel = joinLevel,
			logo = (byte)guildLogo,
			logoBackground = (byte)guildLogoBackground,
			countryCode = countryCode
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.CreateGuild.Response response)
		{
			if (response.Result != NShared.CreateGuild.ResultType.Success)
			{
				if (response.Result == NShared.CreateGuild.ResultType.NotElapsedGuildRequestCoolTime)
				{
					string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
					string message = LocalizationManager.GetString(NShared.CreateGuild.ResultType.NotElapsedGuildRequestCoolTime, GetReJoinHour());
					MessagePopup.Open(title, message);
				}
				else
				{
					MessagePopup.OpenError(response.Result);
				}
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
				_ = (object)null;
				_ = 25;
				_ = 1;
				_ = 0;
				Array.Empty<object>();
				Vespa_System_Action_Extension_Method.fire(onComplete);
			}
		});
	}

	private int GetReJoinHour()
	{
		int guildWithdrawCount = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.GuildWithdrawCount;
		return GameRoot.Instance.GuildPenaltyDataContainer.GetReJoinHour(guildWithdrawCount);
	}

	private int GetRestrictHour(int addCount = 0)
	{
		int guildWithdrawCount = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.GuildWithdrawCount;
		return GameRoot.Instance.GuildPenaltyDataContainer.GetRestrictHour(guildWithdrawCount + addCount);
	}

	public void CheckGuildName(string name, Action onComplete)
	{
		NShared.CheckGuildName.Request request = new NShared.CheckGuildName.Request
		{
			name = name
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.CheckGuildName.Response response)
		{
			if (response.Result == NShared.CheckGuildName.ResultType.Success)
			{
				GetInstance<CreateGuild2>().SetGuildNameChecked(usable: true);
				Vespa_System_Action_Extension_Method.fire(onComplete);
			}
			else if (response.Result == NShared.CheckGuildName.ResultType.AlreadyExistName)
			{
				GetInstance<CreateGuild2>().SetGuildNameChecked(usable: false);
				Vespa_System_Action_Extension_Method.fire(onComplete);
			}
			else
			{
				MessagePopup.OpenError(response.Result);
			}
		});
	}

	public void JoinGuild(GuildInfo info, Action onFreeJoin, Action onAcceptJoin)
	{
		NShared.RequestJoinGuild.Request request = new NShared.RequestJoinGuild.Request
		{
			GuildId = info.Id
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.RequestJoinGuild.Response response)
		{
			if (response.Result != NShared.RequestJoinGuild.ResultType.Success)
			{
				if (response.Result == NShared.RequestJoinGuild.ResultType.NotElapsedGuildRequestCoolTime)
				{
					string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
					string message = LocalizationManager.GetString(NShared.CreateGuild.ResultType.NotElapsedGuildRequestCoolTime, GetReJoinHour());
					MessagePopup.Open(title, message);
				}
				else
				{
					MessagePopup.OpenError(response.Result);
				}
			}
			else
			{
				if (response.JoinWay == 1)
				{
					GlobalAutoBehaviour<GuildManager>.instance.SetJoinRequestGuilds(null);
					GlobalAutoBehaviour<GuildManager>.instance.RemoveAllGuildRaidBootyItems();
					GetGuildBasicInfo(onFreeJoin);
				}
				else
				{
					GlobalAutoBehaviour<GuildManager>.instance.JoinRequestedGuildManager.Add(info);
					if (GlobalAutoBehaviour<GuildManager>.instance.RequestedGuildManager != null)
					{
						GlobalAutoBehaviour<GuildManager>.instance.RequestedGuildManager.Remove(info.Id);
					}
					if (GlobalAutoBehaviour<GuildManager>.instance.SearchedGuildInfo != null && GlobalAutoBehaviour<GuildManager>.instance.SearchedGuildInfo.Id == info.Id)
					{
						GlobalAutoBehaviour<GuildManager>.instance.SearchedGuildInfo = null;
					}
					MessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), LocalizationManager.GetString(Guild.GUILD_JOIN_REQUEST_COMPLETED), null);
					Vespa_System_Action_Extension_Method.fire(onAcceptJoin);
				}
				GlobalAutoBehaviour<AccountManager>.instance.AdventureNoticeManager?.ForeceRefrsh();
			}
		});
	}

	public void CancelRequestJoin(GuildInfo info, Action onComplete, Action onFail)
	{
		NShared.CancelRequestJoinGuild.Request request = new NShared.CancelRequestJoinGuild.Request
		{
			GuildId = info.Id
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.CancelRequestJoinGuild.Response response)
		{
			if (response.Result != NShared.CancelRequestJoinGuild.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				Vespa_System_Action_Extension_Method.fire(onFail);
			}
			else
			{
				GlobalAutoBehaviour<GuildManager>.instance.JoinRequestedGuildManager.Remove(info.Id);
				Vespa_System_Action_Extension_Method.fire(onComplete);
			}
		});
	}

	public void SearchGuild(string search_name, Action onSuccess, Action onNoGuild)
	{
		NShared.SearchGuild.Request request = new NShared.SearchGuild.Request
		{
			name = search_name
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SearchGuild.Response response)
		{
			if (response.Result == NShared.SearchGuild.ResultType.Success)
			{
				GlobalAutoBehaviour<GuildManager>.instance.SearchedGuildInfo = response.Info;
				Vespa_System_Action_Extension_Method.fire(onSuccess);
			}
			else if (response.Result == NShared.SearchGuild.ResultType.NoGuild)
			{
				Vespa_System_Action_Extension_Method.fire(onNoGuild);
			}
			else
			{
				MessagePopup.OpenError(response.Result);
			}
		});
	}

	public void LevelUpGuild(Action onComplete)
	{
		NShared.LevelUpGuild.Request request = new NShared.LevelUpGuild.Request
		{
			Level = (byte)(GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Level + 1)
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.LevelUpGuild.Response response)
		{
			if (response.Result == NShared.LevelUpGuild.ResultType.Success)
			{
				GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.SetActivityPoint(response.GuildActivityPointResult);
				Vespa_System_Action_Extension_Method.fire(onComplete);
			}
			else
			{
				MessagePopup.OpenError(response.Result);
			}
		});
	}

	public void InitGuildSkill(Action onComplete)
	{
		NShared.InitGuildSkill.Request request = new NShared.InitGuildSkill.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.InitGuildSkill.Response response)
		{
			if (response.Result == NShared.InitGuildSkill.ResultType.Success)
			{
				GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.InitGuildSkillLevel(response.GuildActivityPointResult.NewValue, response.GuildSuppressPointResult.NewValue);
				GlobalAutoBehaviour<GuildManager>.instance.ResetSkillLevelUpRequest();
				Vespa_System_Action_Extension_Method.fire(onComplete);
			}
			else
			{
				MessagePopup.OpenError(response.Result);
			}
		});
	}

	public void DestroyGuild(Action onComplete)
	{
		NShared.DestroyGuild.Request request = new NShared.DestroyGuild.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.DestroyGuild.Response response)
		{
			if (response.Result == NShared.DestroyGuild.ResultType.Success)
			{
				GlobalAutoBehaviour<GuildManager>.instance.SetMyGuildInfo(null);
				GlobalAutoBehaviour<GuildManager>.instance.InitGuildMembers(null);
				GlobalAutoBehaviour<GuildManager>.instance.InitGuildJoinRequestPlayers(null);
				GlobalAutoBehaviour<GuildManager>.instance.RemoveAllGuildRaidBootyItems();
				GlobalAutoBehaviour<GuildManager>.instance.RemoveGuildAdventureNotice();
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.GuildWithdrawCount = response.GuildWithdrawCount;
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.GuildWithdrawTime = response.GuildWithdrawTime;
				Vespa_System_Action_Extension_Method.fire(onComplete);
			}
			else
			{
				MessagePopup.OpenError(response.Result);
			}
		});
	}

	public void LevelUpSkill(int GuildSkillIndex, byte skillLevel, Action onComplete)
	{
		NShared.LevelUpGuildSkill.Request request = new NShared.LevelUpGuildSkill.Request
		{
			SkillIndex = GuildSkillIndex,
			SkillLevel = skillLevel
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.LevelUpGuildSkill.Response response)
		{
			if (response.Result == NShared.LevelUpGuildSkill.ResultType.Success)
			{
				UserGuildManager myGuildManager = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager;
				if (myGuildManager != null)
				{
					myGuildManager.SetActivityPoint(response.GuildActivityPointResult);
					myGuildManager.SetSuppressPoint(response.GuildSuppressPointResult);
				}
				GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.SetSkillLevel(GuildSkillIndex, skillLevel);
				GlobalAutoBehaviour<GuildManager>.instance.ResetSkillLevelUpRequest();
				Vespa_System_Action_Extension_Method.fire(onComplete);
			}
			else
			{
				MessagePopup.OpenError(response.Result);
			}
		});
	}

	public void ChangeGuildNotice(string _notice, Action onComplete = null)
	{
		NShared.ApplyGuildSettingChange.Request request = new NShared.ApplyGuildSettingChange.Request
		{
			Notice = _notice
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ApplyGuildSettingChange.Response response)
		{
			if (response.Result != NShared.ApplyGuildSettingChange.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Notice = _notice;
				Vespa_System_Action_Extension_Method.fire(onComplete);
			}
		});
	}

	public void ChangeGuildSetting(int _level, string _introduce, int _joinWay, int _logo, int _back, Action onComplete = null)
	{
		string strIntroduce = null;
		if (string.Compare(_introduce, GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Introduce) != 0)
		{
			strIntroduce = _introduce;
		}
		if (!String_Extension_Method.IsNullOrEmpty(strIntroduce))
		{
			NShared.ApplyGuildSettingChange.ResultType resultType = CheckGuildIntroduce(strIntroduce);
			if (resultType != NShared.ApplyGuildSettingChange.ResultType.Success)
			{
				MessagePopup.OpenError(LocalizationManager.GetString(resultType));
				return;
			}
			strIntroduce = GameRoot.Instance.CensoredWordDataContainer.ReplaceCensoredWord(strIntroduce, isChat: true);
		}
		NShared.ApplyGuildSettingChange.Request request = new NShared.ApplyGuildSettingChange.Request
		{
			TeamLevel = _level,
			Introduce = strIntroduce,
			JoinWay = (byte)_joinWay,
			Logo = (byte)_logo,
			Back = (byte)_back
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ApplyGuildSettingChange.Response response)
		{
			if (response.Result != NShared.ApplyGuildSettingChange.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.ReqTeamLevel = _level;
				GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Introduce = strIntroduce;
				GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.JoinWay = (byte)_joinWay;
				GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Logo = (byte)_logo;
				GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Back = (byte)_back;
				Vespa_System_Action_Extension_Method.fire(onComplete);
			}
		});
	}

	public void WithdrawGuild(Action onComplete)
	{
		NShared.WithdrawGuild.Request request = new NShared.WithdrawGuild.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.WithdrawGuild.Response response)
		{
			if (response.Result == NShared.WithdrawGuild.ResultType.Success)
			{
				GlobalAutoBehaviour<GuildManager>.instance.SetMyGuildInfo(null);
				GlobalAutoBehaviour<GuildManager>.instance.InitGuildMembers(null);
				GlobalAutoBehaviour<GuildManager>.instance.InitGuildJoinRequestPlayers(null);
				GlobalAutoBehaviour<GuildManager>.instance.RemoveAllGuildRaidBootyItems();
				GlobalAutoBehaviour<GuildManager>.instance.RemoveGuildAdventureNotice();
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.GuildWithdrawTime = response.GuildWithdrawTime;
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.GuildWithdrawCount = response.GuildWithdrawCount;
				Vespa_System_Action_Extension_Method.fire(onComplete);
			}
			else
			{
				MessagePopup.OpenError(response.Result);
			}
		});
	}

	void CreateGuild2.Listener.OnClickCreate(string name, int guildLogo, int guildLogoBackground, EGuildJoinWay joinWay, int joinLevel, string countryCode, string introduce)
	{
		NShared.CheckGuildName.ResultType resultType = SingletonAuto<GuildNameChecker>.instance.Check(name);
		switch (resultType)
		{
		default:
			MessagePopup.OpenError(LocalizationManager.GetString(resultType));
			break;
		case NShared.CheckGuildName.ResultType.InvalidGuildNameInThisServer:
			MessagePopup.OpenError(LocalizationManager.GetString(Guild.GUILD_NAME_INPUT_FAILED));
			break;
		case NShared.CheckGuildName.ResultType.Success:
		{
			NShared.ApplyGuildSettingChange.ResultType resultType2 = CheckGuildIntroduce(introduce);
			if (resultType2 != NShared.ApplyGuildSettingChange.ResultType.Success)
			{
				MessagePopup.OpenError(LocalizationManager.GetString(resultType2));
				break;
			}
			introduce = GameRoot.Instance.CensoredWordDataContainer.ReplaceCensoredWord(introduce, isChat: true);
			CreateGuild(name, guildLogo, guildLogoBackground, joinWay, joinLevel, countryCode, introduce, delegate
			{
				GetGuildBasicInfo(delegate
				{
					GetAllGuildMemberInfo(delegate
					{
						GetJoinRequestPlayers(delegate
						{
							CloseAllActiveWindow();
							SceneAutoBehaviour<EntryMenuManagement>.instance.CloseAllActiveWindow(immediately: true);
							SceneAutoBehaviour<EntryMenuManagement>.instance.CloseEntryMenu(null);
							GlobalAutoBehaviour<GuildManager>.instance.EnterGuildChapter();
							GlobalAutoBehaviour<AccountManager>.instance.AdventureNoticeManager?.ForeceRefrsh();
						});
					});
				});
			});
			break;
		}
		}
	}

	void CreateGuild2.Listener.OnClickCheckName(string name)
	{
		if (SingletonAuto<GuildNameChecker>.instance.Check(name) != NShared.CheckGuildName.ResultType.Success)
		{
			GetInstance<CreateGuild2>().SetGuildNameChecked(usable: false);
		}
		else
		{
			CheckGuildName(name, null);
		}
	}

	void CreateGuild2.Listener.OnChangeCountryFlag(string guildCountryCode)
	{
		AddTask(base.TaskFactory.Run(delegate(SelectCountryFlag window)
		{
			window.Open(guildCountryCode, this);
		}), base.TaskFactory.Run(delegate(SelectCountryFlag window)
		{
			window.ScrollToSelectedComponent();
		}));
	}

	void CreateGuild2.Listener.OnClickGuildEdit()
	{
		int index = Vespa_Generic_IList_Extension_Method.GetLast(GameRoot.Instance.GuildArenaSeasonRewardDataContainer.GetAllData()).Index;
		int currentBGIndex = GetInstance<CreateGuild2>().CurrentBGIndex;
		int currentIconIndex = GetInstance<CreateGuild2>().CurrentIconIndex;
		GetInstance<GuildLogoEditPopup>().Open(this, index, currentIconIndex, currentBGIndex);
	}

	void GuildLogoEditPopup.Listener.OnClickApply(int iconIndex, int bgIndex)
	{
		GetInstance<CreateGuild2>().SetGuildLogo(iconIndex, bgIndex);
	}

	void GuildLogoEditPopup.Listener.OnClickReset()
	{
	}

	void GuildList2.Listener.OnClickJoinGuild(GuildInfo info)
	{
		JoinGuild(info, delegate
		{
			GetAllGuildMemberInfo(delegate(bool InfoUpdated)
			{
				if (InfoUpdated)
				{
					CloseAllActiveWindow();
				}
				SceneAutoBehaviour<EntryMenuManagement>.instance.CloseAllActiveWindow(immediately: true);
				SceneAutoBehaviour<EntryMenuManagement>.instance.CloseEntryMenu(null);
				GlobalAutoBehaviour<GuildManager>.instance.EnterGuildChapter();
			});
		}, delegate
		{
			GetInstance<GuildList2>().RefreshListOfCurTab();
		});
	}

	void GuildList2.Listener.OnClickCancelRequest(GuildInfo info, Action onFail)
	{
		CancelRequestJoin(info, delegate
		{
			GetInstance<GuildList2>().RefreshJoinRequestedGuildList();
		}, onFail);
	}

	void GuildList2.Listener.OnClickGetGuildList()
	{
		GetGuildList(delegate
		{
			GetInstance<GuildList2>().SetGetGuildListDelay();
			GetInstance<GuildList2>().SetGuildListSearch(bSearch: false);
			GetInstance<GuildList2>().RefreshGuildList();
		}, bRefresh: true);
	}

	void GuildList2.Listener.OnClickSearchGuild(string search_name)
	{
		if (!String_Extension_Method.IsNullOrEmpty(search_name))
		{
			SearchGuild(search_name, delegate
			{
				GetInstance<GuildList2>().SetGuildListSearch(bSearch: true);
				GetInstance<GuildList2>().RefreshGuildList();
			}, delegate
			{
				GetInstance<GuildList2>().DisplayGuildNotFound();
			});
		}
	}

	bool GuildList2.Listener.OnCloseRequest(GuildList2 closeThis)
	{
		return true;
	}

	void GuildList2.Listener.ReOpen()
	{
	}

	void GuildManage.Listener.LevelUpGuild()
	{
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager == null || GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GetGuildId() == 0L)
		{
			return;
		}
		byte curLevel = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Level;
		int nextLevel = curLevel + 1;
		GuildLevelData data = GameRoot.Instance.GuildLevelDataContainer.GetData(curLevel);
		GuildLevelData data2 = GameRoot.Instance.GuildLevelDataContainer.GetData(nextLevel);
		string stringDirect = LocalizationManager.GetStringDirect("GUILD_LEVELUP");
		string empty = string.Empty;
		bool bCanLevelup = false;
		if (data == null)
		{
			return;
		}
		long activityPoint = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.ActivityPoint;
		if (data2 == null)
		{
			empty = LocalizationManager.GetString(Guild.GUILD_LEVELUP_MAXLEVEL);
		}
		else if (data.ActivityPoint > activityPoint)
		{
			empty = LocalizationManager.GetString(Guild.GUILD_LEVELUP_NOT_ENOUGH_AP, data.ActivityPoint);
		}
		else
		{
			bCanLevelup = true;
			empty = LocalizationManager.GetStringDirect("GUILD_LEVELUP_POPUP", curLevel, nextLevel, data.MaxMember, data2.MaxMember, data.ActivityPoint, activityPoint);
		}
		MessagePopup.Open(stringDirect, empty, delegate
		{
			if (bCanLevelup)
			{
				LevelUpGuild(delegate
				{
					GuildBuildingLevelupPopup.Open(1, curLevel, nextLevel);
					GetInstance<GuildManage>().RefreshBaseUI();
				});
			}
		}, null);
	}

	void GuildManage.Listener.InitGuildSkillLevel()
	{
		if (!CheckIsGuild)
		{
			return;
		}
		string stringDirect = LocalizationManager.GetStringDirect("GUILD_RESET_SKILL");
		string stringDirect2 = LocalizationManager.GetStringDirect("GUILD_RESET_SKILL_POPUP");
		string stringDirect3 = LocalizationManager.GetStringDirect("GUILD_RESET_SKILL_POPUP2");
		MessagePopup.Open2(stringDirect, stringDirect2, stringDirect3, delegate
		{
			InitGuildSkill(delegate
			{
				GetInstance<GuildManage>().RefreshBaseUI();
				GetInstance<GuildManage>().RefreshCurList(GuildManage.ListType.Skill);
			});
		}, null);
	}

	void GuildManage.Listener.BreakGuild()
	{
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager == null || GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GetGuildId() == 0L)
		{
			return;
		}
		string stringDirect = LocalizationManager.GetStringDirect("GUILD_DISMISS");
		string stringDirect2 = LocalizationManager.GetStringDirect("GUILD_DISSMISS_POPUP", GetRestrictHour(1));
		MessagePopup.Open(stringDirect, stringDirect2, delegate
		{
			DestroyGuild(delegate
			{
				GetInstance<GuildManage>().Close();
				SceneAutoBehaviour<EntryMenuManagement>.instance.CloseAllActiveWindow(immediately: true);
				SceneAutoBehaviour<EntryMenuManagement>.instance.CloseEntryMenu(null);
				GlobalAutoBehaviour<GuildManager>.instance.ExistGuildChpater();
			});
		}, null);
	}

	void GuildManage.Listener.LevelUpSkill(int GuildSkillIndex, byte skillLevel)
	{
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager != null && GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GetGuildId() != 0L)
		{
			LevelUpSkill(GuildSkillIndex, skillLevel, delegate
			{
				GetInstance<GuildManage>().RefreshBaseUI();
				GetInstance<GuildManage>().RefreshCurList(GuildManage.ListType.Skill);
			});
		}
	}

	void GuildManage.Listener.ApplyChangeSetting(int _level, string _introduce, int _joinWay, int _logoIdx, int _backIdx)
	{
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager != null && GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GetGuildId() != 0L)
		{
			ChangeGuildSetting(_level, _introduce, _joinWay, _logoIdx, _backIdx);
		}
	}

	void GuildManage.Listener.OnClickManageButton(ManageGuildMemberComponent comp)
	{
		if (GetInstance<GuildManage>().IsOpen())
		{
			GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
			if (myMemberInfo != null && myMemberInfo.Rank <= 2)
			{
				GetInstance<ManageMenu>().Open(comp.MemberInfo, this);
			}
		}
	}

	void GuildManage.Listener.OnClickAcceptJoin(ManageGuildMemberComponent comp, Action onFail)
	{
		if (!GetInstance<GuildManage>().IsOpen())
		{
			return;
		}
		GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
		if (myMemberInfo == null || myMemberInfo.Rank > 2 || comp.WaitingInfo == null)
		{
			return;
		}
		NShared.AcceptJoinRequest.Request request = new NShared.AcceptJoinRequest.Request
		{
			AccountId = comp.WaitingInfo.AccountId
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.AcceptJoinRequest.Response response)
		{
			if (response.Result != NShared.AcceptJoinRequest.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				Vespa_System_Action_Extension_Method.fire(onFail);
			}
			else
			{
				GlobalAutoBehaviour<GuildManager>.instance.AddGuildMember(response.MemberInfo);
				GlobalAutoBehaviour<GuildManager>.instance.RemoveJoinRequestPlayer(comp.WaitingInfo.AccountId);
				GetInstance<GuildManage>().RefreshBaseUI();
				GetInstance<GuildManage>().RefreshCurList(GuildManage.ListType.GuildMember);
				GetInstance<GuildManage>().RefreshCurList(GuildManage.ListType.Waiting);
			}
		});
	}

	void GuildManage.Listener.OnClickRejectJoin(ManageGuildMemberComponent comp, Action onFail)
	{
		if (!GetInstance<GuildManage>().IsOpen())
		{
			return;
		}
		GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
		if (myMemberInfo == null || myMemberInfo.Rank > 2 || comp.WaitingInfo == null)
		{
			return;
		}
		NShared.RejectJoinRequest.Request request = new NShared.RejectJoinRequest.Request
		{
			AccountId = comp.WaitingInfo.AccountId
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.RejectJoinRequest.Response response)
		{
			if (response.Result != NShared.RejectJoinRequest.ResultType.Success && response.Result != NShared.RejectJoinRequest.ResultType.GuildJoinNotRequested)
			{
				MessagePopup.OpenError(response.Result);
				Vespa_System_Action_Extension_Method.fire(onFail);
			}
			else
			{
				GlobalAutoBehaviour<GuildManager>.instance.RemoveJoinRequestPlayer(comp.WaitingInfo.AccountId);
				GetInstance<GuildManage>().RefreshCurList(GuildManage.ListType.Waiting);
			}
		});
	}

	void GuildManage.Listener.OnClickWithdraw()
	{
		WithdrawGuild(delegate
		{
			GetInstance<GuildManage>().Close();
			SceneAutoBehaviour<EntryMenuManagement>.instance.CloseAllActiveWindow(immediately: true);
			SceneAutoBehaviour<EntryMenuManagement>.instance.CloseEntryMenu(null);
			GlobalAutoBehaviour<GuildManager>.instance.ExistGuildChpater();
		});
	}

	void GuildManage.Listener.DonateGuild()
	{
		NShared.ContributeGuild.Request request = new NShared.ContributeGuild.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ContributeGuild.Response response)
		{
			if (response.Result != NShared.ContributeGuild.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
				GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.ContributedGem = response.ContributedGem;
				GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
				if (myMemberInfo != null)
				{
					byte contributeCount = (byte)(myMemberInfo.ContributeCount - 1);
					myMemberInfo.ContributeCount = contributeCount;
					GlobalAutoBehaviour<AccountManager>.instance.ApplyGuildResult(response.GuildPointResult, response.GuildActivityPointResult, response.GuildMemberAccActivityPointResult);
					GetInstance<GuildManage>().RefreshBaseUI();
				}
			}
		});
	}

	void GuildManage.Listener.OnClickVote(int skillIndex)
	{
		NShared.RequestLevelUpGuildSkill.Request request = new NShared.RequestLevelUpGuildSkill.Request
		{
			GuildSkillIndex = (byte)skillIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.RequestLevelUpGuildSkill.Response response)
		{
			if (response.Result != NShared.RequestLevelUpGuildSkill.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
				if (myMemberInfo != null)
				{
					if (skillIndex == 0)
					{
						GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.AddGuildSkillReq(myMemberInfo.ReqGuildSkill, -1);
					}
					else
					{
						GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.AddGuildSkillReq(skillIndex, 1);
					}
					myMemberInfo.ReqGuildSkill = (byte)skillIndex;
					GetInstance<GuildManage>().RefreshCurList(GuildManage.ListType.Skill);
				}
			}
		});
	}

	void GuildManage.Listener.OpenNoticePopup()
	{
		GuildNoticePopup.Open(this, GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Notice);
	}

	void GuildManage.Listener.SendGuildNotice()
	{
		string notice = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Notice;
		if (String_Extension_Method.IsNullOrEmpty(notice))
		{
			return;
		}
		NShared.SendGuildNotice.Request request = new NShared.SendGuildNotice.Request
		{
			Notice = notice
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SendGuildNotice.Response response)
		{
			if (response != null)
			{
				if (response.BaseResult != BaseResultType.Success)
				{
					MessagePopup.OpenError(response.BaseResult);
				}
				else if (response.Result != NShared.SendGuildNotice.ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
				}
			}
		});
	}

	void GuildManage.Listener.OnChangeCountryFlag(string guildCountryCode)
	{
		AddTask(base.TaskFactory.Run(delegate(SelectCountryFlag window)
		{
			window.Open(guildCountryCode, this);
		}), base.TaskFactory.Run(delegate(SelectCountryFlag window)
		{
			window.ScrollToSelectedComponent();
		}));
	}

	void GuildManage.Listener.OnClickGuildFlagSetting()
	{
		GuildManage guildManage = GetInstance<GuildManage>();
		if (!(guildManage == null))
		{
			GetInstance<GuildLogoEditPopup>().Open(guildManage, guildManage.GuildFlagIndex, guildManage.GuildLogoIndex, guildManage.GuildBackIndex);
		}
	}

	void ManageMenu.Listener.OnClickDelegateMasterButton(GuildMemberInfo info)
	{
		GetInstance<ManageMenu>().Close();
		AddTask(new WebServiceTask<NShared.DelegateMaster.Request, NShared.DelegateMaster.Response>(new NShared.DelegateMaster.Request
		{
			DelegateAccountId = info.AccountId
		}, (NShared.DelegateMaster.Request request, NShared.DelegateMaster.Response response) => base.TaskFactory.Branch(base.TaskFactory.Run(() => response.Result == NShared.DelegateMaster.ResultType.Success), base.TaskFactory.Sequence(base.TaskFactory.Run(delegate
		{
			GlobalAutoBehaviour<GuildManager>.instance.MemberManager.SwapMasterMemberInfo(info.AccountId);
			GetInstance<GuildManage>().RefreshBaseUI();
			GetInstance<GuildManage>().RefreshCurList(GuildManage.ListType.GuildMember);
		})), base.TaskFactory.Branch(base.TaskFactory.Run(delegate
		{
			MessagePopup.OpenError(response.Result);
		})))));
	}

	void ManageMenu.Listener.OnClickSetAdminRankButton(GuildMemberInfo info)
	{
		GetInstance<ManageMenu>().Close();
		AddTask(new WebServiceTask<NShared.SetGuildAdminRank.Request, NShared.SetGuildAdminRank.Response>(new NShared.SetGuildAdminRank.Request
		{
			AccountId = info.AccountId,
			GuildRank = 2
		}, (NShared.SetGuildAdminRank.Request request, NShared.SetGuildAdminRank.Response response) => base.TaskFactory.Branch(base.TaskFactory.Run(() => response.Result == NShared.SetGuildAdminRank.ResultType.Success), base.TaskFactory.Sequence(base.TaskFactory.Run(delegate
		{
			GlobalAutoBehaviour<GuildManager>.instance.MemberManager.SetRank(info.AccountId, EGuildMemberRank.GuildAdmin);
			GetInstance<GuildManage>().RefreshCurList(GuildManage.ListType.GuildMember);
		})), base.TaskFactory.Branch(base.TaskFactory.Run(delegate
		{
			MessagePopup.OpenError(response.Result);
		})))));
	}

	void ManageMenu.Listener.OnClickSetMemberRankButton(GuildMemberInfo info)
	{
		GetInstance<ManageMenu>().Close();
		AddTask(new WebServiceTask<NShared.SetGuildAdminRank.Request, NShared.SetGuildAdminRank.Response>(new NShared.SetGuildAdminRank.Request
		{
			AccountId = info.AccountId,
			GuildRank = 3
		}, (NShared.SetGuildAdminRank.Request request, NShared.SetGuildAdminRank.Response response) => base.TaskFactory.Branch(base.TaskFactory.Run(() => response.Result == NShared.SetGuildAdminRank.ResultType.Success), base.TaskFactory.Sequence(base.TaskFactory.Run(delegate
		{
			GlobalAutoBehaviour<GuildManager>.instance.MemberManager.SetRank(info.AccountId, EGuildMemberRank.GuildMember);
			GetInstance<GuildManage>().RefreshCurList(GuildManage.ListType.GuildMember);
		})), base.TaskFactory.Branch(base.TaskFactory.Run(delegate
		{
			MessagePopup.OpenError(response.Result);
		})))));
	}

	void ManageMenu.Listener.OnClickKickButton(GuildMemberInfo info)
	{
		GetInstance<ManageMenu>().Close();
		AddTask(new WebServiceTask<NShared.KickGuildMember.Request, NShared.KickGuildMember.Response>(new NShared.KickGuildMember.Request
		{
			AccountId = info.AccountId
		}, (NShared.KickGuildMember.Request request, NShared.KickGuildMember.Response response) => base.TaskFactory.Branch(base.TaskFactory.Run(() => response.Result == NShared.KickGuildMember.ResultType.Success), base.TaskFactory.Sequence(base.TaskFactory.Run(delegate
		{
			GlobalAutoBehaviour<GuildManager>.instance.MemberManager.Remove(info.AccountId);
			GetInstance<GuildManage>().RefreshBaseUI();
			GetInstance<GuildManage>().RefreshCurList(GuildManage.ListType.GuildMember);
		})), base.TaskFactory.Branch(base.TaskFactory.Run(delegate
		{
			MessagePopup.OpenError(response.Result);
		})))));
	}

	void GuildNoticePopup.Listener.ApplyChangeNotice(string notice)
	{
		if (!GlobalAutoBehaviour<GuildManager>.isValidInstance || !GlobalAutoBehaviour<GuildManager>.instance.CheckIsGuild)
		{
			return;
		}
		NShared.ApplyGuildSettingChange.ResultType resultType = CheckGuildNotice(notice);
		if (resultType != NShared.ApplyGuildSettingChange.ResultType.Success)
		{
			MessagePopup.OpenError(resultType);
			return;
		}
		notice = GameRoot.Instance.CensoredWordDataContainer.ReplaceCensoredWord(notice, isChat: true);
		ChangeGuildNotice(notice, delegate
		{
			GetInstance<GuildManage>().UpdateGuildNotice(notice);
		});
	}

	public static void CheckInput(UIInput input, int MaxLen)
	{
		string value = input.value;
		if (value.Length > MaxLen)
		{
			value = value.Substring(0, MaxLen);
			input.value = value;
		}
	}

	private NShared.ApplyGuildSettingChange.ResultType CheckGuildIntroduce(string value)
	{
		if (value == null)
		{
			return NShared.ApplyGuildSettingChange.ResultType.InvalidGuildIntroduce;
		}
		if (value.Length > GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildIntroduceLength"))
		{
			return NShared.ApplyGuildSettingChange.ResultType.TooLongGuildIntroduce;
		}
		if (!ServerLanguageChecker.CheckValidCharacter(value))
		{
			return NShared.ApplyGuildSettingChange.ResultType.InvalidNoticeInThisServer;
		}
		CensoredWordDataContainer.CensoredResult censoredResult = GameRoot.Instance.CensoredWordDataContainer.IsCensoredWord(value, isChat: true);
		if (censoredResult == CensoredWordDataContainer.CensoredResult.Censored)
		{
			return NShared.ApplyGuildSettingChange.ResultType.CensoredGuildIntroduce;
		}
		_ = 2;
		return NShared.ApplyGuildSettingChange.ResultType.Success;
	}

	private NShared.ApplyGuildSettingChange.ResultType CheckGuildNotice(string value)
	{
		if (value == null)
		{
			return NShared.ApplyGuildSettingChange.ResultType.InvalidGuildNotice;
		}
		if (value.Length > GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildNoticeLength"))
		{
			return NShared.ApplyGuildSettingChange.ResultType.TooLongGuildNotice;
		}
		if (!ServerLanguageChecker.CheckValidCharacter(value))
		{
			return NShared.ApplyGuildSettingChange.ResultType.InvalidNoticeInThisServer;
		}
		CensoredWordDataContainer.CensoredResult censoredResult = GameRoot.Instance.CensoredWordDataContainer.IsCensoredWord(value, isChat: true);
		if (censoredResult == CensoredWordDataContainer.CensoredResult.Censored)
		{
			return NShared.ApplyGuildSettingChange.ResultType.CensoredGuildNotice;
		}
		_ = 2;
		return NShared.ApplyGuildSettingChange.ResultType.Success;
	}

	void GuildTrainingCenter.Listener.LevelUpSkill(int GuildSkillIndex, byte skillLevel)
	{
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager != null && GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GetGuildId() != 0L)
		{
			LevelUpSkill(GuildSkillIndex, skillLevel, delegate
			{
				GetInstance<GuildTrainingCenter>().RefreshWindow(initGuildSkillList: false);
			});
		}
	}

	void GuildTrainingCenter.Listener.OnClickVote(int skillIndex)
	{
		NShared.RequestLevelUpGuildSkill.Request request = new NShared.RequestLevelUpGuildSkill.Request
		{
			GuildSkillIndex = (byte)skillIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.RequestLevelUpGuildSkill.Response response)
		{
			if (response.Result != NShared.RequestLevelUpGuildSkill.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
				if (myMemberInfo != null)
				{
					if (skillIndex == 0)
					{
						GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.AddGuildSkillReq(myMemberInfo.ReqGuildSkill, -1);
						_ = myMemberInfo.ReqGuildSkill;
					}
					else
					{
						GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.AddGuildSkillReq(skillIndex, 1);
						_ = skillIndex;
					}
					myMemberInfo.ReqGuildSkill = (byte)skillIndex;
					GetInstance<GuildTrainingCenter>().RefreshGuildSkillComponents();
				}
			}
		});
	}

	void GuildTrainingCenter.Listener.InitGuildSkillLevel()
	{
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager == null || GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GetGuildId() == 0L)
		{
			return;
		}
		string stringDirect = LocalizationManager.GetStringDirect("GUILD_RESET_SKILL");
		string stringDirect2 = LocalizationManager.GetStringDirect("GUILD_RESET_SKILL_POPUP");
		string stringDirect3 = LocalizationManager.GetStringDirect("GUILD_RESET_SKILL_POPUP2");
		MessagePopup.Open2(stringDirect, stringDirect2, stringDirect3, delegate
		{
			InitGuildSkill(delegate
			{
				GetInstance<GuildTrainingCenter>().RefreshWindow(initGuildSkillList: true);
			});
		}, null);
	}

	private void RequestGuildSuppressSessionInfo(Action<int> onComplete)
	{
		NShared.GetGuildRankingBoardViewSeasonInfo.Request request = new NShared.GetGuildRankingBoardViewSeasonInfo.Request();
		request.BoardType = GuildRankingBoardType.SuppressRanking;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetGuildRankingBoardViewSeasonInfo.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.GetGuildRankingBoardViewSeasonInfo.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				Vespa_System_Action_Extension_Method.fire(onComplete, response.ViewSeasonIndex);
			}
		});
	}

	void GuildRankingBoardSuppress.Listener.GetMyRankingInfo(GuildRankingType rankingType, Action<GuildRankingBoardRankerInfo> onComplete, Action<int> onNext)
	{
		NShared.GetGuildRankingBoardRankInfo.Request request = new NShared.GetGuildRankingBoardRankInfo.Request();
		request.BoardType = GuildRankingBoardType.SuppressRanking;
		request.RankingType = rankingType;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetGuildRankingBoardRankInfo.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.GetGuildRankingBoardRankInfo.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				_myGuildBoardRankInfo = response.RankInfo;
				Vespa_System_Action_Extension_Method.fire(onComplete, _myGuildBoardRankInfo);
				Vespa_System_Action_Extension_Method.fire(onNext, 0);
			}
		});
	}

	void GuildRankingBoardSuppress.Listener.GetMyServerTotalRankingInfo(int viewSeasonIndex, long guildId, Action<GuildRankingBoardServerTotalRankingInfo> onComplete, Action<int> onNext)
	{
		NShared.GetGuildRankingBoardServerTotalRankInfo.Request request = new NShared.GetGuildRankingBoardServerTotalRankInfo.Request();
		request.BoardType = GuildRankingBoardType.SuppressRanking;
		request.RankingType = GuildRankingType.TotalRank;
		request.ViewSeasonIndex = viewSeasonIndex;
		request.GuildId = guildId;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetGuildRankingBoardServerTotalRankInfo.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.GetGuildRankingBoardServerTotalRankInfo.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				_myGuildBoardServerTotalRankInfo = response.RankInfo;
				Vespa_System_Action_Extension_Method.fire(onComplete, _myGuildBoardServerTotalRankInfo);
				Vespa_System_Action_Extension_Method.fire(onNext, 0);
			}
		});
	}

	void GuildRankingBoardSuppress.Listener.GetServerTotalRankingList(int page, int viewSeasonIndex, Action<GuildRankingBoardServerTotalRankingInfo[]> onList)
	{
		if ((_cachedServerTotalRankingDic != null && Time.time - _cachedRankerTime > 10f) || _curRankingType != GuildRankingType.TotalRank)
		{
			_cachedServerTotalRankingDic = null;
			_cachedRankerTime = 0f;
			_curRankingType = GuildRankingType.TotalRank;
		}
		if (_cachedServerTotalRankingDic != null && _cachedServerTotalRankingDic.ContainsKey(page))
		{
			Vespa_System_Action_Extension_Method.fire(onList, _cachedServerTotalRankingDic[page]);
			return;
		}
		NShared.GetGuildRankingBoardServerTotalRankingList.Request request = new NShared.GetGuildRankingBoardServerTotalRankingList.Request();
		request.PageNo = page;
		request.ViewSeasonIndex = viewSeasonIndex;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetGuildRankingBoardServerTotalRankingList.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
				Vespa_System_Action_Extension_Method.fire(onList, null);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				Vespa_System_Action_Extension_Method.fire(onList, null);
			}
			else if (response.Result != NShared.GetGuildRankingBoardServerTotalRankingList.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				Vespa_System_Action_Extension_Method.fire(onList, null);
			}
			else
			{
				GuildRankingBoardServerTotalRankingInfo[] rankingInfos = response.RankingInfos;
				_cachedServerTotalRankingDic = _cachedServerTotalRankingDic ?? new Dictionary<int, GuildRankingBoardServerTotalRankingInfo[]>();
				_cachedServerTotalRankingDic[page] = rankingInfos;
				Vespa_System_Action_Extension_Method.fire(onList, rankingInfos);
			}
		});
	}

	void GuildRankingBoardSuppress.Listener.GetRankerList(int page, GuildRankingType rankingType, Action<GuildRankingBoardRankerInfo[]> onList)
	{
		if ((_cachedRankerDic != null && Time.time - _cachedRankerTime > 10f) || _curRankingType != rankingType)
		{
			_cachedRankerDic = null;
			_cachedRankerTime = 0f;
			_curRankingType = rankingType;
		}
		if (_cachedRankerDic != null && _cachedRankerDic.ContainsKey(page))
		{
			Vespa_System_Action_Extension_Method.fire(onList, _cachedRankerDic[page]);
			return;
		}
		NShared.GetGuildRankingBoardRankerList.Request request = new NShared.GetGuildRankingBoardRankerList.Request();
		request.BoardType = GuildRankingBoardType.SuppressRanking;
		request.PageNo = page;
		request.RankingType = rankingType;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetGuildRankingBoardRankerList.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
				Vespa_System_Action_Extension_Method.fire(onList, null);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				Vespa_System_Action_Extension_Method.fire(onList, null);
			}
			else if (response.Result != NShared.GetGuildRankingBoardRankerList.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				Vespa_System_Action_Extension_Method.fire(onList, null);
			}
			else
			{
				GuildRankingBoardRankerInfo[] rankerInfos = response.RankerInfos;
				_cachedRankerDic = _cachedRankerDic ?? new Dictionary<int, GuildRankingBoardRankerInfo[]>();
				_cachedRankerDic[page] = rankerInfos;
				Vespa_System_Action_Extension_Method.fire(onList, rankerInfos);
			}
		});
	}

	void GuildConstruction.Listener.OnClickInvest(int buildingIndex, int wood, int stone, int metal, Action EndCallback)
	{
		NShared.InvestGuildBuilding.Request request = new NShared.InvestGuildBuilding.Request();
		request.BuildingIndex = buildingIndex;
		request.Wood = wood;
		request.Stone = stone;
		request.Metal = metal;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.InvestGuildBuilding.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.InvestGuildBuilding.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.SetBuildingInfo(response.GuildBuildingInfo);
				GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.SetConstructionResource(response.Wood, response.Stone, response.Metal);
				Vespa_System_Action_Extension_Method.fire(EndCallback);
			}
		});
	}

	void GuildConstruction.Listener.OnClickLevelup(int buildingIndex, int buildingLevel, Action EndCallback)
	{
		NShared.LevelUpGuildBuilding.Request request = new NShared.LevelUpGuildBuilding.Request();
		request.BuildingIndex = buildingIndex;
		request.BuildingLevel = (byte)buildingLevel;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.LevelUpGuildBuilding.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.LevelUpGuildBuilding.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.SetBuildingInfo(response.GuildBuildingInfo);
				GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.SetConstructionResource(response.Wood, response.Stone, response.Metal);
				Vespa_System_Action_Extension_Method.fire(EndCallback);
			}
		});
	}

	void SelectCountryFlag.Listener.OnConfirmCountryFlag(string guildCountryCode)
	{
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager == null || GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info == null)
		{
			if (GetInstance<CreateGuild2>().IsOpen())
			{
				GetInstance<CreateGuild2>().SetGuildCountryCode(guildCountryCode);
			}
		}
		else
		{
			if (guildCountryCode.Equals(GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.CountryCode))
			{
				return;
			}
			NShared.ApplyGuildSettingChange.Request request = new NShared.ApplyGuildSettingChange.Request
			{
				CountryCode = guildCountryCode
			};
			SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ApplyGuildSettingChange.Response response)
			{
				if (response.Result != NShared.ApplyGuildSettingChange.ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
				}
				else
				{
					GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.CountryCode = guildCountryCode;
				}
				if (GetInstance<GuildManage>().IsOpen())
				{
					string countryCode = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.CountryCode;
					GetInstance<GuildManage>().SetGuildCountryCode(countryCode);
				}
			});
		}
	}

	private NShared.GuildRaidDungeonInfo GetGuildRaidDungeonInfo(int chapterIndex)
	{
		if (!GlobalAutoBehaviour<GuildManager>.isValidInstance)
		{
			return null;
		}
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager == null)
		{
			return null;
		}
		return GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildRaidManager?.GetGuildRaidDungeonInfo(chapterIndex);
	}

	private bool InitGuildAttendanceWebManager()
	{
		if (_guildAttendanceWebManager == null)
		{
			_guildAttendanceWebManager = new GuildAttendanceWebManager();
		}
		return _guildAttendanceWebManager != null;
	}

	private void RequestSetGuildAttendanceReward(Action successAction)
	{
		if (InitGuildAttendanceWebManager())
		{
			_guildAttendanceWebManager.RequestSetGuildAttendance(delegate
			{
				Vespa_System_Action_Extension_Method.fire(successAction);
			});
		}
	}

	private void RequestSendGuildAttendanceReward(Action successAction)
	{
		if (InitGuildAttendanceWebManager())
		{
			_guildAttendanceWebManager.RequestSendGuildAttendanceReward(delegate
			{
				Vespa_System_Action_Extension_Method.fire(successAction);
			});
		}
	}

	public void OpenGuildAttendancePopup(bool isReserve)
	{
		VespaCoroutine.Start(CoRequestGetGuildAttendance(delegate
		{
			SceneAutoBehaviour<LobbyPopupQueue>.instance.AddPopup(GetInstance<GuildAttendancePopup>(), delegate(GuildAttendancePopup window)
			{
				window.Open(this);
			}, null, isReserve ? PopupQueue.PopupFlag.DefaultPopupFlag : PopupQueue.PopupFlag.None);
		}, isReserve));
	}

	public void RequestGetGuildAttendance(Action successAction)
	{
		if (InitGuildAttendanceWebManager())
		{
			_guildAttendanceWebManager.RequestGetGuildAttendance(delegate
			{
				Vespa_System_Action_Extension_Method.fire(successAction);
			});
		}
	}

	public IEnumerator CoRequestGetGuildAttendance(Action action, bool isReserve)
	{
		if (!CheckIsGuild)
		{
			yield break;
		}
		while (!CheckOpenPossible)
		{
			yield return null;
		}
		bool successResponse = false;
		DateTime time = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		RequestGetGuildAttendance(delegate
		{
			successResponse = true;
		});
		while (!successResponse)
		{
			if ((GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime() - time).TotalSeconds >= 10.0)
			{
				yield break;
			}
			yield return null;
		}
		Vespa_System_Action_Extension_Method.fire(action);
	}

	private void OpenGuildAttendanceRewardListPopup()
	{
		GuildAttendanceRewardListPopup guildAttendanceRewardListPopup = GetInstance<GuildAttendanceRewardListPopup>();
		if (!(guildAttendanceRewardListPopup == null))
		{
			guildAttendanceRewardListPopup.Open();
		}
	}

	public void CreateGuildAttendanceComponent(ref GuildAttendanceComponent component, bool isPopup, Transform rootTransform, bool isReposition = false)
	{
		if (component != null)
		{
			component.Close();
		}
		FactoryBase.SafeReleaseComponent(ref component);
		if (rootTransform == null)
		{
			return;
		}
		component = GuildAttendanceComponent.PrefabAuto.Factory.Create(rootTransform);
		if (!(component == null))
		{
			component.Open(new GuildAttendanceComponent.GuildAttendanceCompInfo
			{
				IListener = this,
				IsPopup = isPopup
			});
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(component.transform);
			if (isReposition)
			{
				component.transform.localPosition = new Vector3(component.AttendancePositionX, 0f, 0f);
			}
		}
	}

	void GuildAttendancePopup.IListener.CreateGuildAttendanceComponent(ref GuildAttendanceComponent component, Transform rootTransform)
	{
		CreateGuildAttendanceComponent(ref component, isPopup: true, rootTransform);
	}

	void GuildAttendanceComponent.IListener.ClickRewardList()
	{
		OpenGuildAttendanceRewardListPopup();
	}

	void GuildAttendanceComponent.IListener.ClickMiniGuide()
	{
		GuildAttendancePopup guildAttendancePopup = GetInstance<GuildAttendancePopup>();
		if (!(guildAttendancePopup == null))
		{
			GameMiniGuideData data = GameRoot.Instance.GameMiniGuideDataContainer.GetData(guildAttendancePopup.WindowGuideIndex);
			if (data != null)
			{
				SceneAutoBehaviour<LobbyManagement>.instance.OpenMiniGuidePopup(data);
			}
		}
	}

	void GuildAttendanceComponent.IListener.RequestSetAttendanceReward(Action successAction, int today)
	{
		string title = LocalizationManager.GetStringDirect("GUILD_ATTENDANCE_REWARD_TITLE", today);
		string message = LocalizationManager.GetStringDirect("GUILD_ATTENDANCE_REWARD_POPUP_MESSAGE");
		RequestSetGuildAttendanceReward(delegate
		{
			SceneAutoBehaviour<WorldMapManagement>.instance.UpdateGuildAttendanceNewIndicator();
			MessagePopup.Open(title, message, delegate
			{
				Vespa_System_Action_Extension_Method.fire(successAction);
			});
		});
	}

	void GuildAttendanceComponent.IListener.RequestSendPerfectAttendanceReward(Action successAction)
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetStringDirect("GUILD_ATTENDANCE_REWARD_REGULARLY_MESSAGE");
		RequestSendGuildAttendanceReward(delegate
		{
			MessagePopup.Open(title, message, delegate
			{
				Vespa_System_Action_Extension_Method.fire(successAction);
			});
		});
	}
}
