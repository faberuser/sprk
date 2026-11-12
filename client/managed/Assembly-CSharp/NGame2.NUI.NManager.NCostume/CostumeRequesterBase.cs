using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUtil.NCostume;
using NShared;
using NShared.BuyCostume;
using NShared.BuyCustomizingCostumes;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;
using UnityEngine;

namespace NGame2.NUI.NManager.NCostume;

public class CostumeRequesterBase
{
	protected WebServiceRequest _currentServiceRequest;

	public CostumeRequesterBase(WebServiceRequest webServiceRequest)
	{
		_currentServiceRequest = webServiceRequest;
	}

	protected bool IsWebRequesterProcessing()
	{
		return WebServiceRequest.IsProgressing(_currentServiceRequest);
	}

	protected TInstance GetCostumeManagementInstance<TInstance>(bool openedOnly = false) where TInstance : Component, IWindow
	{
		return SceneAutoBehaviour<CostumeManagement>.instance.GetInstance<TInstance>();
	}

	protected void MoveToSubStory(int heroIndex)
	{
		if (SceneAutoBehaviour<SubStoryManagement>.instance.CheckMoveSubStoryChapter(heroIndex))
		{
			SceneAutoBehaviour<SubStoryManagement>.instance.SetEntryNode(null);
			SceneAutoBehaviour<PayShopManagement>.instance.CloseAllActiveWindow();
			GetCostumeManagementInstance<HeroCostumeView>().CloseAndDoAction(delegate
			{
				SceneAutoBehaviour<SubStoryManagement>.instance.MoveSubStoryChapter(heroIndex);
			});
		}
	}

