using System;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NUI.NManager.NCommonImpl;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.BeginCampaign;
using NShared.BuyPurchaseDungeon;
using NShared.NLocalization;
using NShared.PlayerItemTimeDurationInfos;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;
using UnityEngine;

namespace NGame2.NUI.NManager;

public class ParttimeDungeonManagement : BaseManagement<ParttimeDungeonManagement>, ParttimeDungeonList.Listener, BattlePartySetting.Listener
{
	protected WebServiceRequest _currentServiceRequest;

	private float _battleRequestedTime;

	protected override void Awake()
	{
		base.Awake();
		UseBackgroundBlack(use: true);
		AddInstance<ParttimeDungeonList>("Resources/Prefab2/Window/ParttimeDungeon/ParttimeDungeonList");
		AddInstance<BattlePartySetting>("Resources/Prefab2/Window/BattlePartySetting/BattlePartySetting");
		AddInstance<BattleDungeonInfo>("Resources/Prefab2/Window/Dungeon/BattleDungeonInfo");
	}

	public void OpenParttimeDungeonList()
	{
		GetInstance<ParttimeDungeonList>().Open(this);
	}

	void ParttimeDungeonList.Listener.RequestReloadBoosterInfo()
	{
		RequestReloadPlayerItemTimeDurationInfos();
	}

	void ParttimeDungeonList.Listener.OnClickEnter(PurchaseDungeonData purchaseDungeonData)
	{
		if (purchaseDungeonData != null)
		{
			int chapterIndex = purchaseDungeonData.ChapterIndex;
			int dungeonIndex = purchaseDungeonData.DungeonIndex;
			EnterDungeonData enterDungeonData = new EnterDungeonData(chapterIndex, dungeonIndex, dungeonIndex);
			OpenBattleDungeonInfo(enterDungeonData, purchaseDungeonData);
		}
	}

	public void OpenBattleDungeonInfo(EnterDungeonData enterDungeonData, PurchaseDungeonData purchaseDungeonData)
	{
		_ = enterDungeonData.OriginalDungeon;
		BattleDungeonInfoListener battleDungeonInfoListener = new BattleDungeonInfoListener();
		battleDungeonInfoListener.EnterDungeon = enterDungeonData;
		battleDungeonInfoListener.OnReadyBattle = OnReadyBattle;
		PurchaseDungeonRemainTimer purchaseDungeonRemainTimer = new PurchaseDungeonRemainTimer(purchaseDungeonData, null);
		purchaseDungeonRemainTimer.SetShowAtRemainZero(show: true);
		EventDungeonRemainTimer eventDungeonRemainTimer = new EventDungeonRemainTimer(purchaseDungeonRemainTimer);
		eventDungeonRemainTimer.SetShowAtRemainZero(show: true);
		battleDungeonInfoListener.DungeonRemainTimer = eventDungeonRemainTimer;
		GetInstance<BattleDungeonInfo>().Open(enterDungeonData, battleDungeonInfoListener);
	}

	public void OnReadyBattle(EnterDungeonData enterDungeon)
	{
		DefaultBattlePartySettingDataProdiver dataProdiver = GetDataProdiver(enterDungeon);
		GetInstance<BattlePartySetting>().Open(this, dataProdiver);
	}

