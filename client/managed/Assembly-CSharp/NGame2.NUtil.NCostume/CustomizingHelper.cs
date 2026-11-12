using System;
using System.Collections.Generic;
using System.Text;
using NGame2.NAccount;
using NGame2.NUI.NComponent2.NCostume;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared;
using NShared.BuyCustomizingCostumes;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUtil.NCostume;

public class CustomizingHelper
{
	private static List<string> _stringList;

	public static Func<HeroInfo, CostumeData, CostumeState> HeroCostumeStateGetter { get; private set; }

	public static Func<CostumeData, Tuple<int, int>> HeroCostumeCalculrater { get; private set; }

	public static Func<HeroInfo, HairCostumeComponent.OpenData, CostumeState> HairCostumeStateGetter { get; private set; }

	public static Func<HairCostumeData, Tuple<int, int>> HairCostumeCalculrater { get; private set; }

	public static Func<HeroInfo, WeaponCostumeComponent.OpenData, CostumeState> WeaponCostumeStateGetter { get; private set; }

	public static Func<WeaponCostumeData, Tuple<int, int>> WeaponCostumeCalculrater { get; private set; }

	public static Func<HeroInfo, AccessoryCostumeComponent.OpenData, CostumeState> AccessoryCostumeStateGetter { get; private set; }

	public static Func<AccessoryCostumeData, Tuple<int, int>> AccessoryCostumeCalculrater { get; private set; }

	static CustomizingHelper()
	{
		_stringList = new List<string>();
		HeroCostumeStateGetter = (HeroInfo heroInfo, CostumeData costumeData) => HeroCostume.GetCostumeState(heroInfo, costumeData);
		HairCostumeStateGetter = (HeroInfo heroInfo, HairCostumeComponent.OpenData costumeOpenData) => HairCostume.GetCostumeState(heroInfo, costumeOpenData);
		WeaponCostumeStateGetter = (HeroInfo heroInfo, WeaponCostumeComponent.OpenData costumeOpenData) => WeaponCostume.GetCostumeState(heroInfo, costumeOpenData);
		AccessoryCostumeStateGetter = (HeroInfo heroInfo, AccessoryCostumeComponent.OpenData costumeOpenData) => AccessoryCostume.GetCostumeState(heroInfo, costumeOpenData);
		HeroCostumeCalculrater = delegate(CostumeData costumeData)
		{
			int priceGem = HeroCostume.GetPriceGem(costumeData);
			int obtainMileage = HeroCostume.GetObtainMileage(costumeData);
			return new Tuple<int, int>(priceGem, obtainMileage);
		};
		HairCostumeCalculrater = delegate(HairCostumeData costumeData)
		{
			int priceGem = HairCostume.GetPriceGem(costumeData);
			int obtainMileage = HairCostume.GetObtainMileage(costumeData);
			return new Tuple<int, int>(priceGem, obtainMileage);
		};
		WeaponCostumeCalculrater = delegate(WeaponCostumeData costumeData)
		{
			int priceGem = WeaponCostume.GetPriceGem(costumeData);
			int obtainMileage = WeaponCostume.GetObtainMileage(costumeData);
			return new Tuple<int, int>(priceGem, obtainMileage);
		};
		AccessoryCostumeCalculrater = delegate(AccessoryCostumeData costumeData)
		{
			int priceGem = AccessoryCostume.GetPriceGem(costumeData);
			int obtainMileage = AccessoryCostume.GetObtainMileage(costumeData);
			return new Tuple<int, int>(priceGem, obtainMileage);
		};
	}

