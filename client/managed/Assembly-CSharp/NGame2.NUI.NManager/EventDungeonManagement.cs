using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NDispatchBattle;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NManager.NCommonImpl;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.BeginCampaign;
using NShared.GetKingsPassAllRewards;
using NShared.GetPhaseStepInfo;
using NShared.NLocalization;
using NShared.NWorldMap.NEventSubData;
using NShared.RecvRewardPhaseStep;
using NShared.ResetEventDungeon;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NManager;

public class EventDungeonManagement : BaseManagement<EventDungeonManagement>, EventDungeonSelect.Listener, EventDungeonList.Listener, PartyBattleDungeonInfo.Listener, BattlePartySetting.Listener, PhaseStepRewardInfoPopup.Listener
{
	private float _battleRequestedTime;

	private EventDungeonData _eventDungeonData;

	private EventDungeonGroupData _eventDungeonGroupData;

	public int EventChapterIndex;

	public int EventDungeonIndex;

	private static bool skipDungeonList;

	private static int[] eventDungeonGroupIndices;

	public MemoryFragmentData memoryfragmentData;

	public PhaseStepRewardInfoPopup.PhaseRewardStepInfo PhaseStepInfo { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		UseBackgroundBlack(use: true);
		AddInstance<EventDungeonSelect>("Resources/Prefab2/Window/EventDungeon/EventDungeonSelect");
		AddInstance<EventDungeonList>("Resources/Prefab2/Window/EventDungeon/EventDungeonList");
		AddInstance<PartyBattleDungeonInfo>("Resources/Prefab2/Window/Dungeon/PartyBattleDungeonInfo");
		AddInstance<BattlePartySetting>("Resources/Prefab2/Window/BattlePartySetting/BattlePartySetting");
		AddInstance<BattleDungeonInfo>("Resources/Prefab2/Window/Dungeon/BattleDungeonInfo");
	}

	public void OpenEventDungeonSelect(int chapterindex, params int[] eventDungeonGroupIndices)
	{
		EventDungeonManagement.eventDungeonGroupIndices = eventDungeonGroupIndices;
		GetInstance<EventDungeonSelect>().Open(this, chapterindex, eventDungeonGroupIndices);
	}

	public void SetSkipDungeonList(bool isSkip)
	{
		skipDungeonList = isSkip;
	}

	public void OpenEventDungeonList(int chapterIndex, int[] dungeonIndices = null, EventDungeonGroupData eventDungeonGroupData = null)
	{
		if (!skipDungeonList)
		{
			_eventDungeonGroupData = eventDungeonGroupData;
			GetInstance<EventDungeonList>().Open(this, chapterIndex, dungeonIndices);
		}
	}

	public void OpenEventDungeonList(EventDungeonGroupData eventDungeonGroupData, Action closedCallback = null)
	{
		if (!skipDungeonList)
		{
			_eventDungeonGroupData = eventDungeonGroupData;
			GetInstance<EventDungeonList>().Open(this, eventDungeonGroupData.ChapterIndex, eventDungeonGroupData.DungeonIndices);
			GetInstance<EventDungeonList>().SetClosedCallback(closedCallback);
		}
	}

	public void ReturnEventDungeonAndOpenBattlePartySetting(EnterDungeonData enterDungeon, EventDungeonGroupData enterGroup)
	{
		if (skipDungeonList || enterGroup.GroupType == EDungeonGroupType.Secret)
		{
			((PartyBattleDungeonInfo.Listener)this).OnReadyBattle(enterDungeon);
			return;
		}
		AddTask(base.TaskFactory.Sequence(new WaitTask(() => !IsOpened<EventDungeonList>()), base.TaskFactory.Branch(base.TaskFactory.IsOpen<EventDungeonList>(), base.TaskFactory.Run(delegate
		{
			((PartyBattleDungeonInfo.Listener)this).OnReadyBattle(enterDungeon);
		}))));
	}

	void EventDungeonSelect.Listener.OnClickEnterEventDungeonGroup(EventDungeonSelect caller, EventDungeonGroupData eventDungeonGroupData)
	{
		OpenEventDungeonList(eventDungeonGroupData);
	}

