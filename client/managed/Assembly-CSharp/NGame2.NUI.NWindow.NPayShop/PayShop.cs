using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NPayShopItemEvent;
using NGame2.NUI.NComponent2.NPayShop;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NPayShop;

public class PayShop : LobbyFullWindow, PayShopResourceList.IListener, PayShopCategoryList.IListener, PayShopTagList.IListener, PayShopBaseTabList.IListener, PayShopBaseList.IListener, PayShopHotAndNewList.IListener, PayShopProductList.IListener, IUserObserver<UserManager.UserStatInfo>, IUserObserver, IUserObserver<NGame2.NAccount.UserFreeEquipGachaManager.ChangedGachaInfo>, IUserObserver<NGame2.NAccount.UserEquipGachaManager.ChangedGachaInfo>, IUserObserver<PayShopItemEventManager.EventChangedInfo>, IUserObserver<DropBonusEventManager.DropBonusEventChangeInfo>, IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>, IUserObserver<CostumeInfo>, IUserObserver<NGame2.NAccount.UserPlayerCurrencyManager.ChangedCurrencyInfo>, IUserObserver<NGame2.NAccount.UserPayShopDiscountProductManager.ChangedPayShopDiscountProductInfo>
{
	public interface IListener
	{
		void OnClickProduct(int productIndex);

		void OnClickProduct(int productIndex, int comboIndex);

		void OnClickHero(int heroIndex, List<CreatureData> creatureDatas);

		void OnClickCostume(int costumeIndex);

		void OnClickAccessory(int accessoryIndex);

		void OnClickEquipGacha(int gachaIndex);

		void OnClickBuyReservation(int productIndex, int discountPrice);

		void OnClickOpenReservationPopup(int productIndex);
	}

	[Serializable]
	public class PayShopListClass
	{
		public string Category;

		public PayShopBaseList PayShopList;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/PayShop/PayShop";

	public PayShopResourceList PayShopResourceList;

	public PayShopCategoryList CategoryListGroup;

	public PayShopTagList PayShopTagList;

	public PayShopProductList PayShopProductList;

	[SerializeField]
	public List<PayShopListClass> PayShopLists = new List<PayShopListClass>();

	public OpenAnimationMasterController OpenAnimationMasterController;

	public static string[] NoTagCategoryList = new string[5]
	{
		NewPayShopCategoryType.HotNew.ToString(),
		NewPayShopCategoryType.SpecialGacha.ToString(),
		NewPayShopCategoryType.NormalGacha.ToString(),
		NewPayShopCategoryType.PetGacha.ToString(),
		NewPayShopCategoryType.Accessory.ToString()
	};

	private string _currentCategory;

	private int _selectedTag;

	protected float UpdateBeginTime;

	private PayShopBaseList _selectedPayShopList;

	private IListener _listener;

	private Action _closeAction;

	protected float UpdateInterval => 1f;

	public void Open(IListener listener, Action closeAction = null)
	{
		SprkShopSearch.BeforeOpen(this);
		Open();
		_listener = listener;
		_closeAction = closeAction;
		PayShopResourceList.Open(this);
		CategoryListGroup.Open(this);
		PayShopTagList.Open(this);
		PayShopProductList.Open(this);
		UIHelper.EnableGameObject(PayShopProductList, enable: false);
		foreach (PayShopListClass payShopList in PayShopLists)
		{
			payShopList.PayShopList.Open(this);
			UIHelper.EnableGameObject(payShopList.PayShopList, enable: false);
		}
		UpdateCategoryFlag();
		GlobalAutoBehaviour<AccountManager>.instance.PayShopDiscountProductManager.ObserverManager.AddObserver(this);
		SprkShopSearch.AfterOpen(this);
	}

	public void PlayOpenAnimation()
	{
		OpenAnimationMasterController.Play();
	}

	public override void Reset()
	{
		base.Reset();
		_currentCategory = "";
		_selectedPayShopList = null;
		PayShopResourceList.ClearAll();
		CategoryListGroup.ClearAll();
		PayShopTagList.ClearAll();
		PayShopProductList.ClearAll();
		foreach (PayShopListClass payShopList in PayShopLists)
		{
			payShopList.PayShopList.ClearAll();
		}
	}

