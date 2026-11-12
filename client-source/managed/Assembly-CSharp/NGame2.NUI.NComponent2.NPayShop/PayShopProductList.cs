using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NHelper.NPayShop;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopProductList : PayShopBaseFrameList<NewPayShopProductInfo, PayShopProductComponent, PayShopProductFrameComponent, string>, PayShopProductFrameComponent.IListener, PayShopBaseComponent.IListener, PayShopProductComponent.IListener, IPayShopProductList
{
	public new interface IListener : PayShopBaseList.IListener
	{
		void OnClickProduct(int originalIndex, int comboIndex);
	}

	private new IListener _listener;

	private Dictionary<int, int> _comboProductRegistStep = new Dictionary<int, int>();

	public override void Open(PayShopBaseList.IListener listener)
	{
		_listener = listener as IListener;
		base.Open(listener);
	}

	public override void CreateList()
	{
		CreateList(NewPayShopCategoryType.HotNew.ToString());
	}

	public override void CreateList(string category)
	{
		base.CreateList(category);
	}

	public override List<NewPayShopProductInfo> GetRealDataList(string category)
	{
		NewPayShopProductInfo[] products = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.GetProducts(category);
		List<NewPayShopProductInfo> list = new List<NewPayShopProductInfo>();
		list.AddRange(products);
		return SprkShopSearch.Products(list, this, category);
	}

	protected override NewPayShopProductInfo GetRealData(int index)
	{
		return GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(index);
	}

	protected override List<NewPayShopProductInfo> Filtering(List<NewPayShopProductInfo> datas)
	{
		return ProudctFilter(datas);
	}

	public static List<NewPayShopProductInfo> ProudctFilter(List<NewPayShopProductInfo> datas)
	{
		List<NewPayShopProductInfo> list = new List<NewPayShopProductInfo>();
		foreach (NewPayShopProductInfo data in datas)
		{
			if (!IsDurationProduct(data) && !IsPrePurchasedProduct(datas, data) && !IsComboProduct(datas, data) && !CheckIfOwnHeroInPackageItem(data) && !IsNewbieTabProductPurchaseable(data))
			{
				list.Add(data);
			}
		}
		return list;
	}

	public static bool IsDurationProduct(NewPayShopProductInfo info)
	{
		if (info.Durational == 1)
		{
			string beginTime = info.BeginTime;
			string endTime = info.EndTime;
			if (!String_Extension_Method.IsNullOrEmpty(beginTime) && !String_Extension_Method.IsNullOrEmpty(endTime) && Vespa_String_Extension_Method2.TryParseNetworkTime2(beginTime, out var o_dateTime) && Vespa_String_Extension_Method2.TryParseNetworkTime2(endTime, out var o_dateTime2))
			{
				DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
				if (currentServerUTCTime < o_dateTime || o_dateTime2 < currentServerUTCTime)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool IsPrePurchasedProduct(List<NewPayShopProductInfo> itemList, NewPayShopProductInfo info)
	{
		if (itemList.Find((NewPayShopProductInfo i) => i.PrePurchased == info.Index) != null)
		{
			if (GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Get(info.Index) != null)
			{
				return true;
			}
		}
		else if (info.PrePurchased > 0)
		{
			NewPayShopProductInfo newPayShopProductInfo = itemList.Find((NewPayShopProductInfo i) => i.Index == info.PrePurchased);
			if (newPayShopProductInfo != null && GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Get(newPayShopProductInfo.Index) == null)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsComboProduct(List<NewPayShopProductInfo> itemList, NewPayShopProductInfo info)
	{
		if (__Extension_Method_ICollection__.COUNT(info.StepProductGroup) >= 2)
		{
			int groupIndex = ArgParser.GetValueInt(info.StepProductGroup, 0);
			int step = ArgParser.GetValueInt(info.StepProductGroup, 1);
			int num = 1;
			NewPayShopProductInfo[] stepProducts = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.GetStepProducts(info.Category, groupIndex);
			num = ((stepProducts == null) ? 1 : __Extension_Method_ICollection__.COUNT(stepProducts));
			PlayerProductPurchaseInfo playerProductPurchaseInfo = GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Get(info.Index);
			if (playerProductPurchaseInfo != null && step != num && info.PurchasableCount > 0 && playerProductPurchaseInfo.PurchasedCount >= info.PurchasableCount)
			{
				return true;
			}
			if (step >= 2)
			{
				NewPayShopProductInfo newPayShopProductInfo = itemList.Find((NewPayShopProductInfo findInfo) => __Extension_Method_ICollection__.COUNT(findInfo.StepProductGroup) >= 2 && ArgParser.GetValueInt(findInfo.StepProductGroup, 0) == groupIndex && ArgParser.GetValueInt(findInfo.StepProductGroup, 1) == step - 1);
				if (newPayShopProductInfo == null)
				{
					return false;
				}
				PlayerProductPurchaseInfo playerProductPurchaseInfo2 = GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Get(newPayShopProductInfo.Index);
				if (playerProductPurchaseInfo2 == null || playerProductPurchaseInfo2.PurchasedCount < newPayShopProductInfo.PurchasableCount)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool CheckIfOwnHeroInPackageItem(NewPayShopProductInfo info)
	{
		if (info.HeroItemIndex > 0)
		{
			int heroIndexFromHeroItem = GetHeroIndexFromHeroItem(info.HeroItemIndex);
			return GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroIndexFromHeroItem) != null;
		}
		return false;
	}

	public static int GetHeroIndexFromHeroItem(int heroItemIndex)
	{
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(heroItemIndex);
		if (data == null || data.Type != ItemType.Hero)
		{
			return 0;
		}
		if (__Extension_Method_ICollection__.COUNT(data.Value) <= 0)
		{
			return 0;
		}
		return data.GetValueInt(0);
	}

	public static bool IsNewbieTabProductPurchaseable(NewPayShopProductInfo info)
	{
		return PayShopHelper.IsNewbieTabProductPurchaseable(info);
	}

	protected override PrefabAuto<PayShopProductFrameComponent> GetComponentPrefabAuto()
	{
		return PayShopProductFrameComponent.PrefabAuto;
	}

	public override void CreateList(int tagIndex)
	{
		base.CreateList(tagIndex);
	}

	protected override void Sort(List<NewPayShopProductInfo> dataList, int[] sortIndices = null)
	{
		ProductSort(dataList, sortIndices);
		SprkShopSearch.AfterSort(this, dataList);
	}

	public static void ProductSort(List<NewPayShopProductInfo> productInfos, int[] sortIndices = null)
	{
		Dictionary<int, bool> tempSoldout = new Dictionary<int, bool>();
		Func<NewPayShopProductInfo, bool> isSoldOutFunc = delegate(NewPayShopProductInfo info)
		{
			if (!tempSoldout.ContainsKey(info.Index))
			{
				bool value = IsSoldOut(info);
				tempSoldout.Add(info.Index, value);
			}
			return tempSoldout[info.Index];
		};
		Dictionary<int, bool> tempBuyable = new Dictionary<int, bool>();
		Func<NewPayShopProductInfo, bool> isBuyableFunc = delegate(NewPayShopProductInfo info)
		{
			if (!tempBuyable.ContainsKey(info.Index))
			{
				bool value = IsBuyable(info);
				tempBuyable.Add(info.Index, value);
			}
			return tempBuyable[info.Index];
		};
		productInfos.Sort(delegate(NewPayShopProductInfo a, NewPayShopProductInfo b)
		{
			int num = 0;
			bool flag = isSoldOutFunc(a);
			bool value = isSoldOutFunc(b);
			num = flag.CompareTo(value);
			if (num != 0)
			{
				return num;
			}
			bool flag2 = !isBuyableFunc(a);
			bool value2 = !isBuyableFunc(b);
			num = flag2.CompareTo(value2);
			if (num != 0)
			{
				return num;
			}
			bool flag3 = a.CardType == 1;
			bool value3 = b.CardType == 1;
			num = -flag3.CompareTo(value3);
			if (num != 0)
			{
				return num;
			}
			if (a.SmartRating != 0 || b.SmartRating != 0)
			{
				num = -a.SmartRating.CompareTo(b.SmartRating);
				if (num != 0)
				{
					return num;
				}
			}
			if (-a.IsPurchaseMarketing.CompareTo(b.IsPurchaseMarketing) == 0)
			{
				num = a.ShowIndex.CompareTo(b.ShowIndex);
			}
			if (__Extension_Method_ICollection__.IsNullOrEmpty(sortIndices))
			{
				int num2 = ((a.ShowIndex != 0) ? a.ShowIndex : int.MaxValue);
				int value4 = ((b.ShowIndex != 0) ? b.ShowIndex : int.MaxValue);
				num = num2.CompareTo(value4);
			}
			else
			{
				int num3 = Array.Find(sortIndices, (int index) => index == a.Index);
				int value5 = Array.Find(sortIndices, (int index) => index == b.Index);
				num = num3.CompareTo(value5);
			}
			return (num != 0) ? num : a.Index.CompareTo(b.Index);
		});
		SprkSpecialShop.Sort(productInfos);
	}

	public static bool IsSoldOut(NewPayShopProductInfo info)
	{
		PlayerProductPurchaseInfo playerProductPurchaseInfo = GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Get(info.Index);
		if (info.PurchasableCount > 0 && info.PurchasableCount - (playerProductPurchaseInfo?.PurchasedCount ?? 0) <= 0)
		{
			return true;
		}
		return false;
	}

	public static bool IsBuyable(NewPayShopProductInfo productInfo)
	{
		return PayShopHelper.IsBuyable(productInfo);
	}

	public static PayShopFlagType CheckCategoryFlag(string category)
	{
		PayShopCategoryComponent.NewFlagCheckClass newFlagCheckClass = PayShopCategoryComponent.LoadNewFlagCheckClass(category);
		PayShopFlagType payShopFlagType = PayShopFlagType.None;
		NewPayShopProductInfo[] products = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.GetProducts(category);
		List<NewPayShopProductInfo> list = new List<NewPayShopProductInfo>();
		list.AddRange(products);
		foreach (NewPayShopProductInfo item in ProudctFilter(list))
		{
			if (item.Flag == PayShopFlagType.Sale)
			{
				payShopFlagType |= PayShopFlagType.Sale;
			}
			else if (item.Flag == PayShopFlagType.Free && PayShopHelper.IsBuyable(item) && !PayShopHelper.IsNewbieTabProductPurchaseable(item))
			{
				payShopFlagType |= PayShopFlagType.Free;
			}
			if (newFlagCheckClass.NewFlagConfirmValue == 0)
			{
				DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
				newFlagCheckClass.InitNewFlagConfirmValue(currentServerLocalTime);
			}
			else if (newFlagCheckClass.IsNew(item.OpenDate))
			{
				payShopFlagType |= PayShopFlagType.New;
				newFlagCheckClass.NewFlagValue = item.OpenDate;
			}
		}
		PayShopCategoryComponent.SaveNewFlagCheckClass(category, newFlagCheckClass);
		return payShopFlagType;
	}

	protected override void ReleaseComponents()
	{
		base.ReleaseComponents();
	}

	public override void ClearList()
	{
		base.ClearList();
	}

	public override void ClearAll()
	{
		base.ClearAll();
		_comboProductRegistStep.Clear();
	}

	public override void UpdateProduct(int index)
	{
		PayShopProductComponent payShopProductComponent = FindComponent(index);
		if (!(payShopProductComponent == null))
		{
			NewPayShopProductInfo productInfo = payShopProductComponent.ProductInfo;
			if (GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.GetPostFirstPurchaseProduct(productInfo.Category, productInfo.Index) != null)
			{
				UpdateFirstPurchaseProduct(productInfo, payShopProductComponent);
			}
			else if (__Extension_Method_ICollection__.COUNT(productInfo.StepProductGroup) > 0)
			{
				UpdateComboProduct(productInfo, payShopProductComponent);
			}
			else
			{
				payShopProductComponent.Refresh();
			}
		}
	}

	public void UpdateComboProduct(NewPayShopProductInfo info, PayShopProductComponent comp)
	{
		if (__Extension_Method_ICollection__.COUNT(info.StepProductGroup) < 2)
		{
			return;
		}
		int valueInt = ArgParser.GetValueInt(info.StepProductGroup, 0);
		int valueInt2 = ArgParser.GetValueInt(info.StepProductGroup, 1);
		PlayerProductPurchaseInfo playerProductPurchaseInfo = GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Get(info.Index);
		if (playerProductPurchaseInfo != null && playerProductPurchaseInfo.PurchasedCount >= info.PurchasableCount)
		{
			NewPayShopProductInfo stepProduct = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.GetStepProduct(info.Category, valueInt, valueInt2 + 1);
			if (stepProduct == null)
			{
				comp.Refresh();
				return;
			}
			comp.ChangeProductInfo(stepProduct);
			int dataIndex = GetDataIndex(info);
			ChagneData(dataIndex, info, stepProduct);
		}
		else
		{
			comp.Refresh();
		}
	}

	public void UpdateFirstPurchaseProduct(NewPayShopProductInfo info, PayShopProductComponent comp)
	{
		NewPayShopProductInfo postFirstPurchaseProduct = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.GetPostFirstPurchaseProduct(info.Category, info.Index);
		if (postFirstPurchaseProduct == null)
		{
			comp.Refresh();
			return;
		}
		comp.ChangeProductInfo(postFirstPurchaseProduct);
		int dataIndex = GetDataIndex(info);
		ChagneData(dataIndex, info, postFirstPurchaseProduct);
	}

	public void OnClickProduct(int originalIndex, int comboIndex)
	{
		if (_listener != null)
		{
			_listener.OnClickProduct(originalIndex, comboIndex);
		}
	}

	public int GetDataIndex(NewPayShopProductInfo data)
	{
		return _datas.FindIndex((PayShopPairData<NewPayShopProductInfo> d) => d.Datas.Contains(data));
	}

	public void ChagneData(int index, NewPayShopProductInfo old, NewPayShopProductInfo data)
	{
		if (__Extension_Method_ICollection__.COUNT(_datas) > index)
		{
			PayShopPairData<NewPayShopProductInfo> payShopPairData = _datas[index];
			int num = payShopPairData.Datas.FindIndex((NewPayShopProductInfo d) => d == old);
			if (num > -1)
			{
				payShopPairData.Datas[num] = data;
			}
		}
	}

	public void OnChangeComboProduct(int productIndex, int step)
	{
		NewPayShopProductInfo newPayShopProductInfo = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(productIndex);
		if (newPayShopProductInfo == null)
		{
			return;
		}
		if (ArgParser.GetValueInt(newPayShopProductInfo.StepProductGroup, 1) == step)
		{
			if (_comboProductRegistStep.ContainsKey(productIndex))
			{
				_comboProductRegistStep.Remove(productIndex);
			}
		}
		else if (!_comboProductRegistStep.ContainsKey(productIndex))
		{
			_comboProductRegistStep.Add(productIndex, step);
		}
		else
		{
			_comboProductRegistStep[productIndex] = step;
		}
	}

	protected override void OnChangedItem(PayShopProductFrameComponent comp, PayShopPairData<NewPayShopProductInfo> data)
	{
		base.OnChangedItem(comp, data);
		foreach (NewPayShopProductInfo data2 in data.Datas)
		{
			if (_comboProductRegistStep.ContainsKey(data2.Index))
			{
				int nextStep = _comboProductRegistStep[data2.Index];
				PayShopProductComponent payShopProductComponent = comp.FindComponent(data2.Index);
				if (payShopProductComponent != null)
				{
					payShopProductComponent.SetNextStepInfo(ref nextStep);
				}
			}
		}
	}
}
