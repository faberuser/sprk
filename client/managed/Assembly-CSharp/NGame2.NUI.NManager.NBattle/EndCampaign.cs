using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiniJSON;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NBattleField;
using NGame2.NBattleField.NManager.NPlay;
using NGame2.NBattleView;
using NGame2.NContentsInfo;
using NGame2.NEventTrigger;
using NGame2.NGuild;
using NGame2.NNodeTree;
using NGame2.NParty;
using NGame2.NQuest;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NUI.NManager.NEclipse;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NEclipse;
using NGame2.NUI.NWindow.NSelectRewardPopup;
using NGame2.NUtil;
using NGame2.NUtil.NModifyChecker;
using NGameAsset;
using NShared;
using NShared.AbandonMultiplayReward;
using NShared.ApplyForMultiplayReward;
using NShared.BeginCampaign;
using NShared.CompleteScenarioDungeon;
using NShared.EndCampaign;
using NShared.GetMultiplayReward;
using NShared.GetSelectedReward;
using NShared.NEvent;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NShared.NParty;
using NShared.NWorldMap;
using NShared.NWorldMap.NEventSubData;
using NVespa;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NScene;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NManager.NBattle;

public class EndCampaign : BaseManagement<EndCampaign>, PartyManager.IRewardListener, EndBattleWin.Listener, EndBattleLose.Listener, BattleLoseGuidePopup.Listener, EndBattleReward.Listener, MultiplayReward.Listener, NewRaid.Listener, PreSelectRewardPopup.Listener, SkillBookSelectRewardPopup.IListener, EclipseRewardPopup.IListener, EclipseBattleRecordPopup.IListener
{
	private LocalStorageString _testNewRaidIndex = new LocalStorageString("1", typeof(EndCampaign).FullName + "_testNewRaidIndex");

	private LocalStorageString _testNewRaidLevel = new LocalStorageString("1", typeof(EndCampaign).FullName + "_testNewRaidLevel");

	private bool _win;

	public EndBattleReward.StockData _rewardStock = new EndBattleReward.StockData();

	private EndCampaignProxy _endCampaignProxy;

	private int _expSoundCount;

	private ISound _expSound;

	private bool _multiplayRewardRequested;

	private bool _multiplayRewardCalculationStarted;

	private EquipItemInfo[] _multiplayRewardEquipItems;

	private ItemResultInfo[] _multiplayRewardItemResults;

	private float _multiplayRewardTimeoutTime;

	private RewardItemInfo[] _multiplayRewardItems;

	private bool _newQuestDungeonHappened;

	private QuestDungeon _completedQuestDungeonEvent;

	public Coroutine coWaitPartyMember;

	public Coroutine coCheckPartyReadyAll;

	private bool _alreadyRequest;

	private EclipseWebManager _eclipseWebManager;

	protected override void Awake()
	{
		base.Awake();
		AddInstance<EndBattleWin>("Resources/Prefab2/Window/Battle/EndBattleWin");
		AddInstance<EndBattleLose>("Resources/Prefab2/Window/Battle/EndBattleLose");
		AddInstance<EndBattleReward>("Resources/Prefab2/Window/Battle/EndBattleReward");
		AddInstance<RewardBackground>("Resources/Prefab2/Window/Battle/RewardBackground", GameUIRoot.ELayerType.BottomUI);
		AddInstance<BattleStats>("Resources/Prefab2/Window/Battle/BattleStats");
		AddInstance<QuestRewardPopup>("Resources/Prefab2/Window/RewardPopup/BaseRewardPopup");
		AddInstance<MultiplayReward>("Resources/Prefab2/Window/Battle/MultiplayReward");
		AddInstance<QuestDungeonPopup>("Resources/Prefab2/Window/Battle/QuestDungeonPopup");
		AddInstance<CampaignRewardPopup>("Resources/Prefab2/Window/Battle/CampaignRewardPopup");
		AddInstance<PartyRewardPopup>("Resources/Prefab2/Window/Battle/PartyRewardPopup");
		AddInstance<GetNewHero2>("Resources/Prefab2/Window/FullScene/GetNewHero2");
		AddInstance<BattleLoseGuidePopup>("Resources/Prefab2/Window/MessagePopup/BattleLoseGuidePopup");
		AddInstance<NewRaid>("Resources/Prefab2/Window/Raid/NewRaid");
		AddInstance<SkillBookSelectRewardPopup>("Resources/Prefab2/Window/Battle/SkillBookSelectRewardPopup");
		GlobalAutoBehaviour<PartyManager>.instance.AddRewardistener(this);
	}