	public override void OnSystemBackButton()
	{
		base.OnSystemBackButton();
		if (_closeAction != null)
		{
			Vespa_System_Action_Extension_Method.fire(_closeAction);
		}
	}

	public override void Close()
	{
		base.Close();
		GlobalAutoBehaviour<AccountManager>.instance.PayShopDiscountProductManager.ObserverManager.RemoveObserver(this);
	}

	private PayShopBaseList GetPayShopList(string category)
	{
		return PayShopLists.Find((PayShopListClass list) => list.Category == category)?.PayShopList;
	}

	void PayShopCategoryList.IListener.SelectCategory(string category)
	{
		PayShopTagList.ForceAllTagPanelClose();
		ChangeTopResourceInfo(category);
		if (_currentCategory == category)
		{
			ReCreateList();
		}
		else
		{
			SelectCategory(category);
		}
	}

	private void ChangeTopResourceInfo(string category)
	{
		NewPayShopGroupData data = GameRoot.Instance.NewPayShopGroupDataContainer.GetData(category);
		if (data != null)
		{
			CurrencyType linkCurrency = data.LinkCurrency;
			if (linkCurrency != CurrencyType.None)
			{
				PayShopResourceList.ChangeSwitchCurrency(linkCurrency);
			}
			else
			{
				PayShopResourceList.ChangeSwitchCurrency();
			}
		}
	}

	private bool IsNoTagList(string category)
	{
		return NoTagCategoryList.Contains(category);
	}

	public void SelectCategory(string category, bool forceSelect = false)
	{
		if (forceSelect)
		{
			CategoryListGroup.SelectCategory(category);
		}
		if (_selectedTag > 0)
		{
			if (IsNoTagList(_currentCategory))
			{
				ShowPayShopList(PayShopProductList, isShow: false);
				ShowPayShopList(_selectedPayShopList, isShow: true);
			}
			_selectedTag = 0;
			PayShopTagList.SelectCategory(_currentCategory);
		}
		NewPayShopGroupData data = GameRoot.Instance.NewPayShopGroupDataContainer.GetData(category.ToString());
		if (data == null)
		{
			return;
		}
		if (data.IsCustomize)
		{
			if (IsGachaCategory(category))
			{
				PayShopBaseList payShopList = GetPayShopList(category);
				ChangePayShopList(payShopList);
				(payShopList as PayShopEquipGachaList).CreateList(category);
			}
			else
			{
				PayShopBaseList payShopBaseList = GetPayShopList(category);
				if (payShopBaseList == null)
				{
					payShopBaseList = PayShopProductList;
				}
				ChangePayShopList(payShopBaseList);
				payShopBaseList.CreateList();
			}
		}
		else
		{
			ChangePayShopList(PayShopProductList);
			PayShopProductList.CreateList(category);
		}
		PayShopTagList.SelectCategory(category);
		_currentCategory = category;
	}

	private void ChangePayShopList(PayShopBaseList productList)
	{
		if (_selectedPayShopList != productList)
		{
			if (_selectedPayShopList != null)
			{
				_selectedPayShopList.ClearList();
			}
			ShowPayShopList(_selectedPayShopList, isShow: false);
			ShowPayShopList(productList, isShow: true);
			_selectedPayShopList = productList;
		}
		else if (_selectedPayShopList != null)
		{
			_selectedPayShopList.ClearList();
		}
	}

	private void ShowPayShopList(PayShopBaseList list, bool isShow)
	{
		UIHelper.EnableGameObject(list, isShow);
	}

	public void UpdateCategoryFlag()
	{
		foreach (NewPayShopGroupData item in from data in GameRoot.Instance.NewPayShopGroupDataContainer.GetAllData()
			where String_Extension_Method.IsNullOrEmpty(data.ParentCategory)
			select data)
		{
			if (item.IsEnable)
			{
				UpdateCategoryFlag(item.PayShopCategoryType);
			}
		}
	}

