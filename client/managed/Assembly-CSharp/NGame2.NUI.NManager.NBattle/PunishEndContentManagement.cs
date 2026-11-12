using System;
using System.Collections.Generic;
using System.Linq;
using MiniJSON;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NBattleField;
using NGame2.NBattleField.NManager.NPlay;
using NGame2.NBattleView;
using NGame2.NEventTrigger;
using NGame2.NNodeTree;
using NGame2.NQuest;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUtil;
using NGame2.NUtil.NModifyChecker;
using NGameAsset;
using NShared;
using NShared.BeginCampaign;
using NShared.EndContent;
using NShared.NEvent;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NScene;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NManager.NBattle;

public class PunishEndContentManagement : BaseManagement<PunishEndContentManagement>, EndBattleWin.Listener, EndBattleLose.Listener, BattleLoseGuidePopup.Listener, EndBattleReward.Listener
{
	private LocalStorageString _testNewRaidIndex = new LocalStorageString("1", typeof(EndCampaign).FullName + "_testNewRaidIndex");

	private LocalStorageString _testNewRaidLevel = new LocalStorageString("1", typeof(EndCampaign).FullName + "_testNewRaidLevel");

	private bool _win;

	public EndBattleReward.StockData _rewardStock = new EndBattleReward.StockData();

	private EndCampaignProxy _endCampaignProxy;

	private ISound _expSound;

