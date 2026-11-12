using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NContentsInfo;
using NGame2.NDropBonusEvent;
using NGame2.NHelper;
using NGame2.NUI.NComponent2.NPayShop;
using NGame2.NUI.NComponent2.NStat;
using NGame2.NUI.NHelper.NPayShop;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NPayShop;

public class PayShopProductInfoPopup : PayShopBaseInfoPopup
{
	[Serializable]
	public class ArrowData
	{
		public UIWidget Root;

		public UIBundleLoader TextureLoader;

		public void SetTexture(string path)
		{
			TextureLoader.SetAssetPath(path);
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/PayShop/PayShopProductInfoPopup";

	[Header("PayShopProductInfoPopup")]
	public UIScrollGrid ItemScrollGrid;

	public UIScrollGrid BonusScrollGrid;

	public StatListComponent.ColorInfo SpecialGemLabelColor;

	public UIWidget Widget_StepProductRoot;

	public UILabel Label_StepCount;

	public ArrowData LeftProductArrow;

	public ArrowData RightProductArrow;

	public UILabel Label_Desc;

	public UILabel Label_RemainCount;

	public UITable Table_ProductInfo;

	public UILabel Booster_Desc;

	private List<PayShopProductSimpleComponent> _itemList = new List<PayShopProductSimpleComponent>();

	private List<RewardElementData> _listRewardElementData = new List<RewardElementData>();

	private int SortIndex;

	private static int GetPayBackGem(NewPayShopProductInfo info)
	{
		if (info == null || !info.IsPayBack)
		{
			return 0;
		}
		DropBonusEvent effectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(EventBonusType.PaybackGem, string.Empty);
		int num = ((effectiveEvent != null && effectiveEvent.BonusPercentage > 0) ? effectiveEvent.BonusPercentage : 0);
		return Mathf.FloorToInt((float)(info.Gem * num) * 0.01f);
	}

	public override void Open(IListener listener, int index)
	{
		SetItemListCustomSorting();
		base.Open(listener, index);
	}

	public override void Open(IListener listener, int orginalIndex, int comboIndex)
	{
		SetItemListCustomSorting();
		base.Open(listener, orginalIndex, comboIndex);
	}

	public override void Open(IListener listener, NewPayShopProductInfo newPayShopProductInfo)
	{
		SetItemListCustomSorting();
		base.Open(listener, newPayShopProductInfo);
	}

	protected override void Init(NewPayShopProductInfo productInfo)
	{
		base.Init(productInfo);
		GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Get(productInfo.Index);
		SetStepProductUI(productInfo);
		if (productInfo.Category == NewPayShopProductCategoryType.RightAway.ToString())
		{
			InitRightAwayItem(productInfo);
			SprkSpecialShop.Init(this, productInfo);
			SprkShopSearch.DecoratePopup(this, productInfo);
		}
		else
		{
			InitItem(productInfo);
			SprkSpecialShop.Init(this, productInfo);
			SprkShopSearch.DecoratePopup(this, productInfo);
		}
	}

	protected override void SetExtra(NewPayShopProductInfo productInfo, PlayerProductPurchaseInfo purchaseInfo)
	{
		base.SetExtra(productInfo, purchaseInfo);
		SetDesc(productInfo, purchaseInfo);
		SetRemainCount(productInfo);
		if ((bool)Table_ProductInfo)
		{
			Table_ProductInfo.Reposition();
		}
	}

	private void SetRemainCount(NewPayShopProductInfo productInfo)
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.RightAwayProductManager == null)
		{
			UIHelper.EnableGameObject(Label_RemainCount, enable: false);
			return;
		}
		RightAwayProductInfo rightAwayProductInfo = GlobalAutoBehaviour<AccountManager>.instance.RightAwayProductManager.Get(productInfo.Index);
		if (rightAwayProductInfo == null)
		{
			UIHelper.EnableGameObject(Label_RemainCount, enable: false);
			return;
		}
		int limitedQuantity = rightAwayProductInfo.LimitedQuantity;
		string stringDirect = LocalizationManager.GetStringDirect("RIGHTAWAY_SECOND_PRODUCT_REMAINING_COUNT_POPUP_TEXT", limitedQuantity);
		UIHelper.EnableGameObject(Label_RemainCount, limitedQuantity > 0);
		UIHelper.SetLabelText(Label_RemainCount, stringDirect);
	}