	protected void RequestBuyCostume(CostumeData costumeData, Action<CostumeData> OnComplete)
	{
		if (costumeData == null || costumeData.IsFreeCostume())
		{
			return;
		}
		CostumeData BuyCostumeData = costumeData;
		Enum key = null;
		Enum key2 = null;
		PreserveMessagePopup2.EPriceType priceType = PreserveMessagePopup2.EPriceType.None;
		int ReqValue = 0;
		int obtainMileage = 0;
		if (BuyCostumeData.ReqBuyGem > 0)
		{
			key = PayShop.GEM_CONFIRM_TITLE;
			key2 = Costume.COSTUME_BUY_GEM_CONFIRM_MSG_RENEW;
			priceType = PreserveMessagePopup2.EPriceType.Gem;
			ReqValue = HeroCostume.GetPriceGem(BuyCostumeData);
			obtainMileage = HeroCostume.GetObtainMileage(BuyCostumeData);
			long totalGem = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TotalGem;
			int num = ReqValue;
			long num2 = totalGem;
			int num3 = num;
			if (num3 > num2)
			{
				string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
				string message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM);
				MessagePopup.Open(title, message, delegate
				{
					SceneAutoBehaviour<InventoryManagement>.instance.CloseActiveWindows(EDepthOrder.Topmost);
					SceneAutoBehaviour<LobbyManagement>.instance.CloseActiveWindows(EDepthOrder.Topmost);
					GetCostumeManagementInstance<HeroCostumeView>().CloseScene();
					SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
				}, null);
				return;
			}
		}
		if (BuyCostumeData.ReqBuyGold > 0)
		{
			key = Costume.COSTUME_BUY_GOLD_TITLE;
			key2 = Costume.COSTUME_BUY_GOLD_CONFIRM_MSG_RENEW;
			priceType = PreserveMessagePopup2.EPriceType.Gold;
			ReqValue = BuyCostumeData.ReqBuyGold;
			long gold = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gold;
			if (ReqValue > gold)
			{
				MessagePopup.OpenError(NShared.BuyCostume.ResultType.NotEnoughGold);
				return;
			}
		}
		if (BuyCostumeData.ReqBuyMileage > 0)
		{
			key = Costume.COSTUME_BUY_MILEAGE_TITLE;
			key2 = Costume.COSTUME_BUY_MILEAGE_CONFIRM_MSG_RENEW;
			priceType = PreserveMessagePopup2.EPriceType.Mileage;
			ReqValue = BuyCostumeData.ReqBuyMileage;
			long mileage = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Mileage;
			int num4 = ReqValue;
			long num5 = mileage;
			int num6 = num4;
			if (num6 > num5)
			{
				MessagePopup.OpenError(NShared.BuyCostume.ResultType.NotEnoughMileage);
				return;
			}
		}
		string title2 = LocalizationManager.GetString(key);
		string content = LocalizationManager.GetString(key2, $"[{BuyCostumeData.CostumeName}]");
		string yes = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_YES);
		List<IPayShopEventSearchInfo> list = new List<IPayShopEventSearchInfo>();
		list.Add(costumeData);
		PreserveMessagePopup2.OpenPopup4(title2, content, yes, priceType, ReqValue, obtainMileage, list, delegate
		{
			NShared.BuyCostume.Request request = new NShared.BuyCostume.Request
			{
				CostumeIndex = BuyCostumeData.CostumeIndex,
				HeroIndex = BuyCostumeData.HeroIndex,
				BuyGem = ReqValue,
				BuyGold = ReqValue,
				BuyMileage = ReqValue
			};
			_currentServiceRequest = SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.BuyCostume.Response response)
			{
				if (response.Result != NShared.BuyCostume.ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
				}
				else
				{
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
					GlobalAutoBehaviour<AccountManager>.instance.CostumeManager.Apply(response.CostumeResults);
					_ = (object)null;
					_ = 18;
					_ = 1;
					_ = 0;
					Array.Empty<object>();
					GlobalAutoBehaviour<AccountManager>.instance.HeroManager.SetCostumeIndex(BuyCostumeData.HeroIndex, costumeData.CostumeIndex);
					CenterMessage.Open(LocalizationManager.GetString(NShared.NLocalization.Shop.COSTUME_BOUGHT_MSG, BuyCostumeData.CostumeName));
					Vespa_System_Action_Extension_Method.fire(OnComplete, costumeData);
				}
			});
		});
	}

	protected void ApplyCustomizingCostumes(HeroCustomizingDataGetter getter, Action onComplete)
	{
		CreateRequest(getter, delegate(NShared.BuyCustomizingCostumes.Request req, CustomizingCostumesStock stock)
		{
			if (req.BuyGem == 0)
			{
				BaseRequestAction(req, stock, onComplete);
			}
			else
			{
				long totalGem = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TotalGem;
				int buyGem = req.BuyGem;
				long num = totalGem;
				int num2 = buyGem;
				if (num2 > num)
				{
					string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
					string message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM);
					MessagePopup.Open(title, message, delegate
					{
						SceneAutoBehaviour<InventoryManagement>.instance.CloseActiveWindows(EDepthOrder.Topmost);
						SceneAutoBehaviour<LobbyManagement>.instance.CloseActiveWindows(EDepthOrder.Topmost);
						GetCostumeManagementInstance<HeroCostumeView>().CloseScene();
						SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
					}, null);
				}
				else
				{
					string arg = ((__Extension_Method_ICollection__.COUNT(stock.PurchaseProductList) > 0) ? stock.PurchaseProductList[0].Item2 : string.Empty);
					string title2 = LocalizationManager.GetString(PayShop.GEM_CONFIRM_TITLE);
					string content = LocalizationManager.GetString(Costume.COSTUME_BUY_GEM_CONFIRM_MSG_RENEW, $"[{arg}]");
					string yes = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_YES);
					PreserveMessagePopup2.EPriceType priceType = PreserveMessagePopup2.EPriceType.Gem;
					int buyGem2 = req.BuyGem;
					PreserveMessagePopup2.OpenPopup4(title2, content, yes, priceType, buyGem2, stock.Mileage, stock.PayShopEventSearchInfos, delegate
					{
						BaseRequestAction(req, stock, onComplete);
					});
				}
			}
		});
	}

	private void CreateRequest(HeroCustomizingDataGetter getter, Action<NShared.BuyCustomizingCostumes.Request, CustomizingCostumesStock> requestAction)
	{
		CustomizingCostumesStock stock = CustomizingHelper.CollectCostumes(getter);
		CustomizingHelper.CreateRequest(stock, delegate(NShared.BuyCustomizingCostumes.Request req)
		{
			requestAction(req, stock);
		});
	}

	private void BaseRequestAction(NShared.BuyCustomizingCostumes.Request request, CustomizingCostumesStock stock, Action onComplete)
	{
		_currentServiceRequest = SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.BuyCustomizingCostumes.Response response)
		{
			if (response.Result != NShared.BuyCustomizingCostumes.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				CustomizingHelper.OnResponse(stock, request, response);
				Vespa_System_Action_Extension_Method.fire(onComplete);
				if (request.BuyGem > 0 && __Extension_Method_ICollection__.COUNT(stock.PurchaseProductList) > 0)
				{
					CenterMessage.Open(LocalizationManager.GetString(NShared.NLocalization.Shop.COSTUME_BOUGHT_MSG, stock.PurchaseProductList[0].Item2));
				}
			}
		});
	}
}
