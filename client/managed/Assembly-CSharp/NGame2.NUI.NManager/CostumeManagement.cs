using System;
using System.Text;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager.NCostume;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NCustomizing;
using NGame2.NUtil.NCostume;
using NShared;
using NShared.BuyCustomizingCostumes;
using NShared.NLocalization;
using NShared.RefreshPayshopDiscountInfo;
using NShared.ResetAllCustomizingCostumes;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;

namespace NGame2.NUI.NManager;

public class CostumeManagement : BaseManagement<CostumeManagement>, HeroCostumeView.Listener, HeroFullSceneViewBase.Listener
{
	private WebServiceRequest _currentServiceRequest;

	private PoolingList<string> _stringList;

	public MySetCostumeRequester MySetCostumeRequester { get; private set; }

	public HairCostumeRequester HairCostumeRequester { get; private set; }

	public HeroCostumeRequester HeroCostumeRequester { get; private set; }

	public WeaponCostumeRequester WeaponCostumeRequester { get; private set; }

	public AccessoryCostumeRequester AccessoryCostumeRequester { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		UseBackgroundBlack(use: true);
		SetRequester();
		AddInstance<HeroCostumeView>("Resources/Prefab2/Window/FullScene/HeroCostumeView");
		AddInstance<LegendCostumeReservationPopup>("Resources/Prefab2/Window/PayShop/LegendCostumeReservationPopup");
	}

	private void SetRequester()
	{
		MySetCostumeRequester = new MySetCostumeRequester(_currentServiceRequest);
		HairCostumeRequester = new HairCostumeRequester(_currentServiceRequest);
		HeroCostumeRequester = new HeroCostumeRequester(_currentServiceRequest);
		WeaponCostumeRequester = new WeaponCostumeRequester(_currentServiceRequest);
		AccessoryCostumeRequester = new AccessoryCostumeRequester(_currentServiceRequest);
	}

	public void OpenCostumeMySet(int heroIndex)
	{
		OnSelectHero(heroIndex, ToggleType.MySet, 0);
	}

	public void OpenCostume(int viewCostumeIndex = 0)
	{
		CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(viewCostumeIndex);
		if (data != null)
		{
			OnSelectHero(data.HeroIndex, ToggleType.Costume, viewCostumeIndex);
		}
	}

	public void OpenCostume(int heroIndex, int ViewCostumeIndex = 0)
	{
		OnSelectHero(heroIndex, ToggleType.Costume, ViewCostumeIndex);
	}

	public void OpenAccessoryCostume(int heroIndex, AccessoryCostumeData accessoryCostumeData)
	{
		ToggleType toggleType = ToggleType.None;
		foreach (ToggleType item in ToggleTypeHelper.Types())
		{
			if (item.GetAccessoryType() == accessoryCostumeData.PartType)
			{
				toggleType = item;
			}
		}
		if (toggleType == ToggleType.None)
		{
			VespaDebug.LogError($"not found type of accessoryIndex ({accessoryCostumeData.Index})");
		}
		else
		{
			OnSelectHero(heroIndex, toggleType, accessoryCostumeData.Index);
		}
	}

	public void OpenAccessoryCostume(int ViewCostumeIndex = 0)
	{
		AccessoryCostumeData accessoryData = GameRoot.Instance.AccessoryCostumeDataContainer.GetData(ViewCostumeIndex);
		if (accessoryData == null)
		{
			VespaDebug.LogError("accessoryData == null");
			return;
		}
		ToggleType windowToggleType = ToggleType.None;
		foreach (ToggleType item in ToggleTypeHelper.Types())
		{
			if (item.GetAccessoryType() == accessoryData.PartType)
			{
				windowToggleType = item;
			}
		}
		HeroSelect.OpenPopup(delegate(int selectheroIndex)
		{
			if (windowToggleType != ToggleType.None)
			{
				OnSelectHero(selectheroIndex, windowToggleType, ViewCostumeIndex);
			}
			else
			{
				OnSelectHero(selectheroIndex, ToggleType.Head, 0);
			}
		}, (CreatureData creatureData) => __Extension_Method_ICollection__.IsNullOrEmpty(accessoryData.UnableHero) || !TypeIterator.Contains(accessoryData.UnableHero, creatureData.Index));
	}

