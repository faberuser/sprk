using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NBattleField;
using NGame2.NBattleField.NManager.NPlay;
using NGame2.NBattleView;
using NGame2.NContentsInfo;
using NGame2.NEventTrigger;
using NGame2.NGuild;
using NGame2.NLobby;
using NGame2.NMessageLog;
using NGame2.NNodeTree;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NScene.NPreloader;
using NGame2.NSocket;
using NGame2.NSubStory;
using NGame2.NUI;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NGame2.NUI.NWindow.NPunish;
using NGame2.NUtil;
using NShared;
using NShared.BeginCampaign;
using NShared.BeginContent;
using NShared.NEvent;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NShared.NWorldBoss;
using NShared.NWorldMap.NEventSubData;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NBattleContext;

public class BattleContext : SingletonAuto<BattleContext>
{
	public class LastDungeonResultData : LoadingManager.ILastSceneData
	{
		public EnterDungeonData EnterDungeon { get; private set; }

		public bool IsWin { get; set; }

		public LastDungeonResultData(EnterDungeonData enterDungeon)
		{
			EnterDungeon = enterDungeon;
		}
	}

	public const EBattleMenu DefaultBattleMenu = EBattleMenu.TimeStatus | EBattleMenu.SystemButton | EBattleMenu.WaveStatus | EBattleMenu.LootingStatus | EBattleMenu.Retry | EBattleMenu.DamageMeter | EBattleMenu.HeroParty | EBattleMenu.BoosterStatus | EBattleMenu.QuestStatus | EBattleMenu.SystemButtonAuto;

	public LocalAccountEnum<SystemButtonAutoFastContinue.EContinueType> ContinueType = new LocalAccountEnum<SystemButtonAutoFastContinue.EContinueType>(SystemButtonAutoFastContinue.EContinueType.Normal, "BattleSystemContinueStatus");

	private LastDungeonResultData _lastDungeonResult;

	private List<IBattleContextChangeListner> _changeListners = new List<IBattleContextChangeListner>();

	public IDataProvider DataProvider { get; private set; }

	public bool LastBattleWin { get; private set; }

	public int LastBattleStar { get; private set; }

	public StaminaType StaminaType { get; private set; }

	public int StaminaCount { get; private set; }

	public int LastBattleChapterIndex { get; private set; }

	public int LastBattleDungeonIndex { get; private set; }

	public NShared.DungeonDifficultyType LastBattleDifficulty { get; private set; }

	public BattleType LastBattleType { get; private set; }

	public void RegisterListener(IBattleContextChangeListner listner)
	{
		_changeListners.Add(listner);
	}

	public void SetLastDungeonResult(EBoolCheck isWin)
	{
		if (_lastDungeonResult != null)
		{
			if (isWin == EBoolCheck.DontCare)
			{
				_lastDungeonResult = null;
			}
			else
			{
				_lastDungeonResult.IsWin = isWin == EBoolCheck.True;
			}
		}
		GlobalAutoBehaviour<LoadingManager>.instance.LastSceneData = _lastDungeonResult;
	}

	public void MoveSceneToLobby(Action onLoad, EBoolCheck isWin)
	{
		if (_lastDungeonResult != null)
		{
			if (isWin == EBoolCheck.DontCare)
			{
				_lastDungeonResult = null;
			}
			else
			{
				_lastDungeonResult.IsWin = isWin == EBoolCheck.True;
			}
		}
		if (!(DataProvider is EclipseContext))
		{
			EndCampaign.DisableOnetimeBoosterRepeatBattle();
		}
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(new OnClickReturnToMap());
		GlobalAutoBehaviour<LoadingManager>.instance.MoveScene(LoadingManager.ESceneType.Lobby, LoadingManager.ELoadingType.Default, resetEnvironment: true, onLoad, _lastDungeonResult);
	}

	public void BeginCampaign(NShared.BeginCampaign.Request request, Action<NShared.BeginCampaign.Response> onSuccessCallback = null, Action<NShared.BeginCampaign.Response> onErrorCallback = null)
	{
		if (request != null && request.ChapterIndex != 0)
		{
			SetLastBattleChapterDungeon(request.ChapterIndex, request.DungeonIndex);
			BeginCampaignInternal(request, onSuccessCallback, delegate(NShared.BeginCampaign.Response response)
			{
				BeginCampaignOnErrorCallback(request, response, onErrorCallback);
			});
		}
	}

