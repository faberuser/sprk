using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NHelper.NPayShop;
using NGame2.NUtil.NCostume;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopCostumeList : PayShopBaseTabList<CreatureData, PayShopCostumeHeroComponent, PayShopHeroClassTabComponent, CreatureTagType>, IPayShopProductList, PayShopCostumeComponent.IListener
{
	public UIScrollGrid SelectedScrollGrid;

	public EHeroCostumeType HeroCostumeType = EHeroCostumeType.Normal;

	private PayShopCostumeHeroComponent _selectedCostumeHeroComponent;

	private List<PayShopCostumeComponent> _selectedCostumeComponentList = new List<PayShopCostumeComponent>();

	private int _selectedCreatureIndex;

	public override PayShopIndexType PayShopIndexType => PayShopIndexType.Costume;

	public override void CreateList()
	{
		CreateList(CreatureTagType.None);
	}

	public override List<CreatureData> GetDataList(CreatureTagType classTag = CreatureTagType.None)
	{
		if (classTag == CreatureTagType.None)
		{
			return GameRoot.Instance.CreatureDataContainer.GetOnlyHeroList();
		}
		return GameRoot.Instance.CreatureDataContainer.GetListByTagType(classTag);
	}

	protected override List<CreatureData> Filtering(List<CreatureData> datas)
	{
		List<CreatureData> list = new List<CreatureData>();
		list.AddRange(datas);
		list.RemoveAll((CreatureData data) => __Extension_Method_ICollection__.IsNullOrEmpty(PayShopHelper.GetBuyableCostumeByHero(data.Index, HeroCostumeType)));
		return list;
	}

	protected override PrefabAuto<PayShopHeroClassTabComponent> GetTabPrefabAuto()
	{
		return PayShopHeroClassTabComponent.PrefabAuto;
	}

	protected override PrefabAuto<PayShopCostumeHeroComponent> GetComponentPrefabAuto()
	{
		return PayShopCostumeHeroComponent.PrefabAuto;
	}

	protected override int Sort(CreatureData a, CreatureData b)
	{
		int num = 0;
		CostumeGroupData lastCostumeData = PayShopHelper.GetLastCostumeData(a.Index, HeroCostumeType);
		CostumeGroupData lastCostumeData2 = PayShopHelper.GetLastCostumeData(b.Index, HeroCostumeType);
		num = -lastCostumeData.CostumeGroupIndex.CompareTo(lastCostumeData2.CostumeGroupIndex);
		if (num != 0)
		{
			return num;
		}
		bool flag = PayShopHelper.NewCostumeExists(a.Index, HeroCostumeType);
		bool value = PayShopHelper.NewCostumeExists(b.Index, HeroCostumeType);
		num = -flag.CompareTo(value);
		if (num != 0)
		{
			return num;
		}
		return -a.Index.CompareTo(b.Index);
	}

	private int CostumeSort(CostumeData a, CostumeData b)
	{
		int num = 0;
		num = -a.New.CompareTo(b.New);
		if (num != 0)
		{
			return num;
		}
		bool flag = BaseCostume.GetPriceDiscountPercentage(a) == 0;
		bool value = BaseCostume.GetPriceDiscountPercentage(b) == 0;
		num = flag.CompareTo(value);
		if (num != 0)
		{
			return num;
		}
		bool flag2 = GlobalAutoBehaviour<AccountManager>.instance.CostumeManager.Get(a.CostumeIndex) != null;
		bool value2 = GlobalAutoBehaviour<AccountManager>.instance.CostumeManager.Get(b.CostumeIndex) != null;
		num = flag2.CompareTo(value2);
		if (num != 0)
		{
			return num;
		}
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(a.HeroIndex);
		CreatureData data2 = GameRoot.Instance.CreatureDataContainer.GetData(b.HeroIndex);
		num = -data.OpenDate.CompareTo(data2.OpenDate);
		if (num != 0)
		{
			return num;
		}
		num = -a.HeroIndex.CompareTo(b.HeroIndex);
		if (num != 0)
		{
			return num;
		}
		return -a.CostumeIndex.CompareTo(b.CostumeIndex);
	}

	protected override CreatureData GetData(int index)
	{
		return GameRoot.Instance.CreatureDataContainer.GetData(index);
	}

	protected override void OnClickProduct(int index)
	{
		if (_selectedCreatureIndex == index)
		{
			CloseSelectedList();
		}
		else
		{
			OpenSelectedList(index);
		}
	}

	private void OpenSelectedList(int creatureIndex)
	{
		UIHelper.EnableGameObject(ScrollGrid, enable: false);
		UIHelper.EnableGameObject(SelectedScrollGrid, enable: true);
		_selectedCreatureIndex = creatureIndex;
		_selectedCostumeHeroComponent = UIHelper.AddGridItem(SelectedScrollGrid, PayShopCostumeHeroComponent.PrefabAuto);
		if (_selectedCostumeHeroComponent != null)
		{
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(creatureIndex);
			_selectedCostumeHeroComponent.Open(data, this);
			_selectedCostumeHeroComponent.InitHeroCostumeType(HeroCostumeType);
		}
		CostumeData[] buyableCostumeByHero = PayShopHelper.GetBuyableCostumeByHero(creatureIndex, HeroCostumeType);
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(buyableCostumeByHero))
		{
			Array.Sort(buyableCostumeByHero, CostumeSort);
			CostumeData[] array = buyableCostumeByHero;
			foreach (CostumeData costumeData in array)
			{
				PayShopCostumeComponent payShopCostumeComponent = UIHelper.AddGridItem(SelectedScrollGrid, PayShopCostumeComponent.PrefabAuto);
				payShopCostumeComponent.Open(costumeData, this);
				_selectedCostumeComponentList.Add(payShopCostumeComponent);
			}
			UIHelper.RepositionGrid(SelectedScrollGrid);
		}
	}

	public override void CreateList(CreatureTagType category)
	{
		if (_selectedCreatureIndex != 0)
		{
			CloseSelectedList();
		}
		base.CreateList(category);
	}

	protected override void OnChangedItem(PayShopCostumeHeroComponent comp, CreatureData data)
	{
		base.OnChangedItem(comp, data);
		comp.InitHeroCostumeType(HeroCostumeType);
	}

	protected void CloseSelectedList()
	{
		UIHelper.EnableGameObject(ScrollGrid, enable: true);
		UIHelper.EnableGameObject(SelectedScrollGrid, enable: false);
		ReleaseSelectedComponent();
		_selectedCreatureIndex = 0;
	}

	protected override void ReleaseComponents()
	{
		base.ReleaseComponents();
		ReleaseSelectedComponent();
	}

	private void ReleaseSelectedComponent()
	{
		FactoryBase.ReleaseAndClearList(_selectedCostumeComponentList);
		FactoryBase.SafeReleaseComponent(ref _selectedCostumeHeroComponent);
	}

	public override void ClearList()
	{
		base.ClearList();
		CloseSelectedList();
	}

	void PayShopCostumeComponent.IListener.OnClickCostume(int CostumeIndex)
	{
		if (_listener != null)
		{
			_listener.OnClickProduct(PayShopIndexType, CostumeIndex);
		}
	}

	void PayShopCostumeComponent.IListener.OnClickOpenDiscountInfo(int productIndex)
	{
		if (_listener != null)
		{
			_listener.OnClickOpenDiscountInfo(productIndex);
		}
	}

	void PayShopCostumeComponent.IListener.OnClickBuyDiscountProduct(int productIndex, int discountPrice)
	{
		if (_listener != null)
		{
			_listener.OnClickBuyDiscountProduct(productIndex, discountPrice);
		}
	}

	public static PayShopFlagType CheckCategoryFlag(NewPayShopCategoryType payShopCategoryType)
	{
		PayShopCategoryComponent.NewFlagCheckClass newFlagCheckClass = PayShopCategoryComponent.LoadNewFlagCheckClass(payShopCategoryType.ToString());
		bool isLegendCostumeCategory = payShopCategoryType == NewPayShopCategoryType.LegendCostume;
		PayShopFlagType payShopFlagType = PayShopFlagType.None;
		foreach (CostumeData item in (from data in GameRoot.Instance.CostumeDataContainer.GetAllData()
			where data.IsBuy && !data.IsBonusCostume() && !data.IsFreeCostume() && data.IsLegendCostume() == isLegendCostumeCategory
			select data).ToList())
		{
			if (item.New == 1)
			{
				if (newFlagCheckClass.NewFlagConfirmValue == 0)
				{
					DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
					newFlagCheckClass.InitNewFlagConfirmValue(currentServerLocalTime);
					continue;
				}
				if (newFlagCheckClass.IsNew(item.OpenDate))
				{
					payShopFlagType |= PayShopFlagType.New;
					newFlagCheckClass.NewFlagValue = item.OpenDate;
				}
			}
			if (BaseCostume.GetPriceDiscountPercentage(item) > 0)
			{
				payShopFlagType |= PayShopFlagType.Sale;
			}
		}
		PayShopCategoryComponent.SaveNewFlagCheckClass(payShopCategoryType.ToString(), newFlagCheckClass);
		return payShopFlagType;
	}

	public void UpdateProduct(CostumeInfo changedInfo)
	{
		foreach (PayShopCostumeComponent selectedCostumeComponent in _selectedCostumeComponentList)
		{
			if (selectedCostumeComponent.CostumeData.CostumeIndex == changedInfo.CostumeIndex)
			{
				selectedCostumeComponent.Refresh();
				break;
			}
		}
	}

	public void UpdateLegendCostume()
	{
		foreach (PayShopCostumeComponent selectedCostumeComponent in _selectedCostumeComponentList)
		{
			if (selectedCostumeComponent.CostumeData.ProductIndex != 0)
			{
				selectedCostumeComponent.Refresh();
				break;
			}
		}
	}

	public void UpdateByDropBonusEventCloseChangeInfo()
	{
		if (HeroCostumeType != EHeroCostumeType.Normal)
		{
			_selectedCreatureIndex = 0;
			CreateList(CreatureTagType.None);
		}
	}

	public new void UpdateProduct(int heroIndex)
	{
		foreach (PayShopCostumeComponent selectedCostumeComponent in _selectedCostumeComponentList)
		{
			selectedCostumeComponent.Refresh();
		}
	}

	public override void CreateList(int tagIndex)
	{
		NewPayShopTagData data = GameRoot.Instance.NewPayShopTagDataContainer.GetData(tagIndex);
		if (data == null || data.IndexType != PayShopIndexType)
		{
			return;
		}
		if (data.IndexType != PayShopIndexType)
		{
			ReleaseComponents();
			return;
		}
		List<CostumeData> list = new List<CostumeData>();
		int[] indices = data.Indices;
		foreach (int key in indices)
		{
			CostumeGroupData data2 = GameRoot.Instance.CostumeGroupDataContainer.GetData(key);
			if (data2 != null)
			{
				IList<CostumeData> dataByGroup = GameRoot.Instance.CostumeDataContainer.GetDataByGroup(data2.GroupName);
				if (__Extension_Method_ICollection__.COUNT(dataByGroup) != 0)
				{
					list.AddRange(dataByGroup);
				}
			}
		}
		CreateComponentList(list);
	}

	private void CreateComponentList(List<CostumeData> costumeList)
	{
		ReleaseComponents();
		if (_selectedCreatureIndex != 0)
		{
			CloseSelectedList();
		}
		_selectedCreatureIndex = -1;
		UIHelper.EnableGameObject(ScrollGrid, enable: false);
		UIHelper.EnableGameObject(SelectedScrollGrid, enable: true);
		foreach (CostumeData costume in costumeList)
		{
			if ((!costume.IsCoupleCostume() || !costume.IsBonusCostume()) && (costume.CostumeType != EHeroCostumeType.Legend || (costume.ProductIndex != 0 && GlobalAutoBehaviour<AccountManager>.instance.PayShopDiscountProductManager.Get(costume.ProductIndex) != null && GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(costume.ProductIndex) != null && GlobalAutoBehaviour<DropBonusEventManager>.instance.GetPayshopDiscountEffectiveEvent(costume.ProductIndex) != null)))
			{
				PayShopCostumeComponent payShopCostumeComponent = UIHelper.AddGridItem(SelectedScrollGrid, PayShopCostumeComponent.PrefabAuto);
				payShopCostumeComponent.Open(costume, this);
				_selectedCostumeComponentList.Add(payShopCostumeComponent);
			}
		}
		UIHelper.RepositionGrid(SelectedScrollGrid);
	}
}