	private void Start()
	{
		_eclipseWebManager = new EclipseWebManager();
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			base.OnDestroy();
			if (GlobalAutoBehaviour<PartyManager>.isValidInstance)
			{
				GlobalAutoBehaviour<PartyManager>.instance.RemoveRewardListener(this);
			}
			_eclipseWebManager = null;
		}
	}

	public void Open(int[] heroIndices, int[] aiHeroIndices, int[] groupHeroIndices, long[] accountIds, bool win, int star, Dictionary<int, int> killMonsterCount, IList<ResultCreatureInfo> creatureInfos, IList<int> aliveHeroIndices, int pureBattleTime, long elapsedBattleTime, string battleLogString = null)
	{
		GlobalAutoBehaviour<PartyManager>.instance.ChangeMemberStatus(MemberStatus.EndCampaign);
		MessagePopup3Button messagePopup3Button = GlobalAutoBehaviour<PopupManegement>.instance.PopUpExist<MessagePopup3Button>();
		if (messagePopup3Button != null)
		{
			messagePopup3Button.Close2(immediately: true);
		}
		_win = win;
		SingletonAuto<BattleContext>.instance.SetLastDungeonResult((!_win) ? EBoolCheck.False : EBoolCheck.True);
		_rewardStock.HeroIndices = heroIndices;
		_rewardStock.AiHeroIndices = aiHeroIndices;
		_rewardStock.GroupHeroInices = groupHeroIndices;
		_rewardStock.AccountIds = accountIds;
		_rewardStock.OldLevelInfo = Helper.GetAccountTeamLevelInfo();
		_rewardStock.EnterDungeonData = SingletonAuto<CampaignContext>.instance.EnterDungeon;
		_endCampaignProxy = null;
		_multiplayRewardTimeoutTime = 0f;
		_multiplayRewardItems = null;
		_multiplayRewardCalculationStarted = false;
		_newQuestDungeonHappened = false;
		_completedQuestDungeonEvent = null;
		NShared.EndCampaign.Request endCampaignRequest = GetEndCampaignRequest(win, star, killMonsterCount, creatureInfos, aliveHeroIndices, pureBattleTime, elapsedBattleTime, battleLogString);
		if (SingletonAuto<CampaignContext>.instance.EnterDungeon.IsScenarioReplay)
		{
			PlayBattleResultSoundAndBgm(win ? "UI.Campaign.Victory" : "UI.Campaign.Lose", win ? "UI.Campaign.VictoryBGM" : "UI.Campaign.LoseBGM");
			ExecuteEndScenarioReplay(endCampaignRequest);
		}
		else
		{
			if (SingletonAuto<CampaignContext>.instance.IsGuildRaidCampaign() || SingletonAuto<CampaignContext>.instance.IsWorldBossCampaign() || SingletonAuto<CampaignContext>.instance.IsGuildSuppressCampaign())
			{
				PlayBattleResultSoundAndBgm("UI.Campaign.Victory", "UI.Campaign.VictoryBGM");
			}
			else if (!SingletonAuto<CampaignContext>.instance.IsEclipseBattle())
			{
				PlayBattleResultSoundAndBgm(win ? "UI.Campaign.Victory" : "UI.Campaign.Lose", win ? "UI.Campaign.VictoryBGM" : "UI.Campaign.LoseBGM");
			}
			SingletonAuto<BattleContext>.instance.SetBattleResult(win, star);
			ExecuteEndCampaignRequest(endCampaignRequest);
		}
		SceneAutoBehaviour<NVespa.NScene.GuiManager>.instance.Add("EndCampaign", this, delegate
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
			int num = Vespa_String_Extension_Method.TOINT(_testNewRaidIndex.Value, -1);
			int num2 = Vespa_String_Extension_Method.TOINT(_testNewRaidLevel.Value, -1);
			RaidData data = GameRoot.Instance.RaidDataContainer.GetData(num, num2);
			using (new BlockManager().Enable(data != null))
			{
				if (GUILayout.Button("NewRaid"))
				{
					NShared.RaidInfo newRaid = new NShared.RaidInfo();
					newRaid.RaidIndex = num;
					newRaid.RaidLevel = num2;
					ReservePopupWindowOpen(delegate(NewRaid window)
					{
						window.Open(this, newRaid, null);
					});
				}
			}
		});
	}

	public void TutorialEndBattleWin(int star)
	{
		SingletonAuto<BattleFacade>.instance.ForceStopBattle();
		PlayBattleResultSoundAndBgm("UI.Campaign.Victory", "UI.Campaign.VictoryBGM");
		GetInstance<EndBattleWin>().Open(this, star);
	}

	public void TutorialEndBattleReward(HeroExpResultInfo[] heroExpResults, ItemResultInfo[] itemResults, EquipItemInfo[] equipInfos, GoldResultInfo[] goldResults, GemResultInfo[] gemResults)
	{
		long accountId = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
		BattleFacade.PlayerCacheData playerCache = SingletonAuto<BattleFacade>.instance.GetPlayerCache(accountId);
		if (playerCache == null)
		{
			VespaDebug.LogError("Failed to get my player cache : accountId=" + accountId);
			ReturnToLobby();
			return;
		}
		List<int> list = new List<int>();
		List<long> list2 = new List<long>();
		BattleFacade.CreatureCacheData[] allCreatureCache = SingletonAuto<BattleFacade>.instance.GetAllCreatureCache();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allCreatureCache); i++)
		{
			BattleFacade.CreatureCacheData creatureCacheData = allCreatureCache[i];
			if (creatureCacheData.TeamType == playerCache.TeamType)
			{
				list.Add(creatureCacheData.HeroInfo.HeroIndex);
				list2.Add(GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId);
			}
		}
		_endCampaignProxy = new EndCampaignProxy();
		_endCampaignProxy.TutorialResponse(heroExpResults, itemResults, equipInfos, goldResults, gemResults);
		_rewardStock.HeroIndices = list.ToArray();
		_rewardStock.AccountIds = list2.ToArray();
		_rewardStock.OldLevelInfo = Helper.GetAccountTeamLevelInfo();
		_rewardStock.EnterDungeonData = (SingletonAuto<CampaignContext>.isValidInstance ? SingletonAuto<CampaignContext>.instance.EnterDungeon : null);
		CloseInstance<EndBattleWin>();
		BattleViewGenericSingleton<CreatureManager>.instance.ReleaseAllCreatures();
		SceneAutoBehaviour<RuntimeGlobalManager>.instance.ClearAll();
		SceneAutoBehaviour<BattleManagement>.instance.CloseBut();
		GetInstance<RewardBackground>().Open();
		GetInstance<EndBattleReward>().Open(this, _rewardStock, _endCampaignProxy);
		OpenRewardPopup();
		OpenPurchaseMarkteingPopup();
	}

	public void TutorialEndRaidBattleReward(MultiplayReward.Listener listener, HeroExpResultInfo[] heroExpResults, ItemResultInfo[] itemResults, EquipItemInfo[] equipInfos, GoldResultInfo[] goldResults, GemResultInfo[] gemResults, RaidPointResultInfo[] raidPointResults, RewardItemInfo[] multiplayRewardResults)
	{
		CloseInstance<EndBattleWin>();
		BattleViewGenericSingleton<CreatureManager>.instance.ReleaseAllCreatures();
		SceneAutoBehaviour<RuntimeGlobalManager>.instance.ClearAll();
		SceneAutoBehaviour<BattleManagement>.instance.CloseBut();
		_endCampaignProxy = new EndCampaignProxy();
		_endCampaignProxy.TutorialResponse(heroExpResults, itemResults, equipInfos, goldResults, gemResults, raidPointResults);
		long accountId = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
		BattleFacade.PlayerCacheData playerCache = SingletonAuto<BattleFacade>.instance.GetPlayerCache(accountId);
		List<int> list = new List<int>();
		List<long> list2 = new List<long>();
		BattleFacade.CreatureCacheData[] allCreatureCache = SingletonAuto<BattleFacade>.instance.GetAllCreatureCache();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allCreatureCache); i++)
		{
			BattleFacade.CreatureCacheData creatureCacheData = allCreatureCache[i];
			if (creatureCacheData.TeamType == playerCache.TeamType && !creatureCacheData.CreateInfo.IsAi)
			{
				list.Add(creatureCacheData.HeroInfo.HeroIndex);
				list2.Add(GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId);
			}
		}
		_rewardStock.HeroIndices = list.ToArray();
		_rewardStock.AccountIds = list2.ToArray();
		_rewardStock.OldLevelInfo = Helper.GetAccountTeamLevelInfo();
		_rewardStock.EnterDungeonData = (SingletonAuto<CampaignContext>.isValidInstance ? SingletonAuto<CampaignContext>.instance.EnterDungeon : null);
		if (_rewardStock.EnterDungeonData != null)
		{
			_rewardStock.EnterDungeonData.LinkedDungeonData.ReqStamina = 0;
			_rewardStock.EnterDungeonData.LinkedDungeonData.ReqStaminaType = StaminaType.Chicken;
		}
		GetInstance<RewardBackground>().Open();
		GetInstance<EndBattleReward>().Open(this, _rewardStock, _endCampaignProxy);
		GetInstance<EndBattleReward>().SetupTutorialLayout();
		PartyInfo partyInfo = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo;
		MemberInfo[] members = partyInfo.PartyMembers.ToArray();
		ReservePopupWindowOpen(delegate(MultiplayReward window)
		{
			window.Open(listener, 0f, multiplayRewardResults, members, _endCampaignProxy, canSelectReward: true);
		});
		OpenPurchaseMarkteingPopup();
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

	public static void RemoveOnetimeBooster()
	{
		string value = GameRoot.Instance.ConstantDataContainer.GetValue("OneTimeItemBoosterCode");
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(value);
		if (dataByCode != null)
		{
			GlobalAutoBehaviour<AccountManager>.instance.ItemTimeDurationManager.Remove(dataByCode.Index);
		}
	}

	private bool EndOfOnetimeBoosterRepeatBattle()
	{
		if (SingletonAuto<CampaignContext>.instance.OnetimeBoosterCount == 0L)
		{
			return SingletonAuto<CampaignContext>.instance.OnetimeBoosterMaxCount > 0;
		}
		return false;
	}

	public static void DisableOnetimeBoosterRepeatBattle()
	{
		CampaignContext campaignContext = SingletonAuto<CampaignContext>.instance;
		int num = 0;
		campaignContext.OnetimeBoosterCount = num;
		SingletonAuto<CampaignContext>.instance.OnetimeBoosterMaxCount = 0;
		RemoveOnetimeBooster();
	}

	private bool GetStamina(out long curStamina, out long maxStamina, out int reqStamina)
	{
		int num = 0;
		curStamina = num;
		int num2 = 0;
		maxStamina = num2;
		reqStamina = 0;
		EnterDungeonData enterDungeonData = _rewardStock.EnterDungeonData;
		CampaignDungeonData linkedDungeonData = enterDungeonData.LinkedDungeonData;
		if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CheckEventDungeon(enterDungeonData.Chapter, enterDungeonData.LinkedDungeon, out maxStamina, out curStamina))
		{
			reqStamina = 1;
			return true;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(linkedDungeonData.ReqStaminaType, out curStamina, out maxStamina))
		{
			reqStamina = linkedDungeonData.ReqStamina;
			return true;
		}
		return false;
	}

	private void ProcessBattleRepeatForOnetimeBooster()
	{
		if (_rewardStock.EnterDungeonData.LinkedDungeonData == null || !GetStamina(out var curStamina, out var maxStamina, out var reqStamina))
		{
			return;
		}
		long num = curStamina;
		int num2 = reqStamina;
		long val = num / num2;
		int num3 = 0;
		if (System.Math.Max(num3, val) == 0L)
		{
			BattlePartySetting.ExtraOptionData extraOptionData = new BattlePartySetting.ExtraOptionData();
			extraOptionData.IsEndCampaign = true;
			BattleRepeat(extraOptionData);
			return;
		}
		string value = GameRoot.Instance.ConstantDataContainer.GetValue("OneTimeItemBoosterCode");
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(value);
		if (dataByCode == null)
		{
			return;
		}
		UserManager userManager = GlobalAutoBehaviour<AccountManager>.instance.UserManager;
		long i_valueA = 0L;
		long totalGem = userManager.TotalGem;
		int buyGem = dataByCode.BuyGem;
		long num4 = NVespa.Math.Max(i_valueA, totalGem / buyGem);
		if (num4 == 0L)
		{
			if (NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled())
			{
				NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.OnClickStop(sendRoomRepeat: true);
			}
			else
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("BOOSTER_ONETIME_REWARD_INSUFFICIENT_RUBY"));
			}
		}
		else
		{
			OpenSelectItemMessagePopupForOnetimeBooster(maxStamina, curStamina, reqStamina, num4, dataByCode);
		}
	}

	private void OpenSelectItemMessagePopupForOnetimeBooster(long maxStamina, long curStamina, int reqStamina, long onetimeBoosterPlayableCount, ItemData onetimeBoosterItemData)
	{
		if (NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled())
		{
			NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.OnClickStop(sendRoomRepeat: true);
		}
		maxStamina = ((maxStamina >= curStamina) ? maxStamina : curStamina);
		long val = maxStamina / reqStamina;
		int num = 0;
		long val2 = System.Math.Max(num, val);
		long val3 = curStamina / reqStamina;
		int num2 = 0;
		long val4 = System.Math.Max(num2, val3);
		onetimeBoosterPlayableCount = System.Math.Min(onetimeBoosterPlayableCount, val2);
		onetimeBoosterPlayableCount = System.Math.Min(onetimeBoosterPlayableCount, val4);
		List<Tuple<ItemData, int>> list = new List<Tuple<ItemData, int>>();
		long num3 = onetimeBoosterPlayableCount;
		list.Add(new Tuple<ItemData, int>(onetimeBoosterItemData, checked((int)num3)));
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect("REPEATBATTLE_INFORM_BY_BOOSTER");
		SelectItemMessagePopup.OpenWithGemPrice(title, stringDirect, list, onetimeBoosterItemData.BuyGem, delegate(SelectItemMessagePopup selectItemPopup, int idx, int count)
		{
			onetimeBoosterPlayableCount = count;
			SingletonAuto<CampaignContext>.instance.OnetimeBoosterCount = onetimeBoosterPlayableCount;
			BattlePartySetting.OpenRepeatBattleNoticePopup(this);
		}, DisableOnetimeBoosterRepeatBattle, closeOnYes: false);
	}

	private void OnClickRepeatOnce(bool isEndCampaign)
	{
		BattlePartySetting.ExtraOptionData extraOptionData = new BattlePartySetting.ExtraOptionData();
		extraOptionData.IsEndCampaign = isEndCampaign;
		BattleRepeat(extraOptionData);
	}

	private void OnClickBattleRepeat(CampaignDungeonData dungeonData, bool isEndCampaign)
	{
		int chapterIndex = dungeonData.ChapterIndex;
		int dungeonIndex = dungeonData.DungeonIndex;
		SelectRewardData data = GameRoot.Instance.SelectRewardDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data != null)
		{
			switch (dungeonData.BattleType)
			{
			case BattleType.UnderPrison:
			{
				UnderPrisonData data2 = GameRoot.Instance.UnderPrisonDataContainer.GetData(chapterIndex);
				if (data2 != null && data2.IsSelectReward)
				{
					if (!data.IsSelectable())
					{
						BattlePartySetting.OpenRepeatBattleNoticePopup(this, isEndCampaign);
					}
					else
					{
						BattlePartySetting.OpenRepeatBattleNoticePopup(this);
					}
				}
				break;
			}
			case BattleType.Shakmeh:
				ShakmehPartySetting.OpenRepeatBattleNoticePopup(this, isEndCampaign);
				break;
			default:
				BattlePartySetting.OpenRepeatBattleNoticePopup(this);
				break;
			}
		}
		else
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string stringDirect = LocalizationManager.GetStringDirect("REPEATBATTLE_INFORM2");
			MessagePopup.OpenEnabled(title, stringDirect, delegate
			{
				BattleRepeat(new BattlePartySetting.ExtraOptionData
				{
					AutoRepeat = true,
					IsEndCampaign = true
				});
			}, null, "COMMON_CONFIRM", bYesEnabled: true);
		}
	}

	public void ProcessBattleRepeat()
	{
		bool flag = false;
		bool flag2 = NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled();
		if (SingletonAuto<CampaignContext>.isValidInstance)
		{
			EnterDungeonData enterDungeon = SingletonAuto<CampaignContext>.instance.EnterDungeon;
			flag = enterDungeon != null && BattlePartySetting.AutoRepeatable(enterDungeon);
		}
		if (EndOfOnetimeBoosterRepeatBattle() && !NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsPartyPlay)
		{
			ProcessBattleRepeatForOnetimeBooster();
		}
		else if (flag && !flag2)
		{
			EnterDungeonData enterDungeonData = _rewardStock.EnterDungeonData;
			if (enterDungeonData == null || !enterDungeonData.CheckBattleEnter())
			{
				return;
			}
			CampaignDungeonData dungeonData = enterDungeonData.LinkedDungeonData;
			if (AutoNextBattle.IsEnabled())
			{
				AutoNextBattle.instance.OnClickStop();
			}
			if (dungeonData != null && dungeonData.Value != null && dungeonData.Value.Length != 0 && dungeonData.Value[0] == 1)
			{
				BattlePartySetting.ExtraOptionData extraOptionData = new BattlePartySetting.ExtraOptionData();
				extraOptionData.IsEndCampaign = true;
				BattleRepeat(extraOptionData);
				return;
			}
			string title = LocalizationManager.GetString(Battle.REPEAT_BATTLE_ASK_TITLE);
			string message = LocalizationManager.GetString(Battle.REPEAT_BATTLE_ASK_CONTENT);
			MessagePopup3Button.Open(title, message, delegate
			{
				OnClickRepeatOnce(isEndCampaign: true);
			}, delegate
			{
				OnClickBattleRepeat(dungeonData, isEndCampaign: true);
			}, Battle.REPEAT_BATTLE_SINGLETIME, Battle.REPEAT_BATTLE_AUTO);
		}
		else
		{
			BattlePartySetting.ExtraOptionData extraOptionData2 = new BattlePartySetting.ExtraOptionData();
			extraOptionData2.AutoRepeat = flag2;
			extraOptionData2.IsEndCampaign = true;
			BattleRepeat(extraOptionData2);
		}
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

	private void ReserveGetNewHeroPopup()
	{
		if ((_rewardStock != null && _rewardStock.EnterDungeonData != null && _rewardStock.EnterDungeonData.LinkedDungeonData != null && _rewardStock.EnterDungeonData.LinkedDungeonData.BattleType == BattleType.Trial) || _endCampaignProxy == null || _endCampaignProxy.HeroInfos == null)
		{
			return;
		}
		int curLevel = _rewardStock.OldLevelInfo.Level;
		long num = _rewardStock.OldLevelInfo.CurExp;
		EndBattleReward endBattleReward = GetInstance<EndBattleReward>();
		for (int i = 0; i < _endCampaignProxy.HeroInfos.Length; i++)
		{
			HeroInfo heroInfo = _endCampaignProxy.HeroInfos[i];
			if (GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroInfo.HeroIndex) == null || GameRoot.Instance.CreatureDataContainer.GetData(heroInfo.HeroIndex) == null)
			{
				continue;
			}
			endBattleReward.SetButtonEnabledAction((IWindow)GetInstance<GetNewHero2>(), (Action)delegate
			{
				GetInstance<GetNewHero2>().Open(GetNewHero2.OpenData.CreateSingleFromHeroInfo(heroInfo, GetNewHero2.EOpenMode.NewHero, hideInfo: false), null, null);
			});
			if (_endCampaignProxy.ExpResultsByGetHero == null || i >= _endCampaignProxy.ExpResultsByGetHero.Length)
			{
				continue;
			}
			ExpResultInfo expResultInfo = _endCampaignProxy.ExpResultsByGetHero[i];
			Helper.LevelInfo oldLevelInfo = Helper.GetTeamLevelInfo(curLevel, (int)num);
			Helper.LevelInfo curLevelInfo = Helper.GetTeamLevelInfo(expResultInfo.NewLevel, expResultInfo.NewValue);
			endBattleReward.SetTeamExpAction(oldLevelInfo, curLevelInfo);
			curLevel = expResultInfo.NewLevel;
			num = expResultInfo.NewValue;
			if (oldLevelInfo.Level < curLevelInfo.Level)
			{
				TeamLevelupPopup window = TeamLevelupPopup.instance;
				endBattleReward.SetButtonEnabledAction((IWindow)window, (Action)delegate
				{
					TeamLevelupPopup.Open(oldLevelInfo.Level, curLevelInfo.Level);
				});
			}
		}
	}

	private void OpenMultiplayReward()
	{
		if (!IsMultiplayCampaign())
		{
			return;
		}
		PartyInfo partyInfo = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo;
		if (partyInfo == null || !partyInfo.IsValid())
		{
			return;
		}
		RewardItemInfo[] rewardItems = _multiplayRewardItems;
		if (__Extension_Method_ICollection__.COUNT(rewardItems) == 0)
		{
			return;
		}
		MemberInfo[] members = partyInfo.PartyMembers.ToArray();
		bool canSelectReward = CanSelectMultiplayReward();
		if (partyInfo.IsRepeatBattle)
		{
			OnAbandon();
			return;
		}
		ReservePopupWindowOpen(delegate(MultiplayReward window)
		{
			window.Open(this, _multiplayRewardTimeoutTime, rewardItems, members, _endCampaignProxy, canSelectReward);
		});
	}

	private void OpenPartyRewardPopup()
	{
		ReservePopupWindowOpen(delegate(PartyRewardPopup window)
		{
			PartyInfo partyInfo = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo;
			MemberInfo[] array = partyInfo.PartyMembers.ToArray();
			int num = array.Length;
			List<MemberInfo> list = new List<MemberInfo>();
			for (int i = 0; i < num; i++)
			{
				MemberInfo memberInfo = array[i];
				if (memberInfo.IsReceivedResult)
				{
					list.Add(memberInfo);
				}
			}
			_endCampaignProxy.MultiplayMemberResults = new MultiplayMemberResultInfo[list.Count];
			List<CurrencyResultInfo3> eclipsePointList = new List<CurrencyResultInfo3>();
			CurrencyResultInfo3[] currencyResultInfos = _endCampaignProxy.CurrencyResultInfos3;
			foreach (CurrencyResultInfo3 currencyResultInfo in currencyResultInfos)
			{
				if (currencyResultInfo.CurrencyType == CurrencyType.EclipsePoint)
				{
					eclipsePointList.Add(currencyResultInfo);
				}
			}
			for (int k = 0; k < list.Count; k++)
			{
				MemberInfo memberInfo2 = list[k];
				_endCampaignProxy.MultiplayMemberResults[k] = new MultiplayMemberResultInfo();
				_endCampaignProxy.MultiplayMemberResults[k].AccountId = memberInfo2.AccountId;
				_endCampaignProxy.MultiplayMemberResults[k].Nick = memberInfo2.Nick;
				_endCampaignProxy.MultiplayMemberResults[k].MemberIndex = memberInfo2.MemberIndex;
				_endCampaignProxy.MultiplayMemberResults[k].UseStamina = memberInfo2.UseStamina;
				_endCampaignProxy.MultiplayMemberResults[k].HeroExpResults = memberInfo2.HeroExpResults;
				_endCampaignProxy.MultiplayMemberResults[k].FlaskResults = memberInfo2.FlaskResults;
				_endCampaignProxy.MultiplayMemberResults[k].FlaskItemResults = memberInfo2.FlaskItemResults;
				_endCampaignProxy.MultiplayMemberResults[k].CurrencyResults = memberInfo2.CurrencyResults;
				_endCampaignProxy.MultiplayMemberResults[k].ItemResults = memberInfo2.ItemResults;
				_endCampaignProxy.MultiplayMemberResults[k].EquipItemInfos = memberInfo2.EquipItemInfos;
				_endCampaignProxy.MultiplayMemberResults[k].EquipItemBoostInfos = memberInfo2.EquipItemBoostInfos;
				_endCampaignProxy.MultiplayMemberResults[k].CurrencyResults2 = memberInfo2.CurrencyResults2;
				_endCampaignProxy.MultiplayMemberResults[k].ItemResults2 = memberInfo2.ItemResults2;
				_endCampaignProxy.MultiplayMemberResults[k].EquipItemInfos2 = memberInfo2.EquipItemInfos2;
				_endCampaignProxy.MultiplayMemberResults[k].EquipItemBoostInfos2 = memberInfo2.EquipItemBoostInfos2;
			}
			PartyRewardPopup.PartyPopupOpenData partyPopupOpenData = new PartyRewardPopup.PartyPopupOpenData();
			partyPopupOpenData.SetMultiplayMemberResultInfos(_endCampaignProxy.MultiplayMemberResults);
			partyPopupOpenData.ChapterIndex = partyInfo.ChapterIndex;
			partyPopupOpenData.DungeonIndex = partyInfo.DungeonIndex;
			window.OpenWithData(partyPopupOpenData, isGlobal: false, delegate
			{
				ReservePopupWindowOpen(delegate(CampaignRewardPopup campaignRewardWindow)
				{
					BaseRewardPopup.PopupOpenData popupOpen = new BaseRewardPopup.PopupOpenData().SetItemResultInfos(_endCampaignProxy.ItemResults2).SetEquipResultInfos(_endCampaignProxy.EquipItemInfos2).SetBoostEquipResultInfos(_endCampaignProxy.EquipItemBoostInfos)
						.AddCurrencyResultInfos3(_endCampaignProxy.CurrencyResults2)
						.AddCurrencyResultInfos3(_endCampaignProxy.GuildCurrencyResults)
						.AddCurrencyResultInfos3(eclipsePointList.ToArray());
					campaignRewardWindow.OpenWithData(popupOpen, isGlobal: false);
				});
			});
		});
	}

	private void SetMemberResults(MultiplayMemberResultInfo memberResults)
	{
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
			if (__Extension_Method_ICollection__.COUNT(_multiplayRewardItemResults) > 0)
			{
				list.AddRange(_multiplayRewardItemResults);
			}
			if (__Extension_Method_ICollection__.COUNT(_endCampaignProxy.EquipItemInfos) > 0)
			{
				list2.AddRange(_endCampaignProxy.EquipItemInfos);
			}
			if (__Extension_Method_ICollection__.COUNT(_multiplayRewardEquipItems) > 0)
			{
				list2.AddRange(_multiplayRewardEquipItems);
			}
			BaseRewardPopup.PopupOpenData popupOpenData = new BaseRewardPopup.PopupOpenData().SetItemResultInfos(list.ToArray()).SetEquipResultInfos(list2.ToArray()).SetBoostEquipResultInfos(_endCampaignProxy.EquipItemBoostInfos)
				.AddCurrencyResultInfos3(_endCampaignProxy.CurrencyResultInfos3)
				.AddCurrencyResultInfos3(_endCampaignProxy.GuildCurrencyResults)
				.SetSelectedRewardItemCodes(_endCampaignProxy.SelectedRewardItemCodes);
			if (SingletonAuto<CampaignContext>.isValidInstance && SingletonAuto<CampaignContext>.instance.IsSkipRewardPopup)
			{
				int teamKillMeter = SingletonAuto<BattleFacade>.instance.GetTeamKillMeter(TeamType.Left);
				{
					foreach (LobbyScene2.IReturnState item in LobbyScene2.ListReturnState)
					{
						if (item is BaseEntryMenuReturnState baseEntryMenuReturnState)
						{
							baseEntryMenuReturnState.AddSaveData(BaseEntryMenuReturnState.SaveKey.RewardData, popupOpenData);
							baseEntryMenuReturnState.AddSaveData(BaseEntryMenuReturnState.SaveKey.LeftTeamKillCount, teamKillMeter);
						}
					}
					return;
				}
			}
			window.OpenWithData(popupOpenData, isGlobal: false, delegate
			{
				AppReviewPopupOpen();
			});
		});
	}

	private void OpenRewardPopup()
	{
		CheckPortalOpenEffect();
		PartyInfo partyInfo = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo;
		int raidIndex = partyInfo.RaidIndex;
		int raidLevel = partyInfo.RaidLevel;
		int chapterIndex = partyInfo.ChapterIndex;
		int dungeonIndex = partyInfo.DungeonIndex;
		if (raidIndex == 0 && raidLevel == 0 && chapterIndex != 0 && dungeonIndex != 0)
		{
			OpenPartyRewardPopup();
			return;
		}
		OpenCampaignRewardPopup();
		SingletonUpdater<QuestManager>.instance.OpenClearMainQuestConfirmNotiPopup();
	}

	private void OpenPurchaseMarkteingPopup()
	{
		GetInstance<EndBattleReward>().ReservePurchaseMarketingPopup();
	}

	private void OpenQuestDungeonComplete()
	{
		if (_completedQuestDungeonEvent != null)
		{
			int monsterIndex = _completedQuestDungeonEvent.MonsterIndex;
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(monsterIndex);
			string questDungeonText = ((data != null) ? LocalizationManager.GetString(WorldMapEvent.OBJECTIVE_KILL_MONSTER, data.Name) : "");
			ReservePopupWindowOpen(delegate(QuestDungeonPopup window)
			{
				window.Open(QuestDungeonPopup.EventType.EndQuestDungeon, questDungeonText);
			});
		}
	}

	private void OpenNewQuestDungeon()
	{
		if (_newQuestDungeonHappened)
		{
			ReservePopupWindowOpen(delegate(QuestDungeonPopup window)
			{
				window.Open(QuestDungeonPopup.EventType.NewQuestDungeon);
			});
		}
	}

	private void OpenEndBattleReward()
	{
		if (IsMultiplayCampaign())
		{
			PartyInfo partyInfo = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo;
			if (partyInfo != null && partyInfo.IsValid() && !CanSelectMultiplayReward())
			{
				return;
			}
		}
		BattleViewGenericSingleton<CreatureManager>.instance.ReleaseAllCreatures();
		SceneAutoBehaviour<RuntimeGlobalManager>.instance.ClearAll();
		SceneAutoBehaviour<BattleManagement>.instance.CloseBut();
		if (_rewardStock != null && _rewardStock.EnterDungeonData != null && _rewardStock.EnterDungeonData.LinkedDungeonData != null && SingletonAuto<CampaignContext>.isValidInstance && CampaignContext.CheckEachWaveEachTeam(_rewardStock.EnterDungeonData.LinkedDungeonData.BattleType))
		{
			_rewardStock.AiHeroIndices = null;
		}
		GetInstance<RewardBackground>().Open();
		GetInstance<EndBattleReward>().Open(this, _rewardStock, _endCampaignProxy);
		if (_endCampaignProxy != null)
		{
			ReserveTeamLevelupPopup();
			if (!NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled() && !AutoNextBattle.IsEnabled())
			{
				ReserveGetNewHeroPopup();
			}
			bool flag = true;
			if (SingletonAuto<CampaignContext>.isValidInstance && ((SingletonAuto<CampaignContext>.isValidInstance && SingletonAuto<CampaignContext>.instance.IsChallengeRaidMulti()) || (SingletonAuto<CampaignContext>.isValidInstance && SingletonAuto<CampaignContext>.instance.IsChallengeRaidSingle())))
			{
				flag = false;
			}
			if (flag)
			{
				OpenMultiplayReward();
				OpenRewardPopup();
			}
			OpenPurchaseMarkteingPopup();
			if (!NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled() && !AutoNextBattle.IsEnabled())
			{
				OpenQuestDungeonComplete();
				OpenNewQuestDungeon();
			}
		}
	}

	public void RefreshRewardUI()
	{
		GetInstance<EndBattleReward>().SetupResourceUi();
	}

	public void ReturnToLobby()
	{
		SprkConquest.LeaveSoloConquestOnLobbyReturn();
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

	private IEnumerator coOpenLose()
	{
		yield return null;
	}

	private NShared.EndCampaign.Request GetEndCampaignRequest(bool win, int star, Dictionary<int, int> killMonsterCount, IList<ResultCreatureInfo> creatureInfos, IList<int> aliveHeroIndices, int pureBattleTime, long elapsedBattleTime, string battleLogString)
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
		return new NShared.EndCampaign.Request
		{
			ChapterIndex = SingletonAuto<CampaignContext>.instance.EnterDungeon.Chapter,
			DungeonIndex = SingletonAuto<CampaignContext>.instance.EnterDungeon.LinkedDungeon,
			DungeonDifficulty = (int)SingletonAuto<CampaignContext>.instance.EnterDungeon.Difficulty,
			ScenarioDungeon = SingletonAuto<CampaignContext>.instance.EnterDungeon.IsScenario,
			Completed = win,
			Star = star,
			MonsterIndices = list.ToArray(),
			KillCounts = list2.ToArray(),
			AliveHeroIndices = aliveHeroIndices.ToArray(),
			BattleLogString = battleLogString,
			CreatureInfoString = creatureInfoString,
			PureBattleTime = pureBattleTime,
			ElapsedBattleTime = elapsedBattleTime
		};
	}

	private void RequestEndCampaign(NShared.EndCampaign.Request request)
	{
		string[] values = new string[3]
		{
			request.ChapterIndex.ToString(),
			request.DungeonIndex.ToString(),
			((NShared.DungeonDifficultyType)request.DungeonDifficulty/*cast due to .constrained prefix*/).ToString()
		};
		SingletonAuto<ModifyChecker>.instance.CheckStart<FirstClearDungeon>(values);
		bool wasCompleted = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(request.ChapterIndex, request.DungeonIndex, (NShared.DungeonDifficultyType)request.DungeonDifficulty);
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.EndCampaign.Response response)
		{
			if ((response == null && NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled()) || (response != null && response.BaseResult != BaseResultType.Success) || (response != null && response.Result != NShared.EndCampaign.ResultType.Success))
			{
				AutoBattle.Stop();
			}
			if (response != null && response.BaseResult == BaseResultType.Success)
			{
				if (response.Result == NShared.EndCampaign.ResultType.Success)
				{
					_endCampaignProxy = new EndCampaignProxy();
					_endCampaignProxy.OnResponse(request, response);
					_multiplayRewardTimeoutTime = Time.time + (float)response.MultiplayRemainTimeoutTime;
					_multiplayRewardItems = response.MultiplayRewardItems;
					_newQuestDungeonHappened = CheckNewQuestDungeon(response.WorldMapEventInfos);
					_completedQuestDungeonEvent = CheckEndQuestDungeon(request.ChapterIndex, request.DungeonIndex, response.WorldMapEventInfos);
					if (response.CampaignResults != null)
					{
						ChapterDungeonInfo[] campaignResults = response.CampaignResults;
						foreach (ChapterDungeonInfo chapterDungeonInfo in campaignResults)
						{
							GlobalAutoBehaviour<CampaignNodeOpenEffectManager>.instance.CheckNewClearNode(chapterDungeonInfo.ChapterIndex, chapterDungeonInfo.DungeonIndex);
						}
					}
					GlobalAutoBehaviour<AccountManager>.instance.ApplyEndCampaignResponse(response);
					if (request.Completed)
					{
						CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(request.ChapterIndex);
						if (data != null && data.Type == ChapterType.Trial)
						{
							foreach (HeroAwakeData allDatum in GameRoot.Instance.HeroAwakeDataContainer.GetAllData())
							{
								if (allDatum.CheckIndices(request.ChapterIndex, request.DungeonIndex))
								{
									GlobalAutoBehaviour<AccountManager>.instance.HeroManager.ResetAwakeMaterialChallenge(allDatum.HeroIndex);
									break;
								}
							}
						}
						if (data != null && data.Type == ChapterType.DOW)
						{
							GlobalAutoBehaviour<AccountManager>.instance.UnderPrisonManager.SetDiff(request.ChapterIndex, response.Diff);
						}
						if (data != null && data.Type == ChapterType.Event)
						{
							GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetLastEventDungeon(request.ChapterIndex, request.DungeonIndex);
						}
						CampaignDungeonData data2 = GameRoot.Instance.CampaignDungeonDataContainer.GetData(request.ChapterIndex, request.DungeonIndex);
						if (data2 != null && data2.BattleType == BattleType.Shakmeh)
						{
							int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
							int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
							ShakmehBossData data3 = GameRoot.Instance.ShakmehBossDataContainer.GetData(chapterIndex, dungeonIndex);
							if (data3 != null && data3.BossType == ShakmehBossType.Final)
							{
								GlobalAutoBehaviour<AccountManager>.instance.UserManager.RemoveShakmehBuffInfo();
							}
						}
					}
					if (SingletonAuto<CampaignContext>.instance.IsWorldBossCampaign())
					{
						WorldBossManagement.WorldBossKilledByMe = _endCampaignProxy.WorldBossKilledByMe;
						WorldBossManagement.WorldBossKilledRewardInfo = _endCampaignProxy.WorldBossKilledRewardInfo;
					}
					if (response.ItemBoosterResult != null)
					{
						CampaignContext campaignContext = SingletonAuto<CampaignContext>.instance;
						int remainCount = response.ItemBoosterResult.RemainCount;
						campaignContext.OnetimeBoosterCount = remainCount;
						SingletonAuto<CampaignContext>.instance.OnetimeBoosterMaxCount = response.ItemBoosterResult.MaxCount;
					}
					GetInstance<EndBattleWin>().OnEndCampaignResponse(_endCampaignProxy);
					if (!wasCompleted)
					{
						_ = (object)null;
						_ = 7;
						_ = 1;
						_ = 0;
						_ = new object[2] { request.ChapterIndex, request.DungeonIndex };
						if (GameRoot.Instance.CampaignDungeonDataContainer.IsLastDungeonInChapter(request.ChapterIndex, request.DungeonIndex))
						{
							switch (_rewardStock.EnterDungeonData.LinkedDungeonData.BattleType)
							{
							case BattleType.Campaign:
							case BattleType.Boss:
								_ = (object)null;
								_ = 16;
								_ = 1;
								_ = 0;
								(new object[1])[0] = request.ChapterIndex;
								break;
							case BattleType.SubStory:
							case BattleType.SubStoryBoss:
								_ = (object)null;
								_ = 17;
								_ = 1;
								_ = 0;
								(new object[1])[0] = request.ChapterIndex;
								break;
							}
						}
					}
					switch (_rewardStock.EnterDungeonData.LinkedDungeonData.BattleType)
					{
					case BattleType.Raid:
					case BattleType.RaidSingle:
						_ = (object)null;
						_ = 29;
						_ = 1;
						_ = 0;
						Array.Empty<object>();
						break;
					case BattleType.HardRaid:
						_ = (object)null;
						_ = 30;
						_ = 1;
						_ = 0;
						Array.Empty<object>();
						break;
					case BattleType.ChallengeRaidSingle:
					case BattleType.ChallengeRaid:
						_ = (object)null;
						_ = 31;
						_ = 1;
						_ = 0;
						Array.Empty<object>();
						break;
					case BattleType.WorldBoss:
						_ = (object)null;
						_ = 32;
						_ = 1;
						_ = 0;
						Array.Empty<object>();
						break;
					case BattleType.GuildRaid:
						_ = (object)null;
						_ = 33;
						_ = 1;
						_ = 0;
						Array.Empty<object>();
						break;
					case BattleType.GuildSuppress:
						_ = (object)null;
						_ = 34;
						_ = 1;
						_ = 0;
						Array.Empty<object>();
						break;
					case BattleType.Labyrinth:
						_ = (object)null;
						_ = 35;
						_ = 1;
						_ = 0;
						Array.Empty<object>();
						break;
					case BattleType.Maze:
						_ = (object)null;
						_ = 36;
						_ = 1;
						_ = 0;
						Array.Empty<object>();
						break;
					case BattleType.UnderPrison:
						_ = (object)null;
						_ = 37;
						_ = 1;
						_ = 0;
						Array.Empty<object>();
						break;
					case BattleType.OrdealTower:
						_ = (object)null;
						_ = 38;
						_ = 1;
						_ = 0;
						Array.Empty<object>();
						break;
					case BattleType.ChallengeTower:
						_ = (object)null;
						_ = 39;
						_ = 1;
						_ = 0;
						Array.Empty<object>();
						break;
					case BattleType.TreasureHouse:
						_ = (object)null;
						_ = 40;
						_ = 1;
						_ = 0;
						Array.Empty<object>();
						break;
					}
					if (SingletonAuto<CampaignContext>.instance.IsEclipseBattle())
					{
						ProcessEclipseBattleEnd(response);
					}
					return;
				}
				AutoBattle.Stop();
				GlobalAutoBehaviour<AccountManager>.instance.ApplyEndCampaignResponse(response);
			}
			switch (response.Result)
			{
			case NShared.EndCampaign.ResultType.GuildSuppressAlreadyCompleted:
				OpenMessagePopupReturnToLobby(LocalizationManager.GetStringDirect("PUNITIVE_EXPEDITION_TICKET_RETURN"));
				break;
			case NShared.EndCampaign.ResultType.GuildGuildRaidAlreadyCompleted:
				OpenMessagePopupReturnToLobby(LocalizationManager.GetStringDirect("RESULT_TYPE.GUILD_GUILDRAID_ALREADY_COMPLETED"));
				break;
			case NShared.EndCampaign.ResultType.GuildGuildRaidAlreadyFinalCompleted:
				OpenMessagePopupReturnToLobby(LocalizationManager.GetStringDirect("RESULT_TYPE.GUILD_GUILDRAID_ALREADY_FINAL_COMPLETED"));
				break;
			case NShared.EndCampaign.ResultType.GuildRaidInitializing:
			case NShared.EndCampaign.ResultType.WorldBossDamagedByZero:
				OpenMessagePopupReturnToLobby(LocalizationManager.GetResultString(response));
				break;
			default:
			{
				string title = LocalizationManager.GetString(NShared.NLocalization.Common.ERROR);
				string resultString = LocalizationManager.GetResultString(response);
				MessagePopup.Open(title, resultString, delegate
				{
					RequestEndCampaign(request);
				}, delegate
				{
					ReturnToLobby();
				});
				break;
			}
			}
		});
	}

	private void OpenMessagePopupReturnToLobby(string message)
	{
		MessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), message, delegate
		{
			ReturnToLobby();
		});
	}

	private void RequestCompleteScenarioDungeon(NShared.CompleteScenarioDungeon.Request requestPacket)
	{
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(requestPacket, delegate(NShared.CompleteScenarioDungeon.Response response)
		{
			EndBattleWin endBattleWin = null;
			if (response != null && response.BaseResult == BaseResultType.Success && response.Result == NShared.CompleteScenarioDungeon.ResultType.Success)
			{
				_endCampaignProxy = new EndCampaignProxy();
				_newQuestDungeonHappened = false;
				_completedQuestDungeonEvent = null;
				GlobalAutoBehaviour<AccountManager>.instance.ApplyCompleteScenarioDungeonResponse(response);
				endBattleWin = GetInstance<EndBattleWin>();
				endBattleWin.OnEndCampaignResponse(_endCampaignProxy);
			}
			else
			{
				AutoBattle.Stop();
				if (response.BaseResult == BaseResultType.Disabled)
				{
					endBattleWin = GetInstance<EndBattleWin>(openedOnly: true);
					if (endBattleWin != null)
					{
						_endCampaignProxy = new EndCampaignProxy();
						endBattleWin.OnEndCampaignResponse(_endCampaignProxy);
					}
				}
				else
				{
					string title = LocalizationManager.GetString(NShared.NLocalization.Common.ERROR);
					string resultString = LocalizationManager.GetResultString(response);
					MessagePopup.Open(title, resultString, delegate
					{
						RequestCompleteScenarioDungeon(requestPacket);
					}, delegate
					{
						ReturnToLobby();
					});
				}
			}
		});
	}

	private void ExecuteEndCampaignRequest(NShared.EndCampaign.Request endCampaignRequest)
	{
		if (SingletonAuto<CampaignContext>.isValidInstance && SingletonAuto<CampaignContext>.instance.IsEclipseBattle())
		{
			RequestEndCampaign(endCampaignRequest);
			return;
		}
		if ((SingletonAuto<CampaignContext>.isValidInstance && SingletonAuto<CampaignContext>.instance.IsGuildRaidCampaign()) || (SingletonAuto<CampaignContext>.isValidInstance && SingletonAuto<CampaignContext>.instance.IsWorldBossCampaign()) || (SingletonAuto<CampaignContext>.isValidInstance && SingletonAuto<CampaignContext>.instance.IsGuildSuppressCampaign()))
		{
			GetInstance<EndBattleWin>().Open(this, NShared.DungeonDifficultyType.Easy, EndBattleWin.EndType.Complete, 0);
		}
		else if (endCampaignRequest.Completed)
		{
			GetInstance<EndBattleWin>().Open(this, endCampaignRequest.Star);
		}
		else
		{
			ProcessBattleLoseInternal();
		}
		RequestEndCampaign(endCampaignRequest);
	}

	private void ExecuteEndScenarioReplay(NShared.EndCampaign.Request endCampaignRequest)
	{
		if (endCampaignRequest.Completed)
		{
			GetInstance<EndBattleWin>().Open(this, endCampaignRequest.Star);
			RequestCompleteScenarioDungeon(new NShared.CompleteScenarioDungeon.Request
			{
				ChapterIndex = endCampaignRequest.ChapterIndex,
				DungeonIndex = endCampaignRequest.DungeonIndex
			});
		}
		else
		{
			ProcessBattleLoseInternal();
		}
	}

	private void ProcessBattleLoseInternal()
	{
		IDataProvider dataProvider = SingletonAuto<BattleContext>.instance.DataProvider;
		EndBattleLose endBattleLose = GetInstance<EndBattleLose>();
		endBattleLose.Open(this, dataProvider);
		bool flag = false;
		if (SingletonAuto<BattleContext>.isValidInstance && dataProvider != null && dataProvider.GetBattleType() == BattleType.Arena)
		{
			flag = true;
		}
		if (!flag)
		{
			SingletonAuto<ConditionChecker>.instance.FailDungeonCount = SingletonAuto<ConditionChecker>.instance.FailDungeonCount + 1;
			endBattleLose.SetAnimationFinishedAction(delegate
			{
				GetInstance<BattleLoseGuidePopup>().Open(this, (dataProvider != null) ? dataProvider.GetBattleType() : BattleType.None, (dataProvider != null) ? dataProvider.GetBattleDefine() : null, null);
			});
		}
		if (NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled())
		{
			NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.OnBattleLose();
		}
		else if (AutoNextBattle.IsEnabled())
		{
			AutoNextBattle.instance.OnBattleLose();
		}
	}

	public void OpenEclipseRewardPopup(int waveIndex)
	{
		EclipseRewardPopup.Open(this, waveIndex, endEclipse: false);
	}

	private bool CheckEnterCountByUnderPrison()
	{
		if (_rewardStock == null)
		{
			return true;
		}
		if (_rewardStock.EnterDungeonData == null)
		{
			return true;
		}
		if (_rewardStock.EnterDungeonData.LinkedDungeonData == null)
		{
			return true;
		}
		if (_rewardStock.EnterDungeonData.LinkedDungeonData.BattleType != BattleType.UnderPrison)
		{
			return true;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.UnderPrisonManager.GetRemainEnterCountByChapterIndex(_rewardStock.EnterDungeonData.LinkedDungeonData.ChapterIndex) > 0;
	}

	private void OpenBaseRewardPopup(string title)
	{
		if (_endCampaignProxy != null)
		{
			BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(title).SetItemResultInfos(_endCampaignProxy.ItemResults).SetEquipResultInfos(_endCampaignProxy.EquipItemInfos)
				.SetGuildPointResult(_endCampaignProxy.GuildPointResult)
				.AddCurrencyResultInfos3(_endCampaignProxy.CurrencyResultInfos3)
				.AddCurrencyResultInfos3(_endCampaignProxy.GuildCurrencyResults), isGlobal: false);
		}
	}

	private void OpenBaseRewardPopup()
	{
		string title = LocalizationManager.GetString(GuildRaid.GUILDRAID_DEFAULT_REWARD_TITLE);
		OpenBaseRewardPopup(title);
	}

	private void OpenChallengeRaidRewardPopup()
	{
		if (_endCampaignProxy != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect("ORDEALARENA_CLEAR_INFO");
			BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(stringDirect).SetItemResultInfos(_endCampaignProxy.ItemResults).SetEquipResultInfos(_endCampaignProxy.EquipItemInfos)
				.SetBoostEquipResultInfos(_endCampaignProxy.EquipItemBoostInfos)
				.AddCurrencyResultInfos3(_endCampaignProxy.CurrencyResultInfos3)
				.AddCurrencyResultInfos3(_endCampaignProxy.GuildCurrencyResults), isGlobal: false);
		}
	}

	void EndBattleWin.Listener.OnInputEnabled(EndBattleWin caller)
	{
		if (IsMultiplayCampaign())
		{
			if (SingletonAuto<CampaignContext>.isValidInstance && SingletonAuto<CampaignContext>.instance.IsGuildSuppressCampaign())
			{
				int raidIndex = SingletonAuto<CampaignContext>.instance.RaidIndex;
				int raidLevel = SingletonAuto<CampaignContext>.instance.RaidLevel;
				RaidData data = GameRoot.Instance.RaidDataContainer.GetData(raidIndex, raidLevel);
				if (data == null)
				{
					return;
				}
				int num = 0;
				int changedRank = 0;
				long addedScore = 0L;
				long totalScore = 0L;
				UserGuildManager myGuildManager = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager;
				if (myGuildManager != null)
				{
					GuildSuppressManager guildSuppressManager = myGuildManager.GuildSuppressManager;
					if (guildSuppressManager != null)
					{
						GuildSuppressScoreInfo guildSuppressScoreInfo = guildSuppressManager.GetGuildSuppressScoreInfo();
						num = guildSuppressScoreInfo?.Rank ?? 0;
						int lastGuildSuppressRank = GlobalAutoBehaviour<AccountManager>.instance.LastGuildSuppressRank;
						lastGuildSuppressRank = ((lastGuildSuppressRank > 0) ? lastGuildSuppressRank : num);
						changedRank = -(num - lastGuildSuppressRank);
						totalScore = guildSuppressManager.GetGuildSuppressInfo()?.TotalDamage ?? 0;
						addedScore = guildSuppressScoreInfo?.TotalDamage ?? 0;
					}
				}
				caller.ShowGuildSuppressResult(num, changedRank, totalScore, addedScore);
				if (data.Type == RaidType.GuildSuppressSingle)
				{
					string stringDirect = LocalizationManager.GetStringDirect("PUNITIVE_EXPEDITION_PLAY_REWARD_TITLE_DOUBLE");
					OpenBaseRewardPopup(stringDirect);
				}
				else
				{
					OpenBaseRewardPopup();
				}
			}
			else if (SingletonAuto<CampaignContext>.isValidInstance && SingletonAuto<CampaignContext>.instance.IsChallengeRaidMulti())
			{
				long score = ChallengeRaidWrapper.MyRankInfo.Score;
				long currentScore = ChallengeRaidWrapper.MyRankInfo.CurrentScore;
				long lastChallengeRaidBestScore = GlobalAutoBehaviour<AccountManager>.instance.LastChallengeRaidBestScore;
				long num2 = currentScore - lastChallengeRaidBestScore;
				if (num2 < 0)
				{
					num2 = 0L;
				}
				int battleTime = ChallengeRaidWrapper.MyRankInfo.BattleTime;
				int currentBattleTime = ChallengeRaidWrapper.MyRankInfo.CurrentBattleTime;
				caller.ShowChallengeRaidMultiResult(score, currentScore, num2, battleTime, currentBattleTime);
				OpenChallengeRaidRewardPopup();
			}
			else
			{
				GetInstance<EndBattleWin>().ActivateCloseTimeout();
			}
			return;
		}
		if (SingletonAuto<CampaignContext>.isValidInstance && SingletonAuto<CampaignContext>.instance.IsChallengeRaidSingle())
		{
			int num3 = ChallengeRaidWrapper.MyRankInfo.Rank + 1;
			int num4 = GlobalAutoBehaviour<AccountManager>.instance.LastChallengeRaidRank + 1;
			num4 = ((num4 > 0) ? num4 : num3);
			int changedRank2 = -(num3 - num4);
			long score2 = ChallengeRaidWrapper.MyRankInfo.Score;
			int battleTime2 = ChallengeRaidWrapper.MyRankInfo.BattleTime;
			long currentScore2 = ChallengeRaidWrapper.MyRankInfo.CurrentScore;
			long lastChallengeRaidBestScore2 = GlobalAutoBehaviour<AccountManager>.instance.LastChallengeRaidBestScore;
			long num5 = currentScore2 - lastChallengeRaidBestScore2;
			if (num5 < 0)
			{
				num5 = 0L;
			}
			int currentBattleTime2 = ChallengeRaidWrapper.MyRankInfo.CurrentBattleTime;
			caller.ShowChallengeRaidSingleResult(num3, changedRank2, score2, battleTime2, currentScore2, num5, currentBattleTime2);
			OpenChallengeRaidRewardPopup();
		}
		if (SingletonAuto<CampaignContext>.isValidInstance && SingletonAuto<CampaignContext>.instance.IsWorldBossCampaign())
		{
			int num6 = SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.RankInfo.Rank + 1;
			int num7 = SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.LastWorldBossRank + 1;
			num7 = ((num7 > 0) ? num7 : num6);
			int changedRank3 = -(num6 - num7);
			long score3 = SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.RankInfo.Score;
			long lastWorldBossScore = SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.LastWorldBossScore;
			lastWorldBossScore = ((lastWorldBossScore >= 0) ? lastWorldBossScore : score3);
			long addedScore2 = score3 - lastWorldBossScore;
			caller.ShowWorldBossResult(num6, changedRank3, score3, addedScore2);
			string stringDirect2 = LocalizationManager.GetStringDirect("WORLDBOSS_DAILY_ACHIEVEMENT_REWARD");
			OpenBaseRewardPopup(stringDirect2);
		}
		if (!SingletonAuto<CampaignContext>.isValidInstance || !SingletonAuto<CampaignContext>.instance.IsGuildRaidCampaign())
		{
			return;
		}
		int num8 = 0;
		int changedRank4 = 0;
		long num9 = 0L;
		long addedScore3 = 0L;
		UserGuildManager myGuildManager2 = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager;
		if (myGuildManager2 != null)
		{
			GuildRaidManager guildRaidManager = myGuildManager2.GuildRaidManager;
			if (guildRaidManager != null)
			{
				if (SingletonAuto<CampaignContext>.instance.EnterDungeon == null)
				{
					return;
				}
				int chapter = SingletonAuto<CampaignContext>.instance.EnterDungeon.Chapter;
				if (guildRaidManager.GetGuildRaidDungeonInfo(chapter) == null)
				{
					return;
				}
				num8 = guildRaidManager.GetGuildRaidScoreRank(GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId);
				int lastGuildRaidRank = GlobalAutoBehaviour<AccountManager>.instance.LastGuildRaidRank;
				lastGuildRaidRank = ((lastGuildRaidRank > 0) ? lastGuildRaidRank : num8);
				changedRank4 = -(num8 - lastGuildRaidRank);
				num9 = guildRaidManager.GetGuildRaidScoreInfo(GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId)?.Score ?? 0;
				long lastGuildRaidScore = GlobalAutoBehaviour<AccountManager>.instance.LastGuildRaidScore;
				lastGuildRaidScore = ((lastGuildRaidScore >= 0) ? lastGuildRaidScore : num9);
				addedScore3 = NVespa.Math.Max(0L, num9 - lastGuildRaidScore);
			}
		}
		caller.ShowGuildRaidResult(num8, changedRank4, num9, addedScore3);
		OpenBaseRewardPopup();
	}

	bool EndBattleWin.Listener.OnCloseRequest(EndBattleWin closeThis)
	{
		if ((SingletonAuto<CampaignContext>.isValidInstance && SingletonAuto<CampaignContext>.instance.IsGuildRaidCampaign()) || (SingletonAuto<CampaignContext>.isValidInstance && SingletonAuto<CampaignContext>.instance.IsWorldBossCampaign()) || (SingletonAuto<CampaignContext>.isValidInstance && SingletonAuto<CampaignContext>.instance.IsGuildSuppressCampaign()))
		{
			return false;
		}
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
		if (!CheckEnterCountByUnderPrison())
		{
			if (NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled())
			{
				NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.OnClickStop(sendRoomRepeat: true);
			}
			if (!_rewardStock.EnterDungeonData.CheckIsNotInitDailyCount())
			{
				MessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), LocalizationManager.GetString(NShared.BeginCampaign.ResultType.ExceededDailyEntryCount));
			}
		}
		else
		{
			ProcessBattleRepeat();
		}
	}

	void EndBattleReward.Listener.ReturnToLobby()
	{
		ReturnToLobby();
	}

	void EndBattleReward.Listener.ReturnToLobbyAndOpenNextDungeon()
	{
		if (SprkStoryAuto.Advance(this))
		{
			return;
		}
		EnterDungeonData enterDungeonData = _rewardStock.EnterDungeonData;
		int nextDungeonIndex = enterDungeonData.OriginalDungeonData.NextDungeonIndex;
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(enterDungeonData.Chapter, nextDungeonIndex);
		if (!CheckNextDungeon(data))
		{
			AutoNextBattle.StopWithClickButton();
		}
		else if (_rewardStock.EnterDungeonData != null && _rewardStock.EnterDungeonData.ChapterData != null && _rewardStock.EnterDungeonData.ChapterData.Type == ChapterType.Tower && !AutoNextBattle.IsEnabled())
		{
			if (NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled())
			{
				NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.OnClickStop(sendRoomRepeat: true);
			}
			MessagePopup3Button.Open(LocalizationManager.GetStringDirect("UI_COMMON_NEXT_DUNGEON"), LocalizationManager.GetStringDirect("TOWER_CONTINUOUS_BATTLE_ASK_CONTENT"), button1Label: LocalizationManager.GetStringDirect("UI_COMMON_NEXT_DUNGEON"), button2Label: LocalizationManager.GetStringDirect("TOWER_CONTINUOUS_BATTLE_BUTTON"), onButton1: delegate
			{
				LobbyScene2.ReturnState.OpenFlag |= LobbyScene2.ReturnStateOpenFlag.OpenNextDungeon;
				SingletonAuto<BattleContext>.instance.MoveSceneToLobby(null, (!_win) ? EBoolCheck.False : EBoolCheck.True);
			}, onButton2: delegate
			{
				string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
				string stringDirect = LocalizationManager.GetStringDirect("TOWER_CONTINUOUS_BATTLE_MESSAGE");
				MessagePopup.OpenEnabled(title, stringDirect, delegate
				{
					AutoNextBattle.OpenGlobal();
					LobbyScene2.ReturnState.OpenFlag |= LobbyScene2.ReturnStateOpenFlag.OpenNextDungeon;
					SingletonAuto<BattleContext>.instance.MoveSceneToLobby(null, (!_win) ? EBoolCheck.False : EBoolCheck.True);
				}, null, "COMMON_CONFIRM", bYesEnabled: true);
			});
		}
		else
		{
			LobbyScene2.ReturnState.OpenFlag |= LobbyScene2.ReturnStateOpenFlag.OpenNextDungeon;
			SingletonAuto<BattleContext>.instance.MoveSceneToLobby(null, (!_win) ? EBoolCheck.False : EBoolCheck.True);
		}
	}

	void EndBattleReward.Listener.ReturnToLobbyAndOpenNextChapterHideOut()
	{
		EnterDungeonData enterDungeonData = _rewardStock.EnterDungeonData;
		CampaignDungeonData linkedDungeonData = enterDungeonData.LinkedDungeonData;
		_ = linkedDungeonData.ReqStamina;
		if (!GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(linkedDungeonData.ReqStaminaType, out var currentValue, out var maxValue, enterDungeonData.Chapter))
		{
			AutoNextBattle.StopWithClickButton();
			return;
		}
		CampaignDungeonData campaignDungeonData = null;
		long num = currentValue;
		int num2 = 0;
		if (num > num2)
		{
			if (!CheckNextDungeon(linkedDungeonData))
			{
				AutoNextBattle.StopWithClickButton();
			}
			else
			{
				SingletonAuto<BattleContext>.instance.RepeatCampaign(null);
			}
			return;
		}
		int chapter = enterDungeonData.Chapter;
		IList<CampaignDungeonData> allDataOfBattleType = GameRoot.Instance.CampaignDungeonDataContainer.GetAllDataOfBattleType(BattleType.Hideout, chapter);
		IList<CampaignDungeonData> allDataOfBattleType2 = GameRoot.Instance.CampaignDungeonDataContainer.GetAllDataOfBattleType(BattleType.Hideout, chapter + 1);
		if (allDataOfBattleType2.Count <= 0)
		{
			AutoNextBattle.StopWithClickButton();
			return;
		}
		int num3 = -1;
		for (int i = 0; i < allDataOfBattleType.Count; i++)
		{
			if (allDataOfBattleType[i].ChapterIndex == enterDungeonData.Chapter && allDataOfBattleType[i].DungeonIndex == enterDungeonData.LinkedDungeonData.DungeonIndex)
			{
				num3 = i;
				break;
			}
		}
		if (allDataOfBattleType2.Count >= num3)
		{
			campaignDungeonData = allDataOfBattleType2[num3];
		}
		if (num3 == -1 || campaignDungeonData == null)
		{
			AutoNextBattle.StopWithClickButton();
			return;
		}
		if (!GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(campaignDungeonData.ReqStaminaType, out currentValue, out maxValue, campaignDungeonData.ChapterIndex))
		{
			AutoNextBattle.StopWithClickButton();
			return;
		}
		if (!CheckNextDungeon(campaignDungeonData))
		{
			AutoNextBattle.StopWithClickButton();
			return;
		}
		LobbyScene2.ReturnState.OpenFlag |= LobbyScene2.ReturnStateOpenFlag.OpenNextChapterHideOut;
		SingletonAuto<BattleContext>.instance.MoveSceneToLobby(null, (!_win) ? EBoolCheck.False : EBoolCheck.True);
	}

	void EndBattleReward.Listener.ReturnToLobbyAndOpenNextChapterConquestDungeon()
	{
		int num = 0;
		long maxValue = num;
		int num2 = 0;
		long currentValue = num2;
		EnterDungeonData enterDungeonData = _rewardStock.EnterDungeonData;
		CampaignDungeonData linkedDungeonData = enterDungeonData.LinkedDungeonData;
		if (!GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(linkedDungeonData.ReqStaminaType, out currentValue, out maxValue, enterDungeonData.Chapter))
		{
			AutoNextBattle.StopWithClickButton();
			return;
		}
		CampaignDungeonData campaignDungeonData = null;
		long num3 = currentValue;
		int num4 = 0;
		if (num3 > num4)
		{
			if (!CheckNextDungeon(linkedDungeonData))
			{
				AutoNextBattle.StopWithClickButton();
			}
			else
			{
				SingletonAuto<BattleContext>.instance.RepeatCampaign(null);
			}
			return;
		}
		int chapter = enterDungeonData.Chapter;
		CampaignDungeonData[] conquestDungeonData = GameRoot.Instance.CampaignDungeonDataContainer.GetConquestDungeonData(chapter + 1);
		if (__Extension_Method_ICollection__.COUNT(conquestDungeonData) <= 0)
		{
			AutoNextBattle.StopWithClickButton();
			return;
		}
		campaignDungeonData = conquestDungeonData[0];
		if (campaignDungeonData == null)
		{
			AutoNextBattle.StopWithClickButton();
			return;
		}
		if (!GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CheckConquestDungeon(campaignDungeonData.ChapterIndex, campaignDungeonData.DungeonIndex))
		{
			AutoNextBattle.StopWithClickButton();
			return;
		}
		if (!CheckNextDungeon(campaignDungeonData))
		{
			AutoNextBattle.StopWithClickButton();
			return;
		}
		LobbyScene2.ReturnState.OpenFlag |= LobbyScene2.ReturnStateOpenFlag.OpenNextChapterConquestDungeon;
		SingletonAuto<BattleContext>.instance.MoveSceneToLobby(null, (!_win) ? EBoolCheck.False : EBoolCheck.True);
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
		if (AutoNextBattle.IsEnabled())
		{
			long itemBoosterCount = AutoNextBattle.instance.ItemBoosterCount;
			int num = 0;
			if (itemBoosterCount > num)
			{
				string value = GameRoot.Instance.ConstantDataContainer.GetValue("OneTimeItemBoosterCode");
				ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(value);
				UserManager userManager = GlobalAutoBehaviour<AccountManager>.instance.UserManager;
				if (dataByCode != null)
				{
					long i_valueA = 0L;
					long totalGem = userManager.TotalGem;
					int buyGem = dataByCode.BuyGem;
					if (NVespa.Math.Max(i_valueA, totalGem / buyGem) == 0L)
					{
						string stringDirect = LocalizationManager.GetStringDirect("BOOSTER_ONETIME_REWARD_INSUFFICIENT_RUBY");
						MessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), stringDirect);
						return false;
					}
				}
			}
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
		if (AutoNextBattle.IsEnabled() && (AutoNextBattle.instance.AutoNextBattleType == BattleType.Campaign || AutoNextBattle.instance.AutoNextBattleType == BattleType.Conquest))
		{
			NShared.DungeonDifficultyType dungeonDifficultyType = SprkStoryAuto.NextDifficulty(AutoNextBattle.instance.DungeonDifficulty, nextDungeon);
			NShared.DungeonDifficultyType difficulty = ((dungeonDifficultyType > NShared.DungeonDifficultyType.Easy) ? (dungeonDifficultyType - 1) : NShared.DungeonDifficultyType.Easy);
			if (dungeonDifficultyType != NShared.DungeonDifficultyType.Easy && !SprkStoryAuto.LowerDifficultyCleared(GlobalAutoBehaviour<AccountManager>.instance.CampaignManager, nextDungeon.ChapterIndex, nextDungeon.DungeonIndex, difficulty))
			{
				return false;
			}
			bool openDifficulty = GameRoot.Instance.CampaignDungeonDataContainer.GetData(nextDungeon.ChapterIndex, nextDungeon.DungeonIndex).OpenDifficulty;
			int num2 = 0;
			num2 = ((!openDifficulty) ? Helper.GetMaxAvailableDifficulty(nextDungeon.ChapterIndex, nextDungeon.DungeonIndex) : Helper.GetMaxDifficulty(nextDungeon.ChapterIndex, nextDungeon.DungeonIndex));
			if (num2 < (int)dungeonDifficultyType)
			{
				return false;
			}
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
		if (AutoNextBattle.IsEnabled())
		{
			AutoNextBattle.instance.OnClickStop();
		}
		if (_rewardStock != null && !CheckEnterCount(_rewardStock.EnterDungeonData))
		{
			NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.OnClickStop();
			if (!_rewardStock.EnterDungeonData.CheckIsNotInitDailyCount())
			{
				MessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), LocalizationManager.GetString(NShared.BeginCampaign.ResultType.ExceededDailyEntryCount));
			}
			return;
		}
		EnterDungeonData enterDungeonData = _rewardStock.EnterDungeonData;
		CampaignDungeonData linkedDungeonData = enterDungeonData.LinkedDungeonData;
		MemberInfo myPartyMemberInfo = GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo();
		if (GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.IsRepeatBattle)
		{
			int reqStamina = linkedDungeonData.ReqStamina;
			if (!GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(linkedDungeonData.ReqStaminaType, out var currentValue, out var maxValue, enterDungeonData.Chapter) && !GetStamina(out currentValue, out maxValue, out reqStamina))
			{
				NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.OnClickStop(sendRoomRepeat: true);
				return;
			}
			if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CheckEventDungeon(enterDungeonData.Chapter, enterDungeonData.LinkedDungeon, out var _, out var _))
			{
				reqStamina = 1;
			}
			SaveStaminaBoosterInfo saveStaminaBoosterInfo = GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina();
			if (saveStaminaBoosterInfo.IsActiveBattleType(linkedDungeonData.BattleType))
			{
				int num = (int)System.Math.Floor((float)(reqStamina * saveStaminaBoosterInfo.SaveStaminaValue / 100));
				reqStamina -= num;
				if (reqStamina < 0)
				{
					reqStamina = 0;
				}
			}
			long num2 = currentValue;
			int num3 = reqStamina;
			if (num2 < num3)
			{
				string autoRepeatStopMessage = GetAutoRepeatStopMessage(linkedDungeonData);
				bool flag = IsRaidBattleType(linkedDungeonData.BattleType);
				if (flag)
				{
					NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.OnClickStop(sendRoomRepeat: true, PartyRoomRepeat.ResultType.CANCEL_STAMINA);
					MessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), autoRepeatStopMessage);
					return;
				}
				if (!flag && myPartyMemberInfo.UseStamina)
				{
					if (myPartyMemberInfo.IsMaster)
					{
						NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.OnClickStop(sendRoomRepeat: true, PartyRoomRepeat.ResultType.CANCEL_STAMINA);
						MessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), autoRepeatStopMessage);
						return;
					}
					myPartyMemberInfo.UseStamina = false;
				}
			}
			if (GlobalAutoBehaviour<AccountManager>.instance.IsMaxEquipItemCount())
			{
				NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.OnClickStop(sendRoomRepeat: true, PartyRoomRepeat.ResultType.CANCEL_IVENTORY);
				string stringDirect = LocalizationManager.GetStringDirect("REPEATBATTLE_FAIL_INVENTORY");
				MessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), stringDirect);
				return;
			}
			long onetimeBoosterCount = SingletonAuto<CampaignContext>.instance.OnetimeBoosterCount;
			int num4 = 0;
			if (onetimeBoosterCount <= num4)
			{
				DisableOnetimeBoosterRepeatBattle();
				myPartyMemberInfo.UseOnetimeBooster = false;
			}
			if (myPartyMemberInfo.IsMaster)
			{
				coWaitPartyMember = VespaCoroutine.Start(WaitPartyMember());
				coCheckPartyReadyAll = VespaCoroutine.Start(CheckPartyReadyAll());
			}
			else
			{
				GlobalAutoBehaviour<PartyManager>.instance.SendRoomReady(isReady: true);
				coWaitPartyMember = VespaCoroutine.Start(WaitPartyMember());
			}
			return;
		}
		if (linkedDungeonData != null && linkedDungeonData.BattleType == BattleType.Shakmeh && linkedDungeonData.ReqStaminaType == StaminaType.ShakmehMiddleBossKey)
		{
			CurrencyTypeData data = GameRoot.Instance.CurrencyTypeDataContainer.GetData(CurrencyType.ShakmehMiddleBossPoint);
			long userStat = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(UserManager.UserStatType.ShakmehMiddleBossPoint);
			int maxValue2 = data.MaxValue;
			if (userStat >= maxValue2)
			{
				NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.OnClickStop();
				string stringDirect2 = LocalizationManager.GetStringDirect("EVENT_HIDDEN_DUNGEON_INFO");
				string stringDirect3 = LocalizationManager.GetStringDirect("FINAL_BOSS_SHAKMEH_POINT_FULL_NOTICE_DESC");
				MessagePopup.Open(stringDirect2, stringDirect3);
				return;
			}
		}
		int chapterIndex = linkedDungeonData.ChapterIndex;
		int dungeonIndex = linkedDungeonData.DungeonIndex;
		SelectRewardData data2 = GameRoot.Instance.SelectRewardDataContainer.GetData(chapterIndex, dungeonIndex);
		bool isScenarioReplay = _rewardStock.EnterDungeonData.IsScenarioReplay;
		if (!extraOption.AutoRepeat && data2 != null && data2.IsSelectable() && !isScenarioReplay)
		{
			UnderPrisonData data3 = GameRoot.Instance.UnderPrisonDataContainer.GetData(chapterIndex);
			if (data3 != null && data3.Type == UnderPrisonType.SkillBook)
			{
				SkillBookSelectRewardPopup.OpenGlobal(linkedDungeonData.BattleType, data2, this, delegate
				{
					SingletonAuto<BattleContext>.instance.RepeatCampaign(extraOption);
				}, SelectRewardButtonType.OKCancel);
			}
			else
			{
				PreSelectRewardPopup.Open(linkedDungeonData.BattleType, data2, this, delegate
				{
					SingletonAuto<BattleContext>.instance.RepeatCampaign(extraOption);
				});
			}
		}
		else if (extraOption.AutoRepeat && data2 != null && data2.IsSelectable() && !isScenarioReplay)
		{
			SingletonAuto<BattleContext>.instance.RepeatCampaign(extraOption);
		}
		else
		{
			NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.ClearSelectedReward();
			SingletonAuto<BattleContext>.instance.RepeatCampaign(extraOption);
		}
	}

	private string GetAutoRepeatStopMessage(CampaignDungeonData campaignDungeonData)
	{
		string stringDirect = LocalizationManager.GetStringDirect("REPEATBATTLE_FAIL_STAMINA");
		if (campaignDungeonData == null)
		{
			return stringDirect;
		}
		StaminaData data = GameRoot.Instance.StaminaDataContainer.GetData(campaignDungeonData.ReqStaminaType);
		if (data != null && !String_Extension_Method.IsNullOrEmpty(data.AutoRepeatStopMsgKey))
		{
			return LocalizationManager.GetStringDirect(data.AutoRepeatStopMsgKey);
		}
		if (campaignDungeonData.BattleType == BattleType.Conquest || campaignDungeonData.BattleType == BattleType.Hideout)
		{
			stringDirect = LocalizationManager.GetStringDirect("REPEATBATTLE_FAIL_TICKET");
		}
		return stringDirect;
	}

	private bool CheckEnterCount(EnterDungeonData enterDungeonData)
	{
		if (enterDungeonData != null && enterDungeonData.LinkedDungeonData != null)
		{
			return GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CheckEnterCount(enterDungeonData);
		}
		return true;
	}

	private IEnumerator WaitPartyMember()
	{
		int waitTime = GameRoot.Instance.ConstantDataContainer.GetValueInt("RepeatBattleMultiRewardWaiting");
		if (waitTime == 0)
		{
			waitTime = 20;
		}
		float beginTime = Time.realtimeSinceStartup;
		while (!(Time.realtimeSinceStartup - beginTime > (float)waitTime))
		{
			yield return new WaitForSeconds(0.5f);
		}
		NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.OnClickStop(sendRoomRepeat: true);
	}

	private IEnumerator CheckPartyReadyAll()
	{
		while (true)
		{
			bool flag = true;
			if (!GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.IsRepeatBattle)
			{
				break;
			}
			foreach (MemberInfo partyMember in GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.PartyMembers)
			{
				if (!partyMember.IsMaster)
				{
					if (partyMember.Status == MemberStatus.Battle)
					{
						flag = false;
					}
					if (!partyMember.IsBattleReady)
					{
						flag = false;
					}
				}
			}
			if (GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.IsRepeatBattle && flag)
			{
				if (IsRaidBattleType(_rewardStock.EnterDungeonData.LinkedDungeonData.BattleType))
				{
					GlobalAutoBehaviour<PartyManager>.instance.StartBattle(GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.RaidIndex, GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.RaidLevel, 0, delegate(PartyManager.StartBattleResult result)
					{
						GlobalAutoBehaviour<PartyManager>.instance.ProcessStartBattleResult(result, isMaster: true);
					});
					break;
				}
				int[] heroIndices = _rewardStock.HeroIndices;
				long[] accountIds = _rewardStock.AccountIds;
				GlobalAutoBehaviour<PartyManager>.instance.StartBattleParty(heroIndices, accountIds, SingletonAuto<CampaignContext>.instance.OnetimeBoosterCount, delegate(PartyManager.StartBattleResult result)
				{
					if (!__Extension_Method_ICollection__.IsNullOrEmpty(accountIds) && !__Extension_Method_ICollection__.IsNullOrEmpty(heroIndices))
					{
						GlobalAutoBehaviour<PartyManager>.instance.SetPartyInfoForEndBattle(accountIds, heroIndices);
					}
					GlobalAutoBehaviour<PartyManager>.instance.ProcessStartBattleResult(result, isMaster: true);
				});
				break;
			}
			yield return new WaitForSeconds(0.5f);
		}
	}

	public void ShowStatistics()
	{
		if (NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled())
		{
			NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.OnClickStop(sendRoomRepeat: true);
		}
		else if (AutoNextBattle.IsEnabled())
		{
			AutoNextBattle.instance.OnClickStop();
		}
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

	private bool IsMultiplayCampaign()
	{
		CampaignContext context = SingletonAuto<BattleContext>.instance.GetContext<CampaignContext>();
		if (context == null)
		{
			return false;
		}
		return context.MultiplayMasterId > 0;
	}

	private bool CanSelectMultiplayReward()
	{
		if (_endCampaignProxy == null)
		{
			return false;
		}
		if (__Extension_Method_ICollection__.COUNT(_endCampaignProxy.MultiplayMemberIds) == 0)
		{
			return false;
		}
		return TypeIterator.Contains(_endCampaignProxy.MultiplayMemberIds, GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId);
	}

	private static bool CheckNewQuestDungeon(WorldMapEventInfo[] WorldMapEventInfos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(WorldMapEventInfos); i++)
		{
			WorldMapEventInfo worldMapEventInfo = WorldMapEventInfos[i];
			if (GlobalAutoBehaviour<AccountManager>.instance.WorldMapEventManager.Get(worldMapEventInfo.EventIndex, worldMapEventInfo.ChapterIndex, worldMapEventInfo.DungeonIndex) == null && worldMapEventInfo.EventType == NShared.NWorldMap.EventType.QuestDungeon)
			{
				return true;
			}
		}
		return false;
	}

	private static QuestDungeon CheckEndQuestDungeon(int chapterIndex, int dungeonIndex, WorldMapEventInfo[] worldMapEventInfos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(worldMapEventInfos); i++)
		{
			WorldMapEventInfo worldMapEventInfo = worldMapEventInfos[i];
			if (worldMapEventInfo.ChapterIndex == chapterIndex && worldMapEventInfo.DungeonIndex == dungeonIndex && worldMapEventInfo.EventType == NShared.NWorldMap.EventType.QuestDungeon && worldMapEventInfo.Completed)
			{
				QuestDungeon eventData = GlobalAutoBehaviour<AccountManager>.instance.WorldMapEventManager.GetEventData<QuestDungeon>(chapterIndex, dungeonIndex);
				if (eventData == null || !eventData.Completed)
				{
					return GlobalAutoBehaviour<AccountManager>.instance.WorldMapEventManager.ToEventData<QuestDungeon>(worldMapEventInfo);
				}
			}
		}
		return null;
	}

	private void ProcessNextTeam()
	{
		EclipseInfo eclipseInfo = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseInfo();
		if (eclipseInfo != null)
		{
			EclipseDungeonInfo eclipseRewardInfo = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseRewardInfo();
			if (eclipseRewardInfo != null)
			{
				int chapterIndex = eclipseInfo.ChapterIndex;
				int dungeonIndex = eclipseInfo.DungeonIndex;
				EnterDungeonData enterDungeon = new EnterDungeonData(chapterIndex, dungeonIndex, dungeonIndex, NShared.DungeonDifficultyType.Easy);
				BattlePartySetting.ExtraOptionData extraOption = new BattlePartySetting.ExtraOptionData
				{
					OnlineGameSpeedRatio = eclipseRewardInfo.OnlineGameSpeedRatio,
					EnterTicketCount = eclipseRewardInfo.EnterTicketCount
				};
				SingletonAuto<BattleContext>.instance.RequestBeginEclipse(enterDungeon, extraOption);
			}
		}
	}

	public void ProcessNextTeamDirect(int chapterIndex, int dungeonIndex)
	{
		EclipseDungeonInfo eclipseRewardInfo = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseRewardInfo();
		if (eclipseRewardInfo != null)
		{
			EnterDungeonData enterDungeon = new EnterDungeonData(chapterIndex, dungeonIndex, dungeonIndex, NShared.DungeonDifficultyType.Easy);
			BattlePartySetting.ExtraOptionData extraOption = new BattlePartySetting.ExtraOptionData
			{
				OnlineGameSpeedRatio = eclipseRewardInfo.OnlineGameSpeedRatio,
				EnterTicketCount = eclipseRewardInfo.EnterTicketCount
			};
			SingletonAuto<BattleContext>.instance.RequestBeginEclipse(enterDungeon, extraOption);
		}
	}

	private bool AllPartyMemberSelectOrForgiveReward()
	{
		if (_endCampaignProxy != null)
		{
			int num = __Extension_Method_ICollection__.COUNT(_endCampaignProxy.MultiplayMemberIds);
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				long accountId = _endCampaignProxy.MultiplayMemberIds[i];
				MemberInfo partyMemberInfo = GlobalAutoBehaviour<PartyManager>.instance.GetPartyMemberInfo(accountId);
				if (partyMemberInfo != null && partyMemberInfo.DidSelectReward())
				{
					num2++;
				}
			}
			if (num2 == num)
			{
				return true;
			}
		}
		return false;
	}

	void PartyManager.IRewardListener.OnSelectRewardItem(MemberInfo selectedMemberInfo)
	{
		GetInstance<MultiplayReward>().OnPartyMemberSelectRewardItem(selectedMemberInfo);
		if (AllPartyMemberSelectOrForgiveReward())
		{
			RequestGetRewardResultTask();
		}
	}

	bool MultiplayReward.Listener.IsPartyValid()
	{
		return GlobalAutoBehaviour<PartyManager>.instance.PartyInfo?.IsValid() ?? false;
	}

	void MultiplayReward.Listener.OnAbandon(MultiplayReward caller)
	{
		OnAbandon(caller);
	}

	private void OnAbandon(MultiplayReward rewardWinddow = null)
	{
		long masterId = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.GetMasterId();
		string masterServerPrivateIp = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.MasterServerPrivateIp;
		int masterServerPort = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.MasterServerPort;
		if (masterId <= 0)
		{
			return;
		}
		NShared.AbandonMultiplayReward.Request request = new NShared.AbandonMultiplayReward.Request
		{
			MasterAccountId = masterId,
			MasterServerIp = masterServerPrivateIp,
			MasterServerPort = masterServerPort
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.AbandonMultiplayReward.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				AutoBattle.Stop();
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result == NShared.AbandonMultiplayReward.ResultType.Success)
			{
				if (rewardWinddow != null)
				{
					rewardWinddow.SetSelected();
				}
				GlobalAutoBehaviour<PartyManager>.instance.SendSelectRewardItem(-2);
				if (response.RewardCalculated)
				{
					RequestGetRewardResultTask();
				}
			}
			else if (response.Result == NShared.AbandonMultiplayReward.ResultType.AlreadyRewardCalculated)
			{
				if (rewardWinddow != null)
				{
					rewardWinddow.SetSelected();
				}
				RequestGetRewardResultTask();
			}
			else
			{
				AutoBattle.Stop();
				MessagePopup.OpenError(response.Result);
			}
		});
	}

	void MultiplayReward.Listener.OnBid(MultiplayReward caller)
	{
		if (caller.CurrentRewardItemInfo == null)
		{
			return;
		}
		int rewardItemNo = caller.CurrentRewardItemNo;
		if (rewardItemNo < 0)
		{
			return;
		}
		long masterId = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.GetMasterId();
		if (masterId <= 0)
		{
			return;
		}
		string masterServerPrivateIp = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.MasterServerPrivateIp;
		int masterServerPort = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.MasterServerPort;
		NShared.ApplyForMultiplayReward.Request request = new NShared.ApplyForMultiplayReward.Request
		{
			MasterAccountId = masterId,
			MasterServerIp = masterServerPrivateIp,
			MasterServerPort = masterServerPort,
			RewardItemNo = rewardItemNo
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ApplyForMultiplayReward.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				AutoBattle.Stop();
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result == NShared.ApplyForMultiplayReward.ResultType.Success)
			{
				caller.SetSelected();
				GlobalAutoBehaviour<PartyManager>.instance.SendSelectRewardItem(rewardItemNo);
				if (response.RewardCalculated)
				{
					RequestGetRewardResultTask();
				}
			}
			else if (response.Result == NShared.ApplyForMultiplayReward.ResultType.AlreadyRewardCalculated)
			{
				caller.SetSelected();
				RequestGetRewardResultTask();
			}
			else
			{
				AutoBattle.Stop();
				MessagePopup.OpenError(response.Result);
			}
		});
	}

	void MultiplayReward.Listener.OnRewardCalculationEnd(MultiplayReward caller, NShared.GetMultiplayReward.Response response)
	{
		base.TaskFactory.Close<MultiplayReward>().Commit();
	}

	void MultiplayReward.Listener.RequestGetReward()
	{
		RequestGetRewardResultTask();
	}

	private void RequestGetRewardResultTask(int retryCount = 0)
	{
		if (_multiplayRewardRequested || _multiplayRewardCalculationStarted)
		{
			return;
		}
		MultiplayReward multiplayReward = null;
		if (!GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.IsRepeatBattle)
		{
			multiplayReward = GetInstance<MultiplayReward>(openedOnly: true);
		}
		if (retryCount >= 3)
		{
			MessagePopup.OpenError(NShared.GetMultiplayReward.ResultType.Fail);
			if (multiplayReward != null)
			{
				multiplayReward.Close();
			}
			return;
		}
		if (GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.GetMasterId() <= 0)
		{
			if (multiplayReward != null)
			{
				multiplayReward.Close();
			}
			return;
		}
		_multiplayRewardRequested = true;
		long masterId = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.GetMasterId();
		string masterServerPrivateIp = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.MasterServerPrivateIp;
		int masterServerPort = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.MasterServerPort;
		NShared.GetMultiplayReward.Request request = new NShared.GetMultiplayReward.Request
		{
			MasterAccountId = masterId,
			MasterServerIp = masterServerPrivateIp,
			MasterServerPort = masterServerPort
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetMultiplayReward.Response response)
		{
			_multiplayRewardRequested = false;
			if (!_multiplayRewardCalculationStarted)
			{
				if (response.BaseResult != BaseResultType.Success)
				{
					AutoBattle.Stop();
					VespaDebug.LogError(response.BaseResult);
					if (multiplayReward != null)
					{
						multiplayReward.Close();
					}
				}
				else if (response.Result == NShared.GetMultiplayReward.ResultType.Success)
				{
					GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.EquipItemInfos);
					GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
					if (multiplayReward != null)
					{
						multiplayReward.StartRewardCalculation(response);
					}
					_multiplayRewardEquipItems = response.EquipItemInfos;
					_multiplayRewardItemResults = response.ItemResults;
					_multiplayRewardCalculationStarted = true;
				}
				else if (response.Result == NShared.GetMultiplayReward.ResultType.NotCalculated)
				{
					int newRetryCount = retryCount + 1;
					SingletonUpdater<Executer>.instance.DelayRun(3f, this, delegate
					{
						RequestGetRewardResultTask(newRetryCount);
					});
				}
				else
				{
					AutoBattle.Stop();
					MessagePopup.OpenError(response.Result);
					if (multiplayReward != null)
					{
						multiplayReward.Close();
					}
				}
			}
		});
	}

	void NewRaid.Listener.OnRaidList(NewRaid caller)
	{
		RaidListReturnState item = new RaidListReturnState();
		LobbyScene2.ListReturnState.Clear();
		LobbyScene2.ListReturnState.Add(item);
		ReturnToLobby();
	}

	void PreSelectRewardPopup.Listener.TryStartBattle(BattlePartySetting.ExtraOptionData extraOption)
	{
		extraOption = new BattlePartySetting.ExtraOptionData();
		extraOption.AutoRepeat = true;
		int chapter = _rewardStock.EnterDungeonData.Chapter;
		int linkedDungeon = _rewardStock.EnterDungeonData.LinkedDungeon;
		BattleType battleType = _rewardStock.EnterDungeonData.LinkedDungeonData.BattleType;
		bool isScenarioReplay = _rewardStock.EnterDungeonData.IsScenarioReplay;
		SelectRewardData data = GameRoot.Instance.SelectRewardDataContainer.GetData(chapter, linkedDungeon);
		List<string> selectedReward = NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.GetSelectedReward();
		if (data != null && __Extension_Method_ICollection__.IsNullOrEmpty(selectedReward) && !isScenarioReplay)
		{
			switch (battleType)
			{
			case BattleType.UnderPrison:
				if (GameRoot.Instance.UnderPrisonDataContainer.GetData(chapter).Type == UnderPrisonType.SkillBook)
				{
					SkillBookSelectRewardPopup.OpenGlobal(battleType, data, this, delegate
					{
						BattleRepeat(extraOption);
					}, SelectRewardButtonType.OKCancel);
				}
				else
				{
					PreSelectRewardPopup.Open(battleType, data, this, delegate
					{
						BattleRepeat(extraOption);
					});
				}
				break;
			default:
				PreSelectRewardPopup.Open(battleType, data, this, delegate
				{
					BattleRepeat(extraOption);
				});
				break;
			}
		}
		else
		{
			NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.ClearSelectedReward();
			BattleRepeat(extraOption);
		}
	}

	public void GetMultiplayReward()
	{
		OpenRewardPopup();
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

	private bool IsRaidBattleType(BattleType battleType)
	{
		if (battleType == BattleType.Raid || battleType == BattleType.ChallengeRaid || battleType == BattleType.HardRaid || battleType == BattleType.FieldRaid || battleType == BattleType.GargoriaRaid || battleType == BattleType.SeigfriedRaid || battleType == BattleType.AscalonRaid || battleType == BattleType.ArcdimRaid || battleType == BattleType.ArcdimHardRaid)
		{
			return true;
		}
		return false;
	}

	public void OpenByEclipse(int[] heroIndices, long[] accountIds, bool win, int star, Dictionary<int, int> killMonsterCount, IList<int> aliveHeroIndices, int chapterIndex, int dungeonIndex, int difficulty, bool isScenario)
	{
		GlobalAutoBehaviour<PartyManager>.instance.ChangeMemberStatus(MemberStatus.EndCampaign);
		MessagePopup3Button messagePopup3Button = GlobalAutoBehaviour<PopupManegement>.instance.PopUpExist<MessagePopup3Button>();
		if (messagePopup3Button != null)
		{
			messagePopup3Button.Close2(immediately: true);
		}
		_win = win;
		SingletonAuto<BattleContext>.instance.SetLastDungeonResult((!_win) ? EBoolCheck.False : EBoolCheck.True);
		_rewardStock.HeroIndices = heroIndices;
		_rewardStock.AiHeroIndices = null;
		_rewardStock.GroupHeroInices = null;
		_rewardStock.AccountIds = accountIds;
		_rewardStock.OldLevelInfo = Helper.GetAccountTeamLevelInfo();
		_rewardStock.EnterDungeonData = SingletonAuto<EclipseContext>.instance._enterDungeon;
		_endCampaignProxy = null;
		_multiplayRewardTimeoutTime = 0f;
		_multiplayRewardItems = null;
		_multiplayRewardCalculationStarted = false;
		_newQuestDungeonHappened = false;
		_completedQuestDungeonEvent = null;
		NShared.EndCampaign.Request endEclipseRequest = GetEndEclipseRequest(win, star, killMonsterCount, aliveHeroIndices, chapterIndex, dungeonIndex, difficulty, isScenario);
		SingletonAuto<BattleContext>.instance.SetBattleResult(win, star);
		ExecuteEndEclipseRequest(endEclipseRequest);
		SceneAutoBehaviour<NVespa.NScene.GuiManager>.instance.Add("EndCampaign", this, delegate
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
			int num = Vespa_String_Extension_Method.TOINT(_testNewRaidIndex.Value, -1);
			int num2 = Vespa_String_Extension_Method.TOINT(_testNewRaidLevel.Value, -1);
			RaidData data = GameRoot.Instance.RaidDataContainer.GetData(num, num2);
			using (new BlockManager().Enable(data != null))
			{
				if (GUILayout.Button("NewRaid"))
				{
					NShared.RaidInfo newRaid = new NShared.RaidInfo();
					newRaid.RaidIndex = num;
					newRaid.RaidLevel = num2;
					ReservePopupWindowOpen(delegate(NewRaid window)
					{
						window.Open(this, newRaid, null);
					});
				}
			}
		});
	}

	private NShared.EndCampaign.Request GetEndEclipseRequest(bool win, int star, Dictionary<int, int> killMonsterCount, IList<int> aliveHeroIndices, int chapterIndex, int dungeonIndex, int difficulty, bool isScenario)
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
		string creatureInfoString = Json.Serialize(JsonPool.instance.CreateList());
		return new NShared.EndCampaign.Request
		{
			ChapterIndex = chapterIndex,
			DungeonIndex = dungeonIndex,
			DungeonDifficulty = difficulty,
			ScenarioDungeon = isScenario,
			Completed = win,
			Star = star,
			MonsterIndices = list.ToArray(),
			KillCounts = list2.ToArray(),
			AliveHeroIndices = aliveHeroIndices.ToArray(),
			BattleLogString = null,
			CreatureInfoString = creatureInfoString,
			PureBattleTime = 0,
			ElapsedBattleTime = 0L
		};
	}

	private void ExecuteEndEclipseRequest(NShared.EndCampaign.Request endCampaignRequest)
	{
		RequestEndEclipse(endCampaignRequest);
	}

	private void RequestEndEclipse(NShared.EndCampaign.Request request)
	{
		string[] values = new string[3]
		{
			request.ChapterIndex.ToString(),
			request.DungeonIndex.ToString(),
			((NShared.DungeonDifficultyType)request.DungeonDifficulty/*cast due to .constrained prefix*/).ToString()
		};
		SingletonAuto<ModifyChecker>.instance.CheckStart<FirstClearDungeon>(values);
		bool wasCompleted = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(request.ChapterIndex, request.DungeonIndex, (NShared.DungeonDifficultyType)request.DungeonDifficulty);
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.EndCampaign.Response response)
		{
			if ((response == null && NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled()) || (response != null && response.BaseResult != BaseResultType.Success) || (response != null && response.Result != NShared.EndCampaign.ResultType.Success))
			{
				AutoBattle.Stop();
			}
			if (response != null && response.BaseResult == BaseResultType.Success)
			{
				if (response.Result == NShared.EndCampaign.ResultType.Success)
				{
					_endCampaignProxy = new EndCampaignProxy();
					_endCampaignProxy.OnResponse(request, response);
					_multiplayRewardTimeoutTime = Time.time + (float)response.MultiplayRemainTimeoutTime;
					_multiplayRewardItems = response.MultiplayRewardItems;
					_newQuestDungeonHappened = CheckNewQuestDungeon(response.WorldMapEventInfos);
					_completedQuestDungeonEvent = CheckEndQuestDungeon(request.ChapterIndex, request.DungeonIndex, response.WorldMapEventInfos);
					if (response.CampaignResults != null)
					{
						ChapterDungeonInfo[] campaignResults = response.CampaignResults;
						foreach (ChapterDungeonInfo chapterDungeonInfo in campaignResults)
						{
							GlobalAutoBehaviour<CampaignNodeOpenEffectManager>.instance.CheckNewClearNode(chapterDungeonInfo.ChapterIndex, chapterDungeonInfo.DungeonIndex);
						}
					}
					GlobalAutoBehaviour<AccountManager>.instance.ApplyEndCampaignResponse(response);
					if (request.Completed)
					{
						CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(request.ChapterIndex);
						if (data != null && data.Type == ChapterType.Event)
						{
							GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetLastEventDungeon(request.ChapterIndex, request.DungeonIndex);
						}
					}
					GetInstance<EndBattleWin>().OnEndCampaignResponse(_endCampaignProxy);
					if (!wasCompleted)
					{
						_ = (object)null;
						_ = 7;
						_ = 1;
						_ = 0;
						_ = new object[2] { request.ChapterIndex, request.DungeonIndex };
					}
					ProcessEclipseBattleEnd(response);
					return;
				}
				AutoBattle.Stop();
				GlobalAutoBehaviour<AccountManager>.instance.ApplyEndCampaignResponse(response);
			}
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.ERROR);
			string resultString = LocalizationManager.GetResultString(response);
			MessagePopup.Open(title, resultString, delegate
			{
				RequestEndEclipse(request);
			}, delegate
			{
				ReturnToLobby();
			});
		});
	}

	public void ProcessEclipseBattleEnd(NShared.EndCampaign.Response response)
	{
		GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.Apply(response.EclipseDungeonInfo);
		ItemResultInfo[] itemResults = response.ItemResults;
		EquipItemInfo[] equipItemInfos = response.EquipItemInfos;
		CurrencyResultInfo3[] currencyResults = response.CurrencyResults;
		BaseRewardPopup.PopupOpenData popupOpen = new BaseRewardPopup.PopupOpenData().SetItemResultInfos(itemResults).SetEquipResultInfos(equipItemInfos).SetCurrencyResultInfos3(currencyResults);
		_alreadyRequest = false;
		EclipseRewardPopup.OpenEclipseRewardPopup(popupOpen, isGlobal: false, null, this);
	}

	void SkillBookSelectRewardPopup.IListener.TryStartBattle(BattlePartySetting.ExtraOptionData extraOption)
	{
	}

	void SkillBookSelectRewardPopup.IListener.GetReward(string[] itemCodes, Action enableButtonAction)
	{
		if (itemCodes.Length != 0)
		{
			new NShared.GetSelectedReward.Request
			{
				ChapterIndex = SingletonAuto<CampaignContext>.instance.EnterDungeon.Chapter,
				DungeonIndex = SingletonAuto<CampaignContext>.instance.EnterDungeon.LinkedDungeon,
				ItemCodes = itemCodes
			};
			Debug.Log("### HIT");
		}
	}

	void EclipseRewardPopup.IListener.ProgressNextTeam()
	{
		if (_eclipseWebManager != null)
		{
			_eclipseWebManager.RequestGetEclipsInfo(showLoading: true, delegate
			{
				ProcessNextTeam();
			});
		}
	}

	void EclipseRewardPopup.IListener.ShowEclipseBattleRecord()
	{
		if (_alreadyRequest)
		{
			EclipseBattleRecordPopup.Open(this);
			return;
		}
		_eclipseWebManager.RequestGetEclipsDeck(delegate
		{
			_alreadyRequest = true;
			EclipseBattleRecordPopup.Open(this);
		});
	}

	void EclipseBattleRecordPopup.IListener.ShowEclipseClearWaveReward(int deckIndex, int waveIndex)
	{
		EclipseClearWaveRewardPopup.Open(deckIndex, waveIndex);
	}
}