	protected override void Awake()
	{
		base.Awake();
		AddInstance<EndBattleWin>("Resources/Prefab2/Window/Battle/EndBattleWin");
		AddInstance<EndBattleLose>("Resources/Prefab2/Window/Battle/EndBattleLose");
		AddInstance<EndBattleReward>("Resources/Prefab2/Window/Battle/EndBattleReward");
		AddInstance<RewardBackground>("Resources/Prefab2/Window/Battle/RewardBackground", GameUIRoot.ELayerType.BottomUI);
		AddInstance<BattleStats>("Resources/Prefab2/Window/Battle/BattleStats");
		AddInstance<CampaignRewardPopup>("Resources/Prefab2/Window/Battle/CampaignRewardPopup");
		AddInstance<BattleLoseGuidePopup>("Resources/Prefab2/Window/MessagePopup/BattleLoseGuidePopup");
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			base.OnDestroy();
		}
	}

	public void OpenbyEndContent(int[] heroIndices, int[] aiHeroIndices, int[] groupHeroIndices, long[] accountIds, bool win, int star, Dictionary<int, int> killMonsterCount, IList<ResultCreatureInfo> creatureInfos, IList<int> aliveHeroIndices, int pureBattleTime, long elapsedBattleTime, string battleLogString = null)
	{
		_win = win;
		SingletonAuto<BattleContext>.instance.SetLastDungeonResult((!_win) ? EBoolCheck.False : EBoolCheck.True);
		_rewardStock.HeroIndices = heroIndices;
		_rewardStock.AiHeroIndices = aiHeroIndices;
		_rewardStock.GroupHeroInices = groupHeroIndices;
		_rewardStock.AccountIds = accountIds;
		_rewardStock.OldLevelInfo = Helper.GetAccountTeamLevelInfo();
		_rewardStock.EnterDungeonData = SingletonAuto<PunishmentRaidContext>.instance.EnterDungeon;
		_endCampaignProxy = null;
		NShared.EndContent.Request endContentRequest = GetEndContentRequest(win, star, killMonsterCount, creatureInfos, aliveHeroIndices, pureBattleTime, elapsedBattleTime, battleLogString);
		PlayBattleResultSoundAndBgm(win ? "UI.Campaign.Victory" : "UI.Campaign.Lose", win ? "UI.Campaign.VictoryBGM" : "UI.Campaign.LoseBGM");
		SingletonAuto<BattleContext>.instance.SetBattleResult(win, star);
		ExecuteEndContentRequest(endContentRequest);
		SceneAutoBehaviour<NVespa.NScene.GuiManager>.instance.Add("EndContent", this, delegate
		{
			using (new BlockManager().Horizontal())
			{
				GUILayout.Label("Raid Index");
				_testNewRaidIndex.Value = GUILayout.TextField(_testNewRaidIndex.Value);
			}
			using (new BlockManager().Horizontal())
			{
				GUILayout.Label("Raid Level");
				_testNewRaidLevel.Value = GUILayout.TextField(_testNewRaidLevel.Value);
			}
		});
	}

	private void PlayBattleResultSoundAndBgm(string SoundCode, string BgmCode)
	{
		SoundStock stock = new SoundStock
		{
			Code = SoundCode,
			LocalTimer = GlobalTimer.DefaultLocal
		};
		SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 60f);
		SingletonAuto<BgmManager2>.instance.Request(BgmCode, isEvent: false);
	}

	public void ProcessBattleRepeat()
	{
		BattlePartySetting.ExtraOptionData extraOptionData = new BattlePartySetting.ExtraOptionData();
		extraOptionData.AutoRepeat = false;
		extraOptionData.IsEndCampaign = true;
		BattleRepeat(extraOptionData);
	}

	private void ReservePopupWindowOpen<T>(Action<T> action) where T : BaseWindow
	{
		T val = GetInstance<T>();
		if (val == null)
		{
			VespaDebug.LogError("Cannot find window : " + typeof(T).Name);
		}
		else
		{
			GetInstance<EndBattleReward>().SetButtonEnabledAction(val, action);
		}
	}

	private void ReserveTeamLevelupPopup()
	{
		if (_endCampaignProxy != null && _endCampaignProxy.ExpResult != null && _rewardStock.OldLevelInfo.Level < _endCampaignProxy.ExpResult.NewLevel)
		{
			TeamLevelupPopup window = TeamLevelupPopup.instance;
			GetInstance<EndBattleReward>().SetButtonEnabledAction((IWindow)window, (Action)delegate
			{
				TeamLevelupPopup.Open(_rewardStock.OldLevelInfo.Level, _endCampaignProxy.ExpResult.NewLevel);
			});
		}
	}

	private void OpenCampaignRewardPopup()
	{
		ReservePopupWindowOpen(delegate(CampaignRewardPopup window)
		{
			List<ItemResultInfo> list = new List<ItemResultInfo>();
			List<EquipItemInfo> list2 = new List<EquipItemInfo>();
			if (__Extension_Method_ICollection__.COUNT(_endCampaignProxy.ItemResults) > 0)
			{
				list.AddRange(_endCampaignProxy.ItemResults);
			}
			if (__Extension_Method_ICollection__.COUNT(_endCampaignProxy.EquipItemInfos) > 0)
			{
				list2.AddRange(_endCampaignProxy.EquipItemInfos);
			}
			BaseRewardPopup.PopupOpenData popupOpen = new BaseRewardPopup.PopupOpenData().SetItemResultInfos(list.ToArray()).SetEquipResultInfos(list2.ToArray()).SetBoostEquipResultInfos(_endCampaignProxy.EquipItemBoostInfos)
				.AddCurrencyResultInfos3(_endCampaignProxy.CurrencyResultInfos3)
				.AddCurrencyResultInfos3(_endCampaignProxy.GuildCurrencyResults)
				.SetSelectedRewardItemCodes(_endCampaignProxy.SelectedRewardItemCodes);
			window.OpenWithData(popupOpen, isGlobal: false, delegate
			{
				AppReviewPopupOpen();
			});
		});
	}

	private void OpenRewardPopup()
	{
		CheckPortalOpenEffect();
		OpenCampaignRewardPopup();
		SingletonUpdater<QuestManager>.instance.OpenClearMainQuestConfirmNotiPopup();
	}

	private void OpenPurchaseMarkteingPopup()
	{
		GetInstance<EndBattleReward>().ReservePurchaseMarketingPopup();
	}

	private void OpenEndBattleReward()
	{
		BattleViewGenericSingleton<CreatureManager>.instance.ReleaseAllCreatures();
		SceneAutoBehaviour<RuntimeGlobalManager>.instance.ClearAll();
		SceneAutoBehaviour<BattleManagement>.instance.CloseBut();
		GetInstance<RewardBackground>().Open();
		GetInstance<EndBattleReward>().Open(this, _rewardStock, _endCampaignProxy);
		if (_endCampaignProxy != null)
		{
			ReserveTeamLevelupPopup();
			OpenRewardPopup();
			OpenPurchaseMarkteingPopup();
		}
	}

	public void RefreshRewardUI()
	{
		GetInstance<EndBattleReward>().SetupResourceUi();
	}

	public void ReturnToLobby()
	{
		SingletonAuto<BattleContext>.instance.MoveSceneToLobby(null, (!_win) ? EBoolCheck.False : EBoolCheck.True);
	}

	public void ExecReturnToLobbyAndOpenHeroDeck()
	{
		ExecReturnToLobbyAndDoAction(LobbyScene2.ReturnStateOpenFlag.OpenHeroDeck);
	}

	public void ExecReturnToLobbyAndOpenPartySetting()
	{
		ExecReturnToLobbyAndDoAction(LobbyScene2.ReturnStateOpenFlag.OpenPartySetting);
	}

	public void ExecReturnToLobbyAndOpenInventory()
	{
		ExecReturnToLobbyAndDoAction(LobbyScene2.ReturnStateOpenFlag.OpenInventory);
	}

	public void ExecReturnToLobbyAndOpenGameGuide()
	{
		ExecReturnToLobbyAndDoAction(LobbyScene2.ReturnStateOpenFlag.OpenGameGuide);
	}

	public void ExecReturnToLobbyAndOpenCashShop()
	{
		ExecReturnToLobbyAndDoAction(LobbyScene2.ReturnStateOpenFlag.OpenCashShop);
	}

	private void ExecReturnToLobbyAndDoAction(LobbyScene2.ReturnStateOpenFlag flag)
	{
		LobbyScene2.ReturnState.OpenFlag |= flag;
		SingletonAuto<BattleContext>.instance.MoveSceneToLobby(null, (!_win) ? EBoolCheck.False : EBoolCheck.True);
	}

	private NShared.EndContent.Request GetEndContentRequest(bool win, int star, Dictionary<int, int> killMonsterCount, IList<ResultCreatureInfo> creatureInfos, IList<int> aliveHeroIndices, int pureBattleTime, long elapsedBattleTime, string battleLogString)
	{
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		if (__Extension_Method_ICollection__.COUNT(killMonsterCount) > 0)
		{
			foreach (KeyValuePair<int, int> item2 in killMonsterCount)
			{
				list.Add(item2.Key);
				list2.Add(item2.Value);
			}
		}
		_rewardStock.IsDeadHeroes = new bool[__Extension_Method_ICollection__.COUNT(_rewardStock.HeroIndices)];
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_rewardStock.IsDeadHeroes); i++)
		{
			int item = _rewardStock.HeroIndices[i];
			_rewardStock.IsDeadHeroes[i] = !aliveHeroIndices.Contains(item);
		}
		string creatureInfoString = Json.Serialize(BaseJsonMarshaler.EncodeArray(creatureInfos, JsonMarshaler.EncodeJsonObject));
		return new NShared.EndContent.Request
		{
			ChapterIndex = SingletonAuto<PunishmentRaidContext>.instance.EnterDungeon.Chapter,
			DungeonIndex = SingletonAuto<PunishmentRaidContext>.instance.EnterDungeon.LinkedDungeon,
			DungeonDifficulty = (int)SingletonAuto<PunishmentRaidContext>.instance.EnterDungeon.Difficulty,
			Completed = win,
			Star = star,
			MonsterIndices = list.ToArray(),
			KillCounts = list2.ToArray(),
			AliveHeroIndices = aliveHeroIndices.ToArray(),
			BattleLogString = battleLogString,
			CreatureInfoString = creatureInfoString,
			PureBattleTime = pureBattleTime,
			ElapsedBattleTime = elapsedBattleTime,
			GroupIndex = SingletonAuto<PunishmentRaidContext>.instance.GroupIndex,
			DungeonType = SingletonAuto<PunishmentRaidContext>.instance.PunishmentDungeonType,
			ClearWave = SingletonAuto<BattleFacade>.instance.KarmaCache.ClearWaveCount,
			EndWave = SingletonAuto<BattleFacade>.instance.KarmaCache.ClearWaveCount + 1,
			EndKillCount = SingletonAuto<BattleFacade>.instance.GetCurrentWaveKillMeter(TeamType.Left),
			TotalKillCount = SingletonAuto<BattleFacade>.instance.GetTotalKillTeamMeter(TeamType.Left)
		};
	}

	private void RequestEndContent(NShared.EndContent.Request request)
	{
		string[] values = new string[3]
		{
			request.ChapterIndex.ToString(),
			request.DungeonIndex.ToString(),
			((NShared.DungeonDifficultyType)request.DungeonDifficulty/*cast due to .constrained prefix*/).ToString()
		};
		SingletonAuto<ModifyChecker>.instance.CheckStart<FirstClearDungeon>(values);
		bool wasCompleted = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(request.ChapterIndex, request.DungeonIndex, (NShared.DungeonDifficultyType)request.DungeonDifficulty);
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.EndContent.Response response)
		{
			if ((response != null && response.BaseResult != BaseResultType.Success) || (response != null && response.Result != NShared.EndContent.ResultType.Success))
			{
				AutoBattle.Stop();
			}
			if (response != null && response.BaseResult == BaseResultType.Success)
			{
				if (response.Result == NShared.EndContent.ResultType.Success)
				{
					_endCampaignProxy = new EndCampaignProxy();
					_endCampaignProxy.OnResponse(request, response);
					if (response.CampaignResults != null)
					{
						ChapterDungeonInfo[] campaignResults = response.CampaignResults;
						foreach (ChapterDungeonInfo chapterDungeonInfo in campaignResults)
						{
							GlobalAutoBehaviour<CampaignNodeOpenEffectManager>.instance.CheckNewClearNode(chapterDungeonInfo.ChapterIndex, chapterDungeonInfo.DungeonIndex);
						}
					}
					GlobalAutoBehaviour<AccountManager>.instance.ApplyEndContentResponse(response);
					GetInstance<EndBattleWin>().OnEndCampaignResponse(_endCampaignProxy);
					if (!wasCompleted)
					{
						_ = (object)null;
						_ = 7;
						_ = 1;
						_ = 0;
						_ = new object[2] { request.ChapterIndex, request.DungeonIndex };
						GameRoot.Instance.CampaignDungeonDataContainer.IsLastDungeonInChapter(request.ChapterIndex, request.DungeonIndex);
					}
					return;
				}
				AutoBattle.Stop();
				GlobalAutoBehaviour<AccountManager>.instance.ApplyEndContentResponse(response);
			}
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.ERROR);
			string resultString = LocalizationManager.GetResultString(response);
			MessagePopup.Open(title, resultString, delegate
			{
				RequestEndContent(request);
			}, delegate
			{
				ReturnToLobby();
			});
		});
	}

	private void ExecuteEndContentRequest(NShared.EndContent.Request endContentRequest)
	{
		if (endContentRequest.Completed)
		{
			GetInstance<EndBattleWin>().Open(this, endContentRequest.Star);
		}
		else
		{
			ProcessBattleLoseInternal();
		}
		RequestEndContent(endContentRequest);
	}

	private void ProcessBattleLoseInternal()
	{
		IDataProvider dataProvider = SingletonAuto<BattleContext>.instance.DataProvider;
		EndBattleLose endBattleLose = GetInstance<EndBattleLose>();
		endBattleLose.Open(this, dataProvider);
		SingletonAuto<ConditionChecker>.instance.FailDungeonCount = SingletonAuto<ConditionChecker>.instance.FailDungeonCount + 1;
		endBattleLose.SetAnimationFinishedAction(delegate
		{
			GetInstance<BattleLoseGuidePopup>().Open(this, (dataProvider != null) ? dataProvider.GetBattleType() : BattleType.None, (dataProvider != null) ? dataProvider.GetBattleDefine() : null, null);
		});
	}

	void EndBattleWin.Listener.OnInputEnabled(EndBattleWin caller)
	{
	}

	bool EndBattleWin.Listener.OnCloseRequest(EndBattleWin closeThis)
	{
		closeThis.Close();
		OpenEndBattleReward();
		return false;
	}

	void EndBattleWin.Listener.OnClickReturnToLobby(EndBattleWin caller)
	{
		ReturnToLobby();
	}

	void EndBattleReward.Listener.BattleRepeat()
	{
		ProcessBattleRepeat();
	}

	void EndBattleReward.Listener.ReturnToLobby()
	{
		ReturnToLobby();
	}

	void EndBattleReward.Listener.ReturnToLobbyAndOpenNextDungeon()
	{
		EnterDungeonData enterDungeonData = _rewardStock.EnterDungeonData;
		int nextDungeonIndex = enterDungeonData.OriginalDungeonData.NextDungeonIndex;
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(enterDungeonData.Chapter, nextDungeonIndex);
		if (CheckNextDungeon(data))
		{
			LobbyScene2.ReturnState.OpenFlag |= LobbyScene2.ReturnStateOpenFlag.OpenNextDungeon;
			SingletonAuto<BattleContext>.instance.MoveSceneToLobby(null, (!_win) ? EBoolCheck.False : EBoolCheck.True);
		}
	}

	void EndBattleReward.Listener.ReturnToLobbyAndOpenNextChapterHideOut()
	{
	}

	void EndBattleReward.Listener.ReturnToLobbyAndOpenNextChapterConquestDungeon()
	{
	}

	private bool CheckNextDungeon(CampaignDungeonData nextDungeon)
	{
		if (nextDungeon == null)
		{
			return false;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.IsMaxEquipItemCount())
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string message = LocalizationManager.GetString(NShared.BeginCampaign.ResultType.EquipItemFull);
			MessagePopup.OpenEnabled(title, message, SceneAutoBehaviour<EndCampaign>.instance.ExecReturnToLobbyAndOpenInventory, null, "MOVE_BAG", bYesEnabled: true);
			return false;
		}
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanGoToChapter(nextDungeon.ChapterIndex);
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(nextDungeon.ChapterIndex);
		if (data == null)
		{
			return false;
		}
		if (data.Type == ChapterType.WorldMap)
		{
			flag = flag && GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanVisit(nextDungeon.ChapterIndex, nextDungeon.DungeonIndex);
		}
		if (!flag)
		{
			return false;
		}
		return true;
	}

	void EndBattleReward.Listener.ReturnToLobbyAndOpenHeroDeck()
	{
		ExecReturnToLobbyAndOpenHeroDeck();
	}

	void EndBattleReward.Listener.ReturnToLobbyAndOpenPartySetting()
	{
		ExecReturnToLobbyAndOpenPartySetting();
	}

	void EndBattleReward.Listener.ReturnToLobbyAndOpenInventory()
	{
		ExecReturnToLobbyAndOpenInventory();
	}

	void EndBattleReward.Listener.MoveToPurifyRoom(CachedHeroInfo heroInfo)
	{
		LobbyScene2.ListReturnState.Clear();
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(new OnClickReturnToMap());
		LobbyScene2.ListReturnState.Add(new HeroPurifySelectReturnState(heroInfo));
		GlobalAutoBehaviour<LoadingManager>.instance.MoveScene(LoadingManager.ESceneType.Lobby);
	}

	void EndBattleLose.Listener.BattleRepeat()
	{
		ProcessBattleRepeat();
	}

	void EndBattleLose.Listener.ReturnToLobby()
	{
		ReturnToLobby();
	}

	void EndBattleLose.Listener.ReturnToLobbyAndOpenHeroDeck()
	{
		ExecReturnToLobbyAndOpenHeroDeck();
	}

	void EndBattleLose.Listener.ReturnToLobbyAndOpenPartySetting()
	{
		ExecReturnToLobbyAndOpenPartySetting();
	}

	void EndBattleLose.Listener.ReturnToLobbyAndOpenInventory()
	{
		ExecReturnToLobbyAndOpenInventory();
	}

	void BattleLoseGuidePopup.Listener.ReturnToLobbyAndGameGuide()
	{
		ExecReturnToLobbyAndOpenGameGuide();
	}

	public void BattleRepeat(BattlePartySetting.ExtraOptionData extraOption)
	{
		if (_rewardStock != null && !CheckEnterCount(_rewardStock.EnterDungeonData))
		{
			if (!_rewardStock.EnterDungeonData.CheckIsNotInitDailyCount())
			{
				MessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), LocalizationManager.GetString(NShared.BeginCampaign.ResultType.ExceededDailyEntryCount));
			}
		}
		else
		{
			SingletonAuto<BattleContext>.instance.RepeatContent(extraOption);
		}
	}

	private bool CheckEnterCount(EnterDungeonData enterDungeonData)
	{
		if (enterDungeonData != null && enterDungeonData.LinkedDungeonData != null)
		{
			return GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CheckEnterCount(enterDungeonData);
		}
		return true;
	}

	public void ShowStatistics()
	{
		base.TaskFactory.Run(delegate(BattleStats window)
		{
			BattleInstance stoppedBattleInstance = SingletonAuto<BattleFacade>.instance.GetStoppedBattleInstance();
			window.Open(stoppedBattleInstance);
		}).Commit();
	}

	protected override void ChainUpdate()
	{
		base.ChainUpdate();
		if (_expSound != null)
		{
			_expSound.Update();
		}
	}

	private void AppReviewPopupOpen()
	{
		if (_rewardStock == null || _rewardStock.EnterDungeonData == null)
		{
			return;
		}
		int chapter = _rewardStock.EnterDungeonData.Chapter;
		int linkedDungeon = _rewardStock.EnterDungeonData.LinkedDungeon;
		NShared.DungeonDifficultyType difficulty = _rewardStock.EnterDungeonData.Difficulty;
		string[] values = new string[3]
		{
			chapter.ToString(),
			linkedDungeon.ToString(),
			difficulty.ToString()
		};
		if (SingletonAuto<ModifyChecker>.instance.CheckResult<FirstClearDungeon>(values))
		{
			string[] modifyCondition = SingletonAuto<ModifyChecker>.instance.GetModifyCondition<FirstClearDungeon>(values);
			if (GameRoot.Instance.AppReviewConditionDataContainer.IsCheck(modifyCondition))
			{
				AppReviewPopup.Open();
			}
		}
	}

	public void CheckPortalOpenEffect()
	{
		if (_rewardStock == null || _rewardStock.EnterDungeonData == null)
		{
			return;
		}
		int chapter = _rewardStock.EnterDungeonData.Chapter;
		int linkedDungeon = _rewardStock.EnterDungeonData.LinkedDungeon;
		NShared.DungeonDifficultyType difficulty = _rewardStock.EnterDungeonData.Difficulty;
		string[] values = new string[3]
		{
			chapter.ToString(),
			linkedDungeon.ToString(),
			difficulty.ToString()
		};
		if (SingletonAuto<ModifyChecker>.instance.CheckResult<FirstClearDungeon>(values))
		{
			PortalData portalData = GameRoot.Instance.PortalDataContainer.CheckPortalOpenEffectContidion(chapter, linkedDungeon);
			if (portalData != null && portalData.OpenEffectPortalIndex != 0)
			{
				PortalSelect3.SetOpenPortalEffect(portalData.OpenEffectPortalIndex, enable: true);
			}
		}
	}
}
