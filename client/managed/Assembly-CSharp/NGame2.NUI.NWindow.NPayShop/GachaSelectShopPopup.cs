using System;
using System.Collections.Generic;
using System.Text;
using NGame2.IAP;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NPayShop;
using NGame2.NUI.NHelper.NPayShop;
using NGame2.NUI.NManager;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NPayShop;

public class GachaSelectShopPopup : PayShopBaseInfoPopup
{
	public new interface IListener : PayShopBaseInfoPopup.IListener
	{
		void OnClickToggle(int toggleIndex);

		void OnClickRefresh(int toggleIndex);

		void OnClickGachaSelectShopProbabilty(SelectShopInfo selectShopInfo);

		void OnBuyGachaSelectShopProduct(NewPayShopProductInfo productInfo);
	}

	[Serializable]
	public class ToggleInfo
	{
		public UIToggle Root;

		public UIToggle Toggle;

		public UILabel LabelName;
	}

	public enum EPayshopProductInfoType
	{
		Cash,
		Currency
	}

	public const string PrefabPath = "Resources/Prefab2/Window/PayShop/GachaSelectShopPopup";

	public ToggleInfo[] ToggleInfos;

	public UISimpleGrid GachaItemGrid;

	public UILabel LabelProductDesc;

	public UIButton CashBuyButton;

	public PayShopPriceComponent CashPriceComponent;

	public UIButton CurrencyBuyButton;

	public PayShopPriceComponent CurrencyPriceComponent;

	public UILabel LabelShopRemainSec;

	public UILabel LabelProductRemainSec;

	public UILabel LabelRefreshCount;

	public UISimpleGrid CurrencyBonusGrid;

	public UISimpleGrid CashBonusGrid;

	public UIGame3DWidget SoundWidget;

	public UILabel LabelInfo;

	private int _currentToggleIndex;

	private List<GachaSelectShopComponent> _listComponent = new List<GachaSelectShopComponent>();

	private List<PayShopSpecialCurrencyComponent> _lisSpecialComponent = new List<PayShopSpecialCurrencyComponent>();

	private SelectShopNoticeInfo _selectShopNoticeInfo;

	private SelectShopInfo _selectShopInfo;

	private NewPayShopProductInfo _currencyPayShopProductInfo;

	private IListener _iListener;

	private int _maxRestockSec;

	private StringBuilder _sb;

	public void Open(SelectShopNoticeInfo noticeInfo, SelectShopInfo info, IListener listener)
	{
		Open(delegate
		{
			Init(noticeInfo, info, listener);
		}, delegate
		{
		});
	}

	public override void OnClosing()
	{
		base.OnClosing();
		_currentToggleIndex = -1;
		_selectShopInfo = null;
		_selectShopNoticeInfo = null;
		_currencyPayShopProductInfo = null;
		_listener = null;
		_iListener = null;
		_maxRestockSec = 0;
		if (_sb != null)
		{
			_sb.Clear();
		}
		_sb = null;
		UIHelper.EnableGameObject(LabelInfo, enable: false);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateRemainTime();
	}

	protected override void ReleaseComponent()
	{
		base.ReleaseComponent();
		ReleaseItemIconComponent();
		ReleaseSpecialComponent();
	}

	protected void Init(SelectShopNoticeInfo noticeInfo, SelectShopInfo info, IListener listener)
	{
		_listener = listener;
		_iListener = listener;
		_selectShopNoticeInfo = noticeInfo;
		SetMaxRestockSec();
		Setup(info);
		SetToggle(0);
	}

	public void Setup(SelectShopInfo info)
	{
		_selectShopInfo = info;
		_originInfo = GetPayShopProductInfo(EPayshopProductInfoType.Cash, info);
		_currencyPayShopProductInfo = GetPayShopProductInfo(EPayshopProductInfoType.Currency, info);
		_currentInfo = _originInfo;
		SetInfoLabel(info);
		InitToggle();
		SetProdcutTitle(_currentInfo);
		SetLawRoot(_currentInfo);
		SetLocalizedLawButton(_currentInfo);
		SetPriceComponent(_currentInfo, PriceComponent);
		SetPriceComponent(_currencyPayShopProductInfo, CurrencyPriceComponent);
		SetBuyButton(_currentInfo, _currentInfo, PriceComponent, CashBuyButton);
		SetBuyButton(_currencyPayShopProductInfo, _currencyPayShopProductInfo, CurrencyPriceComponent, CurrencyBuyButton);
		SetBonusPoint();
		SetProbabilityButton(_selectShopInfo);
		SetRemainTime();
		SetSelectShopItemComponent();
		SetFreeRestockLabel();
		SetProductDescLabel();
	}

