using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NPayShop;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class LegendCostumeReservationPopup : BaseWindow, IInspectorGuiDrawer, IUserObserver<NGame2.NAccount.UserPayShopDiscountProductManager.ChangedPayShopDiscountProductInfo>, IUserObserver, IUserObserver<DropBonusEventManager.DropBonusEventChangeInfo>, IUserObserver<PlayerProductPurchaseInfo>
{
	[Serializable]
	public class AnimationKeyInfo
	{
		public UIBundleLoader CostumeBundleTex;

		public UIBundleLoader ShadowBundleTex;

		public string AnimationStartKey;

		public string AnimationFinalStartKey;

		public string AnimationDiscountKey;

		public string AnimationFinalDiscountKey;

		public string AnimationSwitchingKey;
	}

	private class PageInfo
	{
		public PayShopDiscountData DiscountData;

		public PayShopDiscountGroupData FirstDiscountGroupData;

		public PayShopDiscountGroupData CurrentGroupData;

		public PayShopDiscountGroupData NextGroupData;

		public int CurrentStep;

		public int LastStep;

		public bool IsEnd;

		public AnimationKeyInfo AnimationKeyInfo;

		public string Title;

		public string PortraitPath;

		public DateTime BeginTime;

		public DateTime EndTime;

		public EReserveButtonState ButtonState;

		public int DefaultPrice;

		public int DiscountPrice;

		public int LimitedCount;

		public int PurchasedCount;

		public int ProductIndex => DiscountData.ProductIndex;
	}

	private enum EReserveButtonState
	{
		CanPurchase,
		CannotPurchaseByEnded,
		CannotPurchaseBySoldOut,
		AlreadyPurchase
	}

	[Flags]
	public enum EShowOption
	{
		None = 0,
		ChangePage = 1,
		MoveToHeroCostumeView = 2
	}

	private enum EChangePageOption
	{
		First,
		NotPurchased,
		Prev,
		Next
	}

	private enum EErrorCode
	{
		DataCountZero,
		DropBonusEventIsNull,
		TimeParseError,
		DiscountDataIsNotExist,
		ProductIndexIsNotExist,
		PageListIsNull,
		CurrentPageInfoIsNull,
		GroupDataIsNull,
		AnimationIndexInvalid,
		AnimationDataIsNull
	}

	public const string PrefabPath = "Resources/Prefab2/Window/PayShop/LegendCostumeReservationPopup";

	[Header("LegendCostume - Outside Buttons")]
	public Transform Button_Root;

	public UIButton MoveToHeroCostumeView_Button;

	public UIButton Purchase_Button;

	public UILabel ButtonPurchase_Label;

	public UIButton ChangePageLeft_Button;

	public UIButton ChangePageRight_Button;

	[Header("LegendCostume - Current")]
	public UILabel CurrentPrice_Label;

	public UILabel CurrentPrice_Shadow;

	public UILabel CurrentDiscount_Label;

	public UILabel CurrentPersonnel_Label;

	public UILabelAligner Current_LabelAligner;

	public UISprite Deco_Left;

	public UISprite Deco_Right;

	[Header("LegendCostume - Next")]
	public UILabel NextPriceUI_Label;

	public UILabel NextPrice_Label;

	public UILabel NextPrice_Shadow;

	public UILabel NextDiscount_Label;

	public UILabel NextPersonnel_Label;

	public UILabelAligner Next_LabelAligner;

	[Header("LegendCostume - Gauge")]
	public UISprite Gauge_Sprite;

	public UILabel Gauge_Label;

	public Transform GaugePoint_Root;

	[Header("LegendCostume - Etc")]
	public UILabel Title_Label;

	public UILabel Title_Shadow;

	public UILabel Date_Label;

	public UIScrollView Guide_ScrollView;

	public Animator Costume_Animator;

	[HideInInspector]
	public List<AnimationKeyInfo> AnimationKeyInfoUIList = new List<AnimationKeyInfo>();

	public static DateTime PrevRequestDateTime;

	public static int RequestCoolTime;

	private List<PageInfo> _pageInfoList;

	private PageInfo _curPageInfo;

	private StringBuilder _sbForLocalization;

	private string _nextPriceNormalUILocalizationCache;

	private string _nextPriceFinalUILocalizationCache;

	private string _buttonCanPurchaseLocalizationCache;

	private string _buttonCannotPurchaseLocalizationCache;

	private string _buttonAlreadyPurchaseLocalizationCache;

	private string _currentPersonnelLocalizationCache;

	private string _nextPersonnelLocalizationCache;

	private string _endPersonnelLocalizationCache;

	private string _progressLocalizationCache;

	private string _discountLocalizationCache;

	private string _dateLocalizationCache;

	private float _gaugeStartX;

	private bool _isClosedAbnormal;

	private bool _isExistErrorPopup;

	private Vector3 _gaugeLabelPosition;

	private Coroutine _animationCoroutine;

	public void Open(int initProductIndex = 0, EShowOption showOption = EShowOption.None)
	{
		Open(delegate
		{
			InitButtons(showOption);
			Init(initProductIndex);
		}, null);
	}

	private void InitButtons(EShowOption showOption)
	{
		bool enable = (showOption & EShowOption.ChangePage) > EShowOption.None;
		bool enable2 = (showOption & EShowOption.MoveToHeroCostumeView) > EShowOption.None;
		UIHelper.EnableGameObject(ChangePageLeft_Button, enable);
		UIHelper.EnableGameObject(ChangePageRight_Button, enable);
		UIHelper.EnableGameObject(MoveToHeroCostumeView_Button, enable2);
	}

	private void Init(int initProductIndex)
	{
		InitDiscountData();
		if (initProductIndex == 0)
		{
			if (_pageInfoList.Count == 1)
			{
				EShowOption showOption = (UIHelper.IsEnabledSelfGameObjet(MoveToHeroCostumeView_Button) ? EShowOption.MoveToHeroCostumeView : EShowOption.None);
				InitButtons(showOption);
			}
			ChangePage(EChangePageOption.NotPurchased, isSwitchAnimationPlay: false);
		}
		else
		{
			PageInfo pageInfo = FindPageInfo(initProductIndex);
			if (pageInfo == null)
			{
				VespaDebug.LogError("ProductIndex is not exist");
				ChangePage(EChangePageOption.First, isSwitchAnimationPlay: false);
			}
			else
			{
				ChangeProduct(pageInfo, isSwitchAnimationPlay: false);
			}
		}
		Guide_ScrollView.ResetPosition();
		MoveToHeroCostumeView_Button.defaultColor = new Color(1f, 1f, 1f, 1f);
		Purchase_Button.defaultColor = new Color(1f, 1f, 1f, 1f);
		if (_curPageInfo != null)
		{
			string state = (IsFinalStep(_curPageInfo) ? _curPageInfo.AnimationKeyInfo.AnimationFinalStartKey : _curPageInfo.AnimationKeyInfo.AnimationStartKey);
			AnimationStart(state);
		}
		AddObserver();
	}

	private bool IsFinalStep(PageInfo pageInfo)
	{
		return pageInfo.CurrentStep >= pageInfo.LastStep;
	}

	private void InitDiscountData()
	{
		int num = GlobalAutoBehaviour<AccountManager>.instance.PayShopDiscountProductManager.Count();
		if (num == 0)
		{
			CloseAbnormal(EErrorCode.DataCountZero, isForceClose: true);
			return;
		}
		_pageInfoList = new List<PageInfo>(num);
		_sbForLocalization = new StringBuilder(128);
		_gaugeStartX = Gauge_Sprite.transform.localPosition.x - (float)Gauge_Sprite.width / 2f;
		InitLocalization();
		RefreshProductInfo();
		RefreshProductButtonStatus();
		SortPages();
	}

	private void InitLocalization()
	{
		_nextPriceNormalUILocalizationCache = LocalizationManager.GetStringDirect("NEW_PAY_SHOP_POPUP_TARGET_PRICE");
		_nextPriceFinalUILocalizationCache = LocalizationManager.GetStringDirect("NEW_PAY_SHOP_POPUP_TARGET_PRICE_END");
		_buttonCanPurchaseLocalizationCache = LocalizationManager.GetStringDirect("NEW_PAY_SHOP_POPUP_LEGENDCOSTUME_BUTTON_PURCHASE");
		_buttonCannotPurchaseLocalizationCache = LocalizationManager.GetStringDirect("EVENT_MISSION_NOTICE_END");
		_buttonAlreadyPurchaseLocalizationCache = LocalizationManager.GetStringDirect("NEW_PAY_SHOP_POPUP_LEGENDCOSTUME_BUTTON_COMPLETE");
		_currentPersonnelLocalizationCache = LocalizationManager.GetStringDirect("NEW_PAY_SHOP_POPUP_NOW_PERSONNEL_2");
		_nextPersonnelLocalizationCache = LocalizationManager.GetStringDirect("NEW_PAY_SHOP_POPUP_TARGET_PERSONNEL");
		_endPersonnelLocalizationCache = LocalizationManager.GetStringDirect("NEW_PAY_SHOP_POPUP_END_AUCTION");
		_progressLocalizationCache = LocalizationManager.GetStringDirect("NEW_PAY_SHOP_POPUP_NOW_PERCENTAGE");
		_discountLocalizationCache = LocalizationManager.GetStringDirect("NEW_PAY_SHOP_POPUP_DISCOUNT");
		_dateLocalizationCache = LocalizationManager.GetStringDirect("NEW_PAY_SHOP_POPUP_DATE");
		string[] replaceFormat = new string[9] { "{0}", "{1}", "{2}", "{3}", "{4}", "{5}", "{6}", "{7}", "{8}" };
		LocalizationReplace(replaceFormat, 2, ref _currentPersonnelLocalizationCache);
		LocalizationReplace(replaceFormat, 1, ref _nextPersonnelLocalizationCache);
		LocalizationReplace(replaceFormat, 2, ref _progressLocalizationCache);
		LocalizationReplace(replaceFormat, 1, ref _discountLocalizationCache);
		LocalizationReplace(replaceFormat, 8, ref _dateLocalizationCache);
	}

	private void LocalizationReplace(string[] replaceFormat, int count, ref string localString)
	{
		for (int i = 0; i < count; i++)
		{
			localString = localString.Replace(replaceFormat[i + 1], replaceFormat[i]);
		}
	}

	public void SortPages()
	{
		List<PageInfo> list = new List<PageInfo>();
		List<PageInfo> list2 = new List<PageInfo>();
		foreach (PageInfo pageInfo in _pageInfoList)
		{
			if (pageInfo.ButtonState == EReserveButtonState.CanPurchase)
			{
				list.Add(pageInfo);
			}
			else
			{
				list2.Add(pageInfo);
			}
		}
		list.Sort((PageInfo lhs, PageInfo rhs) => lhs.ProductIndex.CompareTo(rhs.ProductIndex));
		list2.Sort((PageInfo lhs, PageInfo rhs) => lhs.ProductIndex.CompareTo(rhs.ProductIndex));
		_pageInfoList.Clear();
		_pageInfoList.AddRange(list);
		_pageInfoList.AddRange(list2);
	}

	private void RefreshProductInfo(PayShopDiscountInfo changedInfo = null)
	{
		if (changedInfo == null)
		{
			foreach (PayShopDiscountInfo item in GlobalAutoBehaviour<AccountManager>.instance.PayShopDiscountProductManager)
			{
				RefreshPageInfo(item);
			}
			return;
		}
		RefreshPageInfo(changedInfo);
	}

	private void RefreshPageInfo(PayShopDiscountInfo productInfo)
	{
		if (productInfo == null)
		{
			VespaDebug.LogError("ProductInfo is null");
			return;
		}
		PageInfo pageInfo = FindPageInfo(productInfo.ProductIndex);
		if (pageInfo == null)
		{
			AddNewData(productInfo);
		}
		else
		{
			RefreshPageInfo(pageInfo, productInfo);
		}
	}

	private PageInfo FindPageInfo(int productIndex)
	{
		foreach (PageInfo pageInfo in _pageInfoList)
		{
			if (pageInfo.ProductIndex == productIndex)
			{
				return pageInfo;
			}
		}
		return null;
	}

	private void AddNewData(PayShopDiscountInfo productInfo)
	{
		int productIndex = productInfo.ProductIndex;
		PayShopDiscountData data = GameRoot.Instance.PayShopDiscountDataContainer.GetData(productIndex);
		if (data == null)
		{
			CloseAbnormal(EErrorCode.DiscountDataIsNotExist);
			return;
		}
		DropBonusEvent payshopDiscountEffectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetPayshopDiscountEffectiveEvent(productIndex);
		if (payshopDiscountEffectiveEvent == null)
		{
			CloseAbnormal(EErrorCode.DropBonusEventIsNull);
			return;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(productIndex) == null)
		{
			CloseAbnormal(EErrorCode.DropBonusEventIsNull);
			return;
		}
		_ = payshopDiscountEffectiveEvent.BeginTime;
		_ = payshopDiscountEffectiveEvent.EndTime;
		DateTime clientTime = UIHelper.GetClientTime(payshopDiscountEffectiveEvent.BeginTime);
		if (clientTime == DateTime.MinValue)
		{
			CloseAbnormal(EErrorCode.TimeParseError);
			return;
		}
		DateTime clientTime2 = UIHelper.GetClientTime(payshopDiscountEffectiveEvent.EndTime);
		if (clientTime2 == DateTime.MinValue)
		{
			CloseAbnormal(EErrorCode.TimeParseError);
			return;
		}
		int groupIndex = data.GroupIndex;
		PayShopDiscountGroupData firstDiscountGroupData = new PayShopDiscountGroupData
		{
			GroupIndex = groupIndex,
			Step = 0,
			TargetValue = 0,
			Price = productInfo.DefaultPrice
		};
		int lastStep = GameRoot.Instance.PayShopDiscountGroupDataContainer.GetLastStep(groupIndex);
		int num = data.AnimationIndex - 1;
		AnimationKeyInfo animationKeyInfo = ((num < AnimationKeyInfoUIList.Count) ? AnimationKeyInfoUIList[num] : null);
		if (animationKeyInfo == null)
		{
			animationKeyInfo = AnimationKeyInfoUIList[0];
			CloseAbnormal(EErrorCode.AnimationIndexInvalid);
		}
		string portraitPath = string.Empty;
		CostumeData dataByProductIndex = GameRoot.Instance.CostumeDataContainer.GetDataByProductIndex(productIndex);
		if (dataByProductIndex == null)
		{
			CloseAbnormal(EErrorCode.AnimationDataIsNull);
		}
		else
		{
			portraitPath = dataByProductIndex.IllustPath;
		}
		string stringDirect = LocalizationManager.GetStringDirect(data.TitleKey);
		PageInfo pageInfo = new PageInfo
		{
			DiscountData = data,
			FirstDiscountGroupData = firstDiscountGroupData,
			AnimationKeyInfo = animationKeyInfo,
			DefaultPrice = productInfo.DefaultPrice,
			DiscountPrice = productInfo.DiscountPrice,
			LimitedCount = productInfo.ServerLimitedCount,
			PurchasedCount = productInfo.PurchasedCount,
			CurrentStep = productInfo.Step,
			LastStep = lastStep,
			Title = stringDirect,
			PortraitPath = portraitPath,
			BeginTime = clientTime,
			EndTime = clientTime2
		};
		RefreshPageGroupData(pageInfo);
		_pageInfoList.Add(pageInfo);
	}

	private void RefreshPageInfo(PageInfo pageInfo, PayShopDiscountInfo payShopDiscountInfo)
	{
		pageInfo.DefaultPrice = payShopDiscountInfo.DefaultPrice;
		pageInfo.DiscountPrice = payShopDiscountInfo.DiscountPrice;
		pageInfo.LimitedCount = payShopDiscountInfo.ServerLimitedCount;
		pageInfo.PurchasedCount = payShopDiscountInfo.PurchasedCount;
		bool num = _curPageInfo.ProductIndex == pageInfo.ProductIndex && pageInfo.CurrentStep != payShopDiscountInfo.Step;
		pageInfo.CurrentStep = payShopDiscountInfo.Step;
		bool isEnd = pageInfo.IsEnd;
		RefreshPageGroupData(pageInfo);
		if (num)
		{
			string state = ((isEnd != pageInfo.IsEnd) ? pageInfo.AnimationKeyInfo.AnimationFinalDiscountKey : pageInfo.AnimationKeyInfo.AnimationDiscountKey);
			AnimationStart(state);
		}
	}

	private void RefreshPageGroupData(PageInfo pageInfo)
	{
		int groupIndex = pageInfo.DiscountData.GroupIndex;
		PayShopDiscountGroupData payShopDiscountGroupData = null;
		PayShopDiscountGroupData payShopDiscountGroupData2 = null;
		if (pageInfo.CurrentStep == 0)
		{
			payShopDiscountGroupData = pageInfo.FirstDiscountGroupData;
			payShopDiscountGroupData2 = GameRoot.Instance.PayShopDiscountGroupDataContainer.GetData(groupIndex, 1);
		}
		else if (IsFinalStep(pageInfo))
		{
			payShopDiscountGroupData = GameRoot.Instance.PayShopDiscountGroupDataContainer.GetData(groupIndex, pageInfo.LastStep - 1);
			payShopDiscountGroupData2 = GameRoot.Instance.PayShopDiscountGroupDataContainer.GetData(groupIndex, pageInfo.LastStep);
			pageInfo.IsEnd = true;
		}
		else
		{
			payShopDiscountGroupData = GameRoot.Instance.PayShopDiscountGroupDataContainer.GetData(groupIndex, pageInfo.CurrentStep);
			payShopDiscountGroupData2 = GameRoot.Instance.PayShopDiscountGroupDataContainer.GetData(groupIndex, pageInfo.CurrentStep + 1);
		}
		pageInfo.CurrentGroupData = payShopDiscountGroupData;
		pageInfo.NextGroupData = payShopDiscountGroupData2;
	}

	private void RefreshPageStaticUIInfo()
	{
		if (_curPageInfo == null)
		{
			CloseAbnormal(EErrorCode.CurrentPageInfoIsNull);
			return;
		}
		if (_sbForLocalization == null)
		{
			_sbForLocalization = new StringBuilder(128);
		}
		UIHelper.SetLabelText(Title_Label, _curPageInfo.Title);
		UIHelper.SetLabelText(Title_Shadow, _curPageInfo.Title);
		_sbForLocalization.Length = 0;
		DateTime beginTime = _curPageInfo.BeginTime;
		DateTime endTime = _curPageInfo.EndTime;
		_sbForLocalization.AppendFormat(_dateLocalizationCache, beginTime.Year.ToString(), beginTime.Month.ToString(), beginTime.Day.ToString(), endTime.Year.ToString(), endTime.Month.ToString(), endTime.Day.ToString(), endTime.Hour.ToString(), endTime.Minute.ToString());
		UIHelper.SetLabelText(Date_Label, _sbForLocalization.ToString());
	}

	private void RefreshStepStaticUIInfo()
	{
		if (_curPageInfo == null)
		{
			CloseAbnormal(EErrorCode.CurrentPageInfoIsNull);
			return;
		}
		if (_sbForLocalization == null)
		{
			_sbForLocalization = new StringBuilder(128);
		}
		PayShopDiscountGroupData currentGroupData = _curPageInfo.CurrentGroupData;
		PayShopDiscountGroupData nextGroupData = _curPageInfo.NextGroupData;
		int defaultPrice = _curPageInfo.DefaultPrice;
		if (currentGroupData == null || nextGroupData == null)
		{
			CloseAbnormal(EErrorCode.GroupDataIsNull);
			return;
		}
		string text = ((nextGroupData.Step + 1 >= _curPageInfo.LastStep) ? _nextPriceFinalUILocalizationCache : _nextPriceNormalUILocalizationCache);
		UIHelper.SetLabelText(NextPriceUI_Label, text);
		int price = currentGroupData.Price;
		long num = price;
		UIHelper.SetLabelText(CurrentPrice_Label, Vespa_Int_Extension_Method.ToCommaString(num));
		UIHelper.SetLabelText(CurrentPrice_Shadow, Vespa_Int_Extension_Method.ToCommaString(num));
		long num2 = num;
		int num3 = defaultPrice;
		checked
		{
			long i_value = num3 - num2;
			_sbForLocalization.Length = 0;
			_sbForLocalization.AppendFormat(_discountLocalizationCache, Vespa_Int_Extension_Method.ToCommaString(i_value));
			UIHelper.SetLabelText(CurrentDiscount_Label, _sbForLocalization.ToString());
			int price2 = nextGroupData.Price;
			long num4 = price2;
			UIHelper.SetLabelText(NextPrice_Label, Vespa_Int_Extension_Method.ToCommaString(num4));
			UIHelper.SetLabelText(NextPrice_Shadow, Vespa_Int_Extension_Method.ToCommaString(num4));
			long num5 = num4;
			int num6 = defaultPrice;
			i_value = num6 - num5;
			_sbForLocalization.Length = 0;
			_sbForLocalization.AppendFormat(_discountLocalizationCache, Vespa_Int_Extension_Method.ToCommaString(i_value));
			UIHelper.SetLabelText(NextDiscount_Label, _sbForLocalization.ToString());
			_sbForLocalization.Length = 0;
			if (IsFinalStep(_curPageInfo))
			{
				UIHelper.SetLabelText(NextPersonnel_Label, _endPersonnelLocalizationCache);
			}
			else
			{
				StringBuilder sbForLocalization = _sbForLocalization;
				string nextPersonnelLocalizationCache = _nextPersonnelLocalizationCache;
				int targetValue = nextGroupData.TargetValue;
				sbForLocalization.AppendFormat(nextPersonnelLocalizationCache, Vespa_Int_Extension_Method.ToCommaString(targetValue));
				UIHelper.SetLabelText(NextPersonnel_Label, _sbForLocalization.ToString());
			}
			Current_LabelAligner.Align();
			Next_LabelAligner.Align();
		}
	}

	private void RefreshDynamicUIInfo()
	{
		if (_curPageInfo == null)
		{
			CloseAbnormal(EErrorCode.CurrentPageInfoIsNull);
			return;
		}
		if (_sbForLocalization == null)
		{
			_sbForLocalization = new StringBuilder(128);
		}
		_sbForLocalization.Length = 0;
		PayShopDiscountGroupData currentGroupData = _curPageInfo.CurrentGroupData;
		PayShopDiscountGroupData nextGroupData = _curPageInfo.NextGroupData;
		if (currentGroupData == null || nextGroupData == null)
		{
			CloseAbnormal(EErrorCode.GroupDataIsNull);
			return;
		}
		int num = nextGroupData.TargetValue - currentGroupData.TargetValue;
		int purchasedCount = _curPageInfo.PurchasedCount;
		int targetValue = currentGroupData.TargetValue;
		int num2 = purchasedCount;
		checked
		{
			long num3 = unchecked((long)num2) - unchecked((long)targetValue);
			float value = (float)num3 / (float)num;
			value = Mathf.Clamp(value, 0f, 1f);
			UIHelper.SetSpriteFillAmount(Gauge_Sprite, value);
			float x = _gaugeStartX + value * (float)Gauge_Sprite.width;
			_gaugeLabelPosition.x = x;
			_gaugeLabelPosition.y = GaugePoint_Root.localPosition.y;
			_gaugeLabelPosition.z = GaugePoint_Root.localPosition.z;
			GaugePoint_Root.localPosition = _gaugeLabelPosition;
			int discountRatio = GetDiscountRatio(num, num3);
			_sbForLocalization.AppendFormat(_progressLocalizationCache, Vespa_Int_Extension_Method.ToCommaString(num3), discountRatio.ToString());
			UIHelper.SetLabelText(Gauge_Label, _sbForLocalization.ToString());
			_sbForLocalization.Length = 0;
			int limitedCount = _curPageInfo.LimitedCount;
			long num4 = limitedCount;
			long num5;
			if (_curPageInfo.PurchasedCount <= _curPageInfo.LimitedCount)
			{
				int purchasedCount2 = _curPageInfo.PurchasedCount;
				num5 = purchasedCount2;
			}
			else
			{
				num5 = num4;
			}
			long i_value = num5;
			_sbForLocalization.AppendFormat(_currentPersonnelLocalizationCache, Vespa_Int_Extension_Method.ToCommaString(i_value), Vespa_Int_Extension_Method.ToCommaString(num4));
			UIHelper.SetLabelText(CurrentPersonnel_Label, _sbForLocalization.ToString());
			Deco_Left.ForceUpdateAnchors(includeChildren: false);
			Deco_Right.ForceUpdateAnchors(includeChildren: false);
		}
	}

	private int GetDiscountRatio(int originalValue, long discountValue)
	{
		return Mathf.Clamp(Mathf.FloorToInt((float)discountValue / (float)originalValue * 100f), -100, 100);
	}

	public void RefreshDynamicUIInfo_Button()
	{
		if (_curPageInfo == null)
		{
			CloseAbnormal(EErrorCode.CurrentPageInfoIsNull);
			return;
		}
		switch (_curPageInfo.ButtonState)
		{
		case EReserveButtonState.CanPurchase:
			UIHelper.SetLabelText(ButtonPurchase_Label, _buttonCanPurchaseLocalizationCache);
			UIHelper.EnableButton(Purchase_Button, enable: true, grayscaleIfDisable: true);
			break;
		case EReserveButtonState.CannotPurchaseByEnded:
		case EReserveButtonState.CannotPurchaseBySoldOut:
			UIHelper.SetLabelText(ButtonPurchase_Label, _buttonCannotPurchaseLocalizationCache);
			UIHelper.EnableButton(Purchase_Button, enable: false, grayscaleIfDisable: true);
			break;
		case EReserveButtonState.AlreadyPurchase:
			UIHelper.SetLabelText(ButtonPurchase_Label, _buttonAlreadyPurchaseLocalizationCache);
			UIHelper.EnableButton(Purchase_Button, enable: false, grayscaleIfDisable: true);
			break;
		}
	}

	private void RefreshProductButtonStatus()
	{
		foreach (PageInfo pageInfo in _pageInfoList)
		{
			RefreshProductButtonStatus(pageInfo);
		}
	}

	private void RefreshProductButtonStatus(int productIndex, bool isRemoved = false)
	{
		PageInfo pageInfo = _pageInfoList.Find((PageInfo info) => info.ProductIndex == productIndex);
		if (pageInfo == null)
		{
			VespaDebug.LogError("page is not exist");
		}
		else
		{
			RefreshProductButtonStatus(pageInfo, isRemoved);
		}
	}

	private void RefreshProductButtonStatus(PageInfo pageInfo, bool isRemoved = false)
	{
		int productIndex = pageInfo.ProductIndex;
		PlayerProductPurchaseInfo playerProductPurchaseInfo = GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Get(productIndex);
		if (playerProductPurchaseInfo != null && playerProductPurchaseInfo.PurchasedCount > 0)
		{
			pageInfo.ButtonState = EReserveButtonState.AlreadyPurchase;
		}
		else if (isRemoved || IsProductPurchaseEnded(productIndex))
		{
			pageInfo.ButtonState = EReserveButtonState.CannotPurchaseByEnded;
		}
		else if (pageInfo.LimitedCount <= pageInfo.PurchasedCount)
		{
			pageInfo.ButtonState = EReserveButtonState.CannotPurchaseBySoldOut;
		}
		else
		{
			pageInfo.ButtonState = EReserveButtonState.CanPurchase;
		}
	}

	private bool IsProductPurchaseEnded(int productIndex)
	{
		if (GlobalAutoBehaviour<DropBonusEventManager>.instance.GetPayshopDiscountEffectiveEvent(productIndex) == null)
		{
			return true;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(productIndex) == null)
		{
			return true;
		}
		return false;
	}

	private void ChangePage(EChangePageOption changePageOption, bool isSwitchAnimationPlay)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_pageInfoList))
		{
			CloseAbnormal(EErrorCode.PageListIsNull, isForceClose: true);
			return;
		}
		int count = _pageInfoList.Count;
		if (count == 1)
		{
			ChangeProduct(_pageInfoList[0], isSwitchAnimationPlay: false);
			return;
		}
		int num = 0;
		switch (changePageOption)
		{
		case EChangePageOption.Prev:
		case EChangePageOption.Next:
			num = ((_curPageInfo != null) ? _pageInfoList.FindIndex((PageInfo info) => info.ProductIndex == _curPageInfo.ProductIndex) : 0);
			if (num == -1)
			{
				CloseAbnormal(EErrorCode.ProductIndexIsNotExist);
				break;
			}
			if (changePageOption == EChangePageOption.Prev)
			{
				num--;
			}
			if (changePageOption == EChangePageOption.Next)
			{
				num++;
			}
			num = ((num >= 0) ? (num % count) : (count - Mathf.Abs(num) % count));
			break;
		case EChangePageOption.NotPurchased:
			foreach (PageInfo pageInfo in _pageInfoList)
			{
				if (pageInfo.ButtonState == EReserveButtonState.CanPurchase)
				{
					num = _pageInfoList.FindIndex((PageInfo info) => info.ProductIndex == pageInfo.ProductIndex);
					break;
				}
			}
			break;
		}
		PageInfo pageInfo2 = _pageInfoList[num];
		ChangeProduct(pageInfo2, isSwitchAnimationPlay);
	}

	private void ChangeProduct(PageInfo pageInfo, bool isSwitchAnimationPlay)
	{
		int productIndex = pageInfo.ProductIndex;
		if (_curPageInfo == null || _curPageInfo.ProductIndex != productIndex)
		{
			_curPageInfo = pageInfo;
			UIHelper.SetBundleTexture(_curPageInfo.AnimationKeyInfo.CostumeBundleTex, _curPageInfo.PortraitPath);
			UIHelper.SetBundleTexture(_curPageInfo.AnimationKeyInfo.ShadowBundleTex, _curPageInfo.PortraitPath);
			if (isSwitchAnimationPlay)
			{
				string state = (IsFinalStep(_curPageInfo) ? _curPageInfo.AnimationKeyInfo.AnimationFinalDiscountKey : _curPageInfo.AnimationKeyInfo.AnimationSwitchingKey);
				AnimationStart(state);
			}
			RefreshPageStaticUIInfo();
			RefreshStepStaticUIInfo();
			RefreshDynamicUIInfo();
			RefreshDynamicUIInfo_Button();
		}
	}

	private void AnimationStart(string state)
	{
		if (_animationCoroutine != null)
		{
			VespaCoroutine.Stop(_animationCoroutine);
		}
		_animationCoroutine = VespaCoroutine.Start(KoStartAnimate(state));
	}

	private IEnumerator KoStartAnimate(string state)
	{
		Costume_Animator.enabled = true;
		Costume_Animator.Play(state, 0, 0f);
		yield return new WaitForSeconds(0.1f);
		AnimatorClipInfo[] currentAnimatorClipInfo = Costume_Animator.GetCurrentAnimatorClipInfo(0);
		if (__Extension_Method_ICollection__.COUNT(currentAnimatorClipInfo) != 0)
		{
			float length = currentAnimatorClipInfo[0].clip.length;
			yield return new WaitForSeconds(length);
			Costume_Animator.enabled = false;
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_isClosedAbnormal)
		{
			Close();
		}
	}

	private void CloseAbnormal(EErrorCode errorCode, bool isForceClose = false)
	{
		if (isForceClose)
		{
			_isClosedAbnormal = true;
		}
		if (!_isExistErrorPopup)
		{
			_isExistErrorPopup = true;
			string text = (((uint)errorCode > 1u && errorCode != EErrorCode.PageListIsNull) ? LocalizationManager.GetString(BaseResultType.Exception) : LocalizationManager.GetStringDirect("EQUIP_PRESET_LOAD_UNAVAILABLE"));
			text += $" ({(int)errorCode})";
			MessagePopup.OpenError(text);
		}
	}

	public override void Close()
	{
		RemoveObserver();
		_isClosedAbnormal = false;
		_isExistErrorPopup = false;
		_curPageInfo = null;
		_nextPriceNormalUILocalizationCache = null;
		_nextPriceFinalUILocalizationCache = null;
		_buttonCanPurchaseLocalizationCache = null;
		_buttonCannotPurchaseLocalizationCache = null;
		_buttonAlreadyPurchaseLocalizationCache = null;
		_currentPersonnelLocalizationCache = null;
		_nextPersonnelLocalizationCache = null;
		_endPersonnelLocalizationCache = null;
		_progressLocalizationCache = null;
		_discountLocalizationCache = null;
		_dateLocalizationCache = null;
		if (_animationCoroutine != null)
		{
			VespaCoroutine.Stop(_animationCoroutine);
			_animationCoroutine = null;
		}
		if (_pageInfoList != null)
		{
			_pageInfoList.Clear();
			_pageInfoList = null;
		}
		if (_sbForLocalization != null)
		{
			_sbForLocalization.Clear();
			_sbForLocalization = null;
		}
		base.Close();
	}

	public void OnClickReserve()
	{
		if (_curPageInfo == null)
		{
			CloseAbnormal(EErrorCode.CurrentPageInfoIsNull);
			return;
		}
		if (IsProductPurchaseEnded(_curPageInfo.ProductIndex))
		{
			MessagePopup.OpenError(_buttonCannotPurchaseLocalizationCache);
			RefreshProductButtonStatus(_curPageInfo.ProductIndex);
			RefreshDynamicUIInfo_Button();
			return;
		}
		SceneAutoBehaviour<PayShopManagement>.instance.PurchaseLegendCostumeProcess(_curPageInfo.ProductIndex, _curPageInfo.DiscountPrice, delegate
		{
			Close();
			if (SceneAutoBehaviour<CostumeManagement>.isValidInstance)
			{
				HeroCostumeView instance = SceneAutoBehaviour<CostumeManagement>.instance.GetInstance<HeroCostumeView>();
				if (instance.IsOpen())
				{
					instance.Close();
				}
			}
		}, delegate
		{
			PayShop instance = SceneAutoBehaviour<PayShopManagement>.instance.GetInstance<PayShop>();
			if (instance.IsOpen())
			{
				instance.UpdateLegendCostume();
			}
		});
	}

	public void OnClickMoveHeroCostumeView()
	{
		CostumeData dataByProductIndex = GameRoot.Instance.CostumeDataContainer.GetDataByProductIndex(_curPageInfo.ProductIndex);
		if (dataByProductIndex == null)
		{
			VespaDebug.LogError($"{_curPageInfo.ProductIndex} match costumeData does not exist.");
			return;
		}
		SceneAutoBehaviour<CostumeManagement>.instance.OpenCostume(dataByProductIndex.CostumeIndex);
		Close();
	}

	public void OnClickLeft()
	{
		ChangePage(EChangePageOption.Prev, isSwitchAnimationPlay: true);
	}

	public void OnClickRight()
	{
		ChangePage(EChangePageOption.Next, isSwitchAnimationPlay: true);
	}

	private void AddObserver()
	{
		GlobalAutoBehaviour<DropBonusEventManager>.instance.ObserverManager.AddObserver(this);
		GlobalAutoBehaviour<AccountManager>.instance.PayShopDiscountProductManager.ObserverManager.AddObserver(this);
		GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.ObserverManager.AddObserver(this);
	}

	private void RemoveObserver()
	{
		GlobalAutoBehaviour<DropBonusEventManager>.instance.ObserverManager.RemoveObserver(this);
		GlobalAutoBehaviour<AccountManager>.instance.PayShopDiscountProductManager.ObserverManager.RemoveObserver(this);
		GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.ObserverManager.RemoveObserver(this);
	}

	void IUserObserver<DropBonusEventManager.DropBonusEventChangeInfo>.OnChanged(DropBonusEventManager.DropBonusEventChangeInfo changedInfo)
	{
		if (changedInfo.bonusEvent.BonusType == EventBonusType.PayShopDiscount)
		{
			int productIndex = ParseConditionToProductIndex(changedInfo.bonusEvent.Condition);
			bool isRemoved = changedInfo.changeType == DropBonusEventManager.DropBonusEventChangeType.EventRemoved;
			RefreshProductButtonStatus(productIndex, isRemoved);
			RefreshDynamicUIInfo_Button();
		}
	}

	private int ParseConditionToProductIndex(string condition)
	{
		try
		{
			return int.Parse(condition);
		}
		catch (Exception)
		{
			VespaDebug.LogError("ProductIndex Parse Fail : " + condition);
			return -1;
		}
	}

	void IUserObserver<NGame2.NAccount.UserPayShopDiscountProductManager.ChangedPayShopDiscountProductInfo>.OnChanged(NGame2.NAccount.UserPayShopDiscountProductManager.ChangedPayShopDiscountProductInfo changedInfo)
	{
		int currentStep = _curPageInfo.CurrentStep;
		PayShopDiscountInfo[] changedInfo2 = changedInfo.changedInfo;
		foreach (PayShopDiscountInfo payShopDiscountInfo in changedInfo2)
		{
			RefreshProductInfo(payShopDiscountInfo);
			RefreshProductButtonStatus(payShopDiscountInfo.ProductIndex);
			if (payShopDiscountInfo.ProductIndex == _curPageInfo.ProductIndex)
			{
				RefreshDynamicUIInfo();
				if (currentStep != _curPageInfo.CurrentStep)
				{
					RefreshStepStaticUIInfo();
				}
			}
		}
		RefreshDynamicUIInfo_Button();
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<PlayerProductPurchaseInfo>.OnChanged(PlayerProductPurchaseInfo changedInfo)
	{
		RefreshProductButtonStatus(changedInfo.ProductIndex);
		SortPages();
		if (changedInfo.ProductIndex == _curPageInfo.ProductIndex)
		{
			RefreshDynamicUIInfo_Button();
		}
	}
}