	public void UpdateCategoryFlag(string category)
	{
		List<NewPayShopGroupData> list = (from data in GameRoot.Instance.NewPayShopGroupDataContainer.GetAllData()
			where data.ParentCategory == category
			select data).ToList();
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			PayShopFlagType payShopFlagType = PayShopFlagType.None;
			foreach (NewPayShopGroupData item in list)
			{
				if (item.IsEnable)
				{
					PayShopFlagType payShopFlagType2 = CheckCategoryFlag(item.PayShopCategoryType);
					payShopFlagType |= payShopFlagType2;
					CategoryListGroup.SetCategoryFlag(item.PayShopCategoryType, payShopFlagType2);
				}
			}
			CategoryListGroup.SetCategoryFlag(category, payShopFlagType);
		}
		else
		{
			PayShopFlagType flag = CheckCategoryFlag(category);
			CategoryListGroup.SetCategoryFlag(category, flag);
		}
	}

	public void UpdateMainCategoryFlag(string category)
	{
		NewPayShopGroupData data = GameRoot.Instance.NewPayShopGroupDataContainer.GetData(category);
		if (String_Extension_Method.IsNullOrEmpty(data.ParentCategory))
		{
			UpdateCategoryFlag(category);
		}
		else
		{
			UpdateCategoryFlag(data.ParentCategory);
		}
	}

	public void OnClickClose()
	{
		Close();
		if (_closeAction != null)
		{
			Vespa_System_Action_Extension_Method.fire(_closeAction);
		}
	}

	void PayShopTagList.IListener.OnClickTag(int tag)
	{
		if (IsNoTagList(_currentCategory))
		{
			PayShopBaseList payShopList = GetPayShopList(_currentCategory);
			ShowPayShopList(payShopList, isShow: false);
			ShowPayShopList(PayShopProductList, isShow: true);
			PayShopProductList.SelectTag(tag);
		}
		else
		{
			_selectedPayShopList.SelectTag(tag);
		}
		_selectedTag = tag;
	}

	void PayShopTagList.IListener.OnClickClose()
	{
		if (IsNoTagList(_currentCategory))
		{
			PayShopBaseList payShopList = GetPayShopList(_currentCategory);
			ShowPayShopList(payShopList, isShow: true);
			ShowPayShopList(PayShopProductList, isShow: false);
		}
		else
		{
			_selectedPayShopList.ClearTag();
		}
		_selectedTag = 0;
	}

	void PayShopBaseTabList.IListener.OnClickTab()
	{
		if (PayShopTagList.SelectedTagIndex != 0)
		{
			PayShopTagList.CloseSeletedTag();
		}
	}

	void PayShopBaseList.IListener.OnClickProduct(PayShopIndexType type, int index)
	{
		if (_listener != null)
		{
			switch (type)
			{
			case PayShopIndexType.PayShop:
				_listener.OnClickProduct(index);
				break;
			case PayShopIndexType.Hero:
			{
				PayShopHeroList payShopHeroList = GetPayShopList(NewPayShopCategoryType.Hero.ToString()) as PayShopHeroList;
				_listener.OnClickHero(index, payShopHeroList.Datas);
				break;
			}
			case PayShopIndexType.Costume:
				_listener.OnClickCostume(index);
				break;
			case PayShopIndexType.Accessory:
				_listener.OnClickAccessory(index);
				break;
			case PayShopIndexType.EquipGacha:
				_listener.OnClickEquipGacha(index);
				break;
			}
		}
	}

	void PayShopBaseList.IListener.OnClickOpenDiscountInfo(int productIndex)
	{
		_listener.OnClickOpenReservationPopup(productIndex);
	}

	void PayShopBaseList.IListener.OnClickBuyDiscountProduct(int productIndex, int discountPrice)
	{
		_listener.OnClickBuyReservation(productIndex, discountPrice);
	}

	void PayShopHotAndNewList.IListener.OnClickBanner(int bannerIndex)
	{
		NewPayShopBannerData data = GameRoot.Instance.NewPayShopBannerDataContainer.GetData(bannerIndex);
		if (data == null)
		{
			return;
		}
		if (!String_Extension_Method.IsNullOrEmpty(data.MoveCategory))
		{
			SelectCategory(data.MoveCategory, forceSelect: true);
		}
		else if (data.OpenProductIndex > 0)
		{
			if (!GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.ContainKey(data.OpenProductIndex))
			{
				VespaDebug.LogError("product == null : " + data.OpenProductIndex);
			}
			else
			{
				((PayShopBaseList.IListener)this).OnClickProduct(PayShopIndexType.PayShop, data.OpenProductIndex);
			}
		}
	}