	private void SetDesc(NewPayShopProductInfo productInfo, PlayerProductPurchaseInfo purchaseInfo)
	{
		if (CanResetPurchase(productInfo))
		{
			SetDesc_ResetDate(productInfo);
		}
		else if (productInfo.DurationDay > 0)
		{
			UIHelper.EnableGameObject(Label_Desc, enable: true);
			if (productInfo.ProductType == ProductType.DailyGem)
			{
				SetDesc_DailyRuby(productInfo, purchaseInfo);
			}
			else if (productInfo.ProductType == ProductType.DailyItem)
			{
				SetDesc_DailyItem(productInfo, purchaseInfo);
			}
			else
			{
				SetDesc_Booster(productInfo, purchaseInfo);
			}
		}
		else
		{
			string stringDirect = LocalizationManager.GetStringDirect(productInfo.ProductDesc);
			bool flag = !String_Extension_Method.IsNullOrEmpty(stringDirect);
			UIHelper.EnableGameObject(Label_Desc, flag);
			if (flag)
			{
				UIHelper.SetLabelText(Label_Desc, stringDirect);
			}
		}
		bool flag2 = productInfo.ProductType == ProductType.Booster;
		UIHelper.EnableGameObject(Booster_Desc, flag2);
		if (flag2)
		{
			string stringDirect2 = LocalizationManager.GetStringDirect(productInfo.ProductDetailDesc);
			UIHelper.SetLabelText(Booster_Desc, stringDirect2);
		}
	}

	protected bool CanResetPurchase(NewPayShopProductInfo productInfo)
	{
		bool result = false;
		if (productInfo.ResetType == ResetType.Daily)
		{
			result = productInfo.DailyReset;
		}
		else if (productInfo.ResetType == ResetType.Weekly)
		{
			result = !__Extension_Method_ICollection__.IsNullOrEmpty(productInfo.WeeklyReset);
		}
		else if (productInfo.ResetType == ResetType.Monthly)
		{
			result = !__Extension_Method_ICollection__.IsNullOrEmpty(productInfo.MonthlyResetDate);
		}
		return result;
	}

