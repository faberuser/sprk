using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUtil.NPayShop;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopEquipGachaList : PayShopBaseFrameList<NewEquipGachaData, PayShopEquipGachaComponent, PayShopEquipGachaFrameComponent, string>, PayShopBaseComponent.IListener, IPayShopProductList
{
	public UIWidget NoPetRoot;

	public override PayShopIndexType PayShopIndexType => PayShopIndexType.EquipGacha;

	public override void CreateList()
	{
		CreateList(NewPayShopCategoryType.NormalGacha.ToString());
	}

	public override List<NewEquipGachaData> GetRealDataList(string category)
	{
		return GetDatasByCategory(category);
	}

	public static List<NewEquipGachaData> GetDatasByCategory(string category)
	{
		IList<NewEquipGachaData> datasByCategory = GameRoot.Instance.NewEquipGachaDataContainer.GetDatasByCategory(category);
		List<NewEquipGachaData> list = new List<NewEquipGachaData>();
		list.AddRange(datasByCategory);
		return list;
	}

	protected override List<NewEquipGachaData> Filtering(List<NewEquipGachaData> datas)
	{
		return Filter(datas);
	}

	public static List<NewEquipGachaData> Filter(List<NewEquipGachaData> datas)
	{
		List<NewEquipGachaData> list = new List<NewEquipGachaData>();
		foreach (NewEquipGachaData data in datas)
		{
			if (UIHelper.IsEnableGacha(data.Index))
			{
				list.Add(data);
			}
		}
		return list;
	}

	protected override PrefabAuto<PayShopEquipGachaFrameComponent> GetComponentPrefabAuto()
	{
		return PayShopEquipGachaFrameComponent.PrefabAuto;
	}

	protected override void Sort(List<NewEquipGachaData> productInfos, int[] sortIndices = null)
	{
		productInfos.Sort((NewEquipGachaData a, NewEquipGachaData b) => a.GachaShowIndex.CompareTo(b.GachaShowIndex));
	}

	protected override void ReleaseComponents()
	{
		base.ReleaseComponents();
	}

	public override void UpdateProduct(int index)
	{
		_ = FindComponent(index) == null;
	}

	protected override void OnEndCreateList(bool isEmptyList)
	{
		bool flag = base.SelectedCategory == NewPayShopCategoryType.PetGacha.ToString();
		UIHelper.EnableGameObject(NoPetRoot, flag && isEmptyList);
	}

	private new PayShopEquipGachaComponent FindComponent(int index)
	{
		foreach (PayShopEquipGachaFrameComponent component in _componentList)
		{
			PayShopEquipGachaComponent payShopEquipGachaComponent = component.FindComponent(index);
			if (payShopEquipGachaComponent != null)
			{
				return payShopEquipGachaComponent;
			}
		}
		return null;
	}

	public static PayShopFlagType CheckCategoryFlag(string category)
	{
		PayShopCategoryComponent.NewFlagCheckClass newFlagCheckClass = PayShopCategoryComponent.LoadNewFlagCheckClass(category);
		PayShopFlagType payShopFlagType = PayShopFlagType.None;
		foreach (NewEquipGachaData item in Filter(GetDatasByCategory(category)))
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
			if (Checker.IsDiscount(item))
			{
				payShopFlagType |= PayShopFlagType.Sale;
			}
			if (GlobalAutoBehaviour<AccountManager>.instance.FreeEquipGachaManager.Free(item.Index))
			{
				payShopFlagType |= PayShopFlagType.Free;
			}
		}
		PayShopCategoryComponent.SaveNewFlagCheckClass(category, newFlagCheckClass);
		return payShopFlagType;
	}
}