	void EventDungeonSelect.Listener.OnClickEnterBattleDungeonInfo(EventDungeonSelect caller, EventDungeonGroupData dungeonGroupData)
	{
		_eventDungeonGroupData = dungeonGroupData;
		_eventDungeonData = GameRoot.Instance.EventDungeonDataContainer.GetData(dungeonGroupData.ChapterIndex, dungeonGroupData.DungeonIndices[0]);
		EnterDungeonData enterDungeonData = new EnterDungeonData(dungeonGroupData.ChapterIndex, dungeonGroupData.DungeonIndices[0], dungeonGroupData.DungeonIndices[0]);
		OpenBattleDungeonInfo(enterDungeonData);
	}

	void EventDungeonList.Listener.OnClickEnterEventDungeon(EventDungeonList caller, EventDungeonData eventDungeonData)
	{
		_eventDungeonData = eventDungeonData;
		int chapterIndex = _eventDungeonData.ChapterIndex;
		int dungeonIndex = _eventDungeonData.DungeonIndex;
		int linkedDungeon = dungeonIndex;
		EnterDungeonData enterDungeonData = new EnterDungeonData(chapterIndex, dungeonIndex, linkedDungeon);
		EventChapterIndex = _eventDungeonData.ChapterIndex;
		EventDungeonIndex = _eventDungeonData.DungeonIndex;
		if (eventDungeonData.PrefabName == "PartyBattleDungeonInfo")
		{
			GetInstance<PartyBattleDungeonInfo>().Open(enterDungeonData, this);
		}
		else if (eventDungeonData.PrefabName == "BattleDungeonInfo")
		{
			OpenBattleDungeonInfo(enterDungeonData);
		}
		else
		{
			GetInstance<PartyBattleDungeonInfo>().Open(enterDungeonData, this);
		}
	}

	public void OpenBattleDungeonInfo(EnterDungeonData enterDungeonData)
	{
		_ = SceneAutoBehaviour<LobbyCampaignDungeon>.instance;
		NShared.NWorldMap.NEventSubData.EventDungeon eventDungeonData = null;
		int dungeonIndex = enterDungeonData.OriginalDungeon;
		int worldMapEventIndex = 0;
		SingletonAuto<BattleContext>.instance.GetWorldMapEventDungeonInfo(enterDungeonData.Chapter, ref dungeonIndex, out worldMapEventIndex, out eventDungeonData);
		BattleDungeonInfoListener battleDungeonInfoListener = new BattleDungeonInfoListener();
		battleDungeonInfoListener.EnterDungeon = enterDungeonData;
		battleDungeonInfoListener.OnResetEventDungeon = delegate
		{
			OpenConfirmResetEventDungeon(GetInstance<BattleDungeonInfo>().SetupEventComponents);
		};
		battleDungeonInfoListener.OnReadyBattle = OnReadyBattle;
		battleDungeonInfoListener.DungeonRemainTimer = new EventDungeonRemainTimer(null);
		GetInstance<BattleDungeonInfo>().Open(enterDungeonData, battleDungeonInfoListener);
	}

