using System;
using System.Collections.Generic;
using NGame2.IAP;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NPayShop;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow.NPayShop;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class KingsPassWindow : BaseWindow, KingsPassItemComponent.Listener
{
	public interface Listener
	{
		void ReceivedRewarad(KingsPassMission kingsPassMission, KingsPassItemComponent comp, Action OnSucess);

		void AllReceiveReward(KingsPassSeaonInfo kingsPassSeaonInfo, Action OnSucess);

		void BuyAdditionalReward(KingsPassMission kingsPassMission, KingsPassSeasonMissionData kingsPassSeasonMissionData, Action OnSucess);

		void BuyLevel();

		void BuyKingsPass(int productIndex);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/KingsPass/KingsPassWindow";

	public UIBundleLoader BackgroundLoader;

	public UILabel TitleLabel;

	public UILabelAligner LabelAligner;

	public UILabel SeasonPeriodTimeLabel;

	public UILabel LevelText;

	public UILabel ExpPointText;

	public UISprite Sprite_Progress;

	public UILabel PurchaseLockedLabel;

	public Transform PurchaseLockedRoot;

	public Transform DefaultLawRoot;

	public UIButton DetailWithdrawButton;

	public UILabel WithdrawLabel;

	public Transform JapaneLawRoot;

	public UITable ExpRatioTable;

	public UIBaseGrid ItemListGrid;

	public UIBaseGrid FinalItemGrid;

	public UIButton BuyExpButton;

	public UIButton AllReceiveButton;

	public UIButton BuyKingsPassButton;

	public PayShopPriceComponent PriceComponent;

	public UILabel BuyKingsPassLabel;

	private KingsPassSeaonInfo KingsPassSeasonInfo;

	private KingsPassSeasonData _kingsPassSeasonData;

	private Listener _listener;

	private UIProgressBar _expProgressBar;

	private List<KingsPassExpComponent> _expCompList = new List<KingsPassExpComponent>();

	private List<KingsPassFrameComponent> _itemCompList = new List<KingsPassFrameComponent>();

	public void Open(KingsPassSeaonInfo kingsPassSeaonInfo, Listener listener)
	{
		Open(delegate
		{
			KingsPassSeasonInfo = kingsPassSeaonInfo;
			if (KingsPassSeasonInfo != null)
			{
				_kingsPassSeasonData = GameRoot.Instance.KingsPassSeasonDataContainer.GetData(KingsPassSeasonInfo.SeasonIndex);
				if (_kingsPassSeasonData != null)
				{
					_listener = listener;
					Init();
				}
			}
		}, null);
	}

	private void Init()
	{
		SetKingsPassSeasonUi();
		SetCountryLaw();
		SetKingsPassButton();
		SetLevelExpPoint();
		SetButton();
		SetExpRatioComp();
		SetItemListComp();
	}

	private void SetKingsPassSeasonUi()
	{
		string stringDirect = LocalizationManager.GetStringDirect(_kingsPassSeasonData.TitleKey);
		UIHelper.SetLabelText(TitleLabel, stringDirect);
		string beginTime = KingsPassSeasonInfo.BeginTime;
		string endTime = KingsPassSeasonInfo.EndTime;
		NewPayShopProductInfo newPayShopProductInfo = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(_kingsPassSeasonData.ProductIndex);
		DateTime o_dateTime;
		DateTime o_dateTime2;
		if (newPayShopProductInfo == null)
		{
			if (Vespa_String_Extension_Method2.TryParseNetworkTime2(beginTime, out o_dateTime) && Vespa_String_Extension_Method2.TryParseNetworkTime2(endTime, out o_dateTime2))
			{
				DateTime dateTime = GlobalAutoBehaviour<AccountManager>.instance.ServerUTCToLocal(o_dateTime);
				DateTime dateTime2 = GlobalAutoBehaviour<AccountManager>.instance.ServerUTCToLocal(o_dateTime2);
				string stringDirect2 = LocalizationManager.GetStringDirect("KINGSPASS_DATE_ONLY_SEASON_TEXT", dateTime, dateTime2);
				UIHelper.SetLabelText(SeasonPeriodTimeLabel, stringDirect2);
				if ((bool)LabelAligner)
				{
					LabelAligner.Align();
				}
			}
			return;
		}
		string endTime2 = newPayShopProductInfo.EndTime;
		if (Vespa_String_Extension_Method2.TryParseNetworkTime2(beginTime, out o_dateTime) && Vespa_String_Extension_Method2.TryParseNetworkTime2(endTime, out o_dateTime2) && Vespa_String_Extension_Method2.TryParseNetworkTime2(endTime2, out var o_dateTime3))
		{
			DateTime dateTime3 = GlobalAutoBehaviour<AccountManager>.instance.ServerUTCToLocal(o_dateTime);
			DateTime dateTime4 = GlobalAutoBehaviour<AccountManager>.instance.ServerUTCToLocal(o_dateTime2);
			DateTime dateTime5 = GlobalAutoBehaviour<AccountManager>.instance.ServerUTCToLocal(o_dateTime3);
			string stringDirect3 = LocalizationManager.GetStringDirect("KINGSPASS_DATE_TEXT", dateTime3, dateTime4, dateTime5);
			UIHelper.SetLabelText(SeasonPeriodTimeLabel, stringDirect3);
			if ((bool)LabelAligner)
			{
				LabelAligner.Align();
			}
		}
	}

	public void SetCountryLaw()
	{
		NewPayShopProductInfo newPayShopProductInfo = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(_kingsPassSeasonData.ProductIndex);
		if (newPayShopProductInfo == null)
		{
			UIHelper.EnableGameObject(WithdrawLabel, enable: false);
			UIHelper.EnableGameObject(DetailWithdrawButton, enable: false);
			return;
		}
		string countryServerName = GlobalAutoBehaviour<AccountManager>.instance.CountryServerName;
		bool flag = countryServerName.Equals("Korea");
		bool flag2 = countryServerName.Equals("Taiwan");
		if (flag || flag2)
		{
			UIHelper.EnableGameObject(DefaultLawRoot, enable: true);
			UIHelper.EnableGameObject(JapaneLawRoot, enable: false);
			UIHelper.EnableGameObject(WithdrawLabel, enable: true);
			if (newPayShopProductInfo != null)
			{
				string stringDirect = LocalizationManager.GetStringDirect(newPayShopProductInfo.WithdrawTextKey);
				UIHelper.SetLabelText(WithdrawLabel, stringDirect);
			}
			UIHelper.EnableGameObject(DetailWithdrawButton, flag);
		}
		else
		{
			CountryServerData countryServerData = LobbyManagement.GetCountryServerData();
			bool enable = countryServerData != null && !__Extension_Method_ICollection__.IsNullOrEmpty(countryServerData.Law);
			UIHelper.EnableGameObject(DefaultLawRoot, enable: false);
			UIHelper.EnableGameObject(JapaneLawRoot, enable);
		}
	}

	public void SetKingsPassButton()
	{
		NewPayShopProductInfo newPayShopProductInfo = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(_kingsPassSeasonData.ProductIndex);
		PlayerProductPurchaseInfo playerProductPurchaseInfo = GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Get(_kingsPassSeasonData.ProductIndex);
		string productId = ((newPayShopProductInfo != null) ? newPayShopProductInfo.InAppPurchaseId : string.Empty);
		SkuInfo skuInfo = SingletonAuto<IAPWrapper>.instance.GetSkuInfo(productId);
		bool flag = false;
		if (newPayShopProductInfo != null && Vespa_String_Extension_Method2.TryParseNetworkTime2(newPayShopProductInfo.EndTime, out var o_dateTime))
		{
			flag = o_dateTime > NGame2.NUI.NWindow.Common.GetUTCNow();
		}
		bool flag2 = playerProductPurchaseInfo != null;
		if (!flag2)
		{
			string stringDirect = LocalizationManager.GetStringDirect(flag ? "KINGSPASS_PURCHASE_MARKETING_TEXT" : "KINGSPASS_PURCHASE_MARKETING_NOT_TEXT");
			UIHelper.SetLabelText(PurchaseLockedLabel, stringDirect);
		}
		UIHelper.EnableGameObject(PurchaseLockedRoot, !flag2);
		UIHelper.EnableButton(BuyKingsPassButton, !flag2);
		UIHelper.EnableGameObject(BuyKingsPassButton, flag);
		UIHelper.EnableGameObject(BuyKingsPassLabel, flag && !flag2);
		if (flag)
		{
			PayShopUIHelper.SetPriceComponent(newPayShopProductInfo, playerProductPurchaseInfo, skuInfo, PriceComponent);
		}
	}

	public void SetLevelExpPoint()
	{
		if (_expProgressBar == null)
		{
			_expProgressBar = Sprite_Progress.gameObject.AddComponent<UIProgressBar>();
			_expProgressBar.foregroundWidget = Sprite_Progress;
		}
		PlayerKingsPassExp playerKingsPassExp = GlobalAutoBehaviour<AccountManager>.instance.KingsPassManager.PlayerKingsPassExp;
		int num = playerKingsPassExp?.Level ?? 0;
		string stringDirect = LocalizationManager.GetStringDirect("KINGSPASS_NOW_LEVEL", num);
		UIHelper.SetLabelText(LevelText, stringDirect);
		int level = num + 1;
		KingsPassLevelData data = GameRoot.Instance.KingsPassLevelDataContainer.GetData(_kingsPassSeasonData.LevelIndex, num);
		KingsPassLevelData data2 = GameRoot.Instance.KingsPassLevelDataContainer.GetData(_kingsPassSeasonData.LevelIndex, level);
		if (data == null && data2 == null)
		{
			return;
		}
		bool num2 = data2 == null;
		int num3 = data?.ReqExp ?? 0;
		float num4 = 1f;
		float num5 = 1f;
		if (num2)
		{
			int level2 = num - 1;
			KingsPassLevelData kingsPassLevelData = GameRoot.Instance.KingsPassLevelDataContainer.GetData(_kingsPassSeasonData.LevelIndex, level2);
			if (kingsPassLevelData == null)
			{
				kingsPassLevelData = data;
			}
			num4 = data.ReqExp - kingsPassLevelData.ReqExp;
			num5 = num4;
		}
		else
		{
			num4 = ((playerKingsPassExp != null) ? (playerKingsPassExp.Exp - num3) : 0);
			num5 = data2.ReqExp - num3;
		}
		string stringDirect2 = LocalizationManager.GetStringDirect("KINGSPASS_NOW_POINT", num4, num5);
		float value = ((num5 != 0f) ? (num4 / num5) : 1f);
		_expProgressBar.value = value;
		UIHelper.SetLabelText(ExpPointText, stringDirect2);
		bool enable = num < KingsPassSeasonInfo.CurrentLevel;
		UIHelper.EnableButton(BuyExpButton, enable);
	}

	public void SetButton()
	{
		bool enable = SceneAutoBehaviour<KingsPassManagement>.instance.CanAllReceiveReward();
		UIHelper.EnableButton(AllReceiveButton, enable);
	}

	private void SetExpRatioComp()
	{
		foreach (KingsPassExpData expInfo in GameRoot.Instance.KingsPassExpDataContainer.GetExpInfos(_kingsPassSeasonData.ExpIndex))
		{
			KingsPassExpComponent kingsPassExpComponent = KingsPassExpComponent.PrefabAuto.Factory.Create(ExpRatioTable.transform);
			StaminaType staminaType = expInfo.StaminaType;
			int multiple = expInfo.Multiple;
			kingsPassExpComponent.Open(staminaType, multiple, null);
			_expCompList.Add(kingsPassExpComponent);
		}
		if ((bool)ExpRatioTable)
		{
			ExpRatioTable.Reposition();
		}
	}

	private void SetItemListComp()
	{
		int seasonIndex = _kingsPassSeasonData.SeasonIndex;
		IList<KingsPassLevelData> levelDatas = GameRoot.Instance.KingsPassLevelDataContainer.GetLevelDatas(_kingsPassSeasonData.LevelIndex);
		int num = GlobalAutoBehaviour<AccountManager>.instance.KingsPassManager.PlayerKingsPassExp?.Level ?? 0;
		int num2 = __Extension_Method_ICollection__.COUNT(levelDatas) - 1;
		KingsPassFrameComponent kingsPassFrameComponent = null;
		KingsPassFrameComponent kingsPassFrameComponent2 = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(levelDatas); i++)
		{
			kingsPassFrameComponent = ((i != num2) ? UIHelper.AddGridItem(ItemListGrid, KingsPassFrameComponent.PrefabAuto) : UIHelper.AddGridItem(FinalItemGrid, KingsPassFrameComponent.FinalPrefabAuto));
			KingsPassLevelData kingsPassLevelData = levelDatas[i];
			kingsPassFrameComponent.Open(seasonIndex, kingsPassLevelData.Level, this);
			_itemCompList.Add(kingsPassFrameComponent);
		}
		UIHelper.RepositionGrid(ItemListGrid);
		UIHelper.RepositionGrid(FinalItemGrid);
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_itemCompList); j++)
		{
			KingsPassFrameComponent kingsPassFrameComponent3 = _itemCompList[j];
			if (kingsPassFrameComponent3.EnabledReceive())
			{
				kingsPassFrameComponent2 = kingsPassFrameComponent3;
				break;
			}
		}
		if (kingsPassFrameComponent2 == null)
		{
			if (num == num2 + 1)
			{
				kingsPassFrameComponent2 = (Vespa_Generic_IList_Extension_Method.IsValidIndex(_itemCompList, num2 - 1) ? _itemCompList[num2 - 1] : null);
			}
			else if (num > 0)
			{
				kingsPassFrameComponent2 = (Vespa_Generic_IList_Extension_Method.IsValidIndex(_itemCompList, num - 1) ? _itemCompList[num - 1] : null);
			}
		}
		ScrollTo(kingsPassFrameComponent2);
	}

	private void ScrollTo(KingsPassFrameComponent comp)
	{
		ItemListGrid.ScrollTo(comp, 0f, 0f);
	}

	public void Refresh()
	{
		foreach (KingsPassFrameComponent itemComp in _itemCompList)
		{
			itemComp.Refresh();
		}
		SetButton();
	}

	public override void Reset()
	{
		base.Reset();
		_kingsPassSeasonData = null;
		_listener = null;
		ReleaseComp();
	}

	private void ReleaseComp()
	{
		FactoryBase.ReleaseAndClearList(_expCompList);
		FactoryBase.ReleaseAndClearList(_itemCompList);
	}

	public void OnClickBuyLevel()
	{
		if (_listener != null)
		{
			_listener.BuyLevel();
		}
	}

	public void OnClickBuyKingsPass()
	{
		if (_kingsPassSeasonData != null && _listener != null)
		{
			_listener.BuyKingsPass(_kingsPassSeasonData.ProductIndex);
		}
	}

	public void OnClickAllReceive()
	{
		if (_listener != null)
		{
			_listener.AllReceiveReward(KingsPassSeasonInfo, delegate
			{
				Refresh();
			});
		}
	}

	public void OnClickAgreementDetail()
	{
		PurchasePolicyPopup.NewOpenPopup();
	}

	public void OnClickSpecialCommercialTransactionsLaw()
	{
		CountryServerData countryServerData = LobbyManagement.GetCountryServerData();
		if (countryServerData != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect("MENU_SPECIFIC_COMMERCIAL_TRANSACTIONS_LAW");
			LawRelatedPopup.NewOpen(countryServerData, stringDirect, 0);
		}
	}

	public void OnClickMoneySettlementLaw()
	{
		CountryServerData countryServerData = LobbyManagement.GetCountryServerData();
		if (countryServerData != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect("MENU_MONEY_SETTLEMENT_LAW");
			LawRelatedPopup.NewOpen(countryServerData, stringDirect, 1);
		}
	}

	void KingsPassItemComponent.Listener.OnClick(KingsPassItemComponent comp)
	{
		if (_listener == null || !comp.EnabledReceive())
		{
			return;
		}
		KingsPassSeasonMissionData kingsPassSeasonMissionData = comp.KingsPassSeasonMissionData;
		if (kingsPassSeasonMissionData != null)
		{
			bool num = !String_Extension_Method.IsNullOrEmpty(kingsPassSeasonMissionData.AdditionalRewardPriceType);
			Action onSucess = delegate
			{
				comp.Refresh();
				SetButton();
			};
			if (num)
			{
				_listener.BuyAdditionalReward(comp.KingsPassMission, comp.KingsPassSeasonMissionData, onSucess);
			}
			else
			{
				_listener.ReceivedRewarad(comp.KingsPassMission, comp, onSucess);
			}
		}
	}
}