	private void SetMaxRestockSec()
	{
		_maxRestockSec = GameRoot.Instance.ConstantDataContainer.GetValueInt("SelectShopItemExpirationTimeSec ", 3600);
	}

	protected void SetPriceComponent(NewPayShopProductInfo productInfo, PayShopPriceComponent priceComponent)
	{
		bool flag = productInfo != null;
		UIHelper.EnableGameObject(priceComponent, flag);
		if (flag)
		{
			PlayerProductPurchaseInfo purchaseInfo = GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Get(productInfo.Index);
			SkuInfo skuInfo = SingletonAuto<IAPWrapper>.instance.GetSkuInfo(productInfo.InAppPurchaseId);
			PayShopUIHelper.SetPriceComponent(productInfo, purchaseInfo, skuInfo, priceComponent);
		}
	}

	private void SetBonusPoint()
	{
		ReleaseSpecialComponent();
		SetCurrencyProductBonusPoint();
		SetCashProductBonusPoint();
	}

	private void ReleaseSpecialComponent()
	{
		FactoryBase.ReleaseAndClearList(_lisSpecialComponent);
	}

	private void SetCurrencyProductBonusPoint()
	{
		SetBonusPoint(_currencyPayShopProductInfo, CurrencyBonusGrid);
	}

	private void SetCashProductBonusPoint()
	{
		SetBonusPoint(_currentInfo, CashBonusGrid);
	}

	private void SetBonusPoint(NewPayShopProductInfo info, IGrid grid)
	{
		if (info != null)
		{
			SetBonusPoint(CurrencyType.ShopEventPoint, info.ShopEventPoint, grid);
			SetBonusPoint(CurrencyType.Gold, info.Gold, grid);
			SetBonusPoint(CurrencyType.Mileage, info.Mileage, grid);
			SetBonusPoint(CurrencyType.LuaPoint, info.LuaPoint, grid);
			SetBonusPoint(CurrencyType.LimitedShopEventPoint, info.LimitedShopEventPoint, grid);
			if (grid != null)
			{
				UIHelper.RepositionGrid(grid);
			}
		}
	}

	private void SetBonusPoint(CurrencyType currencyType, int value, IGrid grid)
	{
		if (grid != null && value > 0 && GachaSelctShopHelper.GetObtainType(currencyType, isFree: false) >= ObtainItemType.Bonus)
		{
			PayShopSpecialCurrencyComponent payShopSpecialCurrencyComponent = UIHelper.AddGridItem(grid, PayShopSpecialCurrencyComponent.PrefabAuto);
			if (!(payShopSpecialCurrencyComponent == null))
			{
				UIHelper.EnableGameObject(payShopSpecialCurrencyComponent, enable: true);
				payShopSpecialCurrencyComponent.Open(currencyType, value);
				_lisSpecialComponent.Add(payShopSpecialCurrencyComponent);
			}
		}
	}

	private void SetProbabilityButton(SelectShopInfo selectShopInfo)
	{
		bool enable = selectShopInfo != null;
		UIHelper.EnableGameObject(Button_Probability, enable);
	}

	private void SetProductDescLabel()
	{
		string remainTimeTextAllDelZero = UIHelper.GetRemainTimeTextAllDelZero(_maxRestockSec);
		string stringDirect = LocalizationManager.GetStringDirect("GACHASELECTSHOP_INFO_DESC", remainTimeTextAllDelZero);
		UIHelper.SetLabelText(LabelProductDesc, stringDirect);
	}

