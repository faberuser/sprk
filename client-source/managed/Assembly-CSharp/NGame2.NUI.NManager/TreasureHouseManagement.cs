using System;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.BeginCampaign;
using NShared.GetTreasureHouseInfo;
using NShared.NLocalization;
using NShared.ResetTreasureHouseInfo;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager;

public class TreasureHouseManagement : BaseManagement<TreasureHouseManagement>, TreasureHouseWindow.Listener, BattlePartySetting.Listener
{
	protected override void Awake()
	{
		base.Awake();
		UseBackgroundBlack(use: true);
		AddInstance<TreasureHouseWindow>("Resources/Prefab2/Window/TreasureHouse/TreasureHouseWindow");
		AddInstance<BattlePartySetting>("Resources/Prefab2/Window/BattlePartySetting/BattlePartySetting");
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void OpenTreasureHouseWindow(Action OnOpen = null)
	{
		RequestTreasureHouseInfo(delegate
		{
			TreasureHouseWindow treasureHouseWindow = GetInstance<TreasureHouseWindow>();
			if (treasureHouseWindow != null && !treasureHouseWindow.IsOpen())
			{
				treasureHouseWindow.Open(this);
				Vespa_System_Action_Extension_Method.fire(OnOpen);
			}
		});
	}

	public void RequestTreasureHouseInfo(Action OnSuccess = null, Action OnFailed = null)
	{
		NShared.GetTreasureHouseInfo.Request request = new NShared.GetTreasureHouseInfo.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetTreasureHouseInfo.Response response)
		{
			if (response == null)
			{
				Vespa_System_Action_Extension_Method.fire(OnFailed);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				Vespa_System_Action_Extension_Method.fire(OnFailed);
			}
			else if (response.Result != NShared.GetTreasureHouseInfo.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				Vespa_System_Action_Extension_Method.fire(OnFailed);
			}
			else
			{
				if (response.CurrencyResult != null && response.CurrencyResult.CurrencyType == CurrencyType.TreasureHousePoint && GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(UserManager.UserStatType.TreasureHousePoint) != response.CurrencyResult.NewValue)
				{
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
				}
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetPlayerTreasureHouseInfo(response.PlayerTreasureHouseInfo);
				Vespa_System_Action_Extension_Method.fire(OnSuccess);
			}
		});
	}

	public void OpenEnterDungeon(int chapterIndex, int dungeonIndex)
	{
		int num = 0;
		EnterDungeonData enterDungeon = new EnterDungeonData(chapterIndex, dungeonIndex, dungeonIndex, (NShared.DungeonDifficultyType)num);
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		DefaultBattlePartySettingDataProdiver dataProvider = new DefaultBattlePartySettingDataProdiver(staminaType: data.ReqStaminaType, staminaCount: data.GetStaminaOfDifficulty(GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina(), (NShared.DungeonDifficultyType)num), storageKey: data.LocalStorageKey, startButtonTextKey: Battle.START_BATTLE, enterDungeon: enterDungeon);
		GetInstance<BattlePartySetting>().Open(this, dataProvider);
	}

	void TreasureHouseWindow.Listener.OnClickEnterDungeon(int chapterIndex, int dungeonIndex)
	{
		OpenEnterDungeon(chapterIndex, dungeonIndex);
	}

	void TreasureHouseWindow.Listener.OnClickChangeBoss(CurrencyType currencyType, int price)
	{
		if (!CheckEnoughCurrency(currencyType, price, null))
		{
			return;
		}
		NShared.ResetTreasureHouseInfo.Request request = new NShared.ResetTreasureHouseInfo.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ResetTreasureHouseInfo.Response response)
		{
			if (response != null)
			{
				if (response.BaseResult != BaseResultType.Success)
				{
					MessagePopup.OpenError(response.BaseResult);
				}
				else if (response.Result != NShared.ResetTreasureHouseInfo.ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
				}
				else
				{
					if (response.CurrencyResults != null)
					{
						for (int i = 0; i < __Extension_Method_ICollection__.COUNT(response.CurrencyResults); i++)
						{
							CurrencyResultInfo3 currencyResultInfo = response.CurrencyResults[i];
							if (currencyResultInfo == null || currencyResultInfo.CurrencyType != CurrencyType.TreasureHousePoint || GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(UserManager.UserStatType.TreasureHousePoint) != currencyResultInfo.NewValue)
							{
								GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(currencyResultInfo);
							}
						}
					}
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetPlayerTreasureHouseInfo(response.PlayerTreasureHouseInfo);
					TreasureHouseWindow treasureHouseWindow = GetInstance<TreasureHouseWindow>(openedOnly: true);
					if (treasureHouseWindow != null)
					{
						treasureHouseWindow.PlayChangeBoss();
					}
				}
			}
		});
	}

	void TreasureHouseWindow.Listener.RefreshTreasureHouseInfo(Action OnSuccess)
	{
		RequestTreasureHouseInfo(OnSuccess);
	}

	private bool CheckEnoughCurrency(CurrencyType currencyType, int price, Action action)
	{
		switch (currencyType)
		{
		case CurrencyType.Gold:
			if (price > GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gold)
			{
				OpenNotEnoughGoldPopup(action);
				return false;
			}
			break;
		case CurrencyType.Gem:
		{
			long totalGem = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TotalGem;
			if (price > totalGem)
			{
				OpenNotEnoughGemPopup(action);
				return false;
			}
			break;
		}
		}
		return true;
	}

	private void OpenNotEnoughGoldPopup(Action action)
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GOLD_SHOP_CONFIRM);
		MessagePopup.Open(title, message, delegate
		{
			if (action != null)
			{
				Vespa_System_Action_Extension_Method.fire(action);
			}
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Gold.ToString());
		}, null);
	}

	private void OpenNotEnoughGemPopup(Action action)
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM);
		MessagePopup.Open(title, message, delegate
		{
			if (action != null)
			{
				Vespa_System_Action_Extension_Method.fire(action);
			}
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
		}, null);
	}

	public static bool CheckTreasureHouseDungeonCleard()
	{
		PlayerTreasureHouseInfo treasureHouseInfo = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TreasureHouseInfo;
		if (treasureHouseInfo == null)
		{
			VespaDebug.LogError("treasureHouseInfo is null");
			return false;
		}
		TreasureHouseDungeonData data = GameRoot.Instance.TreasureHouseDungeonDataContainer.GetData(treasureHouseInfo.Index);
		if (data == null)
		{
			VespaDebug.LogError("treasureHouseDungeonData is null");
			return false;
		}
		TreasureHouseInfoData data2 = GameRoot.Instance.TreasureHouseInfoDataContainer.GetData(data.InfoIndex);
		if (data2 == null)
		{
			VespaDebug.LogError("treasureHouseInfoData (TableData) is null");
			return false;
		}
		int enterCount = data2.EnterCount;
		int clearCount = treasureHouseInfo.ClearCount;
		return enterCount <= clearCount;
	}

	void BattlePartySetting.Listener.StartBattle(EnterDungeonData enterDungeon, BattlePartySetting.BattlePartyInfo battlePartyInfo, BattlePartySetting.ExtraOptionData extraOption)
	{
		RequestBegin(enterDungeon, battlePartyInfo, extraOption);
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
			AddLobbyReturnState(enterDungeon);
		}, delegate(NShared.BeginCampaign.Response response)
		{
			if (response != null && response.Result == NShared.BeginCampaign.ResultType.TreasurehouseInitializeEnterPopup)
			{
				CloseAllActiveWindow(immediately: true);
			}
		});
	}

	private void AddLobbyReturnState(EnterDungeonData enterDungeon)
	{
		TreasureHouseReturnState item = new TreasureHouseReturnState();
		LobbyScene2.ListReturnState.Add(item);
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