	public void OpenPartySetting(EnterDungeonData enterDungeon)
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.IsMaxEquipItemCount())
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string message = LocalizationManager.GetString(NShared.BeginCampaign.ResultType.EquipItemFull);
			MessagePopup.OpenEnabled(title, message, delegate
			{
				SceneAutoBehaviour<InventoryManagement>.instance.OpenInventory();
			}, null, "MOVE_BAG", bYesEnabled: true);
		}
	}

	private DefaultBattlePartySettingDataProdiver GetDataProdiver(EnterDungeonData enterDungeon)
	{
		int chapter = enterDungeon.Chapter;
		int originalDungeon = enterDungeon.OriginalDungeon;
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapter, originalDungeon);
		_ = data.ReqStaminaType;
		int staminaOfDifficulty = data.GetStaminaOfDifficulty(GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina(), enterDungeon.Difficulty);
		long staminaCount = staminaOfDifficulty;
		return new DefaultBattlePartySettingDataProdiver(data.LocalStorageKey, data.ReqStaminaType, staminaCount, Battle.START_BATTLE, enterDungeon, GetBanRuleData(chapter, originalDungeon));
	}

	private BanRuleData GetBanRuleData(int chapterInex, int dungeonIndex)
	{
		if (_eventDungeonData == null)
		{
			EventDungeonData data = GameRoot.Instance.EventDungeonDataContainer.GetData(chapterInex, dungeonIndex);
			if (data == null)
			{
				return null;
			}
			_eventDungeonData = data;
		}
		return GameRoot.Instance.BanRuleDataContainer.GetData(_eventDungeonData.BanIndex);
	}

	private void OnReadyBattle(EnterDungeonData enterDungeon)
	{
		DefaultBattlePartySettingDataProdiver dataProdiver = GetDataProdiver(enterDungeon);
		GetInstance<BattlePartySetting>().Open(this, dataProdiver);
	}

	void PartyBattleDungeonInfo.Listener.OnReadyBattle(EnterDungeonData enterDungeon)
	{
		OnReadyBattle(enterDungeon);
	}

	void PartyBattleDungeonInfo.Listener.OnMoveEnter(EnterDungeonData enterDungeon)
	{
	}

	void PartyBattleDungeonInfo.Listener.ReOpen()
	{
	}

	void PartyBattleDungeonInfo.Listener.ViewRewardInfo(EnterDungeonData enterDungeon)
	{
	}

	void PartyBattleDungeonInfo.Listener.ViewBossInfo(HeroInfo heroInfo)
	{
	}

	void PartyBattleDungeonInfo.Listener.OnResetEventDungeon()
	{
		OpenConfirmResetEventDungeon(delegate
		{
			GetInstance<PartyBattleDungeonInfo>().SetupEventComponents();
		});
	}

	void PartyBattleDungeonInfo.Listener.OnClickPartyPlayPopup(EnterDungeonData enterDungeon)
	{
	}

	public static void OpenConfirmResetEventDungeon(Action onConfirm, int chapter = 0, int dungeon = 0)
	{
		if (chapter == 0)
		{
			chapter = SceneAutoBehaviour<EventDungeonManagement>.instance._eventDungeonData.ChapterIndex;
		}
		if (dungeon == 0)
		{
			dungeon = SceneAutoBehaviour<EventDungeonManagement>.instance._eventDungeonData.DungeonIndex;
		}
		EventDungeonData data = GameRoot.Instance.EventDungeonDataContainer.GetData(chapter, dungeon);
		ChapterDungeonInfo chapterDungeonInfo = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.Get(chapter, dungeon);
		if (chapterDungeonInfo == null)
		{
			return;
		}
		int resetCount = chapterDungeonInfo.ResetCount;
		int num = data.MaxResetCount;
		if (num == 0)
		{
			num = 7;
		}
		int num2 = ((resetCount < num) ? resetCount : num);
		int[] resetAmount = data.ResetAmount;
		int reqAmount = 0;
		if (__Extension_Method_ICollection__.COUNT(resetAmount) > num2)
		{
			reqAmount = resetAmount[num2];
		}
		else
		{
			reqAmount = Vespa_Generic_IList_Extension_Method.GetLast(resetAmount);
		}
		if (reqAmount == 0)
		{
			VespaDebug.LogError("ResetEventDungeon RechargeGem is 0");
			return;
		}
		if (NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsValidInstance && NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled())
		{
			NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.StopRepeat();
		}
		ShopCostType costType = ShopCostType.EventDungeonPoint;
		CurrencyType o_value = CurrencyType.EventDungeonPoint;
		if (EnumTools.TryParse<ShopCostType>(data.ResetCurrencyType.ToString(), out costType))
		{
			EnumTools.TryParse<CurrencyType>(data.ResetCurrencyType.ToString(), out o_value);
		}
		string title = LocalizationManager.GetString(NShared.NLocalization.EventDungeon.CHALLENGE_RESET_TITLE);
		string message = LocalizationManager.GetString(NShared.NLocalization.EventDungeon.EVENTDUNGEON_RESET_MESSAGE);
		string confirm = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_INIT);
		Action onYes = delegate
		{
			NShared.ResetEventDungeon.Request request = new NShared.ResetEventDungeon.Request
			{
				ChapterIndex = chapter,
				DungeonIndex = dungeon
			};
			SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ResetEventDungeon.Response response)
			{
				if (response != null && response.BaseResult == BaseResultType.Success)
				{
					if (response.Result == NShared.ResetEventDungeon.ResultType.NotEnoughGem)
					{
						title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
						message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM);
						MessagePopup.Open(title, message, delegate
						{
							SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
						}, null);
					}
					else if (response.Result != NShared.ResetEventDungeon.ResultType.Success)
					{
						MessagePopup.OpenError(response.Result);
					}
					else
					{
						GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
						GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.EventDungeonPointResult);
						GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.EventDungeonPoint2Result);
						GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.Apply(response.DungeonInfo);
						if (SceneAutoBehaviour<EndCampaign>.isValidInstance)
						{
							SceneAutoBehaviour<EndCampaign>.instance.ProcessBattleRepeat();
							SceneAutoBehaviour<EndCampaign>.instance.RefreshRewardUI();
						}
						Vespa_System_Action_Extension_Method.fire(onConfirm);
					}
				}
			});
		};
		CurrencyType currencyType = o_value;
		int num3 = reqAmount;
		Action obj = delegate
		{
			BuyConfirmPopup.Open(title, message, confirm, costType, reqAmount, onYes);
		};
		Action skipAction = onYes;
		Action openAction = obj;
		int num4 = num3;
		UIHelper.BuyConfirmPopupOpen(currencyType, num4, openAction, skipAction);
	}

	void BattlePartySetting.Listener.StartBattle(EnterDungeonData enterDungeon, BattlePartySetting.BattlePartyInfo battlePartyInfo, BattlePartySetting.ExtraOptionData extraOption)
	{
		if (!(Time.time - _battleRequestedTime < 10f))
		{
			RequestBegin(enterDungeon, battlePartyInfo, extraOption);
		}
	}

	private void RequestBegin(EnterDungeonData enterDungeon, BattlePartySetting.BattlePartyInfo battlePartyInfo, BattlePartySetting.ExtraOptionData extraOption)
	{
		NShared.BeginCampaign.Request obj = new NShared.BeginCampaign.Request
		{
			ChapterIndex = enterDungeon.Chapter,
			DungeonIndex = enterDungeon.OriginalDungeon,
			DungeonDifficulty = (int)enterDungeon.Difficulty,
			ScenarioDungeon = enterDungeon.IsScenario,
			HeroIndices = battlePartyInfo.heroIndices,
			LeaderHeroIndex = battlePartyInfo.leaderHeroIndex,
			AccountIds = battlePartyInfo.accountIds
		};
		int onetimeBoosterCount = battlePartyInfo.OnetimeBoosterCount;
		obj.ItemBoosterCount = onetimeBoosterCount;
		obj.AffixIndices = battlePartyInfo.AffixIndices;
		NShared.BeginCampaign.Request request = obj;
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(battlePartyInfo.subHeroIndices))
		{
			request.AiHeroIndices = battlePartyInfo.subHeroIndices;
		}
		if (enterDungeon.IsScenarioReplay)
		{
			SingletonAuto<BattleContext>.instance.BeginScenarioReplay(request, delegate
			{
				AddLobbyReturnState(enterDungeon);
			});
			return;
		}
		SingletonAuto<BattleContext>.instance.BeginCampaign(request, delegate
		{
			if (extraOption != null && extraOption.AutoRepeat)
			{
				NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.OpenGlobal();
			}
			if (IsPhaseStepBattle(enterDungeon))
			{
				Action<Dictionary<BaseEntryMenuReturnState.SaveKey, object>> action = null;
				action = delegate(Dictionary<BaseEntryMenuReturnState.SaveKey, object> dic)
				{
					EventDungeonManagement dungeonManagement = SceneAutoBehaviour<EventDungeonManagement>.instance;
					dungeonManagement.InitPhaseStepInfo(enterDungeon.Chapter, enterDungeon.OriginalDungeon, delegate
					{
						object value = null;
						BaseRewardPopup.PopupOpenData popupOpenData = null;
						int num = 0;
						if (!dic.TryGetValue(BaseEntryMenuReturnState.SaveKey.RewardData, out value))
						{
							VespaDebug.LogError("RewardData is not exist.");
						}
						else
						{
							popupOpenData = value as BaseRewardPopup.PopupOpenData;
							if (!dic.TryGetValue(BaseEntryMenuReturnState.SaveKey.LeftTeamKillCount, out value))
							{
								VespaDebug.LogError("LeftTeamKillCount is not exist.");
							}
							else if (popupOpenData == null || !(value is int num2))
							{
								VespaDebug.LogError("Data is invalid.");
							}
							else
							{
								string stringDirect = LocalizationManager.GetStringDirect("21THANKSGIVING_EVENT_CLEARED_POPUP_MESSAGE", num2, dungeonManagement.PhaseStepInfo.DailyAchievementStep);
								BaseRewardPopup.OpenPopup(popupOpenData, isGlobal: false).ActiveLabel(stringDirect);
							}
						}
					});
				};
				SingletonAuto<CampaignContext>.instance.IsSkipRewardPopup = true;
				List<BaseEntryMenuReturnState.SaveKey> saveKeys = new List<BaseEntryMenuReturnState.SaveKey>(2)
				{
					BaseEntryMenuReturnState.SaveKey.RewardData,
					BaseEntryMenuReturnState.SaveKey.LeftTeamKillCount
				};
				AddLobbyReturnState(enterDungeon, saveKeys, action);
			}
			else
			{
				AddLobbyReturnState(enterDungeon);
			}
		});
	}

	private void AddLobbyReturnState(EnterDungeonData enterDungeon)
	{
		int currentMemoryFragmentIndex = SceneAutoBehaviour<MemoryFragmentManagement>.instance.CurrentMemoryFragmentIndex;
		EventDungeonReturnState eventDungeonReturnState = null;
		if (currentMemoryFragmentIndex != -1)
		{
			MemoryFragmentData data = GameRoot.Instance.MemoryFragmentDataContainer.GetData(currentMemoryFragmentIndex);
			eventDungeonReturnState = new HeroDungeonReturnState(enterDungeon, _eventDungeonGroupData, eventDungeonGroupIndices, data);
		}
		else
		{
			eventDungeonReturnState = new EventDungeonReturnState(enterDungeon, _eventDungeonGroupData, eventDungeonGroupIndices);
		}
		LobbyScene2.ListReturnState.Add(eventDungeonReturnState);
	}

	private void AddLobbyReturnState(EnterDungeonData enterDungeon, List<BaseEntryMenuReturnState.SaveKey> saveKeys, Action<Dictionary<BaseEntryMenuReturnState.SaveKey, object>> onComplete)
	{
		int currentMemoryFragmentIndex = SceneAutoBehaviour<MemoryFragmentManagement>.instance.CurrentMemoryFragmentIndex;
		EventDungeonReturnState eventDungeonReturnState = null;
		if (currentMemoryFragmentIndex != -1)
		{
			MemoryFragmentData data = GameRoot.Instance.MemoryFragmentDataContainer.GetData(currentMemoryFragmentIndex);
			eventDungeonReturnState = new HeroDungeonReturnState(enterDungeon, _eventDungeonGroupData, eventDungeonGroupIndices, data);
		}
		else
		{
			eventDungeonReturnState = new EventDungeonReturnState(enterDungeon, _eventDungeonGroupData, eventDungeonGroupIndices, saveKeys, onComplete);
		}
		LobbyScene2.ListReturnState.Add(eventDungeonReturnState);
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

	void PartyBattleDungeonInfo.Listener.OnClickDispatch(EnterDungeonData enterDungeon)
	{
		if (enterDungeon != null)
		{
			ContentUIDispatchBattle.OpenGlobalRaid(GameRoot.Instance.RaidDataContainer.GetRaidDataByDungeon(enterDungeon.Chapter, enterDungeon.OriginalDungeon));
		}
	}

	private bool IsPhaseStepBattle(EnterDungeonData enterDungeon)
	{
		int chapter = enterDungeon.Chapter;
		int originalDungeon = enterDungeon.OriginalDungeon;
		return GameRoot.Instance.PhaseStepRewardDataContainer.GetPhaseStepDataExist(chapter, originalDungeon);
	}

	public void InitPhaseStepInfo(int chapterIndex, int dungeonIndex, Action onSuccess)
	{
		NShared.GetPhaseStepInfo.Request request = new NShared.GetPhaseStepInfo.Request
		{
			ChapterIndex = chapterIndex,
			DungeonIndex = dungeonIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetPhaseStepInfo.Response response)
		{
			if (response != null && response.BaseResult == BaseResultType.Success)
			{
				if (response.Result != NShared.GetPhaseStepInfo.ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
				}
				else
				{
					if (PhaseStepInfo == null)
					{
						PhaseStepInfo = new PhaseStepRewardInfoPopup.PhaseRewardStepInfo();
					}
					PhaseStepInfo.DailyAchievementStep = response.DailyAchievementStep;
					PhaseStepInfo.GeneralAchievementStep = response.TotalAchievementStep;
					PhaseStepInfo.TotalCount = response.TotalCount;
					Vespa_System_Action_Extension_Method.fire(onSuccess);
				}
			}
		});
	}

	public void OpenPhaseStepRewardInfoPopup(int chapterIndex, int dungeonIndex)
	{
		if (GetInstance<PhaseStepRewardInfoPopup>() == null)
		{
			AddInstance<PhaseStepRewardInfoPopup>("Resources/Prefab2/Window/PhaseStepReward/PhaseStepRewardInfoPopup");
		}
		InitPhaseStepInfo(chapterIndex, dungeonIndex, delegate
		{
			GetInstance<PhaseStepRewardInfoPopup>().Open(this, PhaseStepInfo, chapterIndex, dungeonIndex);
		});
	}

	void PhaseStepRewardInfoPopup.Listener.OnClickGetReward(int chapterIndex, int dungeonIndex, string type)
	{
		PhaseStepGetReward(chapterIndex, dungeonIndex, type);
	}

	private void PhaseStepGetReward(int chapterIndex, int dungeonIndex, string type)
	{
		NShared.RecvRewardPhaseStep.Request request = new NShared.RecvRewardPhaseStep.Request
		{
			ChapterIndex = chapterIndex,
			DungeonIndex = dungeonIndex,
			Type = type
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.RecvRewardPhaseStep.Response response)
		{
			if (response != null && response.BaseResult == BaseResultType.Success)
			{
				if (response.Result == NShared.RecvRewardPhaseStep.ResultType.InvalidStep || response.Result == NShared.RecvRewardPhaseStep.ResultType.PhaseStepDataNotFound)
				{
					MessagePopup.OpenError(NShared.GetKingsPassAllRewards.ResultType.NoCompletedMissions);
				}
				else if (response.Result != NShared.RecvRewardPhaseStep.ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
				}
				else
				{
					PhaseStepRewardInfoPopup phaseStepRewardInfoPopup = GetInstance<PhaseStepRewardInfoPopup>();
					if (!(phaseStepRewardInfoPopup == null))
					{
						GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
						GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
						GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.EquipItemResults);
						GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResultInfos);
						switch ((PhaseStepRewardType)Enum.Parse(typeof(PhaseStepRewardType), type))
						{
						default:
							return;
						case PhaseStepRewardType.Daily:
							PhaseStepInfo.DailyAchievementStep = response.AchievementStep;
							break;
						case PhaseStepRewardType.General:
							PhaseStepInfo.GeneralAchievementStep = response.AchievementStep;
							break;
						}
						phaseStepRewardInfoPopup.Refresh();
						BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetCurrencyResultInfos3(response.CurrencyResults).SetItemResultInfos(response.ItemResults).SetEquipResultInfos(response.EquipItemResults)
							.SetStaminaResultInfos(response.StaminaResultInfos), isGlobal: false);
					}
				}
			}
		});
	}
}