	public static PayShopFlagType CheckCategoryFlag(string category)
	{
		if (category == NewPayShopCategoryType.Hero.ToString())
		{
			return PayShopHeroList.CheckCategoryFlag();
		}
		if (category == NewPayShopCategoryType.Costume.ToString())
		{
			return PayShopCostumeList.CheckCategoryFlag(NewPayShopCategoryType.Costume);
		}
		if (category == NewPayShopCategoryType.LegendCostume.ToString())
		{
			return PayShopCostumeList.CheckCategoryFlag(NewPayShopCategoryType.LegendCostume);
		}
		if (category == NewPayShopCategoryType.Accessory.ToString())
		{
			return PayShopAccessoryList.CheckCategoryFlag();
		}
		if (IsGachaCategory(category))
		{
			return PayShopEquipGachaList.CheckCategoryFlag(category);
		}
		if (!(category == NewPayShopCategoryType.HotNew.ToString()))
		{
			return PayShopProductList.CheckCategoryFlag(category);
		}
		return PayShopFlagType.None;
	}

	void PayShopResourceList.IListener.OnClickStamina()
	{
		SelectCategory(NewPayShopCategoryType.Stamina.ToString(), forceSelect: true);
	}

	void PayShopResourceList.IListener.OnClickRuby()
	{
		SelectCategory(NewPayShopCategoryType.Ruby.ToString(), forceSelect: true);
	}

	void PayShopResourceList.IListener.OnClickGold()
	{
		SelectCategory(NewPayShopCategoryType.Gold.ToString(), forceSelect: true);
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo changedInfo)
	{
		if (changedInfo.UserStatType == UserManager.UserStatType.InventoryExtend)
		{
			if (_currentCategory == NewPayShopCategoryType.Convenience.ToString())
			{
				_selectedPayShopList.Refresh();
			}
		}
		else
		{
			PayShopResourceList.OnChanged(changedInfo);
		}
	}

