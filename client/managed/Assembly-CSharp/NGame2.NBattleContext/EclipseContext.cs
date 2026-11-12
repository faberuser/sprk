using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleField;
using NGame2.NBattleField.NManager.NBattleCondition;
using NGame2.NBattleField.NManager.NPlay;
using NGame2.NGuild;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.BeginCampaign;
using NShared.NCampaign;
using NShared.SaveEclipseResult;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NUtil;

namespace NGame2.NBattleContext;

public class EclipseContext : SingletonAuto<EclipseContext>, OfflinePlay.IDataProvider, IDataProvider, EclipseOfflineModeUpdater.IFuncEclipse
{
	public EnterDungeonData _enterDungeon;

	private NShared.SaveEclipseResult.Request _request;

	private bool _isOnceBattleEnd;

	private bool _isBattleEnd;

	private bool _isOnceLoadNextTeam;

	private bool _isLoadNextTeam;

	private bool _isOnceBattleOut;

	private bool _isBattleOut;

	private bool _isContinousBattle;

	private bool _isOnceEclipseResultFail;

	private bool _isSendEclipseResultFail;

	private string _sendFailMessage;

	public int[] HeroIndices { get; private set; }

	public EclipseBattleInfo EclipseBattleInfoData { get; private set; }

	public int DeckIndex { get; private set; }

	public EclipseContext()
	{
		Init();
	}

	private void Init()
	{
		_request = new NShared.SaveEclipseResult.Request();
		_isBattleEnd = false;
		_isBattleOut = false;
		_isSendEclipseResultFail = false;
		_isLoadNextTeam = false;
		_isOnceBattleEnd = false;
		_isOnceBattleOut = false;
		_isOnceEclipseResultFail = false;
		_isOnceLoadNextTeam = false;
	}

	public void InitEclipseInfo(NShared.BeginCampaign.Request request, NShared.BeginCampaign.Response response, bool isContinuousBattle)
	{
		_enterDungeon = new EnterDungeonData(response.EclipseBattleInfo.CurrentChapterIndex, response.EclipseBattleInfo.CurrentDungeonIndex, response.EclipseBattleInfo.CurrentDungeonIndex, (DungeonDifficultyType)request.DungeonDifficulty, request.ScenarioDungeon);
		_isContinousBattle = isContinuousBattle;
		Init();
		InitEclipseData(request, response);
		DeckIndex = EclipseBattleInfoData.CurrentDeckIndex;
		CachedHeroInfo[] heroInfos = EclipseBattleInfoData.DeckList[DeckIndex - 1].HeroInfos;
		int num = EclipseBattleInfoData.DeckList[DeckIndex - 1].HeroInfos.Length;
		HeroIndices = new int[num];
		for (int i = 0; i < num; i++)
		{
			HeroIndices[i] = heroInfos[i].HeroIndex;
		}
	}

	private void InitEclipseData(NShared.BeginCampaign.Request request, NShared.BeginCampaign.Response response)
	{
		if (response.EclipseBattleInfo != null)
		{
			EclipseBattleInfoData = new EclipseBattleInfo
			{
				MatchIndex = response.EclipseBattleInfo.MatchIndex,
				DeckList = response.EclipseBattleInfo.DeckList,
				DungeonList = response.EclipseBattleInfo.DungeonList,
				GuildSkills = response.EclipseBattleInfo.GuildSkills,
				AccountBuffs = response.EclipseBattleInfo.AccountBuffs,
				ClassBuffDataBases = response.EclipseBattleInfo.ClassBuffDataBases,
				ExtraStatDataBases = response.EclipseBattleInfo.ExtraStatDataBases,
				PetStatDataBases = response.EclipseBattleInfo.PetStatDataBases,
				CurrentDeckIndex = response.EclipseBattleInfo.CurrentDeckIndex,
				CurrentChapterIndex = response.EclipseBattleInfo.CurrentChapterIndex,
				CurrentDungeonIndex = response.EclipseBattleInfo.CurrentDungeonIndex,
				CurrentWaveIndex = response.EclipseBattleInfo.CurrentWaveIndex,
				MaxWaveIndex = response.EclipseBattleInfo.MaxWaveIndex,
				CurrentCreatureLevel = response.EclipseBattleInfo.CurrentCreatureLevel,
				OnlineGameSpeedRatio = request.OnlineGameSpeedRatio
			};
		}
	}

