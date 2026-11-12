using System;
using NGame2.NAccount;
using NShared;
using NShared.RechargeStamina;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;
using UnityEngine;

public class GodkingTrialHelper
{
	public static int GetMinDifficulty(int chapterIndex)
	{
		return GameRoot.Instance.GodkingTrialDataContainer.GetMinDifficulty(chapterIndex);
	}

	public static int GetMaxDifficulty(int chapterIndex)
	{
		return GameRoot.Instance.GodkingTrialDataContainer.GetMaxDifficulty(chapterIndex);
	}

	public static int GetTicketMaxResetCount()
	{
		int result = 0;
		StaminaData data = GameRoot.Instance.StaminaDataContainer.GetData(StaminaType.GodkingTrialKey);
		if (data != null)
		{
			result = ((!data.IsResetLimit) ? (-1) : data.ResetLimitCount);
		}
		return result;
	}

	public static int GetRechargeCount()
	{
		StaminaData data = GameRoot.Instance.StaminaDataContainer.GetData(StaminaType.GodkingTrialKey);
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(data.ResetCount, 0))
		{
			return 1;
		}
		return data.ResetCount[0];
	}

	public static bool CanRechargeTicket()
	{
		int ticketMaxResetCount = GetTicketMaxResetCount();
		int godkingTrialKeyRechargeCount = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.GodkingTrialKeyRechargeCount;
		if (Mathf.Max(0, ticketMaxResetCount - godkingTrialKeyRechargeCount) > 0)
		{
			return true;
		}
		return false;
	}

	public static string GetDungeonName(int chapterIndex)
	{
		GodkingTrialGroupData data = GameRoot.Instance.GodkingTrialGroupDataContainer.GetData(chapterIndex);
		if (data == null)
		{
			return "";
		}
		return LocalizationManager.GetStringDirect(data.DungeonNamePrefixKey);
	}

	public static string GetBossName(int chapterIndex)
	{
		GodkingTrialGroupData data = GameRoot.Instance.GodkingTrialGroupDataContainer.GetData(chapterIndex);
		if (data == null)
		{
			return "";
		}
		return LocalizationManager.GetStringDirect(data.DungeonNameKey);
	}

	public static string GetDungeonFullName(int chapterIndex)
	{
		GodkingTrialGroupData data = GameRoot.Instance.GodkingTrialGroupDataContainer.GetData(chapterIndex);
		if (data == null)
		{
			return "";
		}
		return GetDungeonFullName(data);
	}

	public static string GetDungeonFullName(GodkingTrialGroupData groupData)
	{
		if (groupData == null)
		{
			return "";
		}
		return LocalizationManager.GetStringDirect(groupData.DungeonNamePrefixKey) + " " + LocalizationManager.GetStringDirect(groupData.DungeonNameKey);
	}

	public static int GetDungeonIndexByDifficulty(int difficulty)
	{
		return difficulty;
	}

	public static bool IsOpenedOther(int chapterIndex)
	{
		GodkingTrialDungeonInfo[] values = GlobalAutoBehaviour<AccountManager>.instance.GodkingTrialDungeonInfoManager.GetValues();
		int i = 0;
		for (int num = __Extension_Method_ICollection__.COUNT(values); i < num; i++)
		{
			GodkingTrialDungeonInfo godkingTrialDungeonInfo = values[i];
			if (chapterIndex != godkingTrialDungeonInfo.ChapterIndex && godkingTrialDungeonInfo.IsOpened)
			{
				return true;
			}
		}
		return false;
	}

	public static void SendRqReset(Action<ResultType> onComplete)
	{
		NShared.RechargeStamina.Request request = new NShared.RechargeStamina.Request();
		request.StaminaType = StaminaType.GodkingTrialKey;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.ShowLoadingGauge, delegate(NShared.RechargeStamina.Response response)
		{
			if (response != null && response.BaseResult == BaseResultType.Success)
			{
				if (response.Result != ResultType.Success)
				{
					Vespa_System_Action_Extension_Method.fire(onComplete, response.Result);
				}
				else
				{
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResult);
					Vespa_System_Action_Extension_Method.fire(onComplete, response.Result);
				}
			}
		});
	}
}
