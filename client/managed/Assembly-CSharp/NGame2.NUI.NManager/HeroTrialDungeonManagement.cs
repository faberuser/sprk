using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NTutorial;
using NGame2.NUI.NWindow;
using NGame2.NUtil;
using NShared;
using NShared.BeginCampaign;
using NShared.GetAwakeMaterialChallenge;
using NShared.NLocalization;
using NVespa;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NTask.NRuntime;
using NVespa.NWeb;

namespace NGame2.NUI.NManager;

public class HeroTrialDungeonManagement : BaseManagement<HeroTrialDungeonManagement>, HeroEnterDungeon.Listener, PurifyHeroSelect.Listener, PurifyHeroRoom.Listener, BattlePartySetting.Listener, TrialChallengeHeroSelect.Listener
{
	private static HeroDeck2.EPositionTab SelectedTab = HeroDeck2.EPositionTab.Total;

	private static int SelectedHeroIndex = 1;

	private int _SelectedHeroStar;

	private int _SelectedHeroTranscend;

	private bool _waitForBattle;

	private WebServiceRequest _currentServiceRequest;

	private BattlePartySetting.BattlePartyInfo _battlePartyInfo;

	protected override void Awake()
	{
		base.Awake();
		UseBackgroundBlack(use: true);
		AddInstance<HeroEnterDungeon>("Resources/Prefab2/Window/HeroTrialDungeon/HeroEnterDungeon");
		AddInstance<BattlePartySetting>("Resources/Prefab2/Window/BattlePartySetting/BattlePartySetting");
		AddInstance<PurifyHeroSelect>("Resources/Prefab2/Window/PurifyHero/PurifyHeroSelect");
		AddInstance<PurifyHeroRoom>("Resources/Prefab2/Window/PurifyHero/PurifyHeroRoom");
		AddInstance<TrialChallengeHeroSelect>("Resources/Prefab2/Window/HeroTrialDungeon/TrialChallengeHeroSelect");
	}

	public void OpenTrialChallengeHeroSelect(HeroAwakeType heroAwakeType = HeroAwakeType.Awaken, Action openCompleteCallback = null)
	{
		AddTask(OpenTrialChallengeHeroSelectAsync(null, heroAwakeType, this, openCompleteCallback));
	}

	public void OpenTrialChallengeHeroSelect(IList<CachedHeroInfo> heroInfos, HeroAwakeType heroAwakeType, TrialChallengeHeroSelect.Listener listener = null)
	{
		AddTask(OpenTrialChallengeHeroSelectAsync(heroInfos, heroAwakeType, listener));
	}

	private ITask OpenTrialChallengeHeroSelectAsync(IList<CachedHeroInfo> heroInfos, HeroAwakeType heroAwakeType, TrialChallengeHeroSelect.Listener listener = null, Action openCompleteCallback = null)
	{
		return base.TaskFactory.Run(delegate(TrialChallengeHeroSelect window)
		{
			window.Open(listener, heroAwakeType, heroInfos, openCompleteCallback);
		});
	}

	public void OpenPurifyHeroSelectWindow(HeroAwakeType heroAwakeType = HeroAwakeType.Awaken, CachedHeroInfo selectedHeroInfo = null)
	{
		base.TaskFactory.Run(delegate(PurifyHeroSelect window)
		{
			window.Open(this, heroAwakeType, selectedHeroInfo);
		}).Commit();
	}

	public void OpenHeroEnterDungeon(CachedHeroInfo heroInfo, HeroEnterDungeon.Listener listener = null)
	{
		AddTask(OpenHeroEnterDungeonAsync(heroInfo, listener));
	}

	private ITask OpenHeroEnterDungeonAsync(CachedHeroInfo heroInfo, HeroEnterDungeon.Listener listener = null)
	{
		return base.TaskFactory.Run(delegate(HeroEnterDungeon window)
		{
			window.Open(heroInfo, listener);
		});
	}

	public void OpenPurifyHeroRoom(CachedHeroInfo heroInfo, bool isShortCut = false)
	{
		AddTask(OpenPurifyHeroRoomAsync(heroInfo, isShortCut));
	}

