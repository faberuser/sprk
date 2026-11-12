using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using NGame2.IAP;
using NGame2.NAccount;
using NGame2.NAccount.NExtension;
using NGame2.NContentsInfo;
using NGame2.NDropBonusEvent;
using NGame2.NGuild;
using NGame2.NLogin;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NUI.NComponent2.NPayShop;
using NGame2.NUI.NHelper.NPayShop;
using NGame2.NUI.NManager.Mission;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NGame2.NUI.NWindow.NPayShop;
using NGame2.NUI3.NReturnUserJumping;
using NGame2.NUtil;
using NGame2.NUtil.NModifyChecker;
using NGame2.NUtil.NPayShop;
using NGame2.NUtil.NStringProxy;
using NGameAsset;
using NShared;
using NShared.BeginCampaign;
using NShared.BuyHero;
using NShared.BuyPayShopProduct;
using NShared.ExecEquipGacha;
using NShared.GetEquipGacha;
using NShared.GetFreeEquipGachaInfo;
using NShared.GetSelectShopInfo;
using NShared.NLocalization;
using NShared.PetGachaRoofReward;
using NShared.RestockSelectShopInfo;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;
using UnityEngine;

namespace NGame2.NUI.NManager;

public class PayShopManagement : BaseManagement<PayShopManagement>, NGame2.NUI.NWindow.NPayShop.PayShop.IListener, HeroFullSceneViewBase.Listener, HeroDetailView.Listener, PayShopBaseInfoPopup.IListener, PayShopAttendanceInfoPopup.IListener, ITouchDisabler, IUserObserver<ContentsValueInfo>, IUserObserver, PayShopHeroInfoPopup.IListener, PayShopEquipGachaScene.IListener, GachaSelectShopPopup.IListener
{
	public static IncreaseDic<int, AssetTexture> NewBannerUrlDic = new IncreaseDic<int, AssetTexture>();

	public static LocalStorageBool FirstPurchaseAppReview = new LocalStorageBool(i_default: false, typeof(PayShopManagement).FullName + "FirstPurchaseAppReview");

	private static PayShopUserGroup PayShopUserGroup;

	private static DateTime UserTypeEndTime;

	private DateTime _lastUpdateEquipGachaTime;

	bool ITouchDisabler.IsValid => true;

	protected override void Awake()
	{
		base.Awake();
		UseBackgroundBlack(use: true);
		AddInstance<NGame2.NUI.NWindow.NPayShop.PayShop>("Resources/Prefab2/Window/PayShop/PayShop");
		AddInstance<HeroDetailView>("Resources/Prefab2/Window/FullScene/HeroDetailView");
		AddInstance<PayShopProductInfoPopup>("Resources/Prefab2/Window/PayShop/PayShopProductInfoPopup");
		AddInstance<PayShopAttendanceInfoPopup>("Resources/Prefab2/Window/PayShop/PayShopAttendanceInfoPopup");
		AddInstance<PayShopHeroInfoPopup>("Resources/Prefab2/Window/PayShop/PayShopHeroInfoPopup");
		AddInstance<PayShopEquipGachaScene>("Resources/Prefab2/Window/PayShop/PayShopEquipGachaScene");
		AddInstance<GachaSelectShopPopup>("Resources/Prefab2/Window/PayShop/GachaSelectShopPopup");
		AddInstance<PayShopBuyPopup>("Resources/Prefab3/ReturnUserJumping/PayShopBuyPopup", GameUIRoot.ELayerType.UI3DUI);
	}

	protected override void ChainUpdate()
	{
		base.ChainUpdate();
		UpdateGachaInfo();
	}

