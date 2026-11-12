using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUtil.NStringProxy;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class InvenRandomBoxDesc : BaseWindow, IUserObserver<ItemInfo>, IUserObserver
{
	public interface Listener
	{
		void OnOpenItem(InvenRandomBoxDesc caller, long ItemCount);
	}

	public class ItemGroupData
	{
		public string code;

		public string desc;

		public List<RewardElementData> _rewardElementList_random;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Inventory/InvenRandomBoxDesc";

	public UILabel Label_ItemName;

	public UITexture Texture_Item;

	public UIButton Button_Packages;

	public UILabel Label_MoreThanTwo;

	public UIScrollView UIScrollView;

	public int MaxComponent;

	public UITable ItemGroupTableRoot;

	private Listener _listener;

	private int ItemGroupCount;

	private List<IvenItemGroupComponent> ItemGroupComponentList = new List<IvenItemGroupComponent>();

	private List<RewardElementData> RewardElementList_fixed = new List<RewardElementData>();

	private List<RewardElementData> RewardElementList_added = new List<RewardElementData>();

	private List<ItemGroupData> RewardGroupList = new List<ItemGroupData>();

	private Dictionary<string, int> ItemGroupDupCntDict = new Dictionary<string, int>();

	private CurrencyType _exceptionCurrency;

	private bool canOpenEclipsePointItem = true;

	public ItemData ItemData { get; private set; }

	public long ItemCount { get; private set; }

	public void Open(ItemData itemData, long itemCount, Listener listener)
	{
		Open(delegate
		{
			Init(itemData, itemCount, listener);
		}, null);
	}

	public void Init(ItemData itemData, long itemCount, Listener listener)
	{
		canOpenEclipsePointItem = true;
		ItemGroupTableRoot.transform.DetachChildren();
		ItemGroupTableRoot.repositionNow = true;
		VespaCoroutine.Start(RepositionUITableWithCoroutine());
		_listener = listener;
		ItemData = itemData;
		ItemCount = itemCount;
		_exceptionCurrency = CurrencyType.None;
		SetItemInfo();
		SetItemGroupComponents();
		UIScrollView.ResetPosition();
	}

	private IEnumerator RepositionUITableWithCoroutine()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		ItemGroupTableRoot.Reposition();
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(ItemGroupComponentList);
		RewardElementList_fixed.Clear();
		RewardElementList_added.Clear();
		ItemGroupDupCntDict.Clear();
		for (int i = 0; i < RewardGroupList.Count; i++)
		{
			RewardGroupList[i]._rewardElementList_random.Clear();
		}
		RewardGroupList.Clear();
	}

	public void SetItemGroupComponents()
	{
		if (RewardElementList_fixed.Count > 0)
		{
			IvenItemGroupComponent ivenItemGroupComponent = IvenItemGroupComponent.PrefabAuto.Factory.Create(ItemGroupTableRoot.transform);
			ivenItemGroupComponent.gameObject.name = string.Format("{0}{1}", "InvenItemGroupComponent", 0);
			ivenItemGroupComponent.Open(IvenItemGroupComponent.ItemGroupType.Fixed, LocalizationManager.GetStringDirect("ITEMGROUPDESC_FIXED"), ref RewardElementList_fixed);
			ItemGroupComponentList.Add(ivenItemGroupComponent);
		}
		if (RewardGroupList.Count > 0)
		{
			for (int i = 0; i < RewardGroupList.Count; i++)
			{
				string code = RewardGroupList[i].code;
				int num = ItemGroupDupCntDict[code];
				IvenItemGroupComponent ivenItemGroupComponent = IvenItemGroupComponent.PrefabAuto.Factory.Create(ItemGroupTableRoot.transform);
				ivenItemGroupComponent.gameObject.name = string.Format("{0}{1}", "InvenItemGroupComponent", RewardGroupList.Count - i);
				List<RewardElementData> _rewardElementList = RewardGroupList[i]._rewardElementList_random;
				string stringDirect = LocalizationManager.GetStringDirect(RewardGroupList[i].desc);
				stringDirect = ((!String_Extension_Method.IsNullOrEmpty(stringDirect)) ? LocalizationManager.GetStringDirect(RewardGroupList[i].desc, num) : LocalizationManager.GetStringDirect("ITEMGROUPDESC_DEFAULT"));
				ivenItemGroupComponent.Open(IvenItemGroupComponent.ItemGroupType.Random, stringDirect, ref _rewardElementList, RewardGroupList.Count - i, num);
				ItemGroupComponentList.Add(ivenItemGroupComponent);
			}
		}
		if (RewardElementList_added.Count > 0)
		{
			IvenItemGroupComponent ivenItemGroupComponent = IvenItemGroupComponent.PrefabAuto.Factory.Create(ItemGroupTableRoot.transform);
			ivenItemGroupComponent.gameObject.name = string.Format("{0}{1}", "InvenItemGroupComponent", 99999);
			ivenItemGroupComponent.Open(IvenItemGroupComponent.ItemGroupType.Added, LocalizationManager.GetStringDirect("ITEMGROUPDESC_ADDED"), ref RewardElementList_added);
			ItemGroupComponentList.Add(ivenItemGroupComponent);
		}
		ItemGroupTableRoot.repositionNow = true;
		ItemGroupTableRoot.Reposition();
	}

	private bool IsPackageItem()
	{
		if (ItemData.Type == ItemType.Package || ItemData.Type == ItemType.CinemaPackage || ItemData.Type == ItemType.SoulStonePackage)
		{
			return true;
		}
		return false;
	}

	public void SetItemInfo()
	{
		if (ItemData != null && IsPackageItem())
		{
			UIHelper.SetLabelText(Label_ItemName, ItemNameBuilder.GetItemName(ItemData));
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_Item, UIHelper.GetItemIcon2(ItemData.Code));
			SetupOpenPackageButton();
			SetupPackageObtainableItemList();
		}
	}

	private void SetupOpenPackageButton()
	{
		int num2;
		if (ItemData.Type == ItemType.Package || ItemData.Type == ItemType.SoulStonePackage)
		{
			long itemCount = ItemCount;
			int num = 1;
			num2 = ((itemCount > num) ? 1 : 0);
		}
		else
		{
			num2 = 0;
		}
		bool flag = (byte)num2 != 0;
		long maxOpenPackageItem = GetMaxOpenPackageItem();
		int num3 = 0;
		if (maxOpenPackageItem <= num3)
		{
			flag = false;
		}
		UIHelper.EnableGameObject(Button_Packages, flag);
		if (flag)
		{
			UIHelper.SetLabelText(Label_MoreThanTwo, LocalizationManager.GetString(Inventory.OPEN_PACKAGE_COUNT, maxOpenPackageItem));
		}
	}

	private void RemoveAndSortList(ref List<RewardElementData> _rewardElementList)
	{
		_rewardElementList.RemoveAll((RewardElementData x) => (EnumTools.Parse(x.ItemCode, CurrencyType.None) == CurrencyType.None && GameRoot.Instance.ItemDataContainer.GetDataByCode(x.ItemCode) == null) ? true : false);
		_rewardElementList.Sort(delegate(RewardElementData x, RewardElementData y)
		{
			if (x.ItemRate == 1000 != (y.ItemRate == 1000))
			{
				if (x.ItemRate == 1000)
				{
					return -1;
				}
				return 1;
			}
			CurrencyType currencyType = EnumTools.Parse(x.ItemCode, CurrencyType.None);
			CurrencyType currencyType2 = EnumTools.Parse(y.ItemCode, CurrencyType.None);
			if (currencyType == CurrencyType.None != (currencyType2 == CurrencyType.None))
			{
				if (currencyType != CurrencyType.None)
				{
					return -1;
				}
				return 1;
			}
			if (currencyType == CurrencyType.Gem != (currencyType2 == CurrencyType.Gem))
			{
				if (currencyType == CurrencyType.Gem)
				{
					return -1;
				}
				return 1;
			}
			if (currencyType != CurrencyType.None)
			{
				return currencyType.CompareTo(currencyType2);
			}
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(x.ItemCode);
			ItemData dataByCode2 = GameRoot.Instance.ItemDataContainer.GetDataByCode(y.ItemCode);
			ItemType type = dataByCode.Type;
			ItemType type2 = dataByCode2.Type;
			if (type != type2)
			{
				return type2.CompareTo(type);
			}
			if (dataByCode.Index != dataByCode2.Index)
			{
				return dataByCode.Index.CompareTo(dataByCode2.Index);
			}
			if (x.ItemMax != y.ItemMax)
			{
				return x.ItemMax.CompareTo(y.ItemMax);
			}
			return (x.ItemStar != y.ItemStar) ? x.ItemStar.CompareTo(y.ItemStar) : dataByCode.Grade.CompareTo(dataByCode2.Grade);
		});
	}

	private int GetRewardDataIndex()
	{
		return GameRoot.Instance.PackageItemDataContainer.GetData(ItemData.Code)?.RewardIndex1 ?? 0;
	}

	private void SetupPackageObtainableItemList()
	{
		int rewardDataIndex = GetRewardDataIndex();
		if (rewardDataIndex == 0)
		{
			return;
		}
		new List<RewardElementData>();
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(rewardDataIndex);
		if (data == null)
		{
			return;
		}
		data.GetRewardItemListWithGroupCount(out ItemGroupCount);
		List<RewardElementData> _rewardElementList = data.GetRewardOnlyItemList();
		RemoveAndSortList(ref _rewardElementList);
		Dictionary<HashString, RewardElementData> dictionary = new Dictionary<HashString, RewardElementData>();
		foreach (RewardElementData item in _rewardElementList)
		{
			if (item.ItemRate == 1000)
			{
				RewardElementList_fixed.Add(item);
				continue;
			}
			if (dictionary.ContainsKey(item.ItemCode))
			{
				RewardElementData lfR = dictionary[item.ItemCode];
				if (IsDupReward(lfR, item))
				{
					continue;
				}
			}
			else
			{
				dictionary[item.ItemCode] = item;
			}
			RewardElementList_added.Add(item);
		}
		List<RewardElementData> itemGroupElementList = data.GetItemGroupElementList();
		string text = string.Empty;
		foreach (RewardElementData item2 in itemGroupElementList)
		{
			ItemGroupData itemGroupData = new ItemGroupData();
			LocalizationManager.GetStringDirect("ITEMGROUPDESC_DEFAULT");
			int value = 1;
			itemGroupData._rewardElementList_random = NShared.ItemGroupData.ExtractItemWithGroup(item2, out itemGroupData.desc, out itemGroupData.code);
			if (!text.Equals(itemGroupData.code))
			{
				text = itemGroupData.code;
				ItemGroupDupCntDict.Add(itemGroupData.code, value);
				RemoveAndSortList(ref itemGroupData._rewardElementList_random);
				RewardGroupList.Add(itemGroupData);
			}
			else
			{
				ItemGroupDupCntDict.TryGetValue(itemGroupData.code, out value);
				value++;
				ItemGroupDupCntDict[itemGroupData.code] = value;
			}
		}
	}

	private bool IsDupReward(RewardElementData lfR, RewardElementData rtR)
	{
		if (lfR.ItemStarMin != rtR.ItemStarMin)
		{
			return false;
		}
		if (lfR.ItemStarMax != rtR.ItemStarMax)
		{
			return false;
		}
		if (lfR.ItemMin != rtR.ItemMin)
		{
			return false;
		}
		if (lfR.ItemMax != rtR.ItemMax)
		{
			return false;
		}
		return true;
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo itemInfo)
	{
		if (itemInfo != null && ItemData != null && itemInfo.ItemIndex == ItemData.Index)
		{
			OnClickExitButton();
		}
	}

	private bool CanOpenEclipsePointItem()
	{
		if (!canOpenEclipsePointItem)
		{
			string stringDirect = LocalizationManager.GetStringDirect("CONTENTS_NOT_ADD_ECLIPSE_POINT_MAX_TITLE");
			string stringDirect2 = LocalizationManager.GetStringDirect("CONTENTS_NOT_ADD_ECLIPSE_POINT_MAX_DESC");
			MessagePopup.Open(stringDirect, stringDirect2);
			return false;
		}
		return true;
	}

	private bool CanOpenShakmehPointItem()
	{
		NGame2.NUI.NManager.ShakmehDungeonManagement instance = SceneAutoBehaviour<NGame2.NUI.NManager.ShakmehDungeonManagement>.instance;
		if (instance == null)
		{
			return false;
		}
		if (instance.IsEnoughShakmehBossGauge)
		{
			string stringDirect = LocalizationManager.GetStringDirect("CONTENTS_NOT_ADD_ECLIPSE_POINT_MAX_TITLE");
			string stringDirect2 = LocalizationManager.GetStringDirect("LOCAL_SHAKMEH_MIDDLE_BOSS_POINT_GAIN_FAIL_TEXT");
			MessagePopup.Open(stringDirect, stringDirect2);
			return false;
		}
		return true;
	}

	public void OnClickOpenPackageItem()
	{
		if ((_exceptionCurrency != CurrencyType.EclipsePoint || CanOpenEclipsePointItem()) && (_exceptionCurrency != CurrencyType.ShakmehMiddleBossPoint || CanOpenShakmehPointItem()) && _listener != null)
		{
			Listener listener = _listener;
			int num = 1;
			listener.OnOpenItem(this, num);
		}
	}

	public void OnClickOpenPackages()
	{
		if ((_exceptionCurrency != CurrencyType.EclipsePoint || CanOpenEclipsePointItem()) && (_exceptionCurrency != CurrencyType.ShakmehMiddleBossPoint || CanOpenShakmehPointItem()) && _listener != null)
		{
			long maxOpenPackageItem = GetMaxOpenPackageItem();
			_listener.OnOpenItem(this, maxOpenPackageItem);
		}
	}

	public void OnClickExitButton()
	{
		Close();
	}

	public void OnClickViewButton()
	{
	}

	private long GetMaxOpenPackageItem()
	{
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxPackageItemOpen");
		long num = valueInt;
		int maxOpenCountFromPackageItemTable = GetMaxOpenCountFromPackageItemTable();
		long num2 = maxOpenCountFromPackageItemTable;
		int num3 = 0;
		if (num2 > num3)
		{
			num = num2;
		}
		int amountCurrencyInItem = GetAmountCurrencyInItem(CurrencyType.EclipsePoint);
		int amountCurrencyInItem2 = GetAmountCurrencyInItem(CurrencyType.ShakmehMiddleBossPoint);
		if (amountCurrencyInItem > 0)
		{
			long currentStamina = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(StaminaType.EclipseKey);
			long maxStamina = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetMaxStamina(StaminaType.EclipseKey);
			long num4 = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(UserManager.UserStatType.EclipsePoint);
			int exchangeCostValue = GameRoot.Instance.CurrencyTypeDataContainer.GetCurrencyData(CurrencyType.EclipsePoint).ExchangeCostValue;
			long num7;
			int num8;
			checked
			{
				long num5 = maxStamina - currentStamina;
				int num6 = exchangeCostValue;
				num7 = num5 * num6 - num4;
				num8 = amountCurrencyInItem;
			}
			long num9 = num7 / num8;
			long num10 = num9;
			int num11 = 0;
			if (num10 <= num11)
			{
				int num12 = 0;
				num9 = num12;
			}
			if (num > num9)
			{
				num = num9;
			}
			long num13 = num;
			int num14 = 0;
			if (num13 <= num14)
			{
				canOpenEclipsePointItem = false;
			}
			_exceptionCurrency = CurrencyType.EclipsePoint;
		}
		else if (amountCurrencyInItem2 > 0)
		{
			int num15 = 0;
			long num16 = num15;
			int num17 = 0;
			CurrencyTypeData data = GameRoot.Instance.CurrencyTypeDataContainer.GetData(CurrencyType.ShakmehMiddleBossPoint);
			PackageItemData data2 = GameRoot.Instance.PackageItemDataContainer.GetData(ItemData.Code);
			RewardData data3 = GameRoot.Instance.RewardDataContainer.GetData(data2.RewardIndex1);
			num16 = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(UserManager.UserStatType.ShakmehMiddleBossPoint);
			int maxValue = data.MaxValue;
			long num18 = num16;
			int num19 = maxValue;
			long num20 = checked(num19 - num18);
			num17 = data3.Item1CountMax;
			int num21 = num17;
			num = num20 / num21;
			int num22 = num17;
			long num23 = num20 % num22;
			int num24 = 0;
			if (num23 > num24)
			{
				long num25 = num;
				int num26 = 1;
				num = checked(num25 + num26);
			}
			_exceptionCurrency = CurrencyType.ShakmehMiddleBossPoint;
		}
		return System.Math.Min(num, ItemCount);
	}

	private int GetMaxOpenCountFromPackageItemTable()
	{
		if (ItemData == null)
		{
			return 0;
		}
		if (String_Extension_Method.IsNullOrEmpty(ItemData.Code))
		{
			return 0;
		}
		return GameRoot.Instance.PackageItemDataContainer.GetData(ItemData.Code)?.MaxOpenCount ?? 0;
	}

	private int GetAmountCurrencyInItem(CurrencyType currencyType)
	{
		new List<RewardElementData>();
		GameRoot.Instance.ItemDataContainer.GetData<PackageItemData>(ItemData.Code);
		int rewardDataIndex = GetRewardDataIndex();
		if (rewardDataIndex == 0)
		{
			return 0;
		}
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(rewardDataIndex);
		if (data == null)
		{
			return 0;
		}
		List<RewardElementData> rewardItemListWithGroupCount = data.GetRewardItemListWithGroupCount(out ItemGroupCount);
		int num = 0;
		foreach (RewardElementData item in rewardItemListWithGroupCount)
		{
			if (EnumTools.Parse(item.ItemCode, CurrencyType.None) == currencyType)
			{
				return num += item.ItemMax;
			}
		}
		return num;
	}
}
