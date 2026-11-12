using System;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NManager.NCommonImpl;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWorldMap;
using NShared;
using NShared.BeginCampaign;
using NShared.NLocalization;
using NShared.NWorldMap.NEventSubData;
using NShared.ResetEventDungeon;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NTask;
using UnityEngine;

namespace NGame2.NUI.NManager.NLobby;

public class LobbyCampaignDungeon : BaseManagement<LobbyCampaignDungeon>, BattlePartySetting.Listener, HeroFullSceneViewBase.Listener, HeroDetailView.Listener
{
	private int _worldMapEventIndex;

	private int _worldMapNodeIndex;

	private float _battleRequestedTime;

	private LobbyScene2.IReturnState _heroTryBattleReturnState;

	private EnterDungeonData _enterDungeon;

	protected override void Awake()
	{
		base.Awake();
		AddInstance<BattleDungeonInfo>("Resources/Prefab2/Window/Dungeon/BattleDungeonInfo");
		AddInstance<BattlePartySetting>("Resources/Prefab2/Window/BattlePartySetting/BattlePartySetting");
		AddInstance<PartyBattleDungeonInfo>("Resources/Prefab2/Window/Dungeon/PartyBattleDungeonInfo");
		AddInstance<BossMonsterInfo>("Resources/Prefab2/Window/Dungeon/BossMonsterInfo");
		AddInstance<HeroDetailView>("Resources/Prefab2/Window/FullScene/HeroDetailView");
		AddInstance<PartyPlayPopup>("Resources/Prefab2/Window/PartyDungeon/PartyPlayPopup");
	}

	public void OpenDungeonInfo(int chapterIndex, int dungeonIndex, int linkedDungeonIndex, bool notCurrentNode = false)
	{
		EnterDungeonData enterDungeonData = new EnterDungeonData(chapterIndex, dungeonIndex, linkedDungeonIndex);
		CampaignDungeonData linkedDungeonData = enterDungeonData.LinkedDungeonData;
		if (linkedDungeonData == null)
		{
			return;
		}
		int worldMapEventIndex = 0;
		int worldMapNodeIndex = dungeonIndex;
		NShared.NWorldMap.NEventSubData.EventDungeon eventDungeonData = null;
		SingletonAuto<BattleContext>.instance.GetWorldMapEventDungeonInfo(chapterIndex, ref linkedDungeonIndex, out worldMapEventIndex, out eventDungeonData);
		_worldMapEventIndex = worldMapEventIndex;
		_worldMapNodeIndex = worldMapNodeIndex;
		if (linkedDungeonData.BattleType == BattleType.GuildRaid)
		{
			return;
		}
		if (linkedDungeonData.BattleType == BattleType.Party || linkedDungeonData.BattleType == BattleType.Boss || linkedDungeonData.BattleType == BattleType.SubStoryBoss)
		{
			PartyBattleDungeonInfoListener partyBattleDungeonInfoListener = new PartyBattleDungeonInfoListener
			{
				EnterDungeon = enterDungeonData,
				BossMonsterInfo = GetInstance<BossMonsterInfo>(),
				OnReadyBattle = delegate(EnterDungeonData enterDungeon)
				{
					OpenPartySetting(enterDungeon);
				},
				OnClickPartyPlayPopup = OnClickPartyPlayPopup
			};
			if (notCurrentNode)
			{
				partyBattleDungeonInfoListener.OnMoveEnter = OnMoveEnterDungeon;
			}
			GetInstance<PartyBattleDungeonInfo>().Open(enterDungeonData, partyBattleDungeonInfoListener, notCurrentNode);
			return;
		}
		BattleDungeonInfoListener battleDungeonInfoListener = new BattleDungeonInfoListener
		{
			EnterDungeon = enterDungeonData,
			BattleDungeonInfo = GetInstance<BattleDungeonInfo>(),
			OnReadyBattle = delegate(EnterDungeonData enterDungeon)
			{
				OpenPartySetting(enterDungeon);
			},
			OnResetEventDungeon = delegate
			{
				ResetEventDungeon(chapterIndex, dungeonIndex);
			},
			OnClickPartyPlay = OnClickPartyPlayPopup
		};
		if (notCurrentNode)
		{
			battleDungeonInfoListener.OnMoveEnter = OnMoveEnterDungeon;
		}
		GetInstance<BattleDungeonInfo>().Open(enterDungeonData, battleDungeonInfoListener, notCurrentNode);
	}

