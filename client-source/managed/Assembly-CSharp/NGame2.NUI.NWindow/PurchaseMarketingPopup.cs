using System;
using System.Collections.Generic;
using NGame2.IAP;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NCtr;
using NGame2.NScene;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NShared.NLocalization;
using NShared.SendClickThroughRate;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class PurchaseMarketingPopup : BaseWindow
{
	[Serializable]
	public class ProductUI
	{
		public GameObject PriceRoot;

		public UILabel FreeLabel;

		public UILabel CostLabel;

		public UILabel CurrencyLabel;
	}

	public const string PrefabPathRoot = "Resources/Prefab2/Window/MessagePopup/";

	private static WindowPrefab<PurchaseMarketingPopup> _windowPrefab;

	private static Dictionary<string, WindowPrefab<PurchaseMarketingPopup>> _windowPrefabDic = new Dictionary<string, WindowPrefab<PurchaseMarketingPopup>>();

	public UILabel TitleLabel;

	public UILabel DescLabel;

	public UILabel LimitTimeLabel;

	public UIBundleLoader PopupTexture;

	public UIButton BuyBtn;

	public UIButton GoToShopBtn;

	public List<ProductUI> ProductUIs = new List<ProductUI>();

	private PurchaseMarketingPopupData _purchaseMarketingPopupData;

	private static bool _updateTime = false;

	private float _lastRefreshInfoTime;

	public static PurchaseMarketingPopup instance => _windowPrefab.instance;

	public static bool IsValidInstance
	{
		get
		{
			if (_windowPrefab != null)
			{
				return _windowPrefab.IsOpen();
			}
			return false;
		}
	}

	public static void Open(PurchaseMarketingPopupData purchaseMarketingPopupData)
	{
		if (purchaseMarketingPopupData != null)
		{
			int type = (int)purchaseMarketingPopupData.Type;
			if (type == 3 || type == 4 || type == 5 || type == 6 || type == 9 || type == 10 || type == 11 || type == 12)
			{
				return;
			}
		}
		WindowPrefab<PurchaseMarketingPopup> windowPrefab = null;
		if (!_windowPrefabDic.ContainsKey(purchaseMarketingPopupData.PopupPrefabName))
		{
			windowPrefab = new WindowPrefab<PurchaseMarketingPopup>("Resources/Prefab2/Window/MessagePopup/" + purchaseMarketingPopupData.PopupPrefabName, GameUIRoot.ELayerType.UI);
			_windowPrefabDic.Add(purchaseMarketingPopupData.PopupPrefabName, windowPrefab);
		}
		else
		{
			windowPrefab = _windowPrefabDic[purchaseMarketingPopupData.PopupPrefabName];
		}
		_windowPrefab = windowPrefab;
		_windowPrefab.instance.OpenInternal(purchaseMarketingPopupData);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(_windowPrefab);
		PurchaseMarketingData data = GameRoot.Instance.PurchaseMarketingDataContainer.GetData(purchaseMarketingPopupData.PurchaseIndices[0]);
		if (data != null && data.DurationTime == 0)
		{
			_updateTime = false;
		}
		else
		{
			_updateTime = true;
		}
	}

	private void OpenInternal(PurchaseMarketingPopupData purchaseMarketingPopupData)
	{
		_purchaseMarketingPopupData = purchaseMarketingPopupData;
		Open(delegate
		{
			Init(purchaseMarketingPopupData);
		}, null);
	}

	private void Init(PurchaseMarketingPopupData purchaseMarketingPopupData)
	{
		if (!String_Extension_Method.IsNullOrEmpty(purchaseMarketingPopupData.PopupImage))
		{
			PopupTexture.SetAssetPath(purchaseMarketingPopupData.PopupImage);
		}
		else
		{
			VespaDebug.LogError("PopupImage is null");
		}
		UIHelper.SetLabelText(TitleLabel, LocalizationManager.GetStringDirect(purchaseMarketingPopupData.TitleStringKey));
		UIHelper.SetLabelText(DescLabel, LocalizationManager.GetStringDirect(purchaseMarketingPopupData.DescStringKey));
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(purchaseMarketingPopupData.PurchaseIndices))
		{
			for (int i = 0; i < purchaseMarketingPopupData.PurchaseIndices.Length && ProductUIs.Count > i; i++)
			{
				SetProductInfo(ProductUIs[i], purchaseMarketingPopupData.PurchaseIndices[i]);
			}
		}
	}

	public void SetProductInfo(ProductUI ui, int index)
	{
		int key = _purchaseMarketingPopupData.PurchaseIndices[0];
		PurchaseMarketingData data = GameRoot.Instance.PurchaseMarketingDataContainer.GetData(key);
		if (data == null)
		{
			return;
		}
		NewPayShopProductInfo newPayShopProductInfo = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(data.ProductIndex);
		if (newPayShopProductInfo != null)
		{
			string inAppPurchaseId = newPayShopProductInfo.InAppPurchaseId;
			bool flag = String_Extension_Method.IsNullOrEmpty(inAppPurchaseId);
			UIHelper.EnableGameObject(ui.FreeLabel, flag);
			UIHelper.EnableGameObject(ui.CostLabel, !flag);
			UIHelper.EnableGameObject(ui.CurrencyLabel, !flag);
			if (!flag)
			{
				SkuInfo skuInfo = SingletonAuto<IAPWrapper>.instance.GetSkuInfo(inAppPurchaseId);
				UIHelper.SetLabelForBillingInfo(ui.CostLabel, ui.CurrencyLabel, skuInfo);
			}
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UIHelper.EnableGameObject(LimitTimeLabel, _updateTime);
		if (_updateTime && !(Time.time - _lastRefreshInfoTime < 1f))
		{
			_lastRefreshInfoTime = Time.time;
			UpdateLimitTime();
		}
	}

	private void UpdateLimitTime()
	{
		int remainTime;
		int num = (remainTime = GetRemainTime());
		string remainTimeLabelText = UIHelper.GetRemainTimeLabelText(remainTime);
		if (num > 0)
		{
			UIHelper.SetLabelText(LimitTimeLabel, remainTimeLabelText);
		}
	}

	private int GetRemainTime()
	{
		if (_purchaseMarketingPopupData == null)
		{
			return 0;
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_purchaseMarketingPopupData.PurchaseIndices))
		{
			return 0;
		}
		PurchaseMarketingInfo purchaseMarketingInfo = GlobalAutoBehaviour<AccountManager>.instance.PurchaseMarketingManager.Get(_purchaseMarketingPopupData.PurchaseIndices[0]);
		if (purchaseMarketingInfo == null)
		{
			return 0;
		}
		if (String_Extension_Method.IsNullOrEmpty(purchaseMarketingInfo.EndTime))
		{
			return int.MaxValue;
		}
		int result = 0;
		if (PurchaseMarketingManager.IsPurchaseMarketing(purchaseMarketingInfo) && Vespa_String_Extension_Method2.TryParseNetworkTime2(purchaseMarketingInfo.EndTime, out var o_dateTime))
		{
			result = (int)(o_dateTime - NGame2.NUI.NWindow.Common.GetUTCNow()).TotalSeconds;
			result = System.Math.Max(result, 0);
		}
		return result;
	}

	public void ExecReturnToLobbyAndOpenShop(LobbyScene2.ReturnStateOpenFlag openFlag)
	{
		ExecReturnToLobbyAndDoAction(openFlag);
	}

	private void ExecReturnToLobbyAndDoAction(LobbyScene2.ReturnStateOpenFlag flag)
	{
		LobbyScene2.ReturnState.OpenFlag |= flag;
		SingletonAuto<BattleContext>.instance.MoveSceneToLobby(null, EBoolCheck.False);
	}

	public override void OnSystemBackButton()
	{
		ClosePopup();
	}

	private void ClosePopup()
	{
		SingletonAuto<CtrManager>.instance.Send(GetPurchaseMarketingAdId(), EClickTag.FailType2);
		Close();
	}

	private string GetPurchaseMarketingAdId()
	{
		int key = _purchaseMarketingPopupData.PurchaseIndices[0];
		PurchaseMarketingData data = GameRoot.Instance.PurchaseMarketingDataContainer.GetData(key);
		return $"{CtrManager.ECtrMember.PurchaseMarketing.ToString()}_{data.ProductIndex}";
	}

	public void OnClickClose()
	{
		MessagePopup3Button.Open(LocalizationManager.GetString(PurchaseMarketing.PURCHASE_MARKETING_INFO), LocalizationManager.GetString(PurchaseMarketing.CLOSE_PURCHASE_MARKETING_POPUP_MESSAGE), button1Label: LocalizationManager.GetStringDirect("CLOSE"), button2Label: LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_CANCEL), onButton1: ClosePopup, onButton2: null);
	}

	public void OnClickBuy()
	{
		if (GetRemainTime() <= 0 || __Extension_Method_ICollection__.IsNullOrEmpty(_purchaseMarketingPopupData.PurchaseIndices))
		{
			return;
		}
		Close();
		int key = _purchaseMarketingPopupData.PurchaseIndices[0];
		PurchaseMarketingData data = GameRoot.Instance.PurchaseMarketingDataContainer.GetData(key);
		if (data != null)
		{
			NewPayShopProductInfo newPayShopProductInfo = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(data.ProductIndex);
			if (newPayShopProductInfo != null)
			{
				string inAppPurchaseId = newPayShopProductInfo.InAppPurchaseId;
				SingletonAuto<IAPWrapper>.instance.GetSkuInfo(inAppPurchaseId);
				SceneAutoBehaviour<PayShopManagement>.instance.PurchaseProductProcess(newPayShopProductInfo.Index);
				SingletonAuto<CtrManager>.instance.Send(GetPurchaseMarketingAdId(), EClickTag.Success);
			}
		}
	}

	public void OnClickGoToShop()
	{
		Close();
		NewPayShopCategoryType newPayShopCategoryType = NewPayShopCategoryType.Beginner;
		LobbyScene2.ReturnStateOpenFlag openFlag = LobbyScene2.ReturnStateOpenFlag.OpenNewbieShop;
		if (_purchaseMarketingPopupData.Type == MarketingType.FirstClearChapter_2020 || _purchaseMarketingPopupData.Type == MarketingType.Halloween_2021 || _purchaseMarketingPopupData.Type == MarketingType.EventWorldTree2021)
		{
			newPayShopCategoryType = NewPayShopCategoryType.Event;
			openFlag = LobbyScene2.ReturnStateOpenFlag.OpenEventShop;
		}
		if (SceneAutoBehaviour<LobbyManagement>.isValidInstance)
		{
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(newPayShopCategoryType.ToString());
		}
		else
		{
			ExecReturnToLobbyAndOpenShop(openFlag);
		}
		SingletonAuto<CtrManager>.instance.Send(GetPurchaseMarketingAdId(), EClickTag.FailType1);
	}
}