	public static void SetPayShopUserInfo(PayShopUserGroup group, string endTime)
	{
		PayShopUserGroup = group;
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(endTime, out UserTypeEndTime))
		{
			UserTypeEndTime = DateTime.MaxValue;
		}
	}

	public static PayShopUserGroup GetPayShopUserType()
	{
		return PayShopUserGroup;
	}

	private void UpdateGachaInfo()
	{
		NGame2.NUI.NWindow.NPayShop.PayShop payShop = GetInstance<NGame2.NUI.NWindow.NPayShop.PayShop>();
		if (!(payShop == null) && payShop.IsOpen())
		{
			int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("UpdateGachaInfoMin");
			DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
			if ((currentServerLocalTime - _lastUpdateEquipGachaTime).TotalMinutes >= (double)valueInt)
			{
				_lastUpdateEquipGachaTime = currentServerLocalTime;
				RequestFreeEquipGachaInfo();
				RequestEquipGachaInfo();
			}
		}
	}

	private void RequestFreeEquipGachaInfo()
	{
		NShared.GetFreeEquipGachaInfo.Request request = new NShared.GetFreeEquipGachaInfo.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.Ping, delegate(NShared.GetFreeEquipGachaInfo.Response response)
		{
			if (response.Result == NShared.GetFreeEquipGachaInfo.ResultType.Success)
			{
				GlobalAutoBehaviour<AccountManager>.instance.FreeEquipGachaManager.Apply(response.freeEquipGachaInfos);
			}
		});
	}

	private void RequestEquipGachaInfo()
	{
		NShared.GetEquipGacha.Request request = new NShared.GetEquipGacha.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.Ping, delegate(NShared.GetEquipGacha.Response response)
		{
			if (response.Result == NShared.GetEquipGacha.ResultType.Success)
			{
				GlobalAutoBehaviour<AccountManager>.instance.EquipGachaManager.Apply(response.EquipGachaInfos);
				GlobalAutoBehaviour<AccountManager>.instance.StarterGachaManager.Apply(response.StarterPickupEquipGachaInfos);
				GlobalAutoBehaviour<AccountManager>.instance.StepUpEquipGachamanager.Apply(response.StepUpEquipGachaInfos);
			}
		});
	}

	public void OpenNewPayShop()
	{
		OpenNewPayShop(SprkShopSearch.DefaultCategory("Hero"));
	}

	public void OpenNewPayShop(string category, bool openAnimation = false, Action closeAction = null)
	{
		NGame2.NUI.NWindow.NPayShop.PayShop payShop = GetInstance<NGame2.NUI.NWindow.NPayShop.PayShop>();
		if ((object)payShop != null)
		{
			payShop.Open(this, closeAction);
			payShop.SelectCategory(category, forceSelect: true);
			if (openAnimation)
			{
				payShop.PlayOpenAnimation();
			}
		}
	}

	public void OpenNewPayShop(string category, Action onComplete)
	{
		OpenNewPayShop(category);
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}

	public static void LoadNewBannerImageUrlInfo()
	{
	}

	public static AssetTexture GetNewBannerAssetTexture(int Index)
	{
		return NewBannerUrlDic.GETVALUE(Index, null);
	}

	public static void ReleaseAllBanner()
	{
		foreach (KeyValuePair<int, AssetTexture> item in (IEnumerable<KeyValuePair<int, AssetTexture>>)NewBannerUrlDic)
		{
			item.Value.CloseAndRelease();
			item.Value.Close();
		}
		NewBannerUrlDic.Clear();
	}

	private IEnumerator coRequestInAppPurchase(SkuInfo skuInfo, int productIndex, Action callback, Action commonCallback)
	{
		return ((IEnumerable)new object[0]).GetEnumerator();
	}

	private void OpenAppReviewPopup()
	{
		string[] modifyCondition = SingletonAuto<ModifyChecker>.instance.GetModifyCondition<FirstInAppPurchases>(new string[1] { "True" });
		if (SingletonAuto<ModifyChecker>.instance.Check(modifyCondition) && GameRoot.Instance.AppReviewConditionDataContainer.IsCheck(modifyCondition) && !FirstPurchaseAppReview.Value && AppReviewPopup.Open())
		{
			FirstPurchaseAppReview.Value = true;
		}
	}

	public void BuyHeroShopItem(CreatureData creatureData)
	{
		if (creatureData != null)
		{
			int index = creatureData.Index;
			OpenHeroInfoPopup(index);
		}
	}

	private void OpenHeroDetailView(CachedHeroInfo heroInfo)
	{
		HeroDetailView heroDetailView = GetInstance<HeroDetailView>();
		if (heroDetailView != null)
		{
			heroDetailView.Open(heroInfo, this, HeroDetailView.EToggle.Animation);
		}
	}

	void HeroFullSceneViewBase.Listener.OnOpen()
	{
	}

	void HeroFullSceneViewBase.Listener.OnClose()
	{
	}

	void HeroDetailView.Listener.OnTryBattle(HeroDetailView sender)
	{
		AddTask(base.TaskFactory.Branch(base.TaskFactory.Run(() => HeroDetailView.TryBattle(sender.HeroInfo)), base.TaskFactory.Run(delegate
		{
			LobbyScene2.ListReturnState.Add(new PayShopHeroInfoPopupReturnState(sender.HeroInfo.HeroIndex));
		})));
	}

	public static void PopupUseBoosterMsg(PlayerItemTimeDurationInfo info)
	{
		if (info != null)
		{
			BoosterItemData boosterItemData = null;
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(info.ItemIndex);
			string text;
			if (data == null)
			{
				boosterItemData = GameRoot.Instance.BoosterItemDataContainer.GetBoosterDataByGroupIndex(info.ItemIndex);
				text = ItemNameBuilder.GetBoosterItemName(boosterItemData);
			}
			else
			{
				boosterItemData = GameRoot.Instance.BoosterItemDataContainer.GetData(data.Code);
				text = ItemNameBuilder.GetItemName(data);
			}
			if (boosterItemData != null)
			{
				TimeSpan remainTime = info.GetRemainTime();
				CenterMessage.Open(LocalizationManager.GetString(NShared.NLocalization.PayShop.DAILY_BOOSTER_APPLIED_MSG, text, (int)remainTime.TotalDays, remainTime.Hours, remainTime.Minutes));
			}
		}
	}

	private bool IsAdultCheck()
	{
		CountryServerData countryServerData = LobbyManagement.GetCountryServerData();
		if (countryServerData == null || !countryServerData.IsAdultCheck || SingletonUpdater<LoginManager>.instance.AdultAgreementAccepted.Value)
		{
			return false;
		}
		return true;
	}

	public void RestoreHeroInfoPopup(int heroIndex)
	{
		AddTask(base.TaskFactory.Run(delegate
		{
			OpenNewPayShop(NewPayShopCategoryType.Hero.ToString());
		}), base.TaskFactory.Run(delegate
		{
			if (GameRoot.Instance.CreatureDataContainer.GetData(heroIndex) != null)
			{
				List<CreatureData> datas = PayShopHeroList.GetDatas();
				OpenHeroInfoPopup(heroIndex, isSteupGacha: false, datas);
			}
		}));
	}

	public void OpenProduct(string category, string itemCode)
	{
		if (String_Extension_Method.IsNullOrEmpty(itemCode))
		{
			return;
		}
		NewPayShopProductInfo product = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.GetProduct(category, itemCode);
		if (product != null)
		{
			NGame2.NUI.NWindow.NPayShop.PayShop payShop = GetInstance<NGame2.NUI.NWindow.NPayShop.PayShop>(openedOnly: true);
			if (payShop != null)
			{
				payShop.ScrollToTargetProduct(product.Index);
			}
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<ContentsValueInfo>.OnChanged(ContentsValueInfo changedInfo)
	{
	}

	void NGame2.NUI.NWindow.NPayShop.PayShop.IListener.OnClickProduct(int productIndex)
	{
		OpenPayShopProductInfoPopup(productIndex);
	}

	public void Open_ReturnUserJumping_PayShopBuyPopup(int productIndex)
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(productIndex) != null)
		{
			GetInstance<PayShopBuyPopup>().Open(productIndex);
		}
	}

	public void OpenPayShopProductInfoPopup(int productIndex)
	{
		NewPayShopProductInfo newPayShopProductInfo = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(productIndex);
		if (newPayShopProductInfo != null)
		{
			if (newPayShopProductInfo.ProductType == ProductType.Attendance)
			{
				GetInstance<PayShopAttendanceInfoPopup>().Open(this, productIndex);
			}
			else
			{
				GetInstance<PayShopProductInfoPopup>().Open(this, productIndex);
			}
		}
	}

	public void OepnRightAwayProductInfoPopup(NewPayShopProductInfo newPayShopProductInfo)
	{
		GetInstance<PayShopProductInfoPopup>().Open(this, newPayShopProductInfo);
	}

	void NGame2.NUI.NWindow.NPayShop.PayShop.IListener.OnClickProduct(int productIndex, int comboIndex)
	{
		GetInstance<PayShopProductInfoPopup>().Open(this, productIndex, comboIndex);
	}

	void NGame2.NUI.NWindow.NPayShop.PayShop.IListener.OnClickHero(int heroIndex, List<CreatureData> creatureDatas)
	{
		OpenHeroInfoPopup(heroIndex, isSteupGacha: false, creatureDatas);
	}

	private void OpenHeroInfoPopup(int heroIndex, bool isSteupGacha = false, List<CreatureData> listHeroes = null)
	{
		PayShopHeroInfoPopup payShopHeroInfoPopup = GetInstance<PayShopHeroInfoPopup>();
		if (!(payShopHeroInfoPopup != null) || payShopHeroInfoPopup.IsOpen())
		{
			return;
		}
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
		if (data != null)
		{
			if (listHeroes == null)
			{
				listHeroes = new List<CreatureData>();
				listHeroes.Add(data);
			}
			payShopHeroInfoPopup.Open(data, listHeroes, this);
			payShopHeroInfoPopup.SetEtcButton(!isSteupGacha);
		}
	}

	void NGame2.NUI.NWindow.NPayShop.PayShop.IListener.OnClickCostume(int costumeIndex)
	{
		if (GameRoot.Instance.CostumeDataContainer.GetData(costumeIndex) != null)
		{
			SceneAutoBehaviour<CostumeManagement>.instance.OpenCostume(costumeIndex);
		}
	}

	void NGame2.NUI.NWindow.NPayShop.PayShop.IListener.OnClickAccessory(int accessoryIndex)
	{
		PayShopHeroSelectPopup.OpenPopup(GetAccessoryHeroList(accessoryIndex), delegate(int heroIndex)
		{
			SceneAutoBehaviour<CostumeManagement>.instance.OpenAccessoryView(accessoryIndex, heroIndex);
		});
	}

	void NGame2.NUI.NWindow.NPayShop.PayShop.IListener.OnClickEquipGacha(int gachaIndex)
	{
		NewEquipGachaData data = GameRoot.Instance.NewEquipGachaDataContainer.GetData(gachaIndex);
		if (data.Type != EquipGachaType.Pet && GlobalAutoBehaviour<AccountManager>.instance.IsMaxEquipItemCount())
		{
			OpenGoToInvenPopup();
			return;
		}
		EquipGachaInfo equipGachaInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipGachaManager.Get(gachaIndex);
		if (equipGachaInfo == null)
		{
			OnErrorExecEquipGacha(data, NShared.ExecEquipGacha.ResultType.InvalidGachaData);
			return;
		}
		if (data.TicketOnly)
		{
			bool flag = GlobalAutoBehaviour<AccountManager>.instance.FreeEquipGachaManager.Free(data.Index);
			if (HelperItem.GetFreeGachaItemCount(data.Index) == 0 && !flag)
			{
				OnErrorExecEquipGacha(data, NShared.ExecEquipGacha.ResultType.TicketItemIndexEmpty);
				return;
			}
		}
		if (data.IsStarterPickupGacha())
		{
			bool flag2 = GlobalAutoBehaviour<AccountManager>.instance.StarterGachaManager.IsRemainGachaCount(gachaIndex);
			if (HelperItem.GetFreeGachaItemCount(gachaIndex) <= 0)
			{
				if (!flag2)
				{
					OnErrorExecEquipGacha(data, NShared.ExecEquipGacha.ResultType.StartPickupLimitOver);
					return;
				}
				if (equipGachaInfo.Durational == 1 && !UIHelper.IsEnableDate(equipGachaInfo.BeginTime, equipGachaInfo.EndTime, isUTC: true))
				{
					OnErrorExecEquipGacha(data, NShared.ExecEquipGacha.ResultType.ClosedEquipGacha);
					return;
				}
			}
		}
		else
		{
			if (!equipGachaInfo.OnSale)
			{
				OnErrorExecEquipGacha(data, NShared.ExecEquipGacha.ResultType.ClosedEquipGacha);
				return;
			}
			if (equipGachaInfo.Durational == 1 && !UIHelper.IsEnableDate(equipGachaInfo.BeginTime, equipGachaInfo.EndTime, isUTC: true))
			{
				OnErrorExecEquipGacha(data, NShared.ExecEquipGacha.ResultType.ClosedEquipGacha);
				return;
			}
		}
		OpenEquipGacha(gachaIndex);
	}

	public void OpenEquipGacha(int gachaIndex)
	{
		NewEquipGachaData data = GameRoot.Instance.NewEquipGachaDataContainer.GetData(gachaIndex);
		GetInstance<PayShopEquipGachaScene>().Open(data, this);
	}

	private List<CreatureData> GetAccessoryHeroList(int accessoryIndex)
	{
		List<CreatureData> heroListByOpenType = GameRoot.Instance.CreatureDataContainer.GetHeroListByOpenType(CreatureOpenType.Opened, CreatureOpenType.Scenario, CreatureOpenType.ChapterReward, CreatureOpenType.RecruitOnly);
		foreach (CachedHeroInfo heroInfo in GlobalAutoBehaviour<AccountManager>.instance.HeroManager)
		{
			if (!heroListByOpenType.Any((CreatureData e) => e.Index == heroInfo.HeroIndex))
			{
				CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroInfo.HeroIndex);
				if (data != null)
				{
					heroListByOpenType.Add(data);
				}
			}
		}
		AccessoryCostumeData accessoryData = GameRoot.Instance.AccessoryCostumeDataContainer.GetData(accessoryIndex);
		if (accessoryData != null && !__Extension_Method_ICollection__.IsNullOrEmpty(accessoryData.UnableHero))
		{
			heroListByOpenType.RemoveAll((CreatureData hero) => TypeIterator.Contains(accessoryData.UnableHero, hero.Index));
		}
		heroListByOpenType.Sort((CreatureData l, CreatureData r) => l.Index.CompareTo(r.Index));
		return heroListByOpenType;
	}

	void NGame2.NUI.NWindow.NPayShop.PayShop.IListener.OnClickBuyReservation(int productIndex, int discountPrice)
	{
		PurchaseLegendCostumeProcess(productIndex, discountPrice, null, delegate
		{
			NGame2.NUI.NWindow.NPayShop.PayShop payShop = GetInstance<NGame2.NUI.NWindow.NPayShop.PayShop>();
			if (payShop.IsOpen())
			{
				payShop.UpdateLegendCostume();
			}
		});
	}

	void NGame2.NUI.NWindow.NPayShop.PayShop.IListener.OnClickOpenReservationPopup(int productIndex)
	{
		SceneAutoBehaviour<CostumeManagement>.instance.RequestLegendCostumeReservationPopup(productIndex, LegendCostumeReservationPopup.EShowOption.MoveToHeroCostumeView);
	}

	void PayShopBaseInfoPopup.IListener.OnBuy(int productIndex)
	{
		RequestPurchaseProduct<PayShopProductInfoPopup>(productIndex);
	}

	public void RequestPurchaseProduct<TWindow>(int productIndex) where TWindow : Component, IWindow
	{
		if (CheckPurchaseException<TWindow>(productIndex))
		{
			PurchaseProductProcess(productIndex);
		}
	}

	private bool CheckPurchaseException<TWindow>(int productIndex) where TWindow : Component, IWindow
	{
		NewPayShopProductInfo productInfo = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(productIndex);
		return CheckPurchaseException<TWindow>(productInfo);
	}

	private bool CheckPurchaseException<TWindow>(NewPayShopProductInfo productInfo) where TWindow : Component, IWindow
	{
		if (productInfo == null)
		{
			if (IsOpened<TWindow>())
			{
				CloseInstance<TWindow>();
			}
			VespaDebug.LogError("ProductInfo is Null!!!");
			return false;
		}
		if (String_Extension_Method.IsNullOrEmpty(productInfo.InAppPurchaseId) && !CheckEnoughUserCurrency(productInfo))
		{
			return false;
		}
		bool result = true;
		if (productInfo.DurationDay > 0)
		{
			PlayerProductPurchaseInfo purchaseInfo = GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Get(productInfo.Index);
			double remainDayUntilMonthlyEnd = 0.0;
			result = NGame2.NAccount.UserPlayerProductPurchaseManager.IsPurchasable(productInfo, purchaseInfo, out remainDayUntilMonthlyEnd);
		}
		else if (productInfo.ProductType == ProductType.Item)
		{
			result = CheckItemProductTypeException(productInfo);
		}
		else if (productInfo.ProductType == ProductType.Activation)
		{
			result = CheckActivationProductTypeException(productInfo);
		}
		return result;
	}

	void PayShopBaseInfoPopup.IListener.OnBuyRightAwayProduct(NewPayShopProductInfo productInfo)
	{
		if (CheckRightAwayProductPurcaseException(productInfo))
		{
			PurchaseRightAwayProductProcess(productInfo);
		}
	}

	private bool CheckRightAwayProductPurcaseException(NewPayShopProductInfo productInfo)
	{
		return CheckPurchaseException<PayShopProductInfoPopup>(productInfo);
	}

	private bool CheckEnoughUserCurrency(NewPayShopProductInfo productInfo)
	{
		long price = productInfo.Price;
		long num = 0L;
		NShared.BuyPayShopProduct.ResultType? resultType = null;
		UserManager userManager = GlobalAutoBehaviour<AccountManager>.instance.UserManager;
		switch (productInfo.PriceType)
		{
		case CurrencyType.Gold:
			num = userManager.Gold;
			if (price > num)
			{
				resultType = NShared.BuyPayShopProduct.ResultType.NotEnoughGold;
			}
			break;
		case CurrencyType.Gem:
			num = userManager.TotalGem;
			if (price > num)
			{
				resultType = NShared.BuyPayShopProduct.ResultType.NotEnoughGem;
			}
			break;
		case CurrencyType.Mileage:
			num = userManager.Mileage;
			if (price > num)
			{
				resultType = NShared.BuyPayShopProduct.ResultType.NotEnoughMileage;
			}
			break;
		case CurrencyType.LuaPoint:
			num = userManager.LuaPoint;
			if (price > num)
			{
				resultType = NShared.BuyPayShopProduct.ResultType.NotEnoughLuaPoint;
			}
			break;
		case CurrencyType.ShopEventPoint:
			num = GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.GetCurrencyValue(UserManager.UserStatType.ShopEventPoint);
			if (price > num)
			{
				resultType = NShared.BuyPayShopProduct.ResultType.NotEnoughShopEventPoint;
			}
			break;
		}
		if (resultType.HasValue)
		{
			OnErrorBuyPayShopProduct(resultType.Value);
			return false;
		}
		return true;
	}

	private static bool PurchasableGuildNameChange(ItemData itemData)
	{
		bool result = false;
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager != null && GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GetGuildId() != 0L && GlobalAutoBehaviour<GuildManager>.instance.MemberManager != null)
		{
			GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
			if (myMemberInfo != null && myMemberInfo.Rank == 1)
			{
				result = true;
			}
		}
		return result;
	}

	public static bool CheckItemProductTypeException(NewPayShopProductInfo productInfo)
	{
		bool result = true;
		if (productInfo.Category == NewPayShopProductCategoryType.GachaSelectShop.ToString())
		{
			return true;
		}
		if (productInfo.Category == NewPayShopProductCategoryType.RightAway.ToString())
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(productInfo.ItemInfos); i++)
			{
				PayShopProductItemInfo payShopProductItemInfo = productInfo.ItemInfos[i];
				if (payShopProductItemInfo != null)
				{
					ItemData data = GameRoot.Instance.ItemDataContainer.GetData(payShopProductItemInfo.ItemIndex);
					if (data != null && data.Type == ItemType.GuildNameChange)
					{
						result = PurchasableGuildNameChange(data);
						break;
					}
				}
			}
		}
		else
		{
			string[] useItemCode = productInfo.UseItemCode;
			foreach (string code in useItemCode)
			{
				ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(code);
				if (dataByCode != null && dataByCode.Type == ItemType.GuildNameChange)
				{
					result = PurchasableGuildNameChange(dataByCode);
					break;
				}
			}
		}
		return result;
	}

	private bool CheckActivationProductTypeException(NewPayShopProductInfo productInfo)
	{
		if (productInfo.Category == NewPayShopProductCategoryType.RightAway.ToString())
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(productInfo.ItemInfos); i++)
			{
				PayShopProductItemInfo payShopProductItemInfo = productInfo.ItemInfos[i];
				if (payShopProductItemInfo == null)
				{
					continue;
				}
				ItemData data = GameRoot.Instance.ItemDataContainer.GetData(payShopProductItemInfo.ItemIndex);
				if (data == null)
				{
					continue;
				}
				ItemData<PotionItemData> data2 = GameRoot.Instance.ItemDataContainer.GetData<PotionItemData>(data.Code);
				if (data2 != null)
				{
					ItemActionType actionType = data2.SubData.ActionType;
					if ((actionType == ItemActionType.ExtendEquip || actionType == ItemActionType.ExtendChest) && !CheckActivationProductTypeException(productInfo.Index, data2, actionType))
					{
						return false;
					}
				}
			}
		}
		else
		{
			string[] useItemCode = productInfo.UseItemCode;
			foreach (string itemCode in useItemCode)
			{
				ItemData<PotionItemData> data3 = GameRoot.Instance.ItemDataContainer.GetData<PotionItemData>(itemCode);
				if (data3 != null)
				{
					ItemActionType actionType2 = data3.SubData.ActionType;
					if ((actionType2 == ItemActionType.ExtendEquip || actionType2 == ItemActionType.ExtendChest) && !CheckActivationProductTypeException(productInfo.Index, data3, actionType2))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	private bool CheckActivationProductTypeException(int productIndex, ItemData<PotionItemData> potionData, ItemActionType actionType)
	{
		InventoryType inventoryType = InventoryType.Inventory;
		switch (actionType)
		{
		case ItemActionType.ExtendEquip:
			inventoryType = InventoryType.Inventory;
			break;
		case ItemActionType.ExtendChest:
			inventoryType = InventoryType.Chest;
			break;
		}
		int num = 0;
		int extendMaxCount = GameRoot.Instance.InventoryExtendDataContainer.GetExtendMaxCount(inventoryType);
		int num2 = 0;
		int num3 = 0;
		if (inventoryType == InventoryType.Inventory)
		{
			num = GlobalAutoBehaviour<AccountManager>.instance.GetMaxEquipItemCount();
			num2 = NGame2.NAccount.Helper.GetInventoryExtendSize(extendMaxCount);
			num3 = NGame2.NAccount.Helper.GetInventoryExtendSize(potionData.SubData.ActionValue, addBaseCount: false);
		}
		if (inventoryType == InventoryType.Chest)
		{
			num = GlobalAutoBehaviour<AccountManager>.instance.GetMaxChestItemCount();
			num2 = NGame2.NAccount.Helper.GetChestExtendSize(extendMaxCount);
			num3 = NGame2.NAccount.Helper.GetChestExtendSize(potionData.SubData.ActionValue, addBaseCount: false);
		}
		if (num >= num2)
		{
			return false;
		}
		if (num + num3 > num2)
		{
			int num4 = num2 - num;
			string arg = string.Empty;
			string arg2 = string.Empty;
			if (inventoryType == InventoryType.Inventory)
			{
				arg = LocalizationManager.GetStringDirect("EQUIP_ASK_EXTEND_SHOP_INVENTORY", num4, potionData.Name);
				arg2 = LocalizationManager.GetStringDirect("EQUIP_ASK_EXTEND_SHOP_INVENTORY2");
			}
			if (inventoryType == InventoryType.Chest)
			{
				arg = LocalizationManager.GetStringDirect("EQUIP_ASK_EXTEND_SHOP_CHEST", num4, potionData.Name);
				arg2 = LocalizationManager.GetStringDirect("EQUIP_ASK_EXTEND_SHOP_CHEST2");
			}
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string message = $"{arg}\n{arg2}";
			PayShopMessage3ButtonPopup.Open(title, message, delegate
			{
				PurchaseProductProcess(productIndex);
			});
			return false;
		}
		return true;
	}

	public void PurchaseProductProcess(int productIndex)
	{
		NewPayShopProductInfo newPayShopProductInfo = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(productIndex);
		if (newPayShopProductInfo != null)
		{
			VespaCoroutine.Start(this, coPurchaseProductProcess(newPayShopProductInfo));
		}
	}

	public void PurchaseRightAwayProductProcess(NewPayShopProductInfo productInfo)
	{
		if (productInfo != null)
		{
			VespaCoroutine.Start(this, coPurchaseProductProcess(productInfo));
		}
	}

	public void PurchaseLegendCostumeProcess(int productIndex, int discountPrice, Action onMoveScene = null, Action<NShared.BuyPayShopProduct.Response> onSuccess = null)
	{
		NewPayShopProductInfo info = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(productIndex);
		if (info == null)
		{
			return;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.UserManager.TotalGem >= discountPrice)
		{
			string stringDirect = LocalizationManager.GetStringDirect("PAY_SHOP_BUTTON_PRE_ORDER");
			string stringDirect2 = LocalizationManager.GetStringDirect("NEW_PAY_SHOP_POPUP_LEGENDCOSTUME_CONFIRM_DESC", discountPrice);
			string stringDirect3 = LocalizationManager.GetStringDirect("CONTENTS_CUSTOMIZE_SINGLE_BUY_BTN");
			string stringDirect4 = LocalizationManager.GetStringDirect("NEW_PAY_SHOP_POPUP_CANCLE_BUTTON");
			MessagePopup3Button.Open(stringDirect, stringDirect2, delegate
			{
				DropBonusEvent payshopDiscountEffectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetPayshopDiscountEffectiveEvent(productIndex);
				NewPayShopProductInfo newPayShopProductInfo = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(productIndex);
				if (payshopDiscountEffectiveEvent == null || newPayShopProductInfo == null)
				{
					MessagePopup.OpenError(LocalizationManager.GetStringDirect("EVENT_MISSION_NOTICE_END"));
				}
				else
				{
					RequestBuyPayShopProduct(info, onSuccess, null);
				}
			}, null, null, stringDirect3, stringDirect4);
		}
		else
		{
			OpenPopupNotEnoughMoney(CurrencyType.Gem, onMoveScene);
		}
	}

	private IEnumerator coPurchaseProductProcess(NewPayShopProductInfo productInfo)
	{
		if (!String_Extension_Method.IsNullOrEmpty(productInfo.InAppPurchaseId))
		{
			SkuInfo skuInfo = SingletonAuto<IAPWrapper>.instance.GetSkuInfo(productInfo.InAppPurchaseId, productInfo);
			VespaDebug.LogError($"###info.InAppPurchaseId : {productInfo.InAppPurchaseId}");
			VespaDebug.LogError($"###skuInfo : {skuInfo}");
			if (IsAdultCheck())
			{
				bool isClosePopup = false;
				bool isCheck = false;
				PayShopAdultConfirmPopup.Open(delegate(bool isYes)
				{
					isClosePopup = true;
					isCheck = isYes;
				});
				while (!isClosePopup)
				{
					yield return null;
				}
				if (!isCheck)
				{
					yield break;
				}
				SingletonUpdater<LoginManager>.instance.AdultAgreementAccepted.Value = true;
			}
			if (LoginData.LastLoginMethod == LoginMethodType.Guest)
			{
				bool isClosePopup2 = false;
				LinkAccountPopup.NewOpen(delegate
				{
					isClosePopup2 = true;
				});
				while (!isClosePopup2)
				{
					yield return null;
				}
			}
			RequestBuyPayShopProduct_InAppPurchase(productInfo, skuInfo);
			yield break;
		}
		if (productInfo.PriceType == CurrencyType.Gem)
		{
			bool isClosePopup3 = false;
			bool isCheck2 = false;
			PayShopPriceMessagePopup.Open(productInfo, delegate(bool isYes)
			{
				isCheck2 = isYes;
				isClosePopup3 = true;
			});
			while (!isClosePopup3)
			{
				yield return null;
			}
			if (!isCheck2)
			{
				yield break;
			}
		}
		RequestBuyPayShopProduct(productInfo);
	}

	private void RequestBuyPayShopProduct(NewPayShopProductInfo info)
	{
		if (PayShopHelper.SprkIsCashProduct(info))
		{
			return;
		}
		NewPayShopProductInfo info2 = info;
		RequestBuyPayShopProduct(info2, delegate
		{
			AfterPurchaseProduct(info2);
		}, delegate
		{
			if (IsOpened<GachaSelectShopPopup>())
			{
				GetInstance<GachaSelectShopPopup>().PurchaseRefresh();
			}
		});
	}

	private void AfterPurchaseProduct(NewPayShopProductInfo info)
	{
		if (IsOpened<PayShopProductInfoPopup>())
		{
			CloseInstance<PayShopProductInfoPopup>();
		}
		if (IsOpened<PayShopBuyPopup>())
		{
			CloseInstance<PayShopBuyPopup>();
		}
		if (IsOpened<PayShopAttendanceInfoPopup>())
		{
			CloseInstance<PayShopAttendanceInfoPopup>();
		}
		NGame2.NUI.NWindow.NPayShop.PayShop payShop = GetInstance<NGame2.NUI.NWindow.NPayShop.PayShop>();
		if (payShop.IsOpen())
		{
			payShop.UpdateProduct(info);
			payShop.UpdateMainCategoryFlag(info.Category);
		}
		if (!String_Extension_Method.IsNullOrEmpty(info.InAppPurchaseId))
		{
			OpenAppReviewPopup();
		}
		if (SceneAutoBehaviour<MissionManagement>.isValidInstance)
		{
			SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.RefreshList();
		}
	}

	private void RequestBuyPayShopProduct_InAppPurchase(NewPayShopProductInfo info, SkuInfo skuInfo)
	{
	}

	private void RequestBuyPayShopProduct(NewPayShopProductInfo info, Action<NShared.BuyPayShopProduct.Response> callback, Action commonCallbak)
	{
		if (!PayShopHelper.SprkIsCashProduct(info))
		{
			NewPayShopProductInfo info2 = info;
			NShared.BuyPayShopProduct.Request request = new NShared.BuyPayShopProduct.Request
			{
				Index = info2.Index
			};
			SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.BuyPayShopProduct.Response response)
			{
				ResponseBuyPayShopProduct(info2, response, callback, commonCallbak);
			});
		}
	}

	private void ResponseBuyPayShopProduct(NewPayShopProductInfo info, NShared.BuyPayShopProduct.Response response, Action<NShared.BuyPayShopProduct.Response> callback, Action commonCallbak)
	{
		Vespa_System_Action_Extension_Method.fire(commonCallbak);
		if (response.BaseResult != BaseResultType.Success)
		{
			CloseInstance<PayShopProductInfoPopup>();
			CloseInstance<PayShopBuyPopup>();
			PayShopMessagePopup.OpenError(response.BaseResult);
		}
		else if (response.Result != NShared.BuyPayShopProduct.ResultType.Success)
		{
			OnErrorBuyPayShopProduct(response.Result);
		}
		else
		{
			ApplyToAccountWrapper applyToAccountWrapper = new ApplyToAccountWrapper();
			applyToAccountWrapper.Set(response.CurrencyResults, response.ItemResults, response.EquipItemInfos, response.HeroAddResults, response.Attendance, response.AttendanceDatas, response.ItemTimeDurationInfo, response.PurchaseMarketingResult, response.PlayerProductPurchaseInfo, response.NewbieMissionInfos, response.PlayerAnyMiscs, response.OpendMissionCategories);
			applyToAccountWrapper.Apply();
			applyToAccountWrapper.DoActionAfterApply();
			Vespa_System_Action_Extension_Method.fire(callback, response);
		}
	}

	private void OnErrorBuyPayShopProduct(NShared.BuyPayShopProduct.ResultType error)
	{
		switch (error)
		{
		case NShared.BuyPayShopProduct.ResultType.NotEnoughGem:
			OpenPopupNotEnoughMoney(CurrencyType.Gem, delegate
			{
				if (IsOpened<GachaSelectShopPopup>())
				{
					CloseInstance<GachaSelectShopPopup>();
				}
				if (IsOpened<PayShopProductInfoPopup>())
				{
					CloseInstance<PayShopProductInfoPopup>();
				}
				if (IsOpened<PayShopBuyPopup>())
				{
					CloseInstance<PayShopBuyPopup>();
				}
			});
			break;
		case NShared.BuyPayShopProduct.ResultType.NotEnoughGold:
			OpenPopupNotEnoughMoney(CurrencyType.Gold, delegate
			{
				if (IsOpened<GachaSelectShopPopup>())
				{
					CloseInstance<GachaSelectShopPopup>();
				}
				if (IsOpened<PayShopProductInfoPopup>())
				{
					CloseInstance<PayShopProductInfoPopup>();
				}
				if (IsOpened<PayShopBuyPopup>())
				{
					CloseInstance<PayShopBuyPopup>();
				}
			});
			break;
		default:
			PayShopMessagePopup.OpenError(error);
			break;
		}
	}

	private void OpenPopupNotEnoughMoney(CurrencyType currency, Action callback = null)
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetString((currency == CurrencyType.Gem) ? NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM : NShared.NLocalization.System.MOVE_GOLD_SHOP_CONFIRM);
		PayShopMessage3ButtonPopup.Open(title, message, delegate
		{
			Vespa_System_Action_Extension_Method.fire(callback);
			if (currency == CurrencyType.Gem)
			{
				OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
			}
			else if (currency == CurrencyType.Gold)
			{
				OpenNewPayShop(NewPayShopCategoryType.Gold.ToString());
			}
		});
	}

	public void ExternalPurchaseProductProcess(int productIndex, Action onSuccess, Action onComplete)
	{
		NewPayShopProductInfo newPayShopProductInfo = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(productIndex);
		if (newPayShopProductInfo != null)
		{
			VespaCoroutine.Start(this, coPurchaseProductProcess(newPayShopProductInfo, onSuccess, onComplete));
		}
	}

	private IEnumerator coPurchaseProductProcess(NewPayShopProductInfo productInfo, Action onSuccess, Action onComplete)
	{
		SkuInfo skuInfo = SingletonAuto<IAPWrapper>.instance.GetSkuInfo(productInfo.InAppPurchaseId);
		VespaDebug.LogError($"###info.InAppPurchaseId : {productInfo.InAppPurchaseId}");
		VespaDebug.LogError($"###skuInfo : {skuInfo}");
		if (IsAdultCheck())
		{
			bool isClosePopup = false;
			bool isCheck = false;
			PayShopAdultConfirmPopup.Open(delegate(bool isYes)
			{
				isClosePopup = true;
				isCheck = isYes;
			});
			while (!isClosePopup)
			{
				yield return null;
			}
			if (!isCheck)
			{
				yield break;
			}
			SingletonUpdater<LoginManager>.instance.AdultAgreementAccepted.Value = true;
		}
		if (LoginData.LastLoginMethod == LoginMethodType.Guest)
		{
			bool isClosePopup2 = false;
			LinkAccountPopup.NewOpen(delegate
			{
				isClosePopup2 = true;
			});
			while (!isClosePopup2)
			{
				yield return null;
			}
		}
		RequestBuyPayShopProduct_InAppPurchase(productInfo, skuInfo, onSuccess, onComplete);
	}

	private void RequestBuyPayShopProduct_InAppPurchase(NewPayShopProductInfo info, SkuInfo skuInfo, Action onSuccess, Action onComplete)
	{
	}

	public void OnClickPurchaseLaw_Common()
	{
		PurchasePolicyPopup.NewOpenPopup();
	}

	public void OnClickSpecialCommercialTransactionsLaw_Common()
	{
		CountryServerData countryServerData = LobbyManagement.GetCountryServerData();
		if (countryServerData != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect("MENU_SPECIFIC_COMMERCIAL_TRANSACTIONS_LAW");
			LawRelatedPopup.NewOpen(countryServerData, stringDirect, 0);
		}
	}

	public void OnClickMoneySettlementLaw_Common()
	{
		CountryServerData countryServerData = LobbyManagement.GetCountryServerData();
		if (countryServerData != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect("MENU_MONEY_SETTLEMENT_LAW");
			LawRelatedPopup.NewOpen(countryServerData, stringDirect, 1);
		}
	}

	void PayShopBaseInfoPopup.IListener.OnClickPurchaseLaw()
	{
		OnClickPurchaseLaw_Common();
	}

	void PayShopBaseInfoPopup.IListener.OnClickSpecialCommercialTransactionsLaw()
	{
		OnClickSpecialCommercialTransactionsLaw_Common();
	}

	void PayShopBaseInfoPopup.IListener.OnClickMoneySettlementLaw()
	{
		OnClickMoneySettlementLaw_Common();
	}

	void PayShopBaseInfoPopup.IListener.OnClickProbability(int productIndex)
	{
		NewPayShopProductInfo newPayShopProductInfo = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(productIndex);
		if (newPayShopProductInfo != null && newPayShopProductInfo.ShowProbability)
		{
			ProbabilityPopup.Open(ProbabilityShopType.PayShop, newPayShopProductInfo);
		}
	}

	void PayShopBaseInfoPopup.IListener.OnClickRightAwayProbability(NewPayShopProductInfo payShopProductInfo)
	{
		if (payShopProductInfo != null && payShopProductInfo.ShowProbability)
		{
			ProbabilityPopup.Open(ProbabilityShopType.PayShop, payShopProductInfo);
		}
	}

	void PayShopAttendanceInfoPopup.IListener.OnClickView(AttendanceKeywordGroup attendanceGroup)
	{
		SceneAutoBehaviour<LobbyPopupQueue>.instance.ReopenAttendancePopup(attendanceGroup, isShowPurchaseButton: false);
	}

	void PayShopHeroInfoPopup.IListener.OnClickBuy(CreatureData creatureData)
	{
		if (!CheckRequestBuyHero(creatureData))
		{
			return;
		}
		PayShopPriceMessagePopup.Open(creatureData, delegate(bool isYes)
		{
			if (isYes)
			{
				RequestBuyHero(creatureData);
			}
		});
	}

	private bool CheckRequestBuyHero(CreatureData creatureData)
	{
		int price = PayShopHelper.GetPrice(creatureData);
		long totalGem = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TotalGem;
		int num = price;
		if (totalGem < num)
		{
			OnErrorBuyHero(NShared.BuyHero.ResultType.NotEnoughGem);
			return false;
		}
		return true;
	}

	private void RequestBuyHero(CreatureData creatureData)
	{
		ItemData itemDataByTypeAndValue = GameRoot.Instance.ItemDataContainer.GetItemDataByTypeAndValue(ItemType.Hero, 0, creatureData.Index);
		if (itemDataByTypeAndValue == null)
		{
			return;
		}
		int price = PayShopHelper.GetPrice(creatureData);
		NShared.BuyHero.Request request = new NShared.BuyHero.Request
		{
			HeroIndex = creatureData.Index,
			ItemIndex = itemDataByTypeAndValue.Index,
			BuyGem = price
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.BuyHero.Response response)
		{
			BaseResultType baseResultType = response?.BaseResult ?? BaseResultType.HttpError;
			if (baseResultType != BaseResultType.Success)
			{
				PayShopMessagePopup.OpenError(baseResultType);
			}
			else if (response.Result != NShared.BuyHero.ResultType.Success)
			{
				OnErrorBuyHero(response.Result);
			}
			else
			{
				if (GetInstance<PayShopHeroInfoPopup>().IsOpen())
				{
					GetInstance<PayShopHeroInfoPopup>().Close();
				}
				GlobalAutoBehaviour<AccountManager>.instance.ApplyAddHero(response.HeroResult);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
			}
		});
	}

	private void OnErrorBuyHero(NShared.BuyHero.ResultType result)
	{
		switch (result)
		{
		case NShared.BuyHero.ResultType.NotEnoughGem:
			OpenPopupNotEnoughMoney(CurrencyType.Gem, delegate
			{
				if (GetInstance<PayShopHeroInfoPopup>().IsOpen())
				{
					GetInstance<PayShopHeroInfoPopup>().Close();
				}
				if (GetInstance<PayShopEquipGachaScene>().IsOpen())
				{
					GetInstance<PayShopEquipGachaScene>().Close();
				}
				if (SceneAutoBehaviour<CostumeManagement>.instance.IsOpened<HeroCostumeView>())
				{
					SceneAutoBehaviour<CostumeManagement>.instance.CloseAllActiveWindow();
				}
			});
			break;
		default:
			PayShopMessagePopup.OpenError(result);
			break;
		case NShared.BuyHero.ResultType.Success:
			break;
		}
	}

	void PayShopHeroInfoPopup.IListener.OnClickViewHeroDetail(CachedHeroInfo heroInfo)
	{
		OpenHeroDetailView(heroInfo);
	}

	void PayShopHeroInfoPopup.IListener.OnTryBattle(HeroInfo heroInfo)
	{
		AddTask(base.TaskFactory.Branch(base.TaskFactory.Run(() => HeroDetailView.TryBattle(heroInfo)), base.TaskFactory.Run(delegate
		{
			LobbyScene2.ListReturnState.Add(new PayShopHeroInfoPopupReturnState(heroInfo.HeroIndex));
		})));
	}

	void PayShopHeroInfoPopup.IListener.OnChangedHero(int heroIndex)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
		if (data != null)
		{
			PayShopEquipGachaScene payShopEquipGachaScene = GetInstance<PayShopEquipGachaScene>(openedOnly: true);
			if (payShopEquipGachaScene != null)
			{
				payShopEquipGachaScene.SelectHero(data.Index);
			}
		}
	}

	void PayShopEquipGachaScene.IListener.OnClickGacha(int gachaIndex)
	{
		ExecEquipGacha(gachaIndex);
	}

	void PayShopEquipGachaScene.IListener.OnClickGacha(int gachaIndex, HighGachaCategoryType type)
	{
		ExecEquipGacha(gachaIndex, type);
	}

	void PayShopEquipGachaScene.IListener.OnClickGacha(int gachaIndex, int[] heroIndices)
	{
		ExecEquipGacha(gachaIndex, HighGachaCategoryType.None, heroIndices);
	}

	private void OpenGoToInvenPopup()
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetString(NShared.BeginCampaign.ResultType.EquipItemFull);
		PayShopMessagePopup.OpenYesOnly(title, message, delegate
		{
			CloseInstance<PayShopEquipGachaScene>();
			SceneAutoBehaviour<InventoryManagement>.instance.OpenInventory();
		}).SetYesButtonLabel(LocalizationManager.GetStringDirect("MOVE_BAG"));
	}

	private void ExecEquipGacha(int gachaIndex, HighGachaCategoryType type = HighGachaCategoryType.None, int[] heroIndices = null)
	{
		NewEquipGachaData data = GameRoot.Instance.NewEquipGachaDataContainer.GetData(gachaIndex);
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.FreeEquipGachaManager.Free(gachaIndex);
		int freeGachaItemCount = HelperItem.GetFreeGachaItemCount(gachaIndex);
		if (data.Type != EquipGachaType.Pet && GlobalAutoBehaviour<AccountManager>.instance.IsMaxEquipItemCount())
		{
			OpenGoToInvenPopup();
		}
		else if (flag)
		{
			RequestExecEquipGacha(gachaIndex, type, heroIndices);
		}
		else if (freeGachaItemCount > 0 || data.TicketOnly)
		{
			ExecEquipGacha_Ticket(gachaIndex, type, heroIndices);
		}
		else
		{
			ExecEquipGacha_Cost(gachaIndex, type, heroIndices);
		}
	}

	private void ExecEquipGacha_Ticket(int gachaIndex, HighGachaCategoryType type = HighGachaCategoryType.None, int[] heroIndices = null)
	{
		NewEquipGachaData data = GameRoot.Instance.NewEquipGachaDataContainer.GetData(gachaIndex);
		if (HelperItem.GetFreeGachaItemCount(gachaIndex) <= 0)
		{
			OnErrorExecEquipGacha(data, NShared.ExecEquipGacha.ResultType.TicketItemIndexEmpty);
		}
		else if (data.Type == EquipGachaType.StepUp || data.Index == 11)
		{
			int gachaTicketIndex = GameRoot.Instance.GachaSelectItemDataContainer.GetGachaTicketIndex(gachaIndex);
			string stringDirect = LocalizationManager.GetStringDirect((data.Type == EquipGachaType.StepUp) ? "ASK_USE_STEPUP_GACHA_WITH_COUPON_MESSAGE" : "ASK_USE_ALLINONE_GACHA_WITH_COUPON_MESSAGE");
			string confirmLabel = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_YES);
			string cancelLabel = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_NO);
			IconConfirmPopup.NewOpen(stringDirect, confirmLabel, cancelLabel, delegate
			{
				RequestExecEquipGacha(gachaIndex, type, heroIndices, useTicket: true);
			}, delegate
			{
				ExecEquipGacha_Cost(gachaIndex, type, heroIndices);
			}).AddItem(gachaTicketIndex, 1);
		}
		else
		{
			RequestExecEquipGacha(gachaIndex, type, heroIndices, useTicket: true);
		}
	}

	private void ExecEquipGacha_Cost(int gachaIndex, HighGachaCategoryType type = HighGachaCategoryType.None, int[] heroIndices = null)
	{
		NewEquipGachaData data = GameRoot.Instance.NewEquipGachaDataContainer.GetData(gachaIndex);
		GlobalAutoBehaviour<AccountManager>.instance.FreeEquipGachaManager.Free(gachaIndex);
		HelperItem.GetFreeGachaItemCount(gachaIndex);
		if (data.StarterMileage > 0 && GlobalAutoBehaviour<AccountManager>.instance.StarterGachaManager.GetRemainLimitChance(gachaIndex) <= 0)
		{
			OnErrorExecEquipGacha(data, NShared.ExecEquipGacha.ResultType.StartPickupLimitOver);
		}
		else if (data.OpenGem > 0)
		{
			int gemApplyValidDiscounts = Calculator.GetGemApplyValidDiscounts(data);
			gemApplyValidDiscounts = Calculator.GetStepUpDiscount(data, gemApplyValidDiscounts);
			long totalGem = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TotalGem;
			int num = gemApplyValidDiscounts;
			long num2 = totalGem;
			int num3 = num;
			if (num3 > num2)
			{
				OnErrorExecEquipGacha(data, NShared.ExecEquipGacha.ResultType.NotEnoughGem);
				return;
			}
			PayShopPriceMessagePopup.Open(data, delegate(bool isYes)
			{
				if (isYes)
				{
					RequestExecEquipGacha(gachaIndex, type, heroIndices);
				}
			});
		}
		else
		{
			long gold = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gold;
			if (data.OpenGold > gold)
			{
				OnErrorExecEquipGacha(data, NShared.ExecEquipGacha.ResultType.NotEnoughGold);
			}
			else
			{
				RequestExecEquipGacha(gachaIndex, type, heroIndices);
			}
		}
	}

	private void RequestExecEquipGacha(int gachaIndex, HighGachaCategoryType type = HighGachaCategoryType.None, int[] heroIndices = null, bool useTicket = false)
	{
		NewEquipGachaData equipGachaData = GameRoot.Instance.NewEquipGachaDataContainer.GetData(gachaIndex);
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.FreeEquipGachaManager.Free(gachaIndex);
		int itemIndex = 0;
		bool flag2 = false;
		int discountedCost = 0;
		if (!flag)
		{
			if (useTicket)
			{
				itemIndex = GameRoot.Instance.GachaSelectItemDataContainer.GetGachaTicketIndex(gachaIndex);
			}
			else
			{
				discountedCost = ((equipGachaData.OpenGem > 0) ? equipGachaData.OpenGem : equipGachaData.OpenGold);
				flag2 = Checker.IsDiscount(equipGachaData);
				if (flag2)
				{
					discountedCost = Calculator.GetGemApplyValidDiscounts(equipGachaData);
				}
				discountedCost = Calculator.GetStepUpDiscount(equipGachaData, discountedCost);
			}
		}
		NShared.ExecEquipGacha.Request request = new NShared.ExecEquipGacha.Request
		{
			GachaIndex = gachaIndex,
			ItemIndex = itemIndex,
			Free = flag,
			Discount = flag2,
			DiscountedCost = discountedCost,
			HighGachaCategory = type,
			HeroIndices = heroIndices
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ExecEquipGacha.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.ExecEquipGacha.ResultType.Success)
			{
				OnErrorExecEquipGacha(equipGachaData, response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.FreeEquipGachaManager.Set(response.GachaInfo);
				GlobalAutoBehaviour<AccountManager>.instance.EquipGachaManager.Set(response.EquipGachaInfo);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				GlobalAutoBehaviour<AccountManager>.instance.ApplyItemGachaResult(response.GachaItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
				GlobalAutoBehaviour<AccountManager>.instance.StepUpEquipGachamanager.Apply(response.StepUpEquipGachaInfo);
				GlobalAutoBehaviour<AccountManager>.instance.StarterGachaManager.Set(response.StarterPickupEquipGachaInfo);
				GlobalAutoBehaviour<AccountManager>.instance.ApplyRewardResult(response.StarterPickupRewardInfo);
				GlobalAutoBehaviour<AccountManager>.instance.ApplyRewardResult(response.StepUpRewardInfo);
				GlobalAutoBehaviour<AccountManager>.instance.PetGachaCountManager.Apply(response.PetGachaCountInfos);
				GlobalAutoBehaviour<AccountManager>.instance.ApplyRewardResult(response.PetRewardInfo);
				GlobalAutoBehaviour<AccountManager>.instance.EquipGachaCeilingManager.Apply(response.GachaCeilingCountInfos);
				GlobalAutoBehaviour<AccountManager>.instance.ApplyRewardResult(response.GachaCeilingRewardInfo);
				GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Apply(response.PlayerProductPurchaseInfo);
				if (IsOpened<PayShopEquipGachaScene>())
				{
					GetInstance<PayShopEquipGachaScene>().PlayGachaAnimation(response.GachaItemResults, response.StepUpRewardInfo, response.StarterPickupRewardInfo, response.PetRewardInfo, response.GachaCeilingRewardInfo);
				}
				if (IsOpened<NGame2.NUI.NWindow.NPayShop.PayShop>())
				{
					NGame2.NUI.NWindow.NPayShop.PayShop payShop = GetInstance<NGame2.NUI.NWindow.NPayShop.PayShop>();
					payShop.Refresh();
					payShop.UpdateCategoryFlag(NewPayShopCategoryType.Gacha.ToString());
				}
			}
		});
	}

	private void OnErrorExecEquipGacha(NewEquipGachaData equipGachaData, NShared.ExecEquipGacha.ResultType error)
	{
		switch (error)
		{
		case NShared.ExecEquipGacha.ResultType.Success:
			break;
		case NShared.ExecEquipGacha.ResultType.NotEnoughGem:
			OpenPopupNotEnoughMoney(CurrencyType.Gem, delegate
			{
				CloseInstance<PayShopEquipGachaScene>();
			});
			break;
		case NShared.ExecEquipGacha.ResultType.NotEnoughGold:
			OpenPopupNotEnoughMoney(CurrencyType.Gold, delegate
			{
				CloseInstance<PayShopEquipGachaScene>();
			});
			break;
		case NShared.ExecEquipGacha.ResultType.ClosedEquipGacha:
			if (equipGachaData.Type == EquipGachaType.StepUp)
			{
				string stringDirect = LocalizationManager.GetStringDirect("COMMON_NOTIFICATION");
				string stringDirect2 = LocalizationManager.GetStringDirect("STEPUP_GACHA_CANNOT_USE");
				PayShopMessagePopup.Open(stringDirect, stringDirect2, delegate
				{
					CloseInstance<PayShopEquipGachaScene>();
				});
			}
			else
			{
				PayShopMessagePopup.OpenError(error, delegate
				{
					CloseInstance<PayShopEquipGachaScene>();
				});
			}
			break;
		case NShared.ExecEquipGacha.ResultType.TicketItemIndexEmpty:
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string message = LocalizationManager.GetString(Gacha.NOT_ENOUGH_TICKET_ITEM);
			PayShopMessagePopup.Open(title, message);
			break;
		}
		case NShared.ExecEquipGacha.ResultType.StartPickupLimitOver:
			PayShopMessagePopup.OpenError(error, delegate
			{
				CloseInstance<PayShopEquipGachaScene>();
			});
			break;
		default:
			PayShopMessagePopup.OpenError(error);
			break;
		}
	}

	void PayShopEquipGachaScene.IListener.OnClickRuby()
	{
		OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
	}

	void PayShopEquipGachaScene.IListener.OnClickGold()
	{
		OpenNewPayShop(NewPayShopCategoryType.Gold.ToString());
	}

	void PayShopEquipGachaScene.IListener.OnClickProbability(int gachaIndex, HighGachaCategoryType gachaType)
	{
		NewEquipGachaData data = GameRoot.Instance.NewEquipGachaDataContainer.GetData(gachaIndex);
		if (data != null && data.ShowProbability)
		{
			ProbabilityPopup.Open(ProbabilityShopType.GachaShop, data, gachaType);
		}
	}

	void PayShopEquipGachaScene.IListener.OnClickHeroInfo(int heroIndex, List<CreatureData> listHeros)
	{
		OpenHeroInfoPopup(heroIndex, isSteupGacha: true, listHeros);
	}

	void PayShopEquipGachaScene.IListener.RequestPetMileageReward(int gachaIndex, Action<int> onComplete)
	{
		NShared.PetGachaRoofReward.Request request = new NShared.PetGachaRoofReward.Request
		{
			GachaIndex = gachaIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.PetGachaRoofReward.Response response)
		{
			if (response.BaseResult == BaseResultType.Success && response.Result == NShared.PetGachaRoofReward.ResultType.Success)
			{
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.PetGachaCountManager.Apply(response.PetGachaCountInfo);
				string stringDirect = LocalizationManager.GetStringDirect("PET_MILEAGE_REWARD_TITLE");
				BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(stringDirect).SetItemResultInfos(response.ItemResults), isGlobal: false, delegate
				{
					int mileage = GlobalAutoBehaviour<AccountManager>.instance.PetGachaCountManager.GetMileage(gachaIndex);
					Vespa_System_Action_Extension_Method.fire(onComplete, mileage);
				});
			}
		});
	}

	private bool CheckGachaSelectShopProductPurcaseException(NewPayShopProductInfo productInfo)
	{
		return CheckPurchaseException<GachaSelectShopPopup>(productInfo);
	}

	void GachaSelectShopPopup.IListener.OnBuyGachaSelectShopProduct(NewPayShopProductInfo productInfo)
	{
		if (CheckGachaSelectShopProductPurcaseException(productInfo))
		{
			PurchaseRightAwayProductProcess(productInfo);
		}
	}

	void GachaSelectShopPopup.IListener.OnClickToggle(int toggleIndex)
	{
		string categoryGroup = GlobalAutoBehaviour<AccountManager>.instance.GachaSelectShopManager.GetCategoryGroup(toggleIndex);
		if (String_Extension_Method.IsNullOrEmpty(categoryGroup))
		{
			return;
		}
		GachaSelectShopPopup window = GetInstance<GachaSelectShopPopup>();
		if (window == null)
		{
			return;
		}
		GachaSelctShopHelper.RequestSelectShopinfo(categoryGroup, delegate(NShared.GetSelectShopInfo.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				PayShopMessagePopup.OpenError(response.BaseResult, delegate
				{
					window.Close();
				});
			}
			else if (response.Result != NShared.GetSelectShopInfo.ResultType.Success)
			{
				PayShopMessagePopup.OpenError(response.Result, delegate
				{
					window.Close();
				});
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.GachaSelectShopManager.Add(response.SelectShopInfo);
				window.Setup(response.SelectShopInfo);
			}
		});
	}

	void GachaSelectShopPopup.IListener.OnClickRefresh(int toggleIndex)
	{
		string categoryGroup = GlobalAutoBehaviour<AccountManager>.instance.GachaSelectShopManager.GetCategoryGroup(toggleIndex);
		if (String_Extension_Method.IsNullOrEmpty(categoryGroup))
		{
			return;
		}
		GachaSelectShopPopup window = GetInstance<GachaSelectShopPopup>();
		if (window == null || !window.IsOpen())
		{
			return;
		}
		GachaSelctShopHelper.RequestStockShopInfo(categoryGroup, delegate(NShared.RestockSelectShopInfo.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				PayShopMessagePopup.OpenError(response.BaseResult, delegate
				{
					window.Close();
				});
			}
			else if (response.Result != NShared.RestockSelectShopInfo.ResultType.Success)
			{
				PayShopMessagePopup.OpenError(response.Result, delegate
				{
					window.Close();
				});
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.GachaSelectShopManager.Add(response.SelectShopInfo);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
				window.Setup(response.SelectShopInfo);
			}
		});
	}

	void GachaSelectShopPopup.IListener.OnClickGachaSelectShopProbabilty(SelectShopInfo selectShopInfo)
	{
		ProbabilityPopup.Open(ProbabilityShopType.GachaShop, selectShopInfo);
	}

	public void OpenGachaSelectShopPopup()
	{
		GachaSelectShopPopup window = GetInstance<GachaSelectShopPopup>();
		if (window == null || window.IsOpen())
		{
			return;
		}
		SelectShopNoticeInfo info = GlobalAutoBehaviour<AccountManager>.instance.GachaSelectShopManager.GetSelectShopNoticeInfo();
		if (info == null)
		{
			return;
		}
		GachaSelctShopHelper.RequestSelectShopinfo(info.OpenCategory, delegate(NShared.GetSelectShopInfo.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				PayShopMessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.GetSelectShopInfo.ResultType.Success)
			{
				PayShopMessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.GachaSelectShopManager.Add(response.SelectShopInfo);
				window.Open(info, response.SelectShopInfo, this);
			}
		});
	}
}