	private void InitToggle()
	{
		int num = ((_selectShopNoticeInfo != null) ? __Extension_Method_ICollection__.COUNT(_selectShopNoticeInfo.CategoryGroups) : 0);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ToggleInfos); i++)
		{
			ToggleInfo toggleInfo = ToggleInfos[i];
			if (toggleInfo != null && !(toggleInfo.Root == null))
			{
				bool flag = i < num;
				UIHelper.EnableGameObject(toggleInfo.Root, flag);
				if (flag)
				{
					string stringDirect = LocalizationManager.GetStringDirect(GetToggleNameKey(i));
					UIHelper.SetLabelText(toggleInfo.LabelName, stringDirect);
				}
			}
		}
	}

	private void SetToggle(int index)
	{
		if (_currentToggleIndex == index)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ToggleInfos); i++)
		{
			ToggleInfo toggleInfo = ToggleInfos[i];
			if (toggleInfo != null && !(toggleInfo.Toggle == null) && i == index)
			{
				if (!toggleInfo.Toggle.value)
				{
					toggleInfo.Toggle.value = true;
				}
				_currentToggleIndex = i;
			}
		}
	}

	private bool CanClickToggle(bool isSameToggle, int index)
	{
		if (!isSameToggle)
		{
			return false;
		}
		if (IsSelectShopProductEnd())
		{
			return true;
		}
		return _currentToggleIndex != index;
	}

	private void SetRemainTime()
	{
		SetSelectShopRemainTime();
		SetProductRemainTime();
	}

	private int SetSelectShopRemainTime()
	{
		int remainTimeSec = GachaSelctShopHelper.GetRemainTimeSec(_selectShopNoticeInfo);
		GachaSelctShopHelper.SetRemainTimeLabel(LabelShopRemainSec, remainTimeSec);
		return remainTimeSec;
	}

	private void SetProductRemainTime()
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(Mathf.Min(GachaSelctShopHelper.GetRemainTimeSec(_selectShopInfo), _maxRestockSec));
		string text = $"{timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
		UIHelper.SetLabelText(LabelProductRemainSec, text);
	}

	private void UpdateRemainTime()
	{
		SetSelectShopRemainTime();
		SetProductRemainTime();
	}

	private NewPayShopProductInfo GetPayShopProductInfo(EPayshopProductInfoType eType, SelectShopInfo selectShopInfo)
	{
		if (selectShopInfo == null || __Extension_Method_ICollection__.IsNullOrEmpty(selectShopInfo.PayShopProductInfos))
		{
			return null;
		}
		GachaSelectShopData data = GameRoot.Instance.GachaSelectShopDataContainer.GetData(selectShopInfo.Index);
		if (data == null)
		{
			return null;
		}
		int num = ((eType == EPayshopProductInfoType.Cash) ? data.ProductIndexCash : data.ProductIndexGem);
		NewPayShopProductInfo[] payShopProductInfos = selectShopInfo.PayShopProductInfos;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(payShopProductInfos); i++)
		{
			NewPayShopProductInfo newPayShopProductInfo = payShopProductInfos[i];
			if (newPayShopProductInfo != null && newPayShopProductInfo.Index == num)
			{
				return newPayShopProductInfo;
			}
		}
		return null;
	}

	private void SetSelectShopItemComponent()
	{
		ReleaseItemIconComponent();
		string itemGroupCode = GetItemGroupCode();
		PayShopProductItemInfo[] itemInfos = _selectShopInfo.ItemInfos;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(itemInfos); i++)
		{
			GachaSelectShopComponent gachaSelectShopComponent = UIHelper.AddGridItem(GachaItemGrid, GachaSelectShopComponent.PrefabAuto);
			if (gachaSelectShopComponent == null)
			{
				break;
			}
			gachaSelectShopComponent.Open(itemInfos[i], itemGroupCode);
			_listComponent.Add(gachaSelectShopComponent);
		}
		PayShopProductEquipItemInfo[] equipItemInfos = _selectShopInfo.EquipItemInfos;
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(equipItemInfos); j++)
		{
			GachaSelectShopComponent gachaSelectShopComponent2 = UIHelper.AddGridItem(GachaItemGrid, GachaSelectShopComponent.PrefabAuto);
			if (gachaSelectShopComponent2 == null)
			{
				break;
			}
			gachaSelectShopComponent2.Open(equipItemInfos[j], itemGroupCode);
			_listComponent.Add(gachaSelectShopComponent2);
		}
		PlayItemGridSound();
		UIHelper.RepositionGrid(GachaItemGrid);
	}

	private void PlayItemGridSound()
	{
		if (SoundWidget != null)
		{
			if (SoundWidget.IsPlaying)
			{
				SoundWidget.Stop(immediatelyClear: true);
			}
			SoundWidget.PlayNow();
		}
	}

	public string GetItemGroupCode()
	{
		GachaSelectShopData gachaSelectShopData = GetGachaSelectShopData();
		if (gachaSelectShopData == null)
		{
			return "";
		}
		return gachaSelectShopData.ItemGroupCode;
	}

	private GachaSelectShopData GetGachaSelectShopData()
	{
		if (_selectShopInfo == null)
		{
			return null;
		}
		return GetGachaSelectShopData(_selectShopInfo.Index);
	}

	private GachaSelectShopData GetGachaSelectShopData(int index)
	{
		return GameRoot.Instance.GachaSelectShopDataContainer.GetData(index);
	}

	private void SetFreeRestockLabel()
	{
		int maxFreeRestockCount = GetMaxFreeRestockCount();
		bool flag = _selectShopInfo != null && maxFreeRestockCount > 0;
		UIHelper.EnableGameObject(LabelRefreshCount, flag);
		if (flag)
		{
			int remainFreeRestockCount = GetRemainFreeRestockCount();
			UIHelper.SetLabelText(LabelRefreshCount, $"{remainFreeRestockCount}/{maxFreeRestockCount}");
		}
	}

	private int GetMaxFreeRestockCount()
	{
		return GameRoot.Instance.ConstantDataContainer.GetValueInt("SelectShopRefreshableCount", 3);
	}

	private int GetRemainFreeRestockCount()
	{
		int maxFreeRestockCount = GetMaxFreeRestockCount();
		if (maxFreeRestockCount == 0)
		{
			return 0;
		}
		if (_selectShopInfo == null)
		{
			return 0;
		}
		return System.Math.Max(maxFreeRestockCount - _selectShopInfo.ResetCount, 0);
	}

	public void OnChangeToggleValue(UIToggle toggle)
	{
		if (toggle == null || !toggle.value)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ToggleInfos); i++)
		{
			ToggleInfo toggleInfo = ToggleInfos[i];
			if (toggleInfo != null && !(toggleInfo.Toggle == null) && CanClickToggle(toggleInfo.Toggle == toggle, i))
			{
				_currentToggleIndex = i;
				if (_iListener != null)
				{
					_iListener.OnClickToggle(i);
				}
				break;
			}
		}
	}

	private void ReleaseItemIconComponent()
	{
		FactoryBase.ReleaseAndClearList(_listComponent);
	}

	private bool IsSelectShopProductEnd()
	{
		if (_selectShopInfo == null)
		{
			return true;
		}
		return GachaSelctShopHelper.GetRemainTimeSec(_selectShopInfo) <= 0;
	}

	private int GetReqGemCount()
	{
		return GameRoot.Instance.ConstantDataContainer.GetValueInt("SelectShopPayRefreshGem", 50);
	}

	private string GetToggleNameKey(int index)
	{
		if (_selectShopNoticeInfo == null || __Extension_Method_ICollection__.COUNT(_selectShopNoticeInfo.CategoryNameKeys) <= index)
		{
			return "";
		}
		return _selectShopNoticeInfo.CategoryNameKeys[index];
	}

	public void PurchaseRefresh()
	{
		if (_iListener != null)
		{
			_iListener.OnClickToggle(_currentToggleIndex);
		}
	}

	public void OnClickRefresh()
	{
		if (_selectShopInfo == null)
		{
			return;
		}
		int remainFreeRestockCount = GetRemainFreeRestockCount();
		if (remainFreeRestockCount <= 0)
		{
			int reqGemCount = GetReqGemCount();
			long gem = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Gem;
			long num = gem;
			int num2 = reqGemCount;
			if (num2 <= num)
			{
				string text = LocalizationManager.GetString(NShared.NLocalization.Common.COST_TYPE_RUBY);
				string message = LocalizationManager.GetString(NShared.NLocalization.Shop.SHOP_RESTOCK_CONFIRM, text);
				PayShopPriceCustomMessagePopup.Open(new PayShopPriceCustomMessagePopup.DataClass
				{
					Message = message,
					Currency = CurrencyType.Gem.ToString(),
					Price = reqGemCount
				}, delegate(bool isYes)
				{
					if (isYes && _iListener != null)
					{
						_iListener.OnClickRefresh(_currentToggleIndex);
					}
				});
			}
			else
			{
				string text2 = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
				string message2 = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM);
				PayShopMessage3ButtonPopup.Open(text2, message2, delegate
				{
					Close();
					SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
				});
			}
		}
		else
		{
			string text3 = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string stringDirect = LocalizationManager.GetStringDirect(GetToggleNameKey(_currentToggleIndex));
			string stringDirect2 = LocalizationManager.GetStringDirect("GACHASELECTSHOP_RESTOCK_CONFIRM", stringDirect, remainFreeRestockCount);
			PayShopMessage3ButtonPopup.Open(text3, stringDirect2, delegate
			{
				_iListener.OnClickRefresh(_currentToggleIndex);
			});
		}
	}

	public void OnClickBuyCurrency()
	{
		if (_iListener != null)
		{
			_iListener.OnBuyGachaSelectShopProduct(_currencyPayShopProductInfo);
		}
	}

	public override void OnClickBuy()
	{
		if (_iListener != null)
		{
			_iListener.OnBuyGachaSelectShopProduct(_currentInfo);
		}
	}

	public override void OnClickProbability()
	{
		if (_iListener != null)
		{
			_iListener.OnClickGachaSelectShopProbabilty(_selectShopInfo);
		}
	}

	private void SetInfoLabel(SelectShopInfo info)
	{
		bool flag = false;
		UIHelper.EnableGameObject(LabelInfo, flag);
		if (info == null || !flag)
		{
			return;
		}
		if (_sb == null)
		{
			_sb = new StringBuilder(1000);
		}
		if (_selectShopInfo != null)
		{
			_sb.Append("----SelcetShopInfo-----").AppendLine();
			_sb.Append("Index : ").Append(_selectShopInfo.Index.ToString()).AppendLine();
			_sb.Append("CategoryGroup : ").Append(_selectShopInfo.CategoryGroup).AppendLine();
			_sb.Append("RestockEndTime :").Append(_selectShopInfo.RestockEndTime).AppendLine();
			_sb.Append("ResetCount : ").Append(_selectShopInfo.ResetCount.ToString()).AppendLine();
			_sb.AppendLine().Append("----Cash NewPayshopProductInfo-----").AppendLine();
			if (_currentInfo != null)
			{
				_sb.Append("Index : ").Append(_currentInfo.Index).AppendLine();
				_sb.Append("InAppPurchaseId : ").Append(_currentInfo.InAppPurchaseId).AppendLine();
				AppendBonus(_sb, CurrencyType.ShopEventPoint, _currentInfo.ShopEventPoint);
				AppendBonus(_sb, CurrencyType.Gold, _currentInfo.Gold);
				AppendBonus(_sb, CurrencyType.Mileage, _currentInfo.Mileage);
				AppendBonus(_sb, CurrencyType.LuaPoint, _currentInfo.LuaPoint);
				AppendBonus(_sb, CurrencyType.LimitedShopEventPoint, _currentInfo.LimitedShopEventPoint);
			}
			_sb.AppendLine().Append("----Currency NewPayshopProductInfo-----").AppendLine();
			if (_currencyPayShopProductInfo != null)
			{
				_sb.Append("Index : ").Append(_currencyPayShopProductInfo.Index.ToString()).AppendLine();
				_sb.Append("Currency ").Append(_currencyPayShopProductInfo.PriceType.ToString()).Append(" + ")
					.Append(_currencyPayShopProductInfo.Price.ToString())
					.AppendLine();
				AppendBonus(_sb, CurrencyType.ShopEventPoint, _currencyPayShopProductInfo.ShopEventPoint);
				AppendBonus(_sb, CurrencyType.Gold, _currencyPayShopProductInfo.Gold);
				AppendBonus(_sb, CurrencyType.Mileage, _currencyPayShopProductInfo.Mileage);
				AppendBonus(_sb, CurrencyType.LuaPoint, _currencyPayShopProductInfo.LuaPoint);
				AppendBonus(_sb, CurrencyType.LimitedShopEventPoint, _currencyPayShopProductInfo.LimitedShopEventPoint);
			}
		}
		UIHelper.SetLabelText(LabelInfo, _sb.ToString());
		_sb.Length = 0;
	}

	private void AppendBonus(StringBuilder sb, CurrencyType currencyType, int value)
	{
		if (value > 0)
		{
			_sb.Append("[Bonus] ").Append(currencyType.ToString()).Append(" + ")
				.Append(value.ToString())
				.AppendLine();
		}
	}
}