	private void SetDesc_ResetDate(NewPayShopProductInfo productInfo)
	{
		string text = "";
		if (productInfo.ResetType == ResetType.Daily)
		{
			text = LocalizationManager.GetString(NShared.NLocalization.PayShop.DAILY_RESET_PURCHASE_COUNT);
		}
		else if (productInfo.ResetType == ResetType.Weekly)
		{
			string text2 = string.Empty;
			Array.Sort(productInfo.WeeklyReset);
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(productInfo.WeeklyReset); i++)
			{
				Enum dayOfWeekKey = NGame2.NUI.Common.GetDayOfWeekKey((DayOfWeek)productInfo.WeeklyReset[i]);
				text2 += LocalizationManager.GetString(dayOfWeekKey);
				text2 += ",";
			}
			text2 = text2.TrimEnd(',');
			text = LocalizationManager.GetString(NShared.NLocalization.PayShop.WEEKLY_RESET_PURCHASE_COUNT, text2);
		}
		else if (productInfo.ResetType == ResetType.Monthly)
		{
			string text3 = string.Empty;
			Array.Sort(productInfo.MonthlyResetDate);
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(productInfo.MonthlyResetDate); j++)
			{
				text3 += NGame2.NUI.Common.GetOrdinalizeDay(productInfo.MonthlyResetDate[j]);
				text3 += ",";
			}
			text3 = text3.TrimEnd(',');
			text = LocalizationManager.GetString(NShared.NLocalization.PayShop.RESET_DAYOFMONTH_DESC, text3);
		}
		if (String_Extension_Method.IsNullOrEmpty(text))
		{
			UIHelper.EnableGameObject(Label_Desc, enable: false);
			return;
		}
		UIHelper.EnableGameObject(Label_Desc, enable: true);
		UIHelper.SetLabelText(Label_Desc, text);
	}

	private void SetDesc_DailyRuby(NewPayShopProductInfo productInfo, PlayerProductPurchaseInfo purchaseInfo)
	{
		string text = "";
		double remainDayUntilMonthlyEnd = 0.0;
		if (NGame2.NAccount.UserPlayerProductPurchaseManager.IsPurchasable(productInfo, purchaseInfo, out remainDayUntilMonthlyEnd))
		{
			if (productInfo.Gem > 0)
			{
				int num = 0;
				num += productInfo.Gem;
				num += productInfo.BonusGem;
				if (purchaseInfo == null)
				{
					num += productInfo.FirstGem;
				}
				num += productInfo.DailyGem * productInfo.DurationDay;
				text = LocalizationManager.GetString(NShared.NLocalization.PayShop.RUBY_MONTHLY_BENEFIT, productInfo.DurationDay, productInfo.DailyGem, num);
			}
			else
			{
				text = LocalizationManager.GetStringDirect(productInfo.ProductDesc);
			}
		}
		else
		{
			text = LocalizationManager.GetString(NShared.NLocalization.PayShop.RUBY_MONTHLY_REMAIN_DAY, (int)remainDayUntilMonthlyEnd);
		}
		UIHelper.SetLabelText(Label_Desc, text);
	}

	private void SetDesc_Booster(NewPayShopProductInfo productInfo, PlayerProductPurchaseInfo purchaseInfo)
	{
		double remainDays = NGame2.NAccount.UserPlayerProductPurchaseManager.GetRemainDays(purchaseInfo);
		string text = "";
		if (remainDays <= (double)productInfo.BuyableDay)
		{
			text = LocalizationManager.GetStringDirect(productInfo.ProductDesc);
		}
		else if (remainDays >= 1.0)
		{
			text = LocalizationManager.GetString(NShared.NLocalization.PayShop.RUBY_MONTHLY_REMAIN_DAY, (int)remainDays);
		}
		else
		{
			double remainHours = NGame2.NAccount.UserPlayerProductPurchaseManager.GetRemainHours(purchaseInfo);
			text = LocalizationManager.GetString(NShared.NLocalization.PayShop.RUBY_MONTHLY_REMAIN_HOUR, (int)remainHours);
		}
		UIHelper.SetLabelText(Label_Desc, text);
	}

	private void SetDesc_DailyItem(NewPayShopProductInfo productInfo, PlayerProductPurchaseInfo purchaseInfo)
	{
		string text = "";
		double remainDayUntilMonthlyEnd = 0.0;
		UIHelper.SetLabelText(text: (!NGame2.NAccount.UserPlayerProductPurchaseManager.IsPurchasable(productInfo, purchaseInfo, out remainDayUntilMonthlyEnd)) ? LocalizationManager.GetString(NShared.NLocalization.PayShop.RUBY_MONTHLY_REMAIN_DAY, (int)remainDayUntilMonthlyEnd) : LocalizationManager.GetStringDirect(productInfo.ProductDesc), label: Label_Desc);
	}

	private void InitRightAwayItem(NewPayShopProductInfo productInfo)
	{
		if (productInfo.HeroItemIndex > 0)
		{
			AddProduct(productInfo.HeroItemIndex, 1);
		}
		if (productInfo.ProductType == ProductType.Package)
		{
			int num = ((__Extension_Method_ICollection__.COUNT(productInfo.ItemInfos) > 0) ? productInfo.ItemInfos[0].ItemIndex : (-1));
			if (num == -1)
			{
				return;
			}
			AddPackageProduct(num, productInfo.Count[0]);
		}
		else if (productInfo.ProductType == ProductType.Item || productInfo.ProductType == ProductType.Activation)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(productInfo.ItemInfos); i++)
			{
				PayShopProductItemInfo payShopProductItemInfo = productInfo.ItemInfos[i];
				if (payShopProductItemInfo != null)
				{
					AddProduct(payShopProductItemInfo.ItemIndex, payShopProductItemInfo.ItemCount);
				}
			}
		}
		else if (productInfo.ProductType == ProductType.DailyItem)
		{
			CreateAttendanceItems(productInfo);
		}
		AddGem(productInfo.Gem, RewardGemType.None);
		AddGem(productInfo.FirstGem, RewardGemType.First);
		AddGem(productInfo.BonusGem, RewardGemType.Bonus);
		AddGem(productInfo.DailyGem, RewardGemType.Daily);
		int payBackGem = GetPayBackGem(productInfo);
		AddGem(payBackGem, RewardGemType.Payback);
		AddCurrencyProduct(CurrencyType.ShopEventPoint, productInfo.ShopEventPoint);
		AddCurrencyProduct(CurrencyType.Gold, productInfo.Gold);
		AddCurrencyProduct(CurrencyType.Mileage, productInfo.Mileage);
		AddCurrencyProduct(CurrencyType.LuaPoint, productInfo.LuaPoint);
		AddCurrencyProduct(CurrencyType.LimitedShopEventPoint, productInfo.LimitedShopEventPoint);
		RepositionProductList();
	}

	private void InitItem(NewPayShopProductInfo productInfo)
	{
		if (productInfo.HeroItemIndex > 0)
		{
			AddProduct(productInfo.HeroItemIndex, 1);
		}
		if (productInfo.ProductType == ProductType.Package)
		{
			string code = ((__Extension_Method_ICollection__.COUNT(productInfo.UseItemCode) > 0) ? productInfo.UseItemCode[0] : string.Empty);
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(code);
			if (dataByCode == null)
			{
				return;
			}
			AddPackageProduct(dataByCode.Index, productInfo.Count[0]);
		}
		else if (productInfo.ProductType == ProductType.Item || productInfo.ProductType == ProductType.Activation)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(productInfo.UseItemCode); i++)
			{
				string code2 = productInfo.UseItemCode[i];
				int itemCount = productInfo.Count[i];
				ItemData dataByCode2 = GameRoot.Instance.ItemDataContainer.GetDataByCode(code2);
				if (dataByCode2 != null)
				{
					AddProduct(dataByCode2.Index, itemCount);
				}
			}
		}
		else if (productInfo.ProductType == ProductType.DailyItem)
		{
			CreateAttendanceItems(productInfo);
		}
		AddGem(productInfo.Gem, RewardGemType.None);
		AddGem(productInfo.FirstGem, RewardGemType.First);
		AddGem(productInfo.BonusGem, RewardGemType.Bonus);
		AddGem(productInfo.DailyGem, RewardGemType.Daily);
		int payBackGem = GetPayBackGem(productInfo);
		AddGem(payBackGem, RewardGemType.Payback);
		AddCurrencyProduct(CurrencyType.ShopEventPoint, productInfo.ShopEventPoint);
		AddCurrencyProduct(CurrencyType.Gold, productInfo.Gold);
		AddCurrencyProduct(CurrencyType.Mileage, productInfo.Mileage);
		AddCurrencyProduct(CurrencyType.LuaPoint, productInfo.LuaPoint);
		AddCurrencyProduct(CurrencyType.LimitedShopEventPoint, productInfo.LimitedShopEventPoint);
		RepositionProductList();
	}

	protected PayShopProductSimpleComponent AddProduct(int itemIndex, int itemCount)
	{
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
		if (data == null)
		{
			return null;
		}
		PayShopProductSimpleComponent payShopProductSimpleComponent = CreateComponent(isBonus: false);
		int obtainType = 0;
		payShopProductSimpleComponent.Open(data, itemCount, (ObtainItemType)obtainType);
		payShopProductSimpleComponent.SortIndex = SortIndex++;
		_itemList.Add(payShopProductSimpleComponent);
		return payShopProductSimpleComponent;
	}

	protected void AddPackageProduct(int itemIndex, int itemCount)
	{
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
		if (data != null)
		{
			PackageItemHelper.GetRewardElementList(data, _listRewardElementData, null);
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listRewardElementData); i++)
			{
				CreateInPackageItems(_listRewardElementData[i]);
			}
		}
	}

	private PayShopProductSimpleComponent CreateInPackageItems(RewardElementData rewardElementData)
	{
		if (rewardElementData == null)
		{
			return null;
		}
		CurrencyType currencyType = rewardElementData.GetCurrencyType();
		if (currencyType != CurrencyType.None)
		{
			ObtainItemType obtainType = GetObtainType(currencyType, rewardElementData.IsFree());
			PayShopProductSimpleComponent payShopProductSimpleComponent = CreateComponent(obtainType >= ObtainItemType.Bonus);
			int itemMin = rewardElementData.ItemMin;
			ObtainItemType obtainType2 = obtainType;
			int num = itemMin;
			payShopProductSimpleComponent.Open(currencyType, num, obtainType2);
			payShopProductSimpleComponent.SortIndex = SortIndex++;
			_itemList.Add(payShopProductSimpleComponent);
			if (currencyType == CurrencyType.Gem && rewardElementData.IsFree())
			{
				int itemMin2 = rewardElementData.ItemMin;
				string externKey = "";
				int gemType = 2;
				int num2 = itemMin2;
				string bonusGemString = GetBonusGemString(num2, (RewardGemType)gemType, externKey);
				payShopProductSimpleComponent.SetName(bonusGemString);
			}
			return payShopProductSimpleComponent;
		}
		PayShopProductSimpleComponent payShopProductSimpleComponent2 = CreateComponent(isBonus: false);
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardElementData.ItemCode);
		if (dataByCode != null)
		{
			int itemMin3 = rewardElementData.ItemMin;
			int obtainType3 = 0;
			int num3 = itemMin3;
			payShopProductSimpleComponent2.Open(dataByCode, num3, (ObtainItemType)obtainType3);
		}
		else
		{
			payShopProductSimpleComponent2.OpenError(rewardElementData.ItemCode);
		}
		payShopProductSimpleComponent2.SortIndex = SortIndex++;
		_itemList.Add(payShopProductSimpleComponent2);
		return payShopProductSimpleComponent2;
	}

	protected ObtainItemType GetObtainType(CurrencyType type, bool isFree)
	{
		return GachaSelctShopHelper.GetObtainType(type, isFree);
	}

	private string GetBonusGemString(long gem, RewardGemType gemType, string externKey = "")
	{
		string arg = LocalizationManager.GetString(NShared.NLocalization.Common.RUBY_COUNT, Vespa_Int_Extension_Method.ToCommaString(gem));
		string arg2 = "";
		if (String_Extension_Method.IsNullOrEmpty(externKey))
		{
			switch (gemType)
			{
			case RewardGemType.Bonus:
				arg2 = LocalizationManager.GetString(NShared.NLocalization.PayShop.BUY_GEM_BASIC_BONUS_ADD_MSG);
				break;
			case RewardGemType.Daily:
				arg2 = LocalizationManager.GetString(NShared.NLocalization.PayShop.BUY_GEM_DAILY_ADD_MSG);
				break;
			case RewardGemType.First:
				arg2 = LocalizationManager.GetString(NShared.NLocalization.PayShop.BUY_GEM_FIRST_BONUS_ADD_MSG);
				break;
			case RewardGemType.Payback:
				arg2 = LocalizationManager.GetString(NShared.NLocalization.PayShop.RUBY_PAYBACK_LUA_30);
				break;
			}
			arg2 = $"{arg} {arg2}";
		}
		else
		{
			arg2 = LocalizationManager.GetStringDirect(externKey, gem);
		}
		return NGUIText.EncodeColor(arg2, SpecialGemLabelColor.LabelColor);
	}

	protected void CreateAttendanceItems(NewPayShopProductInfo productInfo)
	{
		if (productInfo.Category == NewPayShopProductCategoryType.RightAway.ToString())
		{
			if (__Extension_Method_ICollection__.COUNT(productInfo.ItemInfos) != 0)
			{
				PayShopProductItemInfo payShopProductItemInfo = productInfo.ItemInfos[0];
				AddProduct(payShopProductItemInfo.ItemIndex, payShopProductItemInfo.ItemCount);
			}
		}
		else if (__Extension_Method_ICollection__.COUNT(productInfo.UseItemCode) != 0)
		{
			string code = productInfo.UseItemCode[0];
			int itemCount = 1;
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(code);
			if (dataByCode != null)
			{
				AddProduct(dataByCode.Index, itemCount);
			}
		}
		if (__Extension_Method_ICollection__.COUNT(productInfo.SelectAttendanceIndices) == 0)
		{
			return;
		}
		int index_MainAttendance = productInfo.SelectAttendanceIndices[0];
		AttendanceData[] allAttendanceDatas = new AttendanceKeywordGroup(index_MainAttendance).GetAllAttendanceDatas();
		foreach (AttendanceData attendanceData in allAttendanceDatas)
		{
			if (attendanceData == null)
			{
				continue;
			}
			AttendanceRewards[] reward = attendanceData.Reward;
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(reward); j++)
			{
				AttendanceRewards attendanceRewards = reward[j];
				if (attendanceRewards != null)
				{
					AttendanceReward[] reward2 = attendanceRewards.Reward;
					for (int k = 0; k < __Extension_Method_ICollection__.COUNT(reward2); k++)
					{
						AttendanceReward rewardItem = reward2[k];
						AddAttendanceRewardItem(rewardItem);
					}
				}
			}
		}
	}

	private void AddAttendanceRewardItem(AttendanceReward rewardItem)
	{
		if (rewardItem == null || rewardItem.ShowType == EAttendanceRewardShowType.None)
		{
			return;
		}
		bool flag = rewardItem.ShowType == EAttendanceRewardShowType.Bonus;
		bool num = rewardItem.Type == RewardType.Item;
		ObtainItemType obtainType = (flag ? ObtainItemType.BonusItem : ObtainItemType.BaseItem);
		int sortIndex = 100 * rewardItem.Day + rewardItem.No;
		PayShopProductSimpleComponent payShopProductSimpleComponent = null;
		int num2 = (num ? rewardItem.Count : rewardItem.Value);
		long num3 = num2;
		if (num)
		{
			int value = rewardItem.Value;
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(value);
			if (data == null)
			{
				return;
			}
			payShopProductSimpleComponent = CreateComponent(flag);
			payShopProductSimpleComponent.Open(data, num3, obtainType);
			AttendanceRewardInfoData data2 = GameRoot.Instance.AttendanceRewardInfoDataContainer.GetData(rewardItem.Index, rewardItem.Day, rewardItem.No);
			string text = ((data2 != null) ? data2.RewardTitleKey : "");
			if (!String_Extension_Method.IsNullOrEmpty(text))
			{
				string stringDirect = LocalizationManager.GetStringDirect(text, Vespa_Int_Extension_Method.ToCommaString(num3));
				payShopProductSimpleComponent.SetName(stringDirect);
			}
		}
		else
		{
			payShopProductSimpleComponent = CreateComponent(flag);
			CurrencyType currencyType = UIHelper.GetCurrencyType(rewardItem.Type);
			payShopProductSimpleComponent.Open(currencyType, num3, obtainType);
			AttendanceRewardInfoData data3 = GameRoot.Instance.AttendanceRewardInfoDataContainer.GetData(rewardItem.Index, rewardItem.Day, rewardItem.No);
			string text2 = ((data3 != null) ? data3.RewardTitleKey : "");
			if (currencyType == CurrencyType.Gem)
			{
				string bonusGemString = GetBonusGemString(num3, RewardGemType.Bonus, text2);
				payShopProductSimpleComponent.SetName(bonusGemString);
			}
			else if (!String_Extension_Method.IsNullOrEmpty(text2))
			{
				string stringDirect2 = LocalizationManager.GetStringDirect(text2, Vespa_Int_Extension_Method.ToCommaString(num3));
				payShopProductSimpleComponent.SetName(stringDirect2);
			}
		}
		payShopProductSimpleComponent.SortIndex = sortIndex;
		_itemList.Add(payShopProductSimpleComponent);
	}

	private PayShopProductSimpleComponent CreateComponent(bool isBonus)
	{
		PayShopProductSimpleComponent payShopProductSimpleComponent = null;
		payShopProductSimpleComponent = ((!isBonus) ? UIHelper.AddGridItem(ItemScrollGrid, PayShopProductSimpleComponent.PrefabAuto) : UIHelper.AddGridItem(BonusScrollGrid, PayShopProductSimpleComponent.PrefabAuto));
		UIHelper.EnableGameObject(payShopProductSimpleComponent, enable: true);
		return payShopProductSimpleComponent;
	}

	protected PayShopProductSimpleComponent AddCurrencyProduct(CurrencyType currency, int value)
	{
		if (value <= 0)
		{
			return null;
		}
		ObtainItemType obtainType = GetObtainType(currency, isFree: false);
		PayShopProductSimpleComponent payShopProductSimpleComponent = CreateComponent(obtainType >= ObtainItemType.Bonus);
		ObtainItemType obtainType2 = obtainType;
		payShopProductSimpleComponent.Open(currency, value, obtainType2);
		payShopProductSimpleComponent.SortIndex = SortIndex++;
		_itemList.Add(payShopProductSimpleComponent);
		return payShopProductSimpleComponent;
	}

	protected void AddGem(int gem, RewardGemType gemType)
	{
		if (gem > 0)
		{
			PayShopProductSimpleComponent payShopProductSimpleComponent = AddCurrencyProduct(CurrencyType.Gem, gem);
			if (gemType != RewardGemType.None)
			{
				string externKey = "";
				string bonusGemString = GetBonusGemString(gem, gemType, externKey);
				payShopProductSimpleComponent.SetName(bonusGemString);
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		_originInfo = null;
		_currentInfo = null;
		ReleaseComponent();
	}

	protected override void ReleaseComponent()
	{
		base.ReleaseComponent();
		FactoryBase.ReleaseAndClearList(_itemList);
		_listRewardElementData.Clear();
		SortIndex = 0;
	}

	private void SetItemListCustomSorting()
	{
		if (ItemScrollGrid != null)
		{
			ItemScrollGrid.Grid.onCustomSort = PayShopUIHelper.GetCustomComparePayShop;
		}
		if (BonusScrollGrid != null)
		{
			BonusScrollGrid.Grid.onCustomSort = PayShopUIHelper.GetCustomComparePayShop;
		}
	}

	protected void RepositionProductList()
	{
		UIHelper.RepositionGrid(ItemScrollGrid);
		UIHelper.RepositionGrid(BonusScrollGrid);
	}

	public void ShowChangeProductButton(bool isShow)
	{
		UIHelper.EnableGameObject(Widget_StepProductRoot, isShow);
		UIHelper.EnableGameObject(LeftProductArrow.Root, isShow);
		UIHelper.EnableGameObject(RightProductArrow.Root, isShow);
	}

	protected void SetStepProductUI(NewPayShopProductInfo productInfo)
	{
		bool flag = __Extension_Method_ICollection__.COUNT(productInfo.StepProductGroup) > 0;
		ShowChangeProductButton(flag);
		if (!flag)
		{
			return;
		}
		int valueInt = ArgParser.GetValueInt(productInfo.StepProductGroup, 0);
		int valueInt2 = ArgParser.GetValueInt(productInfo.StepProductGroup, 1);
		NewPayShopProductInfo[] stepProducts = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.GetStepProducts(productInfo.Category, valueInt);
		if (__Extension_Method_ICollection__.COUNT(stepProducts) == 0)
		{
			return;
		}
		int num = 0;
		NewPayShopProductInfo newPayShopProductInfo = null;
		NewPayShopProductInfo newPayShopProductInfo2 = null;
		NewPayShopProductInfo[] array = stepProducts;
		foreach (NewPayShopProductInfo newPayShopProductInfo3 in array)
		{
			int valueInt3 = ArgParser.GetValueInt(newPayShopProductInfo3.StepProductGroup, 1);
			if (num < valueInt3)
			{
				num = valueInt3;
			}
			if (valueInt3 == valueInt2 - 1)
			{
				newPayShopProductInfo = newPayShopProductInfo3;
			}
			else if (valueInt3 == valueInt2 + 1)
			{
				newPayShopProductInfo2 = newPayShopProductInfo3;
			}
		}
		UIHelper.SetLabelText(Label_StepCount, valueInt2 + "/" + num);
		if (valueInt2 <= 1)
		{
			UIHelper.EnableGameObject(LeftProductArrow.Root, enable: false);
		}
		else if (newPayShopProductInfo != null)
		{
			LeftProductArrow.SetTexture(newPayShopProductInfo.IconPath);
		}
		if (valueInt2 >= num)
		{
			UIHelper.EnableGameObject(RightProductArrow.Root, enable: false);
		}
		else if (newPayShopProductInfo2 != null)
		{
			RightProductArrow.SetTexture(newPayShopProductInfo2.IconPath);
		}
	}

	public void OnClickPrevComboProduct()
	{
		SetOtherComboProduct(-1);
	}

	public void OnClickNextComboProduct()
	{
		SetOtherComboProduct(1);
	}

	private void SetOtherComboProduct(int stepOffset)
	{
		int valueInt = ArgParser.GetValueInt(_currentInfo.StepProductGroup, 0);
		int valueInt2 = ArgParser.GetValueInt(_currentInfo.StepProductGroup, 1);
		NewPayShopProductInfo stepProduct = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.GetStepProduct(_currentInfo.Category, valueInt, valueInt2 + stepOffset);
		if (stepProduct != null)
		{
			ReleaseComponent();
			Init(stepProduct);
		}
	}

	private void SetOtherComboIcon()
	{
	}
}
