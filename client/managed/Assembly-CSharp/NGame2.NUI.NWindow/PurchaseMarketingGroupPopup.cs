using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2.NPayShop;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class PurchaseMarketingGroupPopup : BaseWindow, PayShopProductComponent.IListener, PayShopBaseComponent.IListener
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/PurchaseMarketingGroupPopup";

	public UILabel TitleLabel;

	public UIScrollGrid ScrollGrid;

	private List<PayShopProductComponent> _componentList = new List<PayShopProductComponent>();

	private float _lastUpdateTime;

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_componentList);
	}

	public static void Open(MarketingType type)
	{
		WindowPrefab<PurchaseMarketingGroupPopup> windowPrefab = new WindowPrefab<PurchaseMarketingGroupPopup>("Resources/Prefab2/Window/MessagePopup/PurchaseMarketingGroupPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(type);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	protected void OpenInternal(MarketingType type)
	{
		base.Open();
		Init(type);
	}

	private void Init(MarketingType type)
	{
		string stringDirect = LocalizationManager.GetStringDirect("CONDITION_PACKAGE_TEXT_NAME");
		UIHelper.SetLabelText(TitleLabel, stringDirect);
		SetProduct(type);
	}

	private void SetProduct(MarketingType type)
	{
		NewPayShopProductInfo newPayShopProductInfo = null;
		foreach (PurchaseMarketingInfo item in GlobalAutoBehaviour<AccountManager>.instance.PurchaseMarketingManager)
		{
			if (!PurchaseMarketingManager.IsPurchaseMarketing(item))
			{
				continue;
			}
			PurchaseMarketingPopupData purchaseMarketingPopupData = GameRoot.Instance.PurchaseMarketingPopupDataContainer.GetPurchaseMarketingPopupData(item.PurchaseIndex);
			if (purchaseMarketingPopupData == null || purchaseMarketingPopupData.Type != type)
			{
				continue;
			}
			PurchaseMarketingData data = GameRoot.Instance.PurchaseMarketingDataContainer.GetData(item.PurchaseIndex);
			if (data != null)
			{
				newPayShopProductInfo = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.GetProduct(data.ProductIndex);
				if (newPayShopProductInfo != null)
				{
					PayShopProductComponent payShopProductComponent = UIHelper.AddGridItem(ScrollGrid, PayShopProductComponent.PrefabAuto);
					payShopProductComponent.Open(newPayShopProductInfo, this);
					_componentList.Add(payShopProductComponent);
				}
			}
		}
		UIHelper.RepositionGrid(ScrollGrid);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (Time.time - _lastUpdateTime >= 1f)
		{
			_lastUpdateTime = Time.time;
			if (!UpdateProduct())
			{
				Close();
			}
		}
	}

	private bool UpdateProduct()
	{
		if (__Extension_Method_ICollection__.COUNT(_componentList) <= 0)
		{
			return false;
		}
		bool flag = false;
		bool result = false;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_componentList); i++)
		{
			PayShopProductComponent payShopProductComponent = _componentList[i];
			if (payShopProductComponent == null || !UIHelper.IsEnabledGameObjet(payShopProductComponent) || payShopProductComponent.ProductInfo == null)
			{
				continue;
			}
			PurchaseMarketingInfo purchaseMarketingInfo = GlobalAutoBehaviour<AccountManager>.instance.PurchaseMarketingManager.GetPurchaseMarketingInfo(payShopProductComponent.ProductInfo.Index);
			if (purchaseMarketingInfo != null && Vespa_String_Extension_Method2.TryParseNetworkTime2(purchaseMarketingInfo.EndTime, out var o_dateTime))
			{
				DateTime uTCNow = NGame2.NUI.NWindow.Common.GetUTCNow();
				if ((int)(o_dateTime - uTCNow).TotalSeconds > 0)
				{
					result = true;
					continue;
				}
				flag = true;
				UIHelper.EnableGameObject(payShopProductComponent, enable: false);
			}
		}
		if (flag)
		{
			UIHelper.RepositionGrid(ScrollGrid);
		}
		return result;
	}

	public override void Close()
	{
		base.Close();
	}

	void PayShopBaseComponent.IListener.OnClickProduct(int index)
	{
		if (index != 0 && GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(index) != null)
		{
			Close();
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Event.ToString(), delegate
			{
				SceneAutoBehaviour<PayShopManagement>.instance.OpenPayShopProductInfoPopup(index);
			});
		}
	}

	void PayShopProductComponent.IListener.OnClickProduct(int originalIndex, int comboIndex)
	{
	}

	void PayShopProductComponent.IListener.OnChangeComboProduct(int productIndex, int step)
	{
	}
}