	private ITask OpenPurifyHeroRoomAsync(CachedHeroInfo heroInfo, bool isShortCut)
	{
		return base.TaskFactory.Run(delegate(PurifyHeroRoom window)
		{
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroInfo.HeroIndex);
			window.Open(heroInfo, data, this, isShortCut);
		});
	}

	public void OpenHeroEnterDungeonShortCut(CachedHeroInfo heroInfo)
	{
		OpenHeroEnterDungeon(heroInfo, this);
	}

	void TrialChallengeHeroSelect.Listener.ChallengeEnterHeroDungeon(CachedHeroInfo heroInfo)
	{
		OpenHeroEnterDungeon(heroInfo, this);
	}

	void HeroEnterDungeon.Listener.EnterHeroDungeon(CachedHeroInfo heroInfo)
	{
		AddTask(new WebServiceTask<NShared.GetAwakeMaterialChallenge.Request, NShared.GetAwakeMaterialChallenge.Response>(new NShared.GetAwakeMaterialChallenge.Request
		{
			HeroIndex = heroInfo.HeroIndex
		}, (NShared.GetAwakeMaterialChallenge.Request request, NShared.GetAwakeMaterialChallenge.Response response) => (response.Result != NShared.GetAwakeMaterialChallenge.ResultType.Success) ? base.TaskFactory.Run(delegate
		{
			MessagePopup.OpenError(response.Result);
		}) : new GroupTask().ADDTASK(base.TaskFactory.Run(delegate
		{
			GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
			GlobalAutoBehaviour<AccountManager>.instance.HeroManager.SetAwakeMaterialChallenge(heroInfo.HeroIndex, 1);
		})).ADDTASK(OpenHeroEnterDungeonAsync(heroInfo, this)).ADDTASK(base.TaskFactory.Branch(base.TaskFactory.IsOpen<TrialChallengeHeroSelect>(), base.TaskFactory.Run(delegate(TrialChallengeHeroSelect window)
		{
			window.RefreshAwakeChallengeItemList();
			window.UpdateHeroPortrait(heroInfo);
		})))));
	}

	int HeroEnterDungeon.Listener.GetHeroAwakeStarItemCount(int star, int transcended)
	{
		return GetAwakeHeroItemCount(star, transcended);
	}

	public static int GetAwakeHeroItemCount(int star, int transcended = 0)
	{
		return GameRoot.Instance.CreatureStarDataContainer.GetNextData(star, transcended)?.HeroAwakeItemCount ?? 0;
	}

	void HeroEnterDungeon.Listener.BattleReady(CachedHeroInfo heroInfo)
	{
		OpenBattlePartySetting(heroInfo);
	}

	private void OpenBattlePartySetting(CachedHeroInfo heroInfo)
	{
		if (heroInfo == null)
		{
			VespaDebug.LogError("OpenBattlePartySetting : heroInfo is null");
		}
		else
		{
			OpenSelectedHeroBattlePartySetting(heroInfo.HeroIndex, heroInfo.Star, heroInfo.Transcended);
		}
	}

	public void OpenSelectedHeroBattlePartySetting(int heroIndex, int heroStar, int heroTranscend)
	{
		SelectedHeroIndex = heroIndex;
		_SelectedHeroStar = heroStar;
		_SelectedHeroTranscend = heroTranscend;
		int num = 0;
		int num2 = 0;
		CampaignDungeonData trialDungeonData = GameRoot.Instance.CampaignDungeonDataContainer.GetTrialDungeonData(heroIndex, heroStar, heroTranscend);
		if (trialDungeonData == null)
		{
			VespaDebug.LogError("not found selected hero dungeondata / selectedIndex : " + SelectedHeroIndex);
			return;
		}
		num = trialDungeonData.ChapterIndex;
		num2 = trialDungeonData.DungeonIndex;
		DefaultBattlePartySettingDataProdiver dataProvider = new DefaultBattlePartySettingDataProdiver(trialDungeonData.LocalStorageKey, enterDungeon: new EnterDungeonData(num, num2, num2, NShared.DungeonDifficultyType.Easy), staminaType: StaminaType.None, staminaCount: 0, startButtonTextKey: Battle.ENTER_TRIAL_DUNGEON);
		GetInstance<BattlePartySetting>().Open(this, dataProvider);
	}

	void PurifyHeroSelect.Listener.EnterPurifyRoom(CachedHeroInfo heroInfo)
	{
		OpenPurifyHeroRoom(heroInfo);
	}

	void PurifyHeroSelect.Listener.ReOpen()
	{
		base.TaskFactory.Run(delegate(PurifyHeroSelect window)
		{
			window.ReOpen(this);
		}).Commit();
	}

	void PurifyHeroRoom.Listener.ClickBuyGold()
	{
		if (!SingletonUpdater<TutorialManager>.isValidInstance || !SingletonUpdater<TutorialManager>.instance.IsProgressing)
		{
			AddTask(base.TaskFactory.Sequence(base.TaskFactory.Close<PurifyHeroRoom>(), base.TaskFactory.Run(delegate
			{
				SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Gold.ToString());
			})));
		}
	}

	void BattlePartySetting.Listener.StartBattle(EnterDungeonData enterDungeon, BattlePartySetting.BattlePartyInfo battlePartyInfo, BattlePartySetting.ExtraOptionData extraOption)
	{
		RequestBeginTrialAsync(battlePartyInfo, 0);
	}

	private void RequestBeginTrialAsync(BattlePartySetting.BattlePartyInfo info, int buyPlayKey)
	{
		CampaignDungeonData trialDungeonData = GameRoot.Instance.CampaignDungeonDataContainer.GetTrialDungeonData(SelectedHeroIndex, _SelectedHeroStar, _SelectedHeroTranscend);
		if (trialDungeonData != null)
		{
			NShared.BeginCampaign.Request obj = new NShared.BeginCampaign.Request
			{
				ChapterIndex = trialDungeonData.ChapterIndex,
				DungeonIndex = trialDungeonData.DungeonIndex,
				HeroIndices = info.heroIndices,
				LeaderHeroIndex = info.leaderHeroIndex,
				AccountIds = info.accountIds,
				BuyKey = buyPlayKey
			};
			int onetimeBoosterCount = info.OnetimeBoosterCount;
			obj.ItemBoosterCount = onetimeBoosterCount;
			obj.AffixIndices = info.AffixIndices;
			NShared.BeginCampaign.Request request = obj;
			SingletonAuto<BattleContext>.instance.BeginCampaign(request, delegate
			{
				LobbyScene2.ListReturnState.Add(new HeroTrialDungeonReturnState(SelectedHeroIndex, _SelectedHeroStar, _SelectedHeroTranscend));
			});
		}
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
}