	void IUserObserver<NGame2.NAccount.UserPlayerCurrencyManager.ChangedCurrencyInfo>.OnChanged(NGame2.NAccount.UserPlayerCurrencyManager.ChangedCurrencyInfo changedInfo)
	{
		PayShopResourceList.OnChanged(changedInfo);
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<NGame2.NAccount.UserFreeEquipGachaManager.ChangedGachaInfo>.OnChanged(NGame2.NAccount.UserFreeEquipGachaManager.ChangedGachaInfo changedInfo)
	{
	}

	void IUserObserver<NGame2.NAccount.UserEquipGachaManager.ChangedGachaInfo>.OnChanged(NGame2.NAccount.UserEquipGachaManager.ChangedGachaInfo changedInfo)
	{
	}

	void IUserObserver<PayShopItemEventManager.EventChangedInfo>.OnChanged(PayShopItemEventManager.EventChangedInfo changedInfo)
	{
		if (!(_selectedPayShopList == null))
		{
			_selectedPayShopList.Refresh();
		}
	}

	void IUserObserver<DropBonusEventManager.DropBonusEventChangeInfo>.OnChanged(DropBonusEventManager.DropBonusEventChangeInfo changedInfo)
	{
		DropBonusEvent bonusEvent = changedInfo.bonusEvent;
		if (DropBonusEventManager.IsGachaEventType(bonusEvent.BonusType) && !String_Extension_Method.IsNullOrEmpty(_currentCategory) && IsGachaCategory(_currentCategory))
		{
			DropBonusEventManager.GetGachaEventConditionPropValueInt(bonusEvent, "GachaIndex");
		}
		if (bonusEvent.BonusType == EventBonusType.PayShopDiscount && changedInfo.changeType == DropBonusEventManager.DropBonusEventChangeType.EventRemoved && _currentCategory == NewPayShopCategoryType.LegendCostume.ToString())
		{
			PayShopBaseList payShopList = GetPayShopList(NewPayShopCategoryType.LegendCostume.ToString());
			if (payShopList != null)
			{
				(payShopList as PayShopCostumeList).UpdateByDropBonusEventCloseChangeInfo();
			}
		}
	}

	void IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>.OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo changedInfo)
	{
		if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.Costume)
		{
			if (!(_currentCategory != NewPayShopCategoryType.Costume.ToString()))
			{
				PayShopBaseList selectedPayShopList = _selectedPayShopList;
				if (selectedPayShopList != null)
				{
					(selectedPayShopList as PayShopCostumeList).UpdateProduct(changedInfo.HeroIndex);
				}
			}
		}
		else if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.AddedHero && !(_currentCategory != NewPayShopCategoryType.Hero.ToString()))
		{
			_selectedPayShopList.ReCreateList();
		}
	}

	void IUserObserver<CostumeInfo>.OnChanged(CostumeInfo changedInfo)
	{
		PayShopBaseList payShopList = GetPayShopList(NewPayShopCategoryType.Costume.ToString());
		if (payShopList != null)
		{
			(payShopList as PayShopCostumeList).UpdateProduct(changedInfo);
		}
		UpdateLegendCostume();
	}

	void IUserObserver<NGame2.NAccount.UserPayShopDiscountProductManager.ChangedPayShopDiscountProductInfo>.OnChanged(NGame2.NAccount.UserPayShopDiscountProductManager.ChangedPayShopDiscountProductInfo changedInfo)
	{
		UpdateLegendCostume();
	}

	public void UpdateLegendCostume()
	{
		if (!(_currentCategory != NewPayShopCategoryType.LegendCostume.ToString()))
		{
			PayShopBaseList payShopList = GetPayShopList(NewPayShopCategoryType.LegendCostume.ToString());
			if (payShopList != null)
			{
				(payShopList as PayShopCostumeList).UpdateLegendCostume();
			}
		}
	}

	public void ScrollToTargetProduct(int productIndex)
	{
		NewPayShopProductInfo product = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.GetProduct(productIndex);
		if (product != null)
		{
			SelectCategory(product.Category, forceSelect: true);
		}
	}

	public void ReCreateList()
	{
		if (_selectedPayShopList == null)
		{
			return;
		}
		if (_selectedTag > 0)
		{
			if (IsNoTagList(_currentCategory))
			{
				ShowPayShopList(PayShopProductList, isShow: false);
				ShowPayShopList(_selectedPayShopList, isShow: true);
			}
			_selectedTag = 0;
			PayShopTagList.SelectCategory(_currentCategory);
		}
		_selectedPayShopList.ReCreateList();
	}

	public void Refresh()
	{
		if (!(_selectedPayShopList == null))
		{
			_selectedPayShopList.Refresh();
		}
	}

	public void UpdateProduct(NewPayShopProductInfo info)
	{
		if (_selectedTag > 0 && IsNoTagList(_currentCategory))
		{
			PayShopProductList.UpdateProduct(info.Index);
			SprkSpecialShop.AfterPurchase(this, info);
		}
		else
		{
			_selectedPayShopList.UpdateProduct(info.Index);
			SprkSpecialShop.AfterPurchase(this, info);
		}
	}

	void PayShopProductList.IListener.OnClickProduct(int originalIndex, int comboIndex)
	{
		if (_listener != null)
		{
			_listener.OnClickProduct(originalIndex, comboIndex);
		}
	}

	public override void UpdateDerived()
	{
		if (IsUpdate())
		{
			IntervalUpdateDerived();
		}
		if (_selectedPayShopList == null)
		{
			SprkSpecialShop.Tick(this);
			return;
		}
		_selectedPayShopList.UpdateDerived();
		SprkSpecialShop.Tick(this);
	}

	private void IntervalUpdateDerived()
	{
		if (IsGachaCategory(_currentCategory))
		{
			UpdateGachaFlag();
		}
	}

	private bool IsUpdate()
	{
		if (UpdateInterval <= 0f)
		{
			return true;
		}
		if (UpdateBeginTime == 0f)
		{
			UpdateBeginTime = Time.time;
		}
		if (Time.time - UpdateBeginTime >= UpdateInterval)
		{
			UpdateBeginTime -= Time.time;
			return true;
		}
		return false;
	}

	public void UpdateGachaFlag()
	{
		UpdateCategoryFlag(NewPayShopCategoryType.Gacha.ToString());
	}

	private static bool IsGachaCategory(string category)
	{
		if (category == NewPayShopCategoryType.NormalGacha.ToString() || category == NewPayShopCategoryType.SpecialGacha.ToString() || category == NewPayShopCategoryType.PetGacha.ToString())
		{
			return true;
		}
		return false;
	}
}