	bool IDataProvider.GetPrealodCreateMonster()
	{
		return true;
	}

	void IDataProvider.Preload(BattleScenePreloader preloader)
	{
		preloader.EnterDungeon = _enterDungeon;
		preloader.PreloadDungeonMonster = Vespa_Common_Class_Extension_Method.Interface((IDataProvider)this).GetPrealodCreateMonster();
		int num = DeckIndex - 1;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(HeroIndices); i++)
		{
			EclipseDeckData eclipseDeckData = (Vespa_Generic_IList_Extension_Method.IsValidIndex(EclipseBattleInfoData.DeckList, num) ? EclipseBattleInfoData.DeckList[num] : null);
			if (eclipseDeckData != null)
			{
				CachedHeroInfo cachedHeroInfo = (Vespa_Generic_IList_Extension_Method.IsValidIndex(eclipseDeckData.HeroInfos, i) ? eclipseDeckData.HeroInfos[i] : null);
				if (cachedHeroInfo != null)
				{
					preloader.AddCreature(cachedHeroInfo, null, loadCreatureImage: true);
				}
			}
		}
	}

	EnterDungeonData IDataProvider.GetEnterDungeon()
	{
		return _enterDungeon;
	}

	int OfflinePlay.IDataProvider.GetChapterIndex()
	{
		return _enterDungeon.Chapter;
	}

	int OfflinePlay.IDataProvider.GetDungeonIndex()
	{
		return _enterDungeon.LinkedDungeon;
	}

	DungeonDifficultyType OfflinePlay.IDataProvider.GetDifficulty()
	{
		return _enterDungeon.Difficulty;
	}

	bool OfflinePlay.IDataProvider.IsScenarioDungeon()
	{
		return _enterDungeon.IsScenario;
	}

	int OfflinePlay.IDataProvider.GetRequestDungeonIndex()
	{
		return _enterDungeon.OriginalDungeon;
	}

	BattleDefineData IDataProvider.GetBattleDefine()
	{
		return _enterDungeon.LinkedDungeonData?.GetBattleDefine(_enterDungeon.IsScenario);
	}

	List<IBattleLogRuntime> OfflinePlay.IDataProvider.CreateInitialBattleLog()
	{
		return new List<IBattleLogRuntime> { CreateEnterPlayerBattleLog_Me() };
	}

	private PlayerEnterBattleLog CreateEnterPlayerBattleLog_Me()
	{
		return CreateMyEnterPlayerBattleLog(HeroIndices);
	}

	public PlayerEnterBattleLog CreateMyEnterPlayerBattleLog(IList<int> heroIndices)
	{
		TeamType teamType = TeamType.Left;
		PlayerEnterBattleLog playerEnterBattleLog = new PlayerEnterBattleLog
		{
			PlayerCreateInfo = new BattlePlayerCreateInfo
			{
				Id = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id,
				TeamType = teamType
			}
		};
		BattleCreatureCreateInfo.BuffSkillInfo buffSkillInfo = new BattleCreatureCreateInfo.BuffSkillInfo
		{
			GuildSkillDataBases = EclipseBattleInfoData.GuildSkills,
			AccountBuffDataBases = EclipseBattleInfoData.AccountBuffs,
			ClassBuffDataBasess = EclipseBattleInfoData.ClassBuffDataBases
		};
		BattleCreatureCreateInfo.BuffStatInfo buffStatInfo = new BattleCreatureCreateInfo.BuffStatInfo
		{
			ExtraStatDatas = EclipseBattleInfoData.ExtraStatDataBases,
			PetStatDataBases = EclipseBattleInfoData.PetStatDataBases
		};
		int num = DeckIndex - 1;
		List<BattleCreatureCreateInfo> list = new List<BattleCreatureCreateInfo>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroIndices); i++)
		{
			EclipseDeckData eclipseDeckData = (Vespa_Generic_IList_Extension_Method.IsValidIndex(EclipseBattleInfoData.DeckList, num) ? EclipseBattleInfoData.DeckList[num] : null);
			if (eclipseDeckData == null)
			{
				continue;
			}
			CachedHeroInfo cachedHeroInfo = (Vespa_Generic_IList_Extension_Method.IsValidIndex(eclipseDeckData.HeroInfos, i) ? eclipseDeckData.HeroInfos[i] : null);
			if (cachedHeroInfo != null)
			{
				CachedHeroInfo cachedHeroInfo2 = JsonMarshaler.Clone(cachedHeroInfo);
				cachedHeroInfo2.Validate();
				BattleCreatureCreateInfo battleCreatureCreateInfo = BattleCreatureCreateInfo.CreateMyCreatureCreateInfo(cachedHeroInfo2, isLeader: false, isAi: false, buffSkillInfo, buffStatInfo);
				if (battleCreatureCreateInfo != null)
				{
					list.Add(battleCreatureCreateInfo);
				}
			}
		}
		playerEnterBattleLog.CreatureCreateInfos = list.ToArray();
		playerEnterBattleLog.InitialCreatureInfos = null;
		return playerEnterBattleLog;
	}

	EBattleMenu IDataProvider.GetBattleMenuType()
	{
		EBattleMenu eBattleMenu = EBattleMenu.TimeStatus | EBattleMenu.SystemButton | EBattleMenu.WaveStatus | EBattleMenu.LootingStatus | EBattleMenu.Retry | EBattleMenu.DamageMeter | EBattleMenu.HeroParty | EBattleMenu.BoosterStatus | EBattleMenu.QuestStatus | EBattleMenu.SystemButtonAuto;
		CampaignDungeonData linkedDungeonData = _enterDungeon.LinkedDungeonData;
		if (linkedDungeonData != null)
		{
			BattleDefineData battleDefine = linkedDungeonData.GetBattleDefine(_enterDungeon.IsScenario);
			if (battleDefine != null)
			{
				eBattleMenu = battleDefine.BattleMenu;
			}
		}
		eBattleMenu = (EBattleMenu)((ulong)eBattleMenu & 0xFFFFFFFFFFFFFFEFuL);
		eBattleMenu = (EBattleMenu)((ulong)eBattleMenu & 0xFFFFFFFFFFFFFFFDuL);
		return (EBattleMenu)((ulong)eBattleMenu & 0xFFFFFFFFFFFF7FFFuL);
	}

	BattleFacade.BattleConditionBase IDataProvider.CreateBattleCondition()
	{
		return new CampaignCondition();
	}

	IPlay IDataProvider.GetBattlePlay()
	{
		return SceneAutoBehaviour<OfflinePlay>.instance;
	}

	BattleStartEffectData IDataProvider.GetBattleStartEffectData()
	{
		return new BattleStartEffectData
		{
			PrefabWindow = "Resources/Prefab2/Window/SoulWeapon/EclipseBattleStart",
			ScreenEffectCode = "UI.Arena.Count",
			SoundEffectCode = "UI.Eclipse.BattleStart"
		};
	}

	List<CachedHeroInfo> IDataProvider.GetPreloadBattleLogs()
	{
		List<CachedHeroInfo> list = new List<CachedHeroInfo>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(HeroIndices); i++)
		{
			list.Add(GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(HeroIndices[i]));
		}
		return list;
	}

	bool IDataProvider.CanStartBattle()
	{
		return true;
	}

	bool IDataProvider.CanNextDungeon()
	{
		return false;
	}

	BattleType IDataProvider.GetBattleType()
	{
		return BattleType.Eclipse;
	}

	bool IDataProvider.IsOnline()
	{
		return false;
	}

	GuildFlagData IDataProvider.GetGuildFlagData()
	{
		CampaignDungeonData linkedDungeonData = _enterDungeon.LinkedDungeonData;
		if (linkedDungeonData == null)
		{
			return null;
		}
		if (!GlobalAutoBehaviour<GuildManager>.isValidInstance || GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager == null)
		{
			return null;
		}
		MyGuildInfo info = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info;
		return new GuildFlagData
		{
			EGuildFlagDisplayType = linkedDungeonData.EGuildFlagDisplayType,
			LeftGuildFlag = (info?.Flag ?? 1),
			RightGuildFlag = 1,
			LeftGuildLogo = (info?.Logo ?? 1),
			RightGuildLogo = 1,
			LeftGuildFlagBG = (info?.Back ?? 1),
			RightGuildFlagBG = 1,
			ExistLeftGuild = (info != null),
			ExistRightGuild = false
		};
	}

	void IDataProvider.OnUpdate()
	{
		if (SingletonAuto<BattleFacade>.instance.SendedEclipseStatus != EclipseStatus.None)
		{
			bool isCurrentIndex = false;
			bool isIncludeLogString = false;
			bool isAddDeckIndex = false;
			EclipseStatus eclipseStatus = SingletonAuto<BattleFacade>.instance.SendedEclipseStatus;
			switch (SingletonAuto<BattleFacade>.instance.SendedEclipseStatus)
			{
			case EclipseStatus.BattleTeamEndOn:
				isAddDeckIndex = true;
				_isLoadNextTeam = true;
				eclipseStatus = EclipseStatus.BattleTeamEndOff;
				isIncludeLogString = true;
				break;
			case EclipseStatus.BattleTeamEndOff:
				isAddDeckIndex = true;
				_isBattleEnd = true;
				isIncludeLogString = true;
				break;
			case EclipseStatus.BattleContinue:
				OnSetNextDungeon(SingletonAuto<BattleFacade>.instance.EclipseResultData);
				isIncludeLogString = true;
				break;
			case EclipseStatus.BattleEnd:
				_isBattleEnd = true;
				isIncludeLogString = true;
				break;
			case EclipseStatus.BattleOut:
				isAddDeckIndex = true;
				_isBattleOut = true;
				isIncludeLogString = true;
				break;
			}
			DeckIndex = SingletonAuto<BattleFacade>.instance.EclipseResultData.DeckIndex;
			GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.SetDeckIndex(DeckIndex);
			if (!_isOnceLoadNextTeam && !_isOnceBattleOut && !_isSendEclipseResultFail && !_isOnceBattleEnd)
			{
				RequestEclipseResult(eclipseStatus, isCurrentIndex, isAddDeckIndex, isIncludeLogString);
			}
			SingletonAuto<BattleFacade>.instance.SendedEclipseStatus = EclipseStatus.None;
		}
		if (_isBattleOut && !_isOnceBattleOut)
		{
			_isOnceBattleOut = true;
			AutoBattle.Stop();
			SingletonAuto<BattleFacade>.instance.ForceStopBattle();
			SingletonAuto<BattleContext>.instance.MoveSceneToLobby(null, EBoolCheck.DontCare);
		}
		if (_isSendEclipseResultFail && !_isOnceEclipseResultFail)
		{
			_isOnceEclipseResultFail = true;
			AutoBattle.Stop();
			SingletonAuto<BattleFacade>.instance.ForceStopBattle();
			GlobalMessagePopup.OpenGlobalError(_sendFailMessage, delegate
			{
				SingletonAuto<BattleContext>.instance.ReturnToLobby();
			});
		}
		if (_isLoadNextTeam && !_isOnceLoadNextTeam)
		{
			_isOnceLoadNextTeam = true;
			SceneAutoBehaviour<EndCampaign>.instance.ProcessNextTeamDirect(EclipseBattleInfoData.CurrentChapterIndex, EclipseBattleInfoData.CurrentDungeonIndex);
		}
		if (_isBattleEnd && !_isBattleOut && !_isOnceBattleEnd)
		{
			_isOnceBattleEnd = true;
			OnDungeonEnd();
		}
	}

	private void OnSetNextDungeon(BattleFacade.EclipseResultCacheData resultCacheData)
	{
		_enterDungeon = new EnterDungeonData(resultCacheData.ChapterIndex, resultCacheData.DungeonIndex, resultCacheData.DungeonIndex, _enterDungeon.Difficulty, _enterDungeon.IsScenario);
		SingletonAuto<BattleFacade>.instance.IsSetNextDungeon = true;
	}

	private void RequestEclipseResult(EclipseStatus eclipseStatus, bool isCurrentIndex = false, bool isAddDeckIndex = false, bool isIncludeLogString = false)
	{
		InitRequestData(eclipseStatus, isCurrentIndex, isAddDeckIndex, isIncludeLogString);
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(_request, delegate(NShared.SaveEclipseResult.Response response)
		{
			if (response == null)
			{
				_isSendEclipseResultFail = true;
				_sendFailMessage = LocalizationManager.GetString(BaseResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				_isSendEclipseResultFail = true;
				_sendFailMessage = LocalizationManager.GetString(response.BaseResult);
			}
			else if (response.Result != NShared.SaveEclipseResult.ResultType.Success)
			{
				_isSendEclipseResultFail = true;
				_sendFailMessage = LocalizationManager.GetString(BaseResultType.Fail);
			}
		});
	}

	private void InitRequestData(EclipseStatus eclipseStatus, bool isCurrentIndex = false, bool isAddDeckIndex = false, bool isIncludeLogString = false)
	{
		_request.AccountId = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;
		_request.PlayTime = SingletonAuto<BattleFacade>.instance.EclipseResultData.PlayTime;
		_request.TotalDamage = SingletonAuto<BattleFacade>.instance.EclipseResultData.TotalDamage;
		_request.MatchIndex = EclipseBattleInfoData.MatchIndex;
		_request.CurrentDeckIndex = DeckIndex;
		if (isAddDeckIndex)
		{
			NShared.SaveEclipseResult.Request request = _request;
			int currentDeckIndex = request.CurrentDeckIndex + 1;
			request.CurrentDeckIndex = currentDeckIndex;
		}
		if (isCurrentIndex)
		{
			_request.CurrentChapterIndex = SingletonAuto<BattleFacade>.instance.EclipseResultData.ChapterIndex;
			_request.CurrentDungeonIndex = SingletonAuto<BattleFacade>.instance.EclipseResultData.DungeonIndex;
			_request.CurrentWaveIndex = SingletonAuto<BattleFacade>.instance.EclipseResultData.WaveIndex;
		}
		else
		{
			_request.CurrentChapterIndex = EclipseBattleInfoData.CurrentChapterIndex;
			_request.CurrentDungeonIndex = EclipseBattleInfoData.CurrentDungeonIndex;
			_request.CurrentWaveIndex = EclipseBattleInfoData.CurrentWaveIndex;
		}
		_request.MaxWaveIndex = SingletonAuto<BattleFacade>.instance.EclipseResultData.MaxWaveIndex;
		_request.CurrentCreatureLevel = SingletonAuto<BattleFacade>.instance.EclipseResultData.CreatureLevel;
		_request.LastStatus = eclipseStatus;
		if (isIncludeLogString)
		{
			_request.BattleLogString = SingletonAuto<BattleFacade>.instance.EclipseResultData.BattleLogString;
			_request.CreatureInfoString = SingletonAuto<BattleFacade>.instance.EclipseResultData.CreatureInfoString;
			SingletonAuto<BattleFacade>.instance.EclipseResultData.BattleLogString = null;
			SingletonAuto<BattleFacade>.instance.EclipseResultData.CreatureInfoString = null;
		}
		else
		{
			_request.BattleLogString = null;
			_request.CreatureInfoString = null;
		}
	}

	int EclipseOfflineModeUpdater.IFuncEclipse.GetEclipseGameSpeedRatio()
	{
		return EclipseBattleInfoData.OnlineGameSpeedRatio;
	}

	int EclipseOfflineModeUpdater.IFuncEclipse.GetEclipseInitWaveIndex()
	{
		return EclipseBattleInfoData.CurrentWaveIndex;
	}

	int EclipseOfflineModeUpdater.IFuncEclipse.GetEclipseMaxWaveIndex()
	{
		return EclipseBattleInfoData.MaxWaveIndex;
	}

	long EclipseOfflineModeUpdater.IFuncEclipse.GetAccountID()
	{
		return GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;
	}

	Func<object, string> EclipseOfflineModeUpdater.IFuncEclipse.GetJsonMarshalerEncodeJsonString()
	{
		return JsonMarshaler.EncodeJsonString;
	}

	BaseJsonMarshaler.EncoderDelegate<ResultCreatureInfo> EclipseOfflineModeUpdater.IFuncEclipse.GetJsonMarshalerEncodeJsonObject()
	{
		return JsonMarshaler.EncodeJsonObject;
	}

	bool EclipseOfflineModeUpdater.IFuncEclipse.GetIsContinousBattle()
	{
		return _isContinousBattle;
	}

	int EclipseOfflineModeUpdater.IFuncEclipse.InitEclipseDeckData(ref Dictionary<int, List<BattleCreatureCreateInfo>> EclipseDeckData)
	{
		SetEclipseDeckData(ref EclipseDeckData);
		return EclipseBattleInfoData.CurrentDeckIndex;
	}

	void EclipseOfflineModeUpdater.IFuncEclipse.InitPlayerEnterBattleLog(ref PlayerEnterBattleLog PlayerEnterLog, int[] heroIndices)
	{
		PlayerEnterLog = CampaignContext.CreateMyEnterPlayerBattleLog(heroIndices, null, 0, null, null, null);
	}

	void EclipseOfflineModeUpdater.IFuncEclipse.InitEclipseDungeonData(ref EclipseDungeonData[] EclipseDungeonData)
	{
		if (EclipseDungeonData == null)
		{
			int num = EclipseBattleInfoData.DungeonList.Length;
			EclipseDungeonData = new EclipseDungeonData[num];
			for (int i = 0; i < num; i++)
			{
				EclipseDungeonData[i] = new EclipseDungeonData();
				EclipseDungeonData[i].ChapterIndex = EclipseBattleInfoData.DungeonList[i].ChapterIndex;
				EclipseDungeonData[i].DungeonIndex = EclipseBattleInfoData.DungeonList[i].DungeonIndex;
			}
		}
	}

	private void SetEclipseDeckData(ref Dictionary<int, List<BattleCreatureCreateInfo>> EclipseDeckList)
	{
		BattleCreatureCreateInfo.BuffSkillInfo buffSkillInfo = new BattleCreatureCreateInfo.BuffSkillInfo();
		buffSkillInfo.GuildSkillDataBases = EclipseBattleInfoData.GuildSkills;
		buffSkillInfo.AccountBuffDataBases = EclipseBattleInfoData.AccountBuffs;
		buffSkillInfo.ClassBuffDataBasess = EclipseBattleInfoData.ClassBuffDataBases;
		BattleCreatureCreateInfo.BuffStatInfo buffStatInfo = new BattleCreatureCreateInfo.BuffStatInfo();
		buffStatInfo.ExtraStatDatas = EclipseBattleInfoData.ExtraStatDataBases;
		buffStatInfo.PetStatDataBases = EclipseBattleInfoData.PetStatDataBases;
		EclipseDeckList = new Dictionary<int, List<BattleCreatureCreateInfo>>();
		EclipseDeckData[] deckList = EclipseBattleInfoData.DeckList;
		foreach (EclipseDeckData eclipseDeckData in deckList)
		{
			List<BattleCreatureCreateInfo> value = CreatreBattleCreatureCreateInfo(eclipseDeckData.HeroInfos, buffSkillInfo, buffStatInfo);
			EclipseDeckList.Add(eclipseDeckData.DeckIndex, value);
		}
	}

	private List<BattleCreatureCreateInfo> CreatreBattleCreatureCreateInfo(CachedHeroInfo[] heroInfos, BattleCreatureCreateInfo.BuffSkillInfo buffSkillInfo, BattleCreatureCreateInfo.BuffStatInfo buffStatInfo)
	{
		List<BattleCreatureCreateInfo> list = new List<BattleCreatureCreateInfo>();
		Random random = new Random(DateTime.UtcNow.Millisecond);
		for (int i = 0; i < heroInfos.Length; i++)
		{
			CachedHeroInfo cachedHeroInfo = JsonMarshaler.Clone(heroInfos[i]);
			cachedHeroInfo.Validate();
			BattleCreatureCreateInfo battleCreatureCreateInfo = BattleCreatureCreateInfo.CreateMyCreatureCreateInfo(cachedHeroInfo, isLeader: false, isAi: false, buffSkillInfo, buffStatInfo);
			battleCreatureCreateInfo.RandomSeed = random.Next();
			list.Add(battleCreatureCreateInfo);
		}
		return list;
	}

	private void OnDungeonEnd()
	{
		WaitBattleEnd(GameRoot.Instance.ConstantDataContainer.GetValueInt("EclipseWaitBattleEndExpireSecond", 3), EndEclipseTeam);
	}

	private void WaitBattleEnd(int waitSeconds, Action<BattleInstance> endBattleAction)
	{
		BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
		if (waitSeconds <= 0 || (SingletonAuto<BattleFacade>.instance.IsValidBattle() && battleCache.IsBattleEnded))
		{
			if (SingletonAuto<BattleFacade>.instance.IsValidBattle() && !battleCache.IsBattleEnded)
			{
				SingletonAuto<BattleFacade>.instance.ForceStopBattle();
			}
			Vespa_System_Action_Extension_Method.fire(endBattleAction, SingletonAuto<BattleFacade>.instance.GetStoppedBattleInstance());
		}
		else
		{
			SingletonUpdater<Executer>.instance.DelayRun(1f, this, delegate
			{
				WaitBattleEnd(waitSeconds - 1, endBattleAction);
			});
		}
	}

	private void EndEclipseTeam(BattleInstance battleInstance)
	{
		if (battleInstance == null)
		{
			VespaDebug.LogError("OnBattleResult - battleInstance is null.");
		}
		else if (IsCurrentLastDeck())
		{
			bool win = SingletonAuto<BattleFacade>.instance.GetCampaignCache().Winner == TeamType.Left;
			int campaignResultStar = battleInstance.GetCampaignResultStar(win, _enterDungeon.Difficulty);
			Dictionary<int, int> killCount = battleInstance.GetKillCount(TeamType.Left);
			long[] accountIds = new long[1] { GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id };
			SceneAutoBehaviour<EndCampaign>.instance.OpenByEclipse(HeroIndices, accountIds, win, campaignResultStar, killCount, battleInstance.CreatureContainer.GetAliveCreatureIndices(TeamType.Left), _request.CurrentChapterIndex, _request.CurrentDungeonIndex, (int)_enterDungeon.Difficulty, _enterDungeon.IsScenario);
		}
		else
		{
			SceneAutoBehaviour<EndCampaign>.instance.OpenEclipseRewardPopup(SingletonAuto<BattleFacade>.instance.EclipseResultData.MaxWaveIndex);
		}
	}

	private bool IsCurrentLastDeck()
	{
		if (GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.IsLastDeck();
		}
		return false;
	}

	void OfflinePlay.IDataProvider.HandleBattleEnd(TeamType winTeamType, BattleInstance battleInstance)
	{
	}

	string IDataProvider.GetPreloadFieldName()
	{
		return null;
	}

	int IDataProvider.GetTowerFloor()
	{
		return -1;
	}

	int OfflinePlay.IDataProvider.GetWorldMapEventIndex()
	{
		return -1;
	}

	bool IDataProvider.IsTowerBattle()
	{
		return false;
	}

	void IDataProvider.OnEnterBattle()
	{
	}

	void IDataProvider.OnWaveMoveEnd()
	{
	}

	void OfflinePlay.IDataProvider.HandleBattleBegin()
	{
	}

	OfflinePlay.ResponseData OfflinePlay.IDataProvider.GetResponseData()
	{
		return null;
	}

	List<List<BattleCreatureCreateInfo>> OfflinePlay.IDataProvider.GetBattleCreatureCreateInfoListToWaveList()
	{
		return null;
	}

	BattleInstanceExtraData OfflinePlay.IDataProvider.GetExtraData()
	{
		return null;
	}
}