	private void BeginCampaignOnErrorCallback(NShared.BeginCampaign.Request request, NShared.BeginCampaign.Response response, Action<NShared.BeginCampaign.Response> onErrorCallback)
	{
		RestoredPortal.LogRaidEntryFailure(request, response);
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(request.ChapterIndex, request.DungeonIndex);
		Vespa_System_Action_Extension_Method.fire(onErrorCallback, response);
		if (NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled())
		{
			NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.ClearSelectedReward();
			NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.StopRepeat();
			ShowAutoRepeatStoppedPopup(data, response.Result);
		}
		AutoBattle.Stop();
		if (data != null)
		{
			if (response.Result == NShared.BeginCampaign.ResultType.NotEnoughStamina || response.Result == NShared.BeginCampaign.ResultType.NotEnoughPlayKey || response.Result == NShared.BeginCampaign.ResultType.NotEnoughHideoutDungeonCount || response.Result == NShared.BeginCampaign.ResultType.NotEnoughConquestDungeonCount)
			{
				BattleDefineData battleDefine = data.GetBattleDefine(isScenario: false);
				if (battleDefine == null || (battleDefine.BattleMenu & EBattleMenu.Retry) != EBattleMenu.None)
				{
					Action onSuccess = null;
					if (GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpened<EndBattleReward>())
					{
						onSuccess = SceneAutoBehaviour<EndCampaign>.instance.ProcessBattleRepeat;
					}
					UIHelper.ResolveRecoverStamina(isNotEnough: true, null, data.ReqStaminaType, data.ChapterIndex, onSuccess);
				}
				return;
			}
			if (response.Result == NShared.BeginCampaign.ResultType.MaxDailyCount)
			{
				LobbyCampaignDungeon.OpenConfirmResetEventDungeon(request.ChapterIndex, request.DungeonIndex, null);
				return;
			}
			if (response.Result == NShared.BeginCampaign.ResultType.EquipItemFull)
			{
				if (GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType == LoadingManager.ESceneType.Lobby)
				{
					string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
					string message = LocalizationManager.GetString(NShared.BeginCampaign.ResultType.EquipItemFull);
					MessagePopup.OpenEnabled(title, message, delegate
					{
						SceneAutoBehaviour<InventoryManagement>.instance.OpenInventory();
					}, null, "MOVE_BAG", bYesEnabled: true);
				}
				else
				{
					string title2 = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
					string message2 = LocalizationManager.GetString(NShared.BeginCampaign.ResultType.EquipItemFull);
					MessagePopup.OpenEnabled(title2, message2, SceneAutoBehaviour<EndCampaign>.instance.ExecReturnToLobbyAndOpenInventory, null, "MOVE_BAG", bYesEnabled: true);
				}
				return;
			}
		}
		Action onOk = null;
		if (response.Result == NShared.BeginCampaign.ResultType.GuildRaidInitializing)
		{
			onOk = delegate
			{
				MoveSceneToLobby(delegate
				{
					IList<PortalRenewalData> data2 = GameRoot.Instance.PortalRenewalDataContainer.GetData(PortalRenewalCategory.Guild);
					if (!__Extension_Method_ICollection__.IsNullOrEmpty(data2))
					{
						PortalRenewalData portalRenewalData = data2.First((PortalRenewalData e) => e.SubCategory == PortalRenewalSubCategory.GuildRaid);
						if (portalRenewalData != null)
						{
							LobbyScene2.ListReturnState.Clear();
							SceneAutoBehaviour<ActivityManager>.instance.Run(portalRenewalData.MoveTaskType.ToString(), portalRenewalData.MoveTaskValue);
						}
					}
				}, EBoolCheck.DontCare);
			};
		}
		MessagePopup.OpenError(LocalizationManager.GetResultString(response), onOk);
	}

	public void BeginScenarioReplay(NShared.BeginCampaign.Request request, Action onCallback = null)
	{
		if (request != null)
		{
			SetLastBattleChapterDungeon(request.ChapterIndex, request.DungeonIndex);
			NShared.BeginCampaign.Response response = new NShared.BeginCampaign.Response();
			response.BaseResult = BaseResultType.Success;
			response.Result = NShared.BeginCampaign.ResultType.Success;
			SetCampaign(request, response);
			MoveToBattleScene();
			if (onCallback != null)
			{
				Vespa_System_Action_Extension_Method.fire(onCallback);
			}
		}
	}

	private bool ShowAutoRepeatStoppedPopup(CampaignDungeonData dungeonData, NShared.BeginCampaign.ResultType resultType)
	{
		StaminaType key = dungeonData?.ReqStaminaType ?? StaminaType.Chicken;
		if (GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType == LoadingManager.ESceneType.Lobby)
		{
			return false;
		}
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string text = string.Empty;
		switch (resultType)
		{
		case NShared.BeginCampaign.ResultType.NotEnoughStamina:
		{
			StaminaData data = GameRoot.Instance.StaminaDataContainer.GetData(key);
			if (data != null)
			{
				text = ((!String_Extension_Method.IsNullOrEmpty(data.AutoRepeatStopMsgKey)) ? LocalizationManager.GetStringDirect(data.AutoRepeatStopMsgKey) : LocalizationManager.GetString(NShared.NLocalization.AutoRepeatBattle.AUTO_REPEAT_BATTLE_FAILED_STAMINA));
			}
			break;
		}
		case NShared.BeginCampaign.ResultType.EquipItemFull:
			text = LocalizationManager.GetString(NShared.NLocalization.AutoRepeatBattle.AUTO_REPEAT_BATTLE_FAILED_EQUIP_ITEM_FULL);
			break;
		case NShared.BeginCampaign.ResultType.MaxDailyCount:
			text = LocalizationManager.GetString(NShared.NLocalization.AutoRepeatBattle.AUTO_REPEAT_BATTLE_FAILED_EVENT_MAX_COUNT);
			break;
		}
		if (String_Extension_Method.IsNullOrEmpty(text))
		{
			return false;
		}
		GlobalMessagePopup.OpenGlobal(title, text);
		return true;
	}