	public static CollectCostumeFailReason GetFailResult(List<CollectResult> collectedResultList, out string failReason)
	{
		failReason = string.Empty;
		CollectCostumeFailReason result = CollectCostumeFailReason.None;
		_stringList.Clear();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(collectedResultList); i++)
		{
			if (collectedResultList[i].FailReason != CollectCostumeFailReason.None)
			{
				result = collectedResultList[i].FailReason;
			}
			if (collectedResultList[i].FailReason == CollectCostumeFailReason.ContainIncollectCostume)
			{
				_stringList.Add(collectedResultList[i].CostumeName);
			}
			else if (collectedResultList[i].FailReason == CollectCostumeFailReason.NeedOwn)
			{
				_stringList.Add(collectedResultList[i].CostumeName);
			}
			else if (collectedResultList[i].FailReason == CollectCostumeFailReason.NotExistCostumeData)
			{
				_stringList.Add(CollectCostumeFailReason.NotExistCostumeData.ToString());
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_stringList); j++)
		{
			failReason += _stringList[j];
			if (j < __Extension_Method_ICollection__.COUNT(_stringList) - 1)
			{
				failReason += "\n";
			}
		}
		return result;
	}

	public static bool CheckUnsetLegendCostume(HeroCustomizingDataGetter getter)
	{
		CachedHeroInfo cachedHeroInfo = Vespa_System_Action_Extension_Method.fire(getter.HeroInfoGetter);
		CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(cachedHeroInfo.CostumeIndex);
		CostumeData costumeData = Vespa_System_Action_Extension_Method.fire(getter.HeroCostumeDataGetter);
		if (data != null && costumeData != null && data.CostumeType == EHeroCostumeType.Legend && costumeData.CostumeType != EHeroCostumeType.Legend)
		{
			return true;
		}
		return false;
	}

	public static CustomizingCostumesStock CollectCostumes(HeroCustomizingDataGetter getter)
	{
		CachedHeroInfo cachedHeroInfo = Vespa_System_Action_Extension_Method.fire(getter.HeroInfoGetter);
		CostumeData costumeData = Vespa_System_Action_Extension_Method.fire(getter.HeroCostumeDataGetter);
		HairCostumeComponent.OpenData i_argB = Vespa_System_Action_Extension_Method.fire(getter.HairCostumeOpenDataGetter);
		WeaponCostumeComponent.OpenData i_argB2 = Vespa_System_Action_Extension_Method.fire(getter.WeaponCostumeOpenDataGetter);
		List<AccessoryCostumeComponent.OpenData> list = new List<AccessoryCostumeComponent.OpenData>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(getter.AccessoryCostumeOpenDataGetterList); i++)
		{
			list.Add(Vespa_System_Action_Extension_Method.fire(getter.AccessoryCostumeOpenDataGetterList[i]));
		}
		List<IPayShopEventSearchInfo> list2 = new List<IPayShopEventSearchInfo>();
		int num = 1;
		int num2 = 1;
		List<Tuple<int, string>> list3 = new List<Tuple<int, string>>();
		new StringBuilder();
		StringBuilder stringBuilder = new StringBuilder();
		if (costumeData != null)
		{
			int costumeIndex = costumeData.CostumeIndex;
			string costumeName = costumeData.CostumeName;
			if (Vespa_System_Action_Extension_Method.fire(HeroCostumeStateGetter, cachedHeroInfo, costumeData) == CostumeState.Buyable)
			{
				list3.Add(new Tuple<int, string>(num, costumeName));
				num++;
				list2.Add(costumeData);
			}
			stringBuilder.Append($"{num2}.[{costumeName}] [{costumeIndex}]");
			stringBuilder.Append($"[{Vespa_System_Action_Extension_Method.fire(HeroCostumeStateGetter, cachedHeroInfo, costumeData)}]");
			stringBuilder.AppendLine();
			num2++;
		}
		if (i_argB.HairCostumeData != null)
		{
			int index = i_argB.HairCostumeData.Index;
			string name = i_argB.HairCostumeData.Name;
			if (Vespa_System_Action_Extension_Method.fire(HairCostumeStateGetter, cachedHeroInfo, i_argB) == CostumeState.Buyable && i_argB.HairCostumeData.ReqBuyGem > 0)
			{
				list3.Add(new Tuple<int, string>(num, name));
				num++;
				list2.Add(i_argB.HairCostumeData);
			}
			stringBuilder.Append($"{num2}.[{name}] [{index}]");
			stringBuilder.Append($"[{Vespa_System_Action_Extension_Method.fire(HairCostumeStateGetter, cachedHeroInfo, i_argB)}]");
			stringBuilder.AppendLine();
			num2++;
		}
		if (i_argB2.WeaponCostumeData != null)
		{
			int index2 = i_argB2.WeaponCostumeData.Index;
			string name2 = i_argB2.WeaponCostumeData.Name;
			if (Vespa_System_Action_Extension_Method.fire(WeaponCostumeStateGetter, cachedHeroInfo, i_argB2) == CostumeState.Buyable && i_argB2.WeaponCostumeData.ReqBuyGem > 0)
			{
				list3.Add(new Tuple<int, string>(num, name2));
				num++;
				list2.Add(i_argB2.WeaponCostumeData);
			}
			stringBuilder.Append($"{num2}.[{name2}] [{index2}]");
			stringBuilder.Append($"[{Vespa_System_Action_Extension_Method.fire(WeaponCostumeStateGetter, cachedHeroInfo, i_argB2)}]");
			stringBuilder.AppendLine();
			num2++;
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(list); j++)
		{
			AccessoryCostumeData accessoryCostumeData = list[j].AccessoryCostumeData;
			if (accessoryCostumeData != null)
			{
				if (Vespa_System_Action_Extension_Method.fire(AccessoryCostumeStateGetter, cachedHeroInfo, list[j]) == CostumeState.Buyable)
				{
					list3.Add(new Tuple<int, string>(num, accessoryCostumeData.Name));
					num++;
					list2.Add(accessoryCostumeData);
				}
				stringBuilder.Append($"{num2}.[{accessoryCostumeData.Name}] [{accessoryCostumeData.Index}]");
				stringBuilder.Append($"[{Vespa_System_Action_Extension_Method.fire(AccessoryCostumeStateGetter, cachedHeroInfo, list[j])}]");
				PositionInfo i_this = Vespa_System_Action_Extension_Method.fire(getter.AccessoryPositionInfoGetter, accessoryCostumeData);
				stringBuilder.Append($"[{i_this.LOG()}]");
				stringBuilder.AppendLine();
				num2++;
			}
		}
		CustomizingCostumesStock costumesStock = default(CustomizingCostumesStock);
		costumesStock.HeroIndex = cachedHeroInfo.HeroIndex;
		costumesStock.CollectResultList = new List<CollectResult>();
		costumesStock.CollectResultList.Add(CollectHeroCostume(ref costumesStock, getter));
		costumesStock.CollectResultList.Add(CollectHairCostume(ref costumesStock, getter));
		costumesStock.CollectResultList.Add(CollectWeaponCostume(ref costumesStock, getter));
		costumesStock.CollectResultList.Add(CollectAccessoryCostume(ref costumesStock, getter));
		costumesStock.PurchaseProductList = list3;
		costumesStock.PopupMessage = GetPurchaseListMessage(list3);
		costumesStock.DebugLog = stringBuilder.ToString();
		costumesStock.PayShopEventSearchInfos = list2;
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder2.Append($"Hero ({costumesStock.HeroIndex}), Costume ({costumesStock.CostumeIndex}), Hair({costumesStock.HairCostumeIndex}) ");
		stringBuilder2.Append(string.Format("HideUnique ({1}) Weapon({0}), ", costumesStock.WeaponCostumeIndex, costumesStock.HideUniqueWeapon));
		for (int k = 0; k < __Extension_Method_ICollection__.COUNT(costumesStock.UnsetAccessoryCostumeIndices); k++)
		{
			stringBuilder2.Append($"\n UnsetAccessoryIndices ({costumesStock.UnsetAccessoryCostumeIndices[k]})");
		}
		if (__Extension_Method_ICollection__.COUNT(costumesStock.AccessoryPositionPairs) > 0)
		{
			Dictionary<int, PositionInfo>.Enumerator enumerator = costumesStock.AccessoryPositionPairs.GetEnumerator();
			while (enumerator.MoveNext())
			{
				stringBuilder2.Append($"\n AccessoryIndex ({enumerator.Current.Key}), {enumerator.Current.Value.Log()}");
			}
		}
		stringBuilder2.AppendLine();
		stringBuilder2.Append($"Gem ({costumesStock.BuyReqGem}) Mileage ({costumesStock.Mileage})");
		costumesStock.PacketLog = stringBuilder2.ToString();
		return costumesStock;
	}

	private static string GetPurchaseListMessage(List<Tuple<int, string>> purchaseProductList)
	{
		if (__Extension_Method_ICollection__.COUNT(purchaseProductList) == 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(purchaseProductList); i++)
		{
			Tuple<int, string> tuple = purchaseProductList[i];
			stringBuilder.Append(LocalizationManager.GetStringDirect("CONTENTS_CUSTOMIZE_MY_SET_NAME_FORM", tuple.Item1, tuple.Item2));
			if (i < __Extension_Method_ICollection__.COUNT(purchaseProductList) - 1)
			{
				stringBuilder.AppendLine();
			}
		}
		return stringBuilder.ToString();
	}

	public static void OnResponse(CustomizingCostumesStock buyCostumesStock, NShared.BuyCustomizingCostumes.Request request, NShared.BuyCustomizingCostumes.Response response)
	{
		if (ClientValueManager.CheckCustomizingCostumeValid)
		{
			MessagePopup.OpenLog("Request And Response", "Request\n" + request.Log(), "Response\n" + response.Log());
		}
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		instance.UserManager.Apply(response.CurrencyResults);
		instance.CostumeManager.Apply(response.CostumeInfos);
		instance.AccessoryCostumeManager.Apply(response.PlayerAccessoryCostumeInfos);
		instance.WeaponCostumeManager.Apply(response.WeaponCostumeInfo);
		instance.HairCostumeManager.Apply(response.HairCostumeInfo);
		SetHeroCostume(response.HeroCostumeResultInfo);
		SprkAccessories.AfterResponse();
	}

	private static void AssertResponse(CustomizingCostumesStock buyCostumesStock, NShared.BuyCustomizingCostumes.Request request, NShared.BuyCustomizingCostumes.Response response, AccountManager accountMgr)
	{
		if (response.HeroCostumeResultInfo != null)
		{
			_ = response.HeroCostumeResultInfo;
		}
	}

	public static void SetHeroCostume(HeroCostumeResultInfo recvHeroInfo)
	{
		SprkAccessories.ApplyResult(recvHeroInfo);
		if (recvHeroInfo != null)
		{
			AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
			instance.HeroManager.SetCostumeIndex(recvHeroInfo.HeroIndex, recvHeroInfo.CostumeIndex);
			instance.HeroManager.SetHairCostumeIndex(recvHeroInfo.HeroIndex, recvHeroInfo.HairCostumeIndex);
			instance.HeroManager.SetHideUniqueWeapon(recvHeroInfo.HeroIndex, recvHeroInfo.HideUniqueWeapon);
			instance.HeroManager.SetWeaponCostumeIndex(recvHeroInfo.HeroIndex, recvHeroInfo.WeaponCostumeIndex);
			PlayerAccessoryCostumeInfo playerAccessoryCostumeInfo = instance.AccessoryCostumeManager.Get(new PlayerAccessoryCostumeInfoKey(recvHeroInfo.HeroIndex, recvHeroInfo.AccessoryCostumeIndex1));
			PositionInfo positionInfo = ((playerAccessoryCostumeInfo == null) ? null : PositionInfo.Create().TryAssign(playerAccessoryCostumeInfo.PositionInfo));
			instance.HeroManager.SetAccessoryCostume(recvHeroInfo.HeroIndex, AccessoryPartType.Head, recvHeroInfo.AccessoryCostumeIndex1, positionInfo);
			PlayerAccessoryCostumeInfo playerAccessoryCostumeInfo2 = instance.AccessoryCostumeManager.Get(new PlayerAccessoryCostumeInfoKey(recvHeroInfo.HeroIndex, recvHeroInfo.AccessoryCostumeIndex2));
			PositionInfo positionInfo2 = ((playerAccessoryCostumeInfo2 == null) ? null : PositionInfo.Create().TryAssign(playerAccessoryCostumeInfo2.PositionInfo));
			instance.HeroManager.SetAccessoryCostume(recvHeroInfo.HeroIndex, AccessoryPartType.Eye, recvHeroInfo.AccessoryCostumeIndex2, positionInfo2);
			PlayerAccessoryCostumeInfo playerAccessoryCostumeInfo3 = instance.AccessoryCostumeManager.Get(new PlayerAccessoryCostumeInfoKey(recvHeroInfo.HeroIndex, recvHeroInfo.AccessoryCostumeIndex3));
			PositionInfo positionInfo3 = ((playerAccessoryCostumeInfo3 == null) ? null : PositionInfo.Create().TryAssign(playerAccessoryCostumeInfo3.PositionInfo));
			instance.HeroManager.SetAccessoryCostume(recvHeroInfo.HeroIndex, AccessoryPartType.Nose, recvHeroInfo.AccessoryCostumeIndex3, positionInfo3);
			PlayerAccessoryCostumeInfo playerAccessoryCostumeInfo4 = instance.AccessoryCostumeManager.Get(new PlayerAccessoryCostumeInfoKey(recvHeroInfo.HeroIndex, recvHeroInfo.AccessoryCostumeIndex4));
			PositionInfo positionInfo4 = ((playerAccessoryCostumeInfo4 == null) ? null : PositionInfo.Create().TryAssign(playerAccessoryCostumeInfo4.PositionInfo));
			instance.HeroManager.SetAccessoryCostume(recvHeroInfo.HeroIndex, AccessoryPartType.Body, recvHeroInfo.AccessoryCostumeIndex4, positionInfo4);
		}
	}

	public static void CreateRequest(CustomizingCostumesStock stock, Action<NShared.BuyCustomizingCostumes.Request> requestAction)
	{
		CustomizingCostumesStock customizingCostumesStock = stock;
		NShared.BuyCustomizingCostumes.Request i_argA = new NShared.BuyCustomizingCostumes.Request
		{
			HeroIndex = customizingCostumesStock.HeroIndex,
			HairCostumeIndex = customizingCostumesStock.HairCostumeIndex,
			CostumeIndex = customizingCostumesStock.CostumeIndex,
			WeaponCostumeIndex = customizingCostumesStock.WeaponCostumeIndex,
			HideUniqueWeapon = customizingCostumesStock.HideUniqueWeapon,
			UnsetAccessoryCostumeIndices = customizingCostumesStock.UnsetAccessoryCostumeIndices,
			AccessoryCostumePositionInfo = customizingCostumesStock.AccessoryCostumePositionInfo,
			BuyGem = customizingCostumesStock.BuyReqGem
		};
		Vespa_System_Action_Extension_Method.fire(requestAction, i_argA);
	}

	private static CollectResult CollectHeroCostume(ref CustomizingCostumesStock costumesStock, HeroCustomizingDataGetter getter)
	{
		CollectResult result = default(CollectResult);
		CachedHeroInfo cachedHeroInfo = Vespa_System_Action_Extension_Method.fire(getter.HeroInfoGetter);
		CostumeData costumeData = Vespa_System_Action_Extension_Method.fire(getter.HeroCostumeDataGetter);
		CostumeState costumeState = Vespa_System_Action_Extension_Method.fire(HeroCostumeStateGetter, cachedHeroInfo, costumeData);
		costumesStock.CostumeIndex = cachedHeroInfo.CostumeIndex;
		switch (costumeState)
		{
		case CostumeState.Buyable:
		{
			result.CostumeNeedUpdate = true;
			Tuple<int, int> tuple = Vespa_System_Action_Extension_Method.fire(HeroCostumeCalculrater, costumeData);
			costumesStock.BuyReqGem += tuple.Item1;
			costumesStock.Mileage += tuple.Item2;
			costumesStock.CostumeIndex = costumeData.CostumeIndex;
			break;
		}
		case CostumeState.LegendNotBuyable:
		case CostumeState.LegendBuyable:
			result.CostumeNeedUpdate = false;
			result.Result = CollectCostumeResult.Fail;
			result.FailReason = CollectCostumeFailReason.NeedOwnLegend;
			result.CostumeName = $"[{costumeData.CostumeName}]";
			break;
		case CostumeState.Owned:
			result.CostumeNeedUpdate = true;
			costumesStock.CostumeIndex = costumeData.CostumeIndex;
			break;
		case CostumeState.NeedSubStory:
			result.CostumeNeedUpdate = false;
			result.Result = CollectCostumeResult.Fail;
			result.FailReason = CollectCostumeFailReason.NeedOwn;
			result.CostumeName = $"[{costumeData.CostumeName}]";
			break;
		case CostumeState.NeedAwaken:
			result.CostumeNeedUpdate = false;
			result.Result = CollectCostumeResult.Fail;
			result.FailReason = CollectCostumeFailReason.NeedOwn;
			result.CostumeName = $"[{costumeData.CostumeName}]";
			break;
		}
		return result;
	}

	private static CollectResult CollectHairCostume(ref CustomizingCostumesStock costumesStock, HeroCustomizingDataGetter getter)
	{
		CollectResult result = default(CollectResult);
		CachedHeroInfo cachedHeroInfo = Vespa_System_Action_Extension_Method.fire(getter.HeroInfoGetter);
		CostumeData costumeData = Vespa_System_Action_Extension_Method.fire(getter.HeroCostumeDataGetter);
		CostumeState costumeState = Vespa_System_Action_Extension_Method.fire(HeroCostumeStateGetter, cachedHeroInfo, costumeData);
		HairCostumeComponent.OpenData i_argB = Vespa_System_Action_Extension_Method.fire(getter.HairCostumeOpenDataGetter);
		CostumeState costumeState2 = Vespa_System_Action_Extension_Method.fire(HairCostumeStateGetter, cachedHeroInfo, i_argB);
		HairCostumeData hairCostumeData = i_argB.HairCostumeData;
		costumesStock.HairCostumeIndex = cachedHeroInfo.HairCostumeIndex;
		switch (costumeState2)
		{
		case CostumeState.Buyable:
			result.CostumeNeedUpdate = true;
			if (hairCostumeData == null)
			{
				result.CostumeNeedUpdate = false;
				result.Result = CollectCostumeResult.Fail;
				result.FailReason = CollectCostumeFailReason.NotExistCostumeData;
				return result;
			}
			costumesStock.HairCostumeIndex = hairCostumeData.Index;
			if (hairCostumeData.ReqBuyGem > 0)
			{
				Tuple<int, int> tuple = Vespa_System_Action_Extension_Method.fire(HairCostumeCalculrater, hairCostumeData);
				costumesStock.BuyReqGem += tuple.Item1;
				costumesStock.Mileage += tuple.Item2;
			}
			else if (!__Extension_Method_ICollection__.IsNullOrEmpty(hairCostumeData.NeedCostume) && (costumeData == null || !TypeIterator.Contains(hairCostumeData.NeedCostume, costumeData.CostumeIndex)))
			{
				result.CostumeNeedUpdate = false;
				result.Result = CollectCostumeResult.Fail;
				result.FailReason = CollectCostumeFailReason.ContainIncollectCostume;
				result.CostumeName = $"[{hairCostumeData.Name}]";
			}
			break;
		case CostumeState.Owned:
			result.CostumeNeedUpdate = true;
			if (i_argB.ComponentType == HairCostumeComponent.ComponentType.SelectCostumeData)
			{
				if (hairCostumeData == null)
				{
					result.CostumeNeedUpdate = false;
					result.Result = CollectCostumeResult.Fail;
					result.FailReason = CollectCostumeFailReason.NotExistCostumeData;
					return result;
				}
				costumesStock.HairCostumeIndex = hairCostumeData.Index;
			}
			else if (i_argB.ComponentType == HairCostumeComponent.ComponentType.SelectThema)
			{
				if (costumeState == CostumeState.LegendBuyable)
				{
					result.CostumeNeedUpdate = false;
					result.Result = CollectCostumeResult.Fail;
					result.FailReason = CollectCostumeFailReason.NeedOwnLegend;
					return result;
				}
				costumesStock.HairCostumeIndex = 0;
			}
			break;
		case CostumeState.NeedAwaken:
			result.CostumeNeedUpdate = false;
			result.Result = CollectCostumeResult.Fail;
			result.FailReason = CollectCostumeFailReason.NeedOwn;
			result.CostumeName = $"[{hairCostumeData.Name}]";
			break;
		}
		return result;
	}

	private static CollectResult CollectWeaponCostume(ref CustomizingCostumesStock costumesStock, HeroCustomizingDataGetter getter)
	{
		CollectResult result = default(CollectResult);
		CachedHeroInfo cachedHeroInfo = Vespa_System_Action_Extension_Method.fire(getter.HeroInfoGetter);
		CostumeData costumeData = Vespa_System_Action_Extension_Method.fire(getter.HeroCostumeDataGetter);
		CostumeState costumeState = Vespa_System_Action_Extension_Method.fire(HeroCostumeStateGetter, cachedHeroInfo, costumeData);
		WeaponCostumeComponent.OpenData i_argB = Vespa_System_Action_Extension_Method.fire(getter.WeaponCostumeOpenDataGetter);
		CostumeState costumeState2 = Vespa_System_Action_Extension_Method.fire(WeaponCostumeStateGetter, cachedHeroInfo, i_argB);
		WeaponCostumeData weaponCostumeData = i_argB.WeaponCostumeData;
		costumesStock.HideUniqueWeapon = cachedHeroInfo.HideUniqueWeapon;
		costumesStock.WeaponCostumeIndex = cachedHeroInfo.WeaponCostumeIndex;
		switch (costumeState2)
		{
		case CostumeState.Buyable:
			result.CostumeNeedUpdate = true;
			if (weaponCostumeData == null)
			{
				result.CostumeNeedUpdate = false;
				result.Result = CollectCostumeResult.Fail;
				result.FailReason = CollectCostumeFailReason.NotExistCostumeData;
				return result;
			}
			costumesStock.HideUniqueWeapon = 1;
			costumesStock.WeaponCostumeIndex = weaponCostumeData.Index;
			if (weaponCostumeData.ReqBuyGem > 0)
			{
				Tuple<int, int> tuple = Vespa_System_Action_Extension_Method.fire(WeaponCostumeCalculrater, weaponCostumeData);
				costumesStock.BuyReqGem += tuple.Item1;
				costumesStock.Mileage += tuple.Item2;
			}
			else if (!__Extension_Method_ICollection__.IsNullOrEmpty(weaponCostumeData.NeedCostume) && (costumeData == null || !TypeIterator.Contains(weaponCostumeData.NeedCostume, costumeData.CostumeIndex)))
			{
				result.CostumeNeedUpdate = false;
				result.Result = CollectCostumeResult.Fail;
				result.FailReason = CollectCostumeFailReason.ContainIncollectCostume;
				result.CostumeName = $"[{weaponCostumeData.Name}]";
			}
			break;
		case CostumeState.Owned:
			result.CostumeNeedUpdate = true;
			if (i_argB.ComponentType == WeaponCostumeComponent.ComponentType.SelectCostume)
			{
				if (weaponCostumeData == null)
				{
					result.CostumeNeedUpdate = false;
					result.Result = CollectCostumeResult.Fail;
					result.FailReason = CollectCostumeFailReason.NotExistCostumeData;
					return result;
				}
				costumesStock.HideUniqueWeapon = 1;
				costumesStock.WeaponCostumeIndex = weaponCostumeData.Index;
			}
			else if (i_argB.ComponentType == WeaponCostumeComponent.ComponentType.SelectThema)
			{
				if (costumeState == CostumeState.LegendBuyable)
				{
					result.CostumeNeedUpdate = false;
					result.Result = CollectCostumeResult.Fail;
					result.FailReason = CollectCostumeFailReason.NeedOwnLegend;
					return result;
				}
				costumesStock.HideUniqueWeapon = 1;
				costumesStock.WeaponCostumeIndex = 0;
			}
			else if (i_argB.ComponentType == WeaponCostumeComponent.ComponentType.SelectUnique)
			{
				costumesStock.HideUniqueWeapon = 0;
				costumesStock.WeaponCostumeIndex = 0;
			}
			break;
		case CostumeState.NeedAwaken:
			result.CostumeNeedUpdate = false;
			result.Result = CollectCostumeResult.Fail;
			result.FailReason = CollectCostumeFailReason.NeedOwn;
			result.CostumeName = $"[{weaponCostumeData.Name}]";
			break;
		}
		return result;
	}

	private static CollectResult CollectAccessoryCostume(ref CustomizingCostumesStock costumesStock, HeroCustomizingDataGetter getter)
	{
		return SprkAccessories.Collect(ref costumesStock, getter);
	}

	private static string GetSerializePosition(Dictionary<int, PositionInfo> accessoryCostumePositionContainer)
	{
		return MiniJson.Serialize(BaseJsonMarshaler.EncodeDictionary(accessoryCostumePositionContainer, JsonMarshaler.EncodeJsonObject));
	}

	private static PositionInfo GetAccessoryPositionInfo(HeroCustomizingDataGetter getter, HeroInfo heroInfo, AccessoryCostumeData accessoryData)
	{
		PositionInfo positionInfo = Vespa_System_Action_Extension_Method.fire(getter.AccessoryPositionInfoGetter, accessoryData);
		if (positionInfo == null)
		{
			VespaDebug.LogError($"GetAccessoryPositionInfo GetterPositionInfo == null // heroIndex({heroInfo.HeroIndex}) accessoryData({accessoryData.Index})");
			positionInfo = PositionInfo.Create();
		}
		positionInfo.Round();
		HeroCostumeValidChecker.SetValidPositionInfo(heroInfo.HeroIndex, accessoryData.PivotType, positionInfo);
		return positionInfo;
	}
}