	private void ResetEventDungeon(int chapterIndex, int dungeonIndex)
	{
		OpenConfirmResetEventDungeon(chapterIndex, dungeonIndex, delegate
		{
			GetInstance<BattleDungeonInfo>().SetupEventComponents();
		});
	}

	public static void OpenConfirmResetEventDungeon(int chapterIndex, int dungeonIndex, Action onConfirm)
	{
		ChapterDungeonInfo chapterDungeonInfo = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.Get(chapterIndex, dungeonIndex);
		if (chapterDungeonInfo == null)
		{
			return;
		}
		int resetCount = chapterDungeonInfo.ResetCount;
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("ResetEventDungeonCountMax");
		int reqGem = GameRoot.Instance.ConstantDataContainer.GetValueInt("ResetEventDungeonCount" + ((resetCount + 1 < valueInt) ? (resetCount + 1) : valueInt));
		if (reqGem == 0)
		{
			VespaDebug.LogError("ResetEventDungeon RechargeGem is 0");
			return;
		}
		if (NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsValidInstance && NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled())
		{
			NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.StopRepeat();
		}
		string title = LocalizationManager.GetString(NShared.NLocalization.EventDungeon.CHALLENGE_RESET_TITLE);
		string message = LocalizationManager.GetString(NShared.NLocalization.EventDungeon.CHALLENGE_RESET_MESSAGE);
		string confirm = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_INIT);
		Action onYes = delegate
		{
			NShared.ResetEventDungeon.Request request = new NShared.ResetEventDungeon.Request
			{
				ChapterIndex = chapterIndex,
				DungeonIndex = dungeonIndex
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
						GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.Apply(response.DungeonInfo);
						if (SceneAutoBehaviour<EndCampaign>.isValidInstance)
						{
							SceneAutoBehaviour<EndCampaign>.instance.ProcessBattleRepeat();
							SceneAutoBehaviour<EndCampaign>.instance.RefreshRewardUI();
						}
						if (SceneAutoBehaviour<LobbyCampaignDungeon>.isValidInstance)
						{
							BattlePartySetting battlePartySetting = SceneAutoBehaviour<LobbyCampaignDungeon>.instance.GetInstance<BattlePartySetting>();
							if (battlePartySetting != null && battlePartySetting.IsOpen())
							{
								battlePartySetting.ForceRefreshStaminaCount();
							}
							MultiPartySetting multiPartySetting = SceneAutoBehaviour<MultiPartyManagement>.instance.GetInstance<MultiPartySetting>();
							if (multiPartySetting != null && multiPartySetting.IsOpen())
							{
								multiPartySetting.ForceRefreshStaminaCount();
							}
						}
						Vespa_System_Action_Extension_Method.fire(onConfirm);
					}
				}
			});
		};
		int num = reqGem;
		Action obj = delegate
		{
			BuyConfirmPopup.Open(title, message, confirm, ShopCostType.Gem, reqGem, onYes);
		};
		Action skipAction = onYes;
		Action openAction = obj;
		int num2 = num;
		UIHelper.BuyConfirmPopupOpen(CurrencyType.Gem, num2, openAction, skipAction);
	}

	public void OpenBattleDungeonInfo(int chapterIndex, int dungeonIndex)
	{
		int linkedDungeonIndex = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.GetLinkedDungeonIndex(chapterIndex, dungeonIndex);
		OpenDungeonInfo(chapterIndex, dungeonIndex, linkedDungeonIndex);
	}

	public void OpenPartySetting(EnterDungeonData enterDungeon)
	{
		_enterDungeon = enterDungeon;
		if (GlobalAutoBehaviour<AccountManager>.instance.IsMaxEquipItemCount())
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
			_battleRequestedTime = 0f;
			OpenSoloPartySettingAsync(enterDungeon).Commit();
		}
	}

	private ITask OpenSoloPartySettingAsync(EnterDungeonData enterDungeon)
	{
		return base.TaskFactory.Open(delegate(BattlePartySetting window)
		{
			DefaultBattlePartySettingDataProdiver dataProvider = GetDataProvider(enterDungeon);
			window.Init(this, dataProvider);
		});
	}

	public DefaultBattlePartySettingDataProdiver GetDataProvider(EnterDungeonData enterDungeon)
	{
		DefaultBattlePartySettingDataProdiver defaultBattlePartySettingDataProdiver = SprkStoryAuto.StoryProvider(enterDungeon);
		if (defaultBattlePartySettingDataProdiver != null)
		{
			return defaultBattlePartySettingDataProdiver;
		}
		if (enterDungeon == null)
		{
			enterDungeon = new EnterDungeonData();
		}
		CampaignDungeonData originalDungeonData = enterDungeon.OriginalDungeonData;
		StaminaType staminaType = originalDungeonData?.ReqStaminaType ?? StaminaType.Chicken;
		int staminaCount = originalDungeonData?.GetStaminaOfDifficulty(GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina(), enterDungeon.Difficulty) ?? 1;
		return new DefaultBattlePartySettingDataProdiver(originalDungeonData.LocalStorageKey, staminaType, staminaCount, Battle.START_BATTLE, enterDungeon);
	}

	public void OpenHeroDetailView(CachedHeroInfo heroInfo, LobbyScene2.IReturnState heroTryBattleReturnState = null, HeroDetailView.EToggle eToggle = HeroDetailView.EToggle.Animation)
	{
		AddTask(base.TaskFactory.Run(delegate
		{
			_heroTryBattleReturnState = heroTryBattleReturnState;
		}), base.TaskFactory.Run(delegate(HeroDetailView window)
		{
			window.Open(heroInfo, this, eToggle);
		}));
	}

	void BattlePartySetting.Listener.StartBattle(EnterDungeonData enterDungeon, BattlePartySetting.BattlePartyInfo battlePartyInfo, BattlePartySetting.ExtraOptionData extraOption)
	{
		if (_battleRequestedTime != 0f && Time.time - _battleRequestedTime < 10f)
		{
			return;
		}
		Action action = delegate
		{
			_battleRequestedTime = Time.time;
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
				Repeat = false,
				WorldMapEventIndex = _worldMapEventIndex,
				WorldMapNodeIndex = _worldMapNodeIndex
			};
			int onetimeBoosterCount = battlePartyInfo.OnetimeBoosterCount;
			obj.ItemBoosterCount = onetimeBoosterCount;
			obj.AffixIndices = battlePartyInfo.AffixIndices;
			NShared.BeginCampaign.Request request = obj;
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(battlePartyInfo.subHeroIndices))
			{
				request.AiHeroIndices = battlePartyInfo.subHeroIndices;
			}
			if (!enterDungeon.IsScenarioReplay)
			{
				RequestBeginCampaign(request, enterDungeon, extraOption);
			}
			else
			{
				RequestScenarioReplay(request, enterDungeon, extraOption);
			}
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

	private void RequestBeginCampaign(NShared.BeginCampaign.Request request, EnterDungeonData enterDungeon, BattlePartySetting.ExtraOptionData extraOption)
	{
		SprkStoryAuto.PrepareStoryRequest(request);
		NShared.BeginCampaign.Request request2 = request;
		SingletonAuto<BattleContext>.instance.BeginCampaign(request2, delegate
		{
			if (extraOption != null && extraOption.AutoRepeat)
			{
				NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.OpenGlobal();
			}
			if (extraOption != null && extraOption.AutoNext)
			{
				AutoNextBattle.OpenGlobal(SprkStoryAuto.ContinueType(enterDungeon.LinkedDungeonData.BattleType), enterDungeon.Difficulty, request2.ItemBoosterCount);
			}
			SprkStoryAuto.SaveStoryPosition(GlobalAutoBehaviour<AccountManager>.instance.CampaignManager, enterDungeon.Chapter, enterDungeon.OriginalDungeon);
			SingletonAuto<CampaignContext>.instance.SetInitialHeroInfo(enterDungeon.InitialHeroInfos);
			LobbyScene2.ListReturnState.Add(new CampaignReturnState(enterDungeon));
			LobbyScene2.ListReturnState.Add(new ChapterFlowReturnState(enterDungeon.Chapter, enterDungeon.OriginalDungeon));
		}, delegate
		{
			SprkStoryAuto.Stop();
			_battleRequestedTime = 0f;
		});
	}

	private void RequestScenarioReplay(NShared.BeginCampaign.Request request, EnterDungeonData enterDungeon, BattlePartySetting.ExtraOptionData extraOption)
	{
		SingletonAuto<BattleContext>.instance.BeginScenarioReplay(request, delegate
		{
			GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.SaveLatelyBattleNodePos(enterDungeon.Chapter, enterDungeon.OriginalDungeon);
			LobbyScene2.ListReturnState.Add(new CampaignReturnState(enterDungeon));
		});
	}

	bool BattlePartySetting.Listener.OnCloseRequest(BattlePartySetting closeThis)
	{
		BattleDungeonInfo battleDungeonInfo = GetInstance<BattleDungeonInfo>();
		if (battleDungeonInfo != null)
		{
			battleDungeonInfo.SetupEventComponents();
		}
		return true;
	}

	void BattlePartySetting.Listener.OpenHeroDetailView(CachedHeroInfo heroInfo)
	{
		OpenHeroDetailView(heroInfo);
	}

	bool BattlePartySetting.Listener.OpenAutoNextBattleSelect()
	{
		if (SprkStoryAuto.ShowChoice(_enterDungeon))
		{
			return true;
		}
		NShared.DungeonDifficultyType difficulty = GetDataProvider(_enterDungeon).GetEnterDungeon().Difficulty;
		BattleType battleType = _enterDungeon.LinkedDungeonData.BattleType;
		if (battleType == BattleType.TreasureHouse || battleType == BattleType.SubStory || battleType == BattleType.SubStoryBoss || (battleType == BattleType.Campaign && difficulty == NShared.DungeonDifficultyType.Normal) || (battleType == BattleType.Boss && difficulty == NShared.DungeonDifficultyType.Normal))
		{
			return false;
		}
		string stringDirect = LocalizationManager.GetStringDirect("TOWER_SELECT_TITLE");
		string stringDirect2 = LocalizationManager.GetStringDirect("CHAPTER_CONTINUOUS_BATTLE_DETAIL");
		string stringDirect3 = LocalizationManager.GetStringDirect("START_BATTLE");
		string stringDirect4 = LocalizationManager.GetStringDirect("TOWER_CONTINUOUS_BATTLE_BUTTON");
		string confirmAutoContent = LocalizationManager.GetStringDirect("CHAPTER_CONTINUOUS_BATTLE_MESSAGE");
		switch (battleType)
		{
		case BattleType.Campaign:
			stringDirect2 = LocalizationManager.GetStringDirect("CHAPTER_CONTINUOUS_BATTLE_DETAIL");
			confirmAutoContent = LocalizationManager.GetStringDirect("CHAPTER_CONTINUOUS_BATTLE_MESSAGE");
			break;
		case BattleType.Hideout:
			stringDirect2 = LocalizationManager.GetStringDirect("HIDEOUT_CONTINUOUS_BATTLE_DETAIL");
			confirmAutoContent = LocalizationManager.GetStringDirect("CONTINUOUS_BATTLE_MESSAGE2");
			break;
		case BattleType.Conquest:
			stringDirect2 = LocalizationManager.GetStringDirect("HUNTING_CONTINUOUS_BATTLE_DETAIL");
			confirmAutoContent = LocalizationManager.GetStringDirect("CONTINUOUS_BATTLE_MESSAGE2");
			break;
		}
		MessagePopup3Button.Open(stringDirect, stringDirect2, delegate
		{
			GetInstance<BattlePartySetting>().TryStartBattle(null);
		}, delegate
		{
			MessagePopup.OpenEnabled(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), confirmAutoContent, delegate
			{
				BattlePartySetting.ExtraOptionData extraOption = new BattlePartySetting.ExtraOptionData
				{
					AutoNext = true
				};
				GetInstance<BattlePartySetting>().TryStartBattle(extraOption);
			}, null, "COMMON_CONFIRM", bYesEnabled: true);
		}, stringDirect3, stringDirect4);
		return true;
	}

	void HeroFullSceneViewBase.Listener.OnOpen()
	{
	}

	void HeroFullSceneViewBase.Listener.OnClose()
	{
	}

	void HeroDetailView.Listener.OnTryBattle(HeroDetailView sender)
	{
		if (HeroDetailView.TryBattle(sender.HeroInfo) && _heroTryBattleReturnState != null)
		{
			LobbyScene2.ListReturnState.Add(_heroTryBattleReturnState);
			_heroTryBattleReturnState = null;
		}
	}

	private void Start()
	{
	}

	private void OnMoveEnterDungeon(EnterDungeonData enterDungeon)
	{
		CampaignNodeData linkedNodeData = enterDungeon.LinkedNodeData;
		if (linkedNodeData == null)
		{
			return;
		}
		AddTask(base.TaskFactory.Close<BattleDungeonInfo>(), base.TaskFactory.Close<PartyBattleDungeonInfo>(), base.TaskFactory.Run(delegate
		{
			MapLayout mapLayout = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
			if (mapLayout != null && mapLayout.MapViewer != null)
			{
				mapLayout.MapViewer.OnDragEndWorldMap();
			}
		}), base.TaskFactory.Run(delegate
		{
			NodeFunctionType function = linkedNodeData.Function;
			Action onEndMoveAction = GetOnEndMoveAction(function, enterDungeon);
			SceneAutoBehaviour<LobbyManagement>.instance.SetOnPlayerMoveEnd(onEndMoveAction);
			SceneAutoBehaviour<WorldMapManagement>.instance.MovePlayerToDungeon(enterDungeon.Chapter, enterDungeon.LinkedDungeon, DisableTouch: false);
		}));
	}

	private void OnClickPartyPlayPopup(EnterDungeonData enterDungeon)
	{
		PartyPlayPopup partyPlayPopup = GetInstance<PartyPlayPopup>();
		if (!partyPlayPopup.IsOpen())
		{
			partyPlayPopup.Open(enterDungeon, SceneAutoBehaviour<MultiPartyManagement>.instance);
		}
	}

	private Action GetOnEndMoveAction(NodeFunctionType functionType, EnterDungeonData enterDungeon)
	{
		Action after = null;
		if (!GlobalAutoBehaviour<AccountManager>.instance.IsMaxEquipItemCount())
		{
			after = delegate
			{
				SceneAutoBehaviour<LobbyCampaignDungeon>.instance.OpenPartySetting(enterDungeon);
			};
		}
		return delegate
		{
			SceneAutoBehaviour<LobbyManagement>.instance.ExecuteCurrentNode(after);
		};
	}
}
