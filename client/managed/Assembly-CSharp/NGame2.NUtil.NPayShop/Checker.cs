using System;
using NGame2.NAccount;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUtil.NPayShop;

public class Checker
{
	public static bool HasAnySaleEquipGacha2()
	{
		foreach (NewEquipGachaData allDatum in GameRoot.Instance.NewEquipGachaDataContainer.GetAllData())
		{
			if (IsDiscount(allDatum))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsDiscountStepupGacha(NewEquipGachaData equipGachaData, out int stepUpGachaDiscount)
	{
		stepUpGachaDiscount = 0;
		if (equipGachaData == null || equipGachaData.Type != EquipGachaType.StepUp)
		{
			return false;
		}
		StepUpEquipGachaInfo stepUpEquipGachaInfo = GlobalAutoBehaviour<AccountManager>.instance.StepUpEquipGachamanager.Get(equipGachaData.Index);
		if (stepUpEquipGachaInfo == null)
		{
			return false;
		}
		StepUpGachaRewardData data = GameRoot.Instance.StepUpGachaRewardDataContainer.GetData(stepUpEquipGachaInfo.GachaIndex, stepUpEquipGachaInfo.CompletedStep);
		if (data == null)
		{
			return false;
		}
		StepUpGachaRewardData nexData = GameRoot.Instance.StepUpGachaRewardDataContainer.GetNexData(data);
		if (nexData == null)
		{
			return false;
		}
		stepUpGachaDiscount = nexData.Discount;
		return nexData.Discount != 0;
	}

	public static bool IsDiscount(NewEquipGachaData equipGachaData)
	{
		EquipGachaInfo equipGachaInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipGachaManager.Get(equipGachaData.Index);
		return IsDiscount(equipGachaData, equipGachaInfo);
	}

	public static bool IsDiscount(NewEquipGachaData equipGachaData, EquipGachaInfo equipGachaInfo)
	{
		if (equipGachaData == null || equipGachaData.DailyMaxSale == 0)
		{
			return false;
		}
		if (equipGachaInfo == null || equipGachaInfo.Discount == 0)
		{
			return false;
		}
		if (String_Extension_Method.IsNullOrEmpty(equipGachaInfo.LastDiscountedTime))
		{
			return true;
		}
		if (IsInitialized(equipGachaInfo.LastDiscountedTime, equipGachaInfo.SaleResetTime))
		{
			return true;
		}
		return equipGachaInfo.RemainNumberOfDiscount > 0;
	}

	private static bool IsInitialized(string lastCheckedServerLocalTime, string resetTimeHHMMSS)
	{
		if (String_Extension_Method.IsNullOrEmpty(resetTimeHHMMSS))
		{
			VespaDebug.LogError("resetTimeString Is Null Or Empty");
			return false;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(lastCheckedServerLocalTime, out var o_dateTime))
		{
			VespaDebug.LogError("lastCheckedServerLocalTime TryParse Fail");
			return false;
		}
		DateTime dateTime = DateTime.Parse(resetTimeHHMMSS);
		DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
		DateTime dateTime2 = NVespa.Math.CreateDateTime(currentServerLocalTime.Year, currentServerLocalTime.Month, currentServerLocalTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
		if (o_dateTime < dateTime2)
		{
			return dateTime2 < currentServerLocalTime;
		}
		return false;
	}
}