	public void OpenAccessoryView(int accessoryIndex, int heroIndex)
	{
		AccessoryCostumeData data = GameRoot.Instance.AccessoryCostumeDataContainer.GetData(accessoryIndex);
		if (data == null)
		{
			VespaDebug.LogError("accessoryData == null");
			return;
		}
		ToggleType toggleType = ToggleType.None;
		foreach (ToggleType item in ToggleTypeHelper.Types())
		{
			if (item.GetAccessoryType() == data.PartType)
			{
				toggleType = item;
			}
		}
		OnSelectHero(heroIndex, toggleType, accessoryIndex);
	}

	private void OnSelectHero(int heroIndex, ToggleType toggleType, int ViewCostumeIndex)
	{
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroIndex);
		if (cachedHeroInfo == null)
		{
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
			if (data == null)
			{
				VespaDebug.LogError("CretureData is null : heroIndex=" + heroIndex);
				return;
			}
			cachedHeroInfo = new CachedHeroInfo(data);
		}
		GetInstance<HeroCostumeView>().Open(cachedHeroInfo, this, toggleType, delegate
		{
			if (ViewCostumeIndex > 0)
			{
				GetInstance<HeroCostumeView>().SelectCostume(toggleType, ViewCostumeIndex);
			}
		});
	}

	void HeroCostumeView.Listener.OnClickPlusGem()
	{
		GetInstance<HeroCostumeView>().CloseAndDoAction(delegate
		{
			SceneAutoBehaviour<InventoryManagement>.instance.CloseActiveWindows(EDepthOrder.Topmost);
			SceneAutoBehaviour<LobbyManagement>.instance.CloseActiveWindows(EDepthOrder.Topmost);
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
		});
	}

	void HeroCostumeView.Listener.OnClickPlusGold()
	{
		GetInstance<HeroCostumeView>().CloseAndDoAction(delegate
		{
			SceneAutoBehaviour<InventoryManagement>.instance.CloseActiveWindows(EDepthOrder.Topmost);
			SceneAutoBehaviour<LobbyManagement>.instance.CloseActiveWindows(EDepthOrder.Topmost);
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Gold.ToString());
		});
	}

	void HeroCostumeView.Listener.OnClickViewMotion(HeroCostumeView caller)
	{
		CachedHeroInfo heroinfo = SprkMotionCostumes.Snapshot(caller.CurrentLocalInfo);
		SceneAutoBehaviour<LobbyManagement>.instance.OpenHeroDetailView(heroinfo);
	}

	void HeroFullSceneViewBase.Listener.OnOpen()
	{
		Hero3DViewPanel hero3DView = SceneAutoBehaviour<MyHeroManagement>.instance.GetHero3DView();
		if (UIHelper.IsEnabledGameObjet(hero3DView))
		{
			hero3DView.ActivatePreview(isActive: false);
		}
	}

	void HeroFullSceneViewBase.Listener.OnClose()
	{
		Hero3DViewPanel hero3DView = SceneAutoBehaviour<MyHeroManagement>.instance.GetHero3DView();
		if (UIHelper.IsEnabledGameObjet(hero3DView))
		{
			hero3DView.ActivatePreview(isActive: true);
		}
	}

	void HeroCostumeView.Listener.ProcessSelectedCustomizingCostumes(HeroCustomizingDataGetter dataGetter, Action onComplete)
	{
		CustomizingCostumesStock stock = CustomizingHelper.CollectCostumes(dataGetter);
		CustomizingHelper.CreateRequest(stock, delegate(NShared.BuyCustomizingCostumes.Request request)
		{
			string failReason = string.Empty;
			switch (CustomizingHelper.GetFailResult(stock.CollectResultList, out failReason))
			{
			case CollectCostumeFailReason.ContainIncollectCostume:
			case CollectCostumeFailReason.NeedOwn:
			{
				string stringDirect = LocalizationManager.GetStringDirect("CONTENT_CUSTOMIZE_TOTAL_BUY_FAIL_NO_COSTUME_TITLE");
				string stringDirect2 = LocalizationManager.GetStringDirect("CONTENT_CUSTOMIZE_TOTAL_BUY_FAIL_NO_COSTUME_DESC", failReason);
				MessagePopup.Open(stringDirect, stringDirect2);
				break;
			}
			case CollectCostumeFailReason.None:
			case CollectCostumeFailReason.NeedOwnLegend:
				if (request.BuyGem > 0)
				{
					Action<CustomizingCostumesStock, NShared.BuyCustomizingCostumes.Request, Action> onConfirm = RequestBuyCostumes;
					long totalGem = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TotalGem;
					int buyGem = request.BuyGem;
					long num = totalGem;
					int num2 = buyGem;
					if (num2 > num)
					{
						onConfirm = delegate
						{
							GoToRubyShop();
						};
					}
					bool useExceptLegendLabel = false;
					CostumeData costumeData = Vespa_System_Action_Extension_Method.fire(dataGetter.HeroCostumeDataGetter);
					if (costumeData != null && costumeData.CostumeType == EHeroCostumeType.Legend)
					{
						useExceptLegendLabel = true;
					}
					if (CustomizingHelper.CheckUnsetLegendCostume(dataGetter))
					{
						OpenLegendUnequipApplyPopup(needBuy: true, delegate
						{
							OpenPurchaseConfirmPopup(stock, request, onConfirm, onComplete, useExceptLegendLabel);
						});
					}
					else
					{
						OpenPurchaseConfirmPopup(stock, request, onConfirm, onComplete, useExceptLegendLabel);
					}
				}
				else if (CustomizingHelper.CheckUnsetLegendCostume(dataGetter))
				{
					OpenLegendUnequipApplyPopup(needBuy: false, delegate
					{
						RequestBuyCostumes(stock, request, onComplete);
					});
				}
				else
				{
					RequestBuyCostumes(stock, request, onComplete);
				}
				break;
			}
		});
	}

	private void OpenLegendUnequipApplyPopup(bool needBuy, Action onComplete)
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect(needBuy ? "COSTUME_LEGEND_UNEQUIP_NOTICE_DRESSROOM_PURCHASE" : "COSTUME_LEGEND_UNEQUIP_NOTICE_DRESSROOM_APPLY");
		MessagePopup3Button.Open(title, stringDirect, null, onComplete, NShared.NLocalization.Common.COMMON_NO, NShared.NLocalization.Common.COMMON_YES);
	}

	private void GoToRubyShop()
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM);
		MessagePopup.Open(title, message, delegate
		{
			SceneAutoBehaviour<InventoryManagement>.instance.CloseActiveWindows(EDepthOrder.Topmost);
			SceneAutoBehaviour<LobbyManagement>.instance.CloseActiveWindows(EDepthOrder.Topmost);
			HeroCostumeView heroCostumeView = GetInstance<HeroCostumeView>(openedOnly: true);
			if ((bool)heroCostumeView)
			{
				heroCostumeView.CloseScene();
			}
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
		}, null);
	}

	private void OpenPurchaseConfirmPopup(CustomizingCostumesStock stock, NShared.BuyCustomizingCostumes.Request request, Action<CustomizingCostumesStock, NShared.BuyCustomizingCostumes.Request, Action> onYes, Action onComlete, bool useExceptLegendLabel)
	{
		string title = LocalizationManager.GetString(PayShop.GEM_CONFIRM_TITLE);
		string stringDirect = LocalizationManager.GetStringDirect("CONTENT_CUSTOMIZE_BUY_ALL_INFO_DESC", stock.PopupMessage);
		string yes = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_YES);
		if (!useExceptLegendLabel)
		{
			PreserveMessagePopup2.OpenPopup4(title, stringDirect, yes, PreserveMessagePopup2.EPriceType.Gem, stock.BuyReqGem, stock.Mileage, stock.PayShopEventSearchInfos, delegate
			{
				onYes(stock, request, onComlete);
			});
		}
		else
		{
			PreserveMessagePopup2.OpenPopup5(title, stringDirect, yes, PreserveMessagePopup2.EPriceType.Gem, stock.BuyReqGem, stock.Mileage, stock.PayShopEventSearchInfos, delegate
			{
				onYes(stock, request, onComlete);
			});
		}
	}

	private void RequestBuyCostumes(CustomizingCostumesStock stock, NShared.BuyCustomizingCostumes.Request req, Action onComplete)
	{
		stock.RefreshPrice();
		if (req.BuyGem != stock.BuyReqGem)
		{
			req.BuyGem = stock.BuyReqGem;
		}
		_currentServiceRequest = SingletonUpdater<WebServiceManager>.instance.Requester.Request(req, delegate(NShared.BuyCustomizingCostumes.Response response)
		{
			if (response.Result != NShared.BuyCustomizingCostumes.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				CustomizingHelper.OnResponse(stock, req, response);
				Vespa_System_Action_Extension_Method.fire(onComplete);
				if (stock.BuyReqGem > 0 && __Extension_Method_ICollection__.COUNT(stock.PurchaseProductList) > 0)
				{
					if (__Extension_Method_ICollection__.COUNT(stock.PurchaseProductList) == 1)
					{
						CenterMessage.Open(LocalizationManager.GetString(NShared.NLocalization.Shop.COSTUME_BOUGHT_MSG, stock.PurchaseProductList[0].Item2));
					}
					else
					{
						StringBuilder stringBuilder = new StringBuilder();
						stringBuilder.Append(LocalizationManager.GetStringDirect("CONTENT_CUSTOMIZE_BUY_MULTI_CENTER_MESSAGE"));
						for (int i = 0; i < __Extension_Method_ICollection__.COUNT(stock.PurchaseProductList); i++)
						{
							Tuple<int, string> tuple = stock.PurchaseProductList[i];
							stringBuilder.AppendLine();
							stringBuilder.Append(LocalizationManager.GetStringDirect("CONTENTS_CUSTOMIZE_MY_SET_NAME_FORM", tuple.Item1, tuple.Item2));
						}
						CenterMessage.Open(stringBuilder.ToString());
					}
				}
			}
		});
	}

	private void RequestResetCostumes(int heroIndex, Action onComplete)
	{
		NShared.ResetAllCustomizingCostumes.Request request = new NShared.ResetAllCustomizingCostumes.Request();
		request.HeroIndex = heroIndex;
		_currentServiceRequest = SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ResetAllCustomizingCostumes.Response response)
		{
			if (response.Result != NShared.ResetAllCustomizingCostumes.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				CustomizingHelper.SetHeroCostume(response.HeroCostumeResult);
				Vespa_System_Action_Extension_Method.fire(onComplete);
			}
		});
	}

	public void RequestLegendCostumeReservationPopup(int productIndex = 0, LegendCostumeReservationPopup.EShowOption showOption = LegendCostumeReservationPopup.EShowOption.None)
	{
		if (LegendCostumeReservationPopup.RequestCoolTime > 0 && (DateTime.Now - LegendCostumeReservationPopup.PrevRequestDateTime).TotalSeconds < (double)LegendCostumeReservationPopup.RequestCoolTime)
		{
			OpenLegendCostumeReservationPopup(productIndex, showOption);
			return;
		}
		RequestRefreshPayshopDiscountInfo(delegate
		{
			OpenLegendCostumeReservationPopup(productIndex, showOption);
		});
	}

	private void OpenLegendCostumeReservationPopup(int productIndex = 0, LegendCostumeReservationPopup.EShowOption showOption = LegendCostumeReservationPopup.EShowOption.None)
	{
		LegendCostumeReservationPopup legendCostumeReservationPopup = GetInstance<LegendCostumeReservationPopup>();
		if (!(legendCostumeReservationPopup == null))
		{
			legendCostumeReservationPopup.Open(productIndex, showOption);
		}
	}

	public void RequestRefreshPayshopDiscountInfo(Action onSuccess = null, Action onFail = null)
	{
		NShared.RefreshPayshopDiscountInfo.Request request = new NShared.RefreshPayshopDiscountInfo.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.RefreshPayshopDiscountInfo.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				Vespa_System_Action_Extension_Method.fire(onFail);
			}
			else if (response.Result != NShared.RefreshPayshopDiscountInfo.ResultType.Success)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
				Vespa_System_Action_Extension_Method.fire(onFail);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.PayShopDiscountProductManager.Reset(response.PayShopDiscountInfos);
				LegendCostumeReservationPopup.RequestCoolTime = response.RefreshTimeSec;
				LegendCostumeReservationPopup.PrevRequestDateTime = DateTime.Now;
				Vespa_System_Action_Extension_Method.fire(onSuccess);
			}
		});
	}

	void HeroCostumeView.Listener.ResetHeroCostumes(int heroIndex, Action onComplete)
	{
		RequestResetCostumes(heroIndex, onComplete);
	}

	void HeroCostumeView.Listener.OpenLegendCostumeReservationPooup(int costumeIndex)
	{
		CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(costumeIndex);
		if (data != null)
		{
			int num = data.ProductIndex;
			if (data.IsBonusCostume())
			{
				num = GameRoot.Instance.CostumeDataContainer.GetData(data.MainIndex)?.ProductIndex ?? 0;
			}
			if (num != 0)
			{
				RequestLegendCostumeReservationPopup(num);
			}
		}
	}
}
