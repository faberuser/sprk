using System;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI3.NAttendance;

public static class AttendanceHelper
{
	public static Component CreateRewardComponent<T>(T listener, AttendanceReward reward, bool isGrayscale, Transform parentTm) where T : CurrencyIconComponent.Listener, ItemIconComponent.Listener
	{
		if (parentTm == null)
		{
			return null;
		}
		if (reward.Type == RewardType.Gold)
		{
			CurrencyIconComponent currencyIconComponent = CurrencyIconComponent.PrefabAuto.Factory.Create(parentTm);
			if (currencyIconComponent == null)
			{
				return null;
			}
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(currencyIconComponent.transform);
			int value = reward.Value;
			object obj = listener;
			int num = value;
			currencyIconComponent.Open(CurrencyType.Gold, num, (CurrencyIconComponent.Listener)obj);
			currencyIconComponent.SetIconGrayScale(isGrayscale);
			return currencyIconComponent;
		}
		if (reward.Type == RewardType.Gem)
		{
			CurrencyIconComponent currencyIconComponent2 = CurrencyIconComponent.PrefabAuto.Factory.Create(parentTm);
			if (currencyIconComponent2 == null)
			{
				return null;
			}
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(currencyIconComponent2.transform);
			int value2 = reward.Value;
			object obj2 = listener;
			int num2 = value2;
			currencyIconComponent2.Open(CurrencyType.Gem, num2, (CurrencyIconComponent.Listener)obj2);
			currencyIconComponent2.SetIconGrayScale(isGrayscale);
			return currencyIconComponent2;
		}
		if (reward.Type == RewardType.Item)
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(reward.Value);
			if (data == null)
			{
				return null;
			}
			ItemIconComponent itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(parentTm);
			if (itemIconComponent == null)
			{
				return null;
			}
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(itemIconComponent.transform);
			if (data.Type == ItemType.Equip)
			{
				EquipItemInfo equipItemInfo = new EquipItemInfo();
				equipItemInfo.ItemIndex = reward.Value;
				if (!itemIconComponent.Open(equipItemInfo, listener))
				{
					FactoryBase.ReleaseComponent(itemIconComponent);
					return null;
				}
				itemIconComponent.SetIconGrayScale(isGrayscale);
				return itemIconComponent;
			}
			itemIconComponent.Open(data, listener);
			itemIconComponent.SetIconGrayScale(isGrayscale);
			int count = reward.Count;
			Color? color = null;
			int num3 = count;
			itemIconComponent.SetItemCount(num3, color);
			return itemIconComponent;
		}
		return null;
	}

	public static bool IsAttendanceProgress(AttendanceInfo attendanceInfo, AttendanceData attendanceData)
	{
		if (attendanceInfo == null)
		{
			return false;
		}
		if (!AccountManager.IsLogined())
		{
			return false;
		}
		if (attendanceData.SendMail == 1)
		{
			return false;
		}
		if (attendanceInfo.CompletedDay > __Extension_Method_ICollection__.COUNT(attendanceData.Reward))
		{
			return false;
		}
		DateTime endTime_Local;
		if (attendanceInfo.CompletedDay == __Extension_Method_ICollection__.COUNT(attendanceData.Reward))
		{
			if (attendanceData.ManualStart == 1 && attendanceInfo.TryGetCompletedTime_ServerLocal(out var completedTime_Local))
			{
				DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
				DateTime date = completedTime_Local.AddDays(1.0).Date;
				return currentServerLocalTime < date;
			}
		}
		else if (attendanceInfo.TryGetEndTime_ServerLocal(out endTime_Local))
		{
			return GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime() < endTime_Local;
		}
		return false;
	}

	public static bool IsPayShopProductSelling(int attendanceIndex)
	{
		NewPayShopProductInfo productByAttendanceIndex = GetProductByAttendanceIndex(attendanceIndex);
		if (productByAttendanceIndex == null)
		{
			return false;
		}
		if (productByAttendanceIndex.IsPurchaseMarketing)
		{
			return PurchaseMarketingManager.IsPurchaseMarketingByProduct(productByAttendanceIndex.Index);
		}
		DateTime o_dateTime = DateTime.MinValue;
		DateTime uTCNow = NGame2.NUI.NWindow.Common.GetUTCNow();
		if (Vespa_String_Extension_Method2.TryParseNetworkTime2(productByAttendanceIndex.EndTime, out o_dateTime) && uTCNow.CompareTo(o_dateTime) <= 0)
		{
			return true;
		}
		return false;
	}

	private static NewPayShopProductInfo GetProductByAttendanceIndex(int attendanceIndex)
	{
		NewPayShopProductInfo[] products = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.GetProducts(ProductType.Attendance);
		if (__Extension_Method_ICollection__.COUNT(products) == 0)
		{
			return null;
		}
		return Array.Find(products, (NewPayShopProductInfo p) => __Extension_Method_ICollection__.COUNT(p.SelectAttendanceIndices) > 0 && TypeIterator.Contains(p.SelectAttendanceIndices, attendanceIndex));
	}
}