	private void BeginCampaignInternal(NShared.BeginCampaign.Request request, Action<NShared.BeginCampaign.Response> onSuccessCallback, Action<NShared.BeginCampaign.Response> onErrorCallback)
	{
		VespaCoroutine.Start(coBeginCampaign(request, onSuccessCallback, onErrorCallback));
	}

	private IEnumerator coBeginCampaign(NShared.BeginCampaign.Request request, Action<NShared.BeginCampaign.Response> onSuccessCallback, Action<NShared.BeginCampaign.Response> onErrorCallback)
	{
		SceneChanger sceneChanger = null;
		if (BattleScene.QuickRestart.Value && GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType == LoadingManager.ESceneType.Battle2)
		{
			sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(5f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
			while (!sceneChanger.IsCompleteAnimation())
			{
				yield return null;
			}
			ResetContext();
		}
		QueryMarketInfo queryMarketInfo = new QueryMarketInfo();
		queryMarketInfo.Query(null);
		using (new LobbyPopupDisabler())
		{
			bool executeEnd = false;
			NShared.BeginCampaign.Response response = null;
			SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.BeginCampaign.Response iResponse)
			{
				executeEnd = true;
				response = iResponse;
			});
			while (!executeEnd || !queryMarketInfo.IsDone)
			{
				yield return null;
			}
			if (!queryMarketInfo.Verify())
			{
				yield break;
			}
			if (response == null || response.BaseResult != BaseResultType.Success || response.Result != NShared.BeginCampaign.ResultType.Success)
			{
				Vespa_System_Action_Extension_Method.fire(onErrorCallback, response);
				yield break;
			}
			try
			{
				SetCampaign(request, response);
			}
			catch
			{
				response = new NShared.BeginCampaign.Response();
				response.BaseResult = BaseResultType.HttpError;
				Vespa_System_Action_Extension_Method.fire(onErrorCallback, response);
				yield break;
			}
			GlobalAutoBehaviour<AccountManager>.instance.ApplyBeginCampaignResponse(response);
			Vespa_System_Action_Extension_Method.fire(onSuccessCallback, response);
			if (BattleScene.QuickRestart.Value)
			{
				BaseScene baseScene = UnityEngine.Object.FindObjectOfType(typeof(BaseScene)) as BaseScene;
				if (GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType == LoadingManager.ESceneType.Battle2 && baseScene != null)
				{
					sceneChanger.Close();
					baseScene.OnSceneLoaded(null);
				}
				else
				{
					MoveToBattleScene();
				}
			}
			else
			{
				MoveToBattleScene();
			}
			foreach (IBattleContextChangeListner changeListner in _changeListners)
			{
				changeListner.OnBattleContextChange();
			}
		}
	}

	public void ResetContext()
	{
		BattleViewGenericSingleton<DropItemManager>.instance.ClearAll();
		BattleViewGenericSingleton<CreatureManager>.instance.ReleaseAllCreatures();
		SceneAutoBehaviour<RuntimeGlobalManager>.instance.ClearAll();
		SceneAutoBehaviour<BattleManagement>.instance.CloseBut();
		if (SceneAutoBehaviour<EndCampaign>.isValidInstance)
		{
			SceneAutoBehaviour<EndCampaign>.instance.CloseBut();
		}
		SceneAutoBehaviour<CameraManager2>.instance.SwitchCam_Opening();
		GameSetting.Init();
	}

	public void RepeatContent(BattlePartySetting.ExtraOptionData extraOption)
	{
		EnterDungeonData enterDungeon = SingletonAuto<PunishmentRaidContext>.instance.EnterDungeon;
		_ = enterDungeon.Chapter;
		_ = enterDungeon.OriginalDungeon;
		_ = enterDungeon.LinkedDungeon;
		_ = enterDungeon.Difficulty;
		NShared.BeginContent.Request request = new NShared.BeginContent.Request();
		request.ChapterIndex = enterDungeon.Chapter;
		request.DungeonIndex = enterDungeon.OriginalDungeon;
		request.DungeonDifficulty = (int)enterDungeon.Difficulty;
		request.HeroIndices = SingletonAuto<PunishmentRaidContext>.instance.HeroIndices;
		request.AiHeroIndices = SingletonAuto<PunishmentRaidContext>.instance.AiHeroIndices;
		request.GroupIndex = SingletonAuto<PunishmentRaidContext>.instance.GroupIndex;
		request.DungeonType = SingletonAuto<PunishmentRaidContext>.instance.PunishmentDungeonType;
		request.AffixIndices = SingletonAuto<PunishmentRaidContext>.instance.ExtraData.AffixIndices;
		switch (SingletonAuto<PunishmentRaidContext>.instance.PunishmentDungeonType)
		{
		case EPunishmentDungeonType.Boss:
			request.Level = SingletonAuto<PunishmentRaidContext>.instance.ExtraData.InfinityLevel;
			SprkSoloRetry.PreserveShards(request, SingletonAuto<PunishmentRaidContext>.instance);
			break;
		case EPunishmentDungeonType.Karma:
			request.FlaskItemIndex = SingletonAuto<PunishmentRaidContext>.instance.ExtraData.FlaskItemIndex;
			request.FlaskItemCount = SingletonAuto<PunishmentRaidContext>.instance.ExtraData.FlaskItemCount;
			break;
		}
		long[] accountIds = SingletonAuto<PunishmentRaidContext>.instance.AccountIds;
		RequestBeginContent(request, accountIds, null, null);
	}

	public void RepeatCampaign(BattlePartySetting.ExtraOptionData extraOption)
	{
		EnterDungeonData enterDungeon = SingletonAuto<CampaignContext>.instance.EnterDungeon;
		int chapterIndex = enterDungeon.Chapter;
		int dungeonIndex = enterDungeon.OriginalDungeon;
		int dungeonIndex2 = enterDungeon.LinkedDungeon;
		NShared.DungeonDifficultyType difficultyType = enterDungeon.Difficulty;
		int worldMapEventIndex = SingletonAuto<CampaignContext>.instance.WorldMapEventIndex;
		int worldMapNodeIndex = SingletonAuto<CampaignContext>.instance.WorldMapNodeIndex;
		int towerIndex = SingletonAuto<CampaignContext>.instance.TowerIndex;
		int towerFloor = SingletonAuto<CampaignContext>.instance.TowerFloor;
		int worldBossIndex = SingletonAuto<CampaignContext>.instance.WorldBossIndex;
		int raidIndex = SingletonAuto<CampaignContext>.instance.RaidIndex;
		int raidLevel = SingletonAuto<CampaignContext>.instance.RaidLevel;
		if (!enterDungeon.IsScenario)
		{
			NShared.NWorldMap.NEventSubData.EventDungeon eventDungeonData = null;
			GetWorldMapEventDungeonInfo(chapterIndex, ref dungeonIndex2, out worldMapEventIndex, out eventDungeonData);
		}
		long itemBoosterCount = SingletonAuto<CampaignContext>.instance.OnetimeBoosterCount;
		bool repeat = extraOption == null || extraOption.IsEndCampaign;
		if (AutoNextBattle.IsEnabled())
		{
			long itemBoosterCount2 = AutoNextBattle.instance.ItemBoosterCount;
			int num = 0;
			if (itemBoosterCount2 > num)
			{
				itemBoosterCount = AutoNextBattle.instance.ItemBoosterCount;
				repeat = false;
			}
		}
		NShared.BeginCampaign.Request request = new NShared.BeginCampaign.Request
		{
			ChapterIndex = chapterIndex,
			DungeonIndex = dungeonIndex,
			LinkedDungeonIndex = dungeonIndex2,
			DungeonDifficulty = (int)difficultyType,
			ScenarioDungeon = enterDungeon.IsScenario,
			HeroIndices = SingletonAuto<CampaignContext>.instance.HeroIndices,
			AiHeroIndices = SingletonAuto<CampaignContext>.instance.AiHeroIndices,
			GroupHeroIndices = SingletonAuto<CampaignContext>.instance.GroupHeroIndices,
			LeaderHeroIndex = SingletonAuto<CampaignContext>.instance.LeaderHeroIndex,
			AccountIds = SingletonAuto<CampaignContext>.instance.AccountIds,
			WorldMapEventIndex = worldMapEventIndex,
			WorldMapNodeIndex = worldMapNodeIndex,
			WorldBossIndex = worldBossIndex,
			TowerIndex = towerIndex,
			TowerFloor = towerFloor,
			Repeat = repeat,
			ItemBoosterCount = itemBoosterCount,
			RaidIndex = raidIndex,
			RaidLevel = raidLevel,
			DeltaHeroIndices = SingletonAuto<CampaignContext>.instance.DeltaHeroIndices,
			ZetaHeroIndices = SingletonAuto<CampaignContext>.instance.ZetaHeroIndices,
			AffixIndices = SingletonAuto<CampaignContext>.instance.ExtraData.AffixIndices
		};
		if (GameRoot.Instance.SelectRewardDataContainer.GetData(request.ChapterIndex, request.DungeonIndex) != null)
		{
			request.SelectedRewardItemCodes = NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.GetSelectedRewardToArray();
		}
		if (!enterDungeon.IsScenarioReplay)
		{
			BeginCampaign(request, delegate
			{
				if (extraOption != null && extraOption.AutoRepeat)
				{
					NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.OpenGlobal();
				}
				if (SubStoryUtillity.IsSubStoryBattleType(enterDungeon))
				{
					SingletonAuto<CampaignContext>.instance.SetForcedUserBattleCreatureCreateInfos(SubStoryUtillity.GetBattleCreatureInfos(enterDungeon).ToArray());
				}
				if (GameRoot.Instance.DungeonHeroSetDataContainer.IsDungeonHeroSet(chapterIndex, dungeonIndex, difficultyType))
				{
					List<BattleCreatureCreateInfo> dungeonHeroSetBattleCreatureInfos = NGame2.NAccount.Helper.GetDungeonHeroSetBattleCreatureInfos(enterDungeon, SingletonAuto<CampaignContext>.instance.HeroIndices);
					SingletonAuto<CampaignContext>.instance.SetForcedUserBattleCreatureCreateInfos(dungeonHeroSetBattleCreatureInfos.ToArray());
				}
			});
		}
		else
		{
			BeginScenarioReplay(request);
		}
	}

	public void GetWorldMapEventDungeonInfo(int chapterIndex, ref int dungeonIndex, out int worldMapEventIndex, out NShared.NWorldMap.NEventSubData.EventDungeon eventDungeonData)
	{
		eventDungeonData = null;
		List<WorldMapEventInfo> list = GlobalAutoBehaviour<AccountManager>.instance.WorldMapEventManager.Get(chapterIndex, dungeonIndex);
		worldMapEventIndex = ((list.Count > 0) ? list[0].EventIndex : 0);
		using PoolingList<NShared.NWorldMap.NEventSubData.EventDungeon> poolingList = GlobalAutoBehaviour<AccountManager>.instance.WorldMapEventManager.GetEventDataOfType<NShared.NWorldMap.NEventSubData.EventDungeon>();
		for (int i = 0; i < poolingList.Count; i++)
		{
			NShared.NWorldMap.NEventSubData.EventDungeon eventDungeon = poolingList[i];
			if (eventDungeon.ChapterIndex == chapterIndex && eventDungeon.DungeonIndex == dungeonIndex)
			{
				eventDungeonData = eventDungeon;
				worldMapEventIndex = eventDungeon.EventIndex;
				dungeonIndex = eventDungeon.EventDungeonIndex;
				break;
			}
		}
	}

	public void SetEclipse(NShared.BeginCampaign.Request request, NShared.BeginCampaign.Response response, bool isContinuousBattle)
	{
		EnterDungeonData enterDungeon = new EnterDungeonData(request.ChapterIndex, request.DungeonIndex, request.LinkedDungeonIndex, (NShared.DungeonDifficultyType)request.DungeonDifficulty, request.ScenarioDungeon);
		_lastDungeonResult = new LastDungeonResultData(enterDungeon);
		DataProvider = SingletonAuto<EclipseContext>.instance;
		DataProvider.GetEnterDungeon();
		NShared.DungeonDifficultyType dungeonDifficulty = (NShared.DungeonDifficultyType)request.DungeonDifficulty;
		SetLastBattleChapterDungeon(request.ChapterIndex, request.DungeonIndex, dungeonDifficulty);
		SingletonAuto<EclipseContext>.instance.InitEclipseInfo(request, response, isContinuousBattle);
	}

	public void SetPunishmentRaidContent(NShared.BeginContent.Request request, NShared.BeginContent.Response response, long[] accountIds)
	{
		EnterDungeonData enterDungeon = new EnterDungeonData(request.ChapterIndex, request.DungeonIndex, request.DungeonIndex, (NShared.DungeonDifficultyType)request.DungeonDifficulty);
		_lastDungeonResult = new LastDungeonResultData(enterDungeon);
		DataProvider = SingletonAuto<PunishmentRaidContext>.instance;
		NShared.DungeonDifficultyType dungeonDifficulty = (NShared.DungeonDifficultyType)request.DungeonDifficulty;
		SetLastBattleChapterDungeon(request.ChapterIndex, request.DungeonIndex, dungeonDifficulty);
		SingletonAuto<PunishmentRaidContext>.instance.Init(request, response, accountIds);
	}

	public void SetCampaign(NShared.BeginCampaign.Request request, NShared.BeginCampaign.Response response)
	{
		EnterDungeonData enterDungeon = new EnterDungeonData(request.ChapterIndex, request.DungeonIndex, request.LinkedDungeonIndex, (NShared.DungeonDifficultyType)request.DungeonDifficulty, request.ScenarioDungeon);
		_lastDungeonResult = new LastDungeonResultData(enterDungeon);
		DataProvider = SingletonAuto<CampaignContext>.instance;
		EnterDungeonData enterDungeon2 = DataProvider.GetEnterDungeon();
		NShared.DungeonDifficultyType dungeonDifficulty = (NShared.DungeonDifficultyType)request.DungeonDifficulty;
		SetLastBattleChapterDungeon(request.ChapterIndex, request.DungeonIndex, dungeonDifficulty);
		SingletonAuto<CampaignContext>.instance.BeginCampaignRequest(request);
		SingletonAuto<CampaignContext>.instance.BeginCampaignResponse(response);
		if (GameRoot.Instance.DungeonHeroSetDataContainer.IsDungeonHeroSet(request.ChapterIndex, request.DungeonIndex, (NShared.DungeonDifficultyType)request.DungeonDifficulty))
		{
			List<BattleCreatureCreateInfo> dungeonHeroSetBattleCreatureInfos = NGame2.NAccount.Helper.GetDungeonHeroSetBattleCreatureInfos(enterDungeon, request.HeroIndices);
			SingletonAuto<CampaignContext>.instance.SetForcedUserBattleCreatureCreateInfos(dungeonHeroSetBattleCreatureInfos.ToArray());
		}
		WorldBossInfo worldBossInfo = response.WorldBossInfo;
		if (worldBossInfo != null)
		{
			SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.SetWorldBossInfo(worldBossInfo);
			enterDungeon2.InitialHeroInfos = worldBossInfo.CreateResultCreatureInfos();
		}
		NShared.GuildRaidDungeonInfo guildRaidDungeonInfo = response.GuildRaidDungeonInfo;
		if (guildRaidDungeonInfo != null)
		{
			GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager?.GuildRaidManager.Apply(guildRaidDungeonInfo);
			enterDungeon2.InitialHeroInfos = guildRaidDungeonInfo.CreateResultCreatureInfos();
			SingletonAuto<MessageLogHandler>.instance.SendToGuild(new GuildRaidBegin
			{
				Nick = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Nick,
				ChapterIndex = enterDungeon2.Chapter,
				DungeonIndex = enterDungeon2.OriginalDungeon
			});
		}
		GuildSuppressScoreInfo guildSuppressScoreInfo = response.GuildSuppressScoreInfo;
		if (guildSuppressScoreInfo != null)
		{
			GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager?.GuildSuppressManager.Apply(guildSuppressScoreInfo);
		}
		GuildSuppressPlayInfo guildSuppressPlayInfo = response.GuildSuppressPlayInfo;
		if (guildSuppressPlayInfo != null)
		{
			GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager?.GuildSuppressManager.Apply(guildSuppressPlayInfo);
		}
		GuildSuppressInfo guildSuppressInfo = response.GuildSuppressInfo;
		if (guildSuppressInfo != null)
		{
			GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager?.GuildSuppressManager.Apply(guildSuppressInfo);
		}
		else
		{
			SingletonAuto<CampaignContext>.instance.SetInitialHeroInfo(enterDungeon2.InitialHeroInfos);
		}
	}

	public void SetCustomCampaign(int chapterIndex, int dungeonIndex, NShared.DungeonDifficultyType difficultType)
	{
		DataProvider = SingletonAuto<CustomCampaignContext>.instance;
		SetLastBattleChapterDungeon(chapterIndex, dungeonIndex, difficultType);
		SingletonAuto<CustomCampaignContext>.instance.Init(chapterIndex, dungeonIndex, difficultType);
	}

	public void SetArena(MatchInfo matchInfo)
	{
		SetLastBattleChapterDungeon(matchInfo.ChapterIndex, matchInfo.DungeonIndex);
		DataProvider = SingletonAuto<ArenaContext>.instance;
		MatchArena.MatchSeasonInfo matchSeasonInfo = SingletonAuto<MatchArena>.instance.GetMatchSeasonInfo(matchInfo.ArenaType);
		SingletonAuto<ArenaContext>.instance.Init(matchInfo, matchSeasonInfo);
	}

	public void SetOfflineArena(int[] heroIndices, int leaderHeroIndex, MatchedAccountInfo matchedNpcInfo, int chapterIndex, int dungeonIndex, MatchedAccountInfo myMatchInfo)
	{
		SetLastBattleChapterDungeon(chapterIndex, dungeonIndex);
		DataProvider = SingletonAuto<OfflineArenaContext>.instance;
		SingletonAuto<OfflineArenaContext>.instance.Init(heroIndices, leaderHeroIndex, matchedNpcInfo, chapterIndex, dungeonIndex, myMatchInfo);
	}

	public void SetOrdealArena(int[] heroIndices, int leaderHeroIndex, MatchedAccountInfo matchedNpcInfo, int chapterIndex, int dungeonIndex, MatchedAccountInfo myMatchInfo)
	{
		SetLastBattleChapterDungeon(chapterIndex, dungeonIndex);
		DataProvider = SingletonAuto<OrdealArenaContext>.instance;
		SingletonAuto<OrdealArenaContext>.instance.Init(heroIndices, leaderHeroIndex, matchedNpcInfo, chapterIndex, dungeonIndex, myMatchInfo);
	}

	public void SetArenaReplay(ArenaReplayInfo replayInfo, BaseBattleLog[] battleLogs)
	{
		SetLastBattleChapterDungeon(replayInfo.ChapterIndex, replayInfo.DungeonIndex);
		DataProvider = SingletonAuto<ArenaReplayContext>.instance;
		SingletonAuto<ArenaReplayContext>.instance.Init(replayInfo, battleLogs);
	}

	public void SetCampaignReplay(int chapterIndex, int dungeonIndex, NShared.DungeonDifficultyType difficulty, BaseBattleLog[] battleLogs)
	{
		SetLastBattleChapterDungeon(chapterIndex, dungeonIndex);
		DataProvider = SingletonAuto<CampaignReplayContext>.instance;
		SingletonAuto<CampaignReplayContext>.instance.Init(chapterIndex, dungeonIndex, difficulty, battleLogs);
	}

	public void MoveToBattleScene(LoadingManager.ELoadingType loadingType = LoadingManager.ELoadingType.Default, Action onLoad = null)
	{
		if (((SingletonAuto<NGame2.NAccount.UserCampaignManager.ChapterDungeonInfo>.isValidInstance && NGame2.NAccount.UserCampaignManager.ChapterIndex != PortalAdventureNoticeList.OriginChapterIndex) || NGame2.NAccount.UserCampaignManager.DungeonIndex != PortalAdventureNoticeList.OriginDungeonIndex || (PortalAdventureNoticeList.OriginChapterIndex != 0 && PortalAdventureNoticeList.OriginDungeonIndex != 0)) && GameRoot.Instance.CampaignNodeDataContainer.GetData(NGame2.NAccount.UserCampaignManager.ChapterIndex, NGame2.NAccount.UserCampaignManager.DungeonIndex) != null)
		{
			PortalAdventureNoticeList.OriginChapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
			PortalAdventureNoticeList.OriginDungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		}
		IEvent triggerEvent = new EnterBattleLoading();
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
		GlobalAutoBehaviour<LoadingManager>.instance.MoveScene(LoadingManager.ESceneType.Battle2, loadingType, onLoad);
	}

	public void Preload(BattleScenePreloader preloader)
	{
		if (DataProvider == null)
		{
			VespaDebug.LogError("Failed to preload. dataProvider is null");
		}
		else
		{
			DataProvider.Preload(preloader);
		}
	}

	public T GetContext<T>() where T : class, IDataProvider
	{
		return DataProvider as T;
	}

	public void StartContextInBattleScene(Action onReadyBattle)
	{
		if (DataProvider == null)
		{
			VespaDebug.LogError("Cannot start battle. DataProvider is null.");
			return;
		}
		SetBattleResult(win: false, 0);
		IPlay battlePlay = DataProvider.GetBattlePlay();
		if (battlePlay == null)
		{
			VespaDebug.LogError("StartContextInBattleScene : Play is null!");
			return;
		}
		SingletonAuto<BattleFacade>.instance.BattleCondition = DataProvider.CreateBattleCondition();
		battlePlay.Setup(DataProvider, onReadyBattle);
	}

	public void SetBattleResult(bool win, int star)
	{
		LastBattleWin = win;
		LastBattleStar = star;
	}

	public void ReturnToLobby(Action onReturn = null)
	{
		VespaCoroutine.Start(coBattleToLobby(onReturn));
	}

	private IEnumerator coBattleToLobby(Action onReturn)
	{
		if (PvpLoading.isValidInstance)
		{
			PvpLoading.ReleaseInstance();
		}
		if (GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType == LoadingManager.ESceneType.Lobby)
		{
			Vespa_System_Action_Extension_Method.fire(onReturn);
			yield break;
		}
		bool sceneMoved = false;
		MoveSceneToLobby(delegate
		{
			sceneMoved = true;
		}, EBoolCheck.DontCare);
		while (!sceneMoved)
		{
			yield return null;
		}
		Vespa_System_Action_Extension_Method.fire(onReturn);
	}

	private void SetLastBattleChapterDungeon(int chapterIndex, int dungeonIndex, NShared.DungeonDifficultyType difficultyType = NShared.DungeonDifficultyType.Easy)
	{
		LastBattleChapterIndex = chapterIndex;
		LastBattleDungeonIndex = dungeonIndex;
		LastBattleDifficulty = difficultyType;
		LastBattleType = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex)?.BattleType ?? BattleType.None;
	}

	public void StopBattle()
	{
		IPlay battlePlay = DataProvider.GetBattlePlay();
		if (battlePlay is OnlinePlay)
		{
			(battlePlay as OnlinePlay).StopBattle();
		}
	}

	public void RequestBeginEclipse(EnterDungeonData enterDungeon, BattlePartySetting.ExtraOptionData extraOption, bool isOnline = false)
	{
		extraOption.IsContinousBattle = ContinueType.Value == SystemButtonAutoFastContinue.EContinueType.Continue;
		NShared.BeginCampaign.Request request = null;
		if (isOnline)
		{
			string serverPrivateIp = GlobalAutoBehaviour<AccountManager>.instance.ServerPrivateIp;
			int serverPort = GlobalAutoBehaviour<AccountManager>.instance.ServerPort;
			request = new NShared.BeginCampaign.Request
			{
				ChapterIndex = enterDungeon.Chapter,
				DungeonIndex = enterDungeon.OriginalDungeon,
				LinkedDungeonIndex = enterDungeon.LinkedDungeon,
				DungeonDifficulty = (int)enterDungeon.Difficulty,
				ScenarioDungeon = enterDungeon.IsScenario,
				OnlineGameSpeedRatio = extraOption.OnlineGameSpeedRatio,
				EnterTicketCount = extraOption.EnterTicketCount,
				MultiplayMasterId = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId,
				MultiplayMemberIds = new long[1] { GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId },
				MultiplayMasterServerIp = serverPrivateIp,
				MultiplayMasterServerPort = serverPort,
				Repeat = false
			};
		}
		else
		{
			request = new NShared.BeginCampaign.Request
			{
				ChapterIndex = enterDungeon.Chapter,
				DungeonIndex = enterDungeon.OriginalDungeon,
				LinkedDungeonIndex = enterDungeon.LinkedDungeon,
				DungeonDifficulty = (int)enterDungeon.Difficulty,
				ScenarioDungeon = enterDungeon.IsScenario,
				OnlineGameSpeedRatio = extraOption.OnlineGameSpeedRatio,
				EnterTicketCount = extraOption.EnterTicketCount,
				Repeat = false
			};
		}
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.BeginCampaign.Response response)
		{
			if (response != null)
			{
				if (response.BaseResult != BaseResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
				}
				else if (response.Result == NShared.BeginCampaign.ResultType.EquipItemFull)
				{
					string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
					string message = LocalizationManager.GetString(NShared.BeginCampaign.ResultType.EquipItemFull);
					MessagePopup.OpenEnabled(title, message, delegate
					{
						SceneAutoBehaviour<InventoryManagement>.instance.OpenInventory();
					}, null, "MOVE_BAG", bYesEnabled: true);
				}
				else if (response.Result != NShared.BeginCampaign.ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
				}
				else if (isOnline)
				{
					ConnectToBattleServer(request, response, extraOption);
				}
				else
				{
					MoveToBattleScene();
					SetEclipse(request, response, extraOption.IsContinousBattle);
					GlobalAutoBehaviour<AccountManager>.instance.ApplyBeginCampaignResponse(response);
					LobbyScene2.ListReturnState.Clear();
					LobbyScene2.ListReturnState.Add(new EclipseReturnState());
				}
			}
		});
	}

	private void ConnectToBattleServer(NShared.BeginCampaign.Request request, NShared.BeginCampaign.Response response, BattlePartySetting.ExtraOptionData extraOption)
	{
		WaitingPopup.Open(this);
		string battleServerAddress = response.BattleServerAddress;
		int battleServerPort = response.BattleServerPort;
		GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector.Connect(battleServerAddress, battleServerPort, delegate(bool connected)
		{
			WaitingPopup.Close(this);
			if (!connected)
			{
				if (NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled())
				{
					NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.OnClickStop(sendRoomRepeat: true);
				}
				VespaDebug.LogError("Failed to connect to battle server. addr=" + battleServerAddress + ":" + battleServerPort);
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				BattleLoginReq battleLoginReq = new BattleLoginReq
				{
					AccountId = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id
				};
				CachedUserInfo cachedUserInfo = new CachedUserInfo
				{
					AccountId = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id
				};
				battleLoginReq.UserInfos = new CachedUserInfo[1] { cachedUserInfo };
				EclipseDeckResult[] savedDeckInfos = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetSavedDeckInfos();
				if (savedDeckInfos != null)
				{
					if (savedDeckInfos.Length != 0)
					{
						List<int> list = new List<int>();
						string[] array = savedDeckInfos[0].HeroIndices.Split(',');
						foreach (string s in array)
						{
							int result = 0;
							if (int.TryParse(s, out result) && result != 0)
							{
								list.Add(result);
							}
						}
						battleLoginReq.HeroIndices = list.ToArray();
					}
					battleLoginReq.MatchUid = 0L;
					battleLoginReq.MasterId = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
					battleLoginReq.GameServerHost = GlobalAutoBehaviour<AccountManager>.instance.ServerHost;
					battleLoginReq.IsEclipse = true;
					battleLoginReq.IsEclipseContinuousBattle = extraOption.IsContinousBattle;
					battleLoginReq.GameSpeedRatio = extraOption.OnlineGameSpeedRatio;
					battleLoginReq.ExtraData = new BattleInstanceExtraData
					{
						AffixIndices = request.AffixIndices
					};
					battleLoginReq.EnableTraceLogToFile = ClientValueManager.IsEnableTraceLogToFile;
					WaitingPopup.Open(this);
					GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector.Request(battleLoginReq, delegate(BattleLoginRes res)
					{
						WaitingPopup.Close(this);
						if (res == null || res.Result != BattleLoginRes.ResultType.Success)
						{
							if (NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled())
							{
								NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.OnClickStop(sendRoomRepeat: true);
							}
							VespaDebug.LogError("Failed to BattleLogin : " + ((res != null) ? res.Result.ToString() : "null response"));
							MessagePopup.OpenError(response.Result);
							MoveSceneToLobby(null, EBoolCheck.DontCare);
						}
						else
						{
							MoveToBattleScene();
							SetCampaign(request, response);
							GlobalAutoBehaviour<AccountManager>.instance.ApplyBeginCampaignResponse(response);
							LobbyScene2.ListReturnState.Clear();
							LobbyScene2.ListReturnState.Add(new EclipseReturnState());
						}
					});
				}
			}
		});
	}

	public void RequestBeginContent(NShared.BeginContent.Request request, long[] accountIds, Action onSuccessCallback, Action onErrorCallback)
	{
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.BeginContent.Response response)
		{
			if (response == null)
			{
				Vespa_System_Action_Extension_Method.fire(onErrorCallback);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				Vespa_System_Action_Extension_Method.fire(onErrorCallback);
			}
			else
			{
				if (response.Result == NShared.BeginContent.ResultType.Success)
				{
					switch (request.DungeonType)
					{
					case EPunishmentDungeonType.Boss:
						SetPunishmentRaidContent(request, response, accountIds);
						break;
					case EPunishmentDungeonType.Karma:
						SetPunishmentRaidContent(request, response, accountIds);
						break;
					default:
						Vespa_System_Action_Extension_Method.fire(onErrorCallback);
						return;
					}
					GlobalAutoBehaviour<AccountManager>.instance.ApplyBeginContentResponse(response);
					Vespa_System_Action_Extension_Method.fire(onSuccessCallback);
					MoveToBattleScene();
					{
						foreach (IBattleContextChangeListner changeListner in _changeListners)
						{
							changeListner.OnBattleContextChange();
						}
						return;
					}
				}
				if (!NGame2.NUI.NWindow.NPunish.Helper.IsOpenPunishRaid(GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.Get(request.GroupIndex, request.DungeonType)))
				{
					MessagePopup.OpenError(response.Result, delegate
					{
						SceneAutoBehaviour<PunishManagment>.instance.CloseByExpireDungeon();
					});
					Vespa_System_Action_Extension_Method.fire(onErrorCallback);
				}
				else
				{
					MessagePopup.OpenError(response.Result);
					Vespa_System_Action_Extension_Method.fire(onErrorCallback);
				}
			}
		});
	}
}