	private DefaultBattlePartySettingDataProdiver GetDataProdiver(EnterDungeonData enterDungeon)
	{
		int chapter = enterDungeon.Chapter;
		int originalDungeon = enterDungeon.OriginalDungeon;
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapter, originalDungeon);
		_ = data.ReqStaminaType;
		int staminaOfDifficulty = data.GetStaminaOfDifficulty(GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina(), enterDungeon.Difficulty);
		return new DefaultBattlePartySettingDataProdiver(data.LocalStorageKey, data.ReqStaminaType, staminaOfDifficulty, Battle.START_BATTLE, enterDungeon);
	}

	void ParttimeDungeonList.Listener.OnClickBuy(PurchaseDungeonData purchaseDungeonData)
	{
		if (purchaseDungeonData == null)
		{
			VespaDebug.LogError("purchaseDungeonData == null");
			return;
		}
		int price = GetPrice(purchaseDungeonData.Price);
		string stringDirect = LocalizationManager.GetStringDirect(GetCurrencyLocalkey(purchaseDungeonData.CurrencyType));
		bool isEnoghPrice = GetPlayerCurrency(purchaseDungeonData.CurrencyType) >= price;
		string text = ((GetBoosterDurationSec(purchaseDungeonData) == 0) ? "0" : UIHelper.GetRemainTimeTextAllDelZero(GetBoosterDurationSec(purchaseDungeonData)));
		string stringDirect2 = LocalizationManager.GetStringDirect(purchaseDungeonData.DungeonName);
		string stringDirect3 = LocalizationManager.GetStringDirect("SIDEQUEST_PURCHASE_BUY_CONFIRM_DESC", text, price, stringDirect);
		string yes = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_YES);
		PreserveMessagePopup2.EPriceType priceType = UIHelper.ConvertToPriceType(purchaseDungeonData.CurrencyType);
		PreserveMessagePopup2.OpenPriceConfirm(stringDirect2, stringDirect3, yes, priceType, price, delegate
		{
			RequestBuyPurchaseDungeon(purchaseDungeonData);
		}, isEnoghPrice);
	}

	private int GetBoosterDurationSec(PurchaseDungeonData purchaseDungeonData)
	{
		return GameRoot.Instance.BoosterItemDataContainer.GetData(purchaseDungeonData.BoosterItemCode)?.Duration ?? 0;
	}

	private string GetCurrencyLocalkey(CurrencyType currency)
	{
		string result = "COST_TYPE_RUBY";
		if (currency == CurrencyType.Gold)
		{
			result = "COST_TYPE_GOLD";
		}
		return result;
	}

	private long GetPlayerCurrency(CurrencyType currency)
	{
		switch (currency)
		{
		case CurrencyType.Gem:
			return GlobalAutoBehaviour<AccountManager>.instance.UserManager.TotalGem;
		case CurrencyType.Gold:
			return GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gold;
		default:
			VespaDebug.LogError($"not defined curreny type ({currency})");
			return 0L;
		}
	}

	private void RequestBuyPurchaseDungeon(PurchaseDungeonData purchaseDungeonData)
	{
		if (WebServiceRequest.IsProgressing(_currentServiceRequest))
		{
			VespaDebug.LogError("NVespa.NWeb.WebServiceRequest.IsProgressing(_currentServiceRequest) == true");
			return;
		}
		if (purchaseDungeonData == null)
		{
			VespaDebug.LogError("purchaseDungeonData == null");
			return;
		}
		NShared.BuyPurchaseDungeon.Request request = new NShared.BuyPurchaseDungeon.Request();
		request.ChapterIndex = purchaseDungeonData.ChapterIndex;
		request.DungeonIndex = purchaseDungeonData.DungeonIndex;
		request.Price = GetPrice(purchaseDungeonData.Price);
		CurrencyType currenyType = purchaseDungeonData.CurrencyType;
		_currentServiceRequest = SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.BuyPurchaseDungeon.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else if (response.Result == NShared.BuyPurchaseDungeon.ResultType.NotEnoughCost)
			{
				string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
				string message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM);
				MessagePopup.Open(title, message, delegate
				{
					SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(ConvertToShopCategory(currenyType).ToString());
				}, null);
			}
			else if (response.Result != NShared.BuyPurchaseDungeon.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				GlobalAutoBehaviour<AccountManager>.instance.ItemTimeDurationManager.Apply(response.ItemTimeDurations);
			}
		});
	}

	private void RequestReloadPlayerItemTimeDurationInfos()
	{
		if (WebServiceRequest.IsProgressing(_currentServiceRequest))
		{
			VespaDebug.LogError("NVespa.NWeb.WebServiceRequest.IsProgressing(_currentServiceRequest) == true");
			return;
		}
		NShared.PlayerItemTimeDurationInfos.Request request = new NShared.PlayerItemTimeDurationInfos.Request();
		_currentServiceRequest = SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.PlayerItemTimeDurationInfos.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else if (response.Result != NShared.PlayerItemTimeDurationInfos.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.ItemTimeDurationManager.Apply(response.ItemTimeDurations);
			}
		});
	}

	private NewPayShopCategoryType ConvertToShopCategory(CurrencyType currencytype)
	{
		NewPayShopCategoryType result = NewPayShopCategoryType.Ruby;
		if (currencytype == CurrencyType.Gold)
		{
			result = NewPayShopCategoryType.Gold;
		}
		return result;
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
			AddLobbyReturnState(enterDungeon);
		});
	}

	private void AddLobbyReturnState(EnterDungeonData enterDungeon)
	{
		PurchaseDungeonData data = GameRoot.Instance.PurchaseDungeonDataContainer.GetData(enterDungeon.Chapter, enterDungeon.OriginalDungeon);
		ParttimeDungeonReturnState item = new ParttimeDungeonReturnState(enterDungeon, data);
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

	public static int GetPrice(int price)
	{
		float discountRatio = GetDiscountRatio(EventBonusType.DiscountPurchaseDungeon);
		price = (int)System.Math.Ceiling(System.Math.Round((float)price * discountRatio, 2));
		return price;
	}

	private static float GetDiscountRatio(EventBonusType discountBonusType)
	{
		float num = 1f;
		if (!GameRoot.Instance.HeroBonusDataContainer.BonusOwnedHeroIndices.ContainsKey(discountBonusType))
		{
			return num;
		}
		foreach (int item in GameRoot.Instance.HeroBonusDataContainer.BonusOwnedHeroIndices[discountBonusType])
		{
			CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(item);
			if (cachedHeroInfo == null)
			{
				continue;
			}
			HeroBonusData data = GameRoot.Instance.HeroBonusDataContainer.GetData(cachedHeroInfo.HeroIndex, cachedHeroInfo.Star, cachedHeroInfo.Transcended);
			if (data != null)
			{
				HeroEventBonusData[] heroBonusTypeDatas = data.GetHeroBonusTypeDatas(discountBonusType);
				for (int i = 0; i < heroBonusTypeDatas.Length; i++)
				{
					int num2 = Vespa_String_Extension_Method.TOINT(heroBonusTypeDatas[i].BonusValue[0], 0);
					num *= 1f - (float)num2 * 0.01f;
				}
			}
		}
		return num;
	}
}
