using System;
using NGame2.NAccount;
using NShared;
using NShared.OpenPunishmentRaid;
using NShared.ResetPunishmentRaid;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NPunish;

internal static class Helper
{
	public static void RequestOpenKarmaDungeon(int groupIndex, Action onSuccess)
	{
		NShared.OpenPunishmentRaid.Request request = new NShared.OpenPunishmentRaid.Request
		{
			GroupIndex = groupIndex,
			DungeonType = 1
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.OpenPunishmentRaid.Response response)
		{
			if (response.Result != NShared.OpenPunishmentRaid.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResult);
				GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.Set(response.PunishmentRaidInfos);
				GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.Apply(response.OpenPunishmentRaidInfo);
				Vespa_System_Action_Extension_Method.fire(onSuccess);
			}
		});
	}

	public static void RequestOpenBoss(int groupIndex, int level, Action onSuccess)
	{
		NShared.OpenPunishmentRaid.Request request = new NShared.OpenPunishmentRaid.Request
		{
			GroupIndex = groupIndex,
			Level = level,
			DungeonType = 2
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.OpenPunishmentRaid.Response response)
		{
			if (response.Result != NShared.OpenPunishmentRaid.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResult);
				GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.Set(response.PunishmentRaidInfos);
				GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.Apply(response.OpenPunishmentRaidInfo);
				Vespa_System_Action_Extension_Method.fire(onSuccess);
			}
		});
	}

	public static void RequestResetBoss(int groupIndex, Action OnSuccess)
	{
		NShared.ResetPunishmentRaid.Request request = new NShared.ResetPunishmentRaid.Request
		{
			GroupIndex = groupIndex,
			DungeonType = 2
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ResetPunishmentRaid.Response response)
		{
			if (response.Result != NShared.ResetPunishmentRaid.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.Set(response.PunishmentRaidInfos);
				GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.Apply(response.OpenPunishmentRaidInfo);
				Vespa_System_Action_Extension_Method.fire(OnSuccess);
			}
		});
	}

	public static void RequestStartDungeon()
	{
	}

	public static void RequesNoticeDungeon()
	{
	}

	public static void RequestEndDungeon()
	{
	}

	public static int GetOpenStamina(PunishmentGroupData groupData)
	{
		int clearCount = GetClearCount(GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.Get(groupData.GroupIndex, groupData.PunishmentDungeonType));
		return GetOpenStaminaCount(groupData, clearCount);
	}

	public static int GetClearCount(OpenPunishmentRaidInfo openPunishmentRaidInfo)
	{
		if (openPunishmentRaidInfo == null)
		{
			return 0;
		}
		if (IsExpireRaid(openPunishmentRaidInfo))
		{
			return 0;
		}
		return openPunishmentRaidInfo.ClearCount;
	}

	private static int GetOpenStaminaCount(PunishmentGroupData groupData, int clearCount)
	{
		int openStaminaCount = groupData.OpenStaminaCount;
		if (clearCount == 0)
		{
			return openStaminaCount;
		}
		int num = openStaminaCount;
		int[] staminaIncreaseCount = groupData.StaminaIncreaseCount;
		if (__Extension_Method_ICollection__.COUNT(staminaIncreaseCount) == 0)
		{
			int staminaIncreaseRate = groupData.StaminaIncreaseRate;
			num = openStaminaCount + clearCount * staminaIncreaseRate;
		}
		else
		{
			for (int i = 0; i < clearCount; i++)
			{
				int num2 = i;
				if (i >= __Extension_Method_ICollection__.COUNT(staminaIncreaseCount))
				{
					num2 = __Extension_Method_ICollection__.COUNT(staminaIncreaseCount) - 1;
				}
				num += staminaIncreaseCount[num2];
			}
		}
		return num;
	}

	public static bool IsExpireRaid(OpenPunishmentRaidInfo openPunishmentRaidInfo)
	{
		if (openPunishmentRaidInfo == null)
		{
			return false;
		}
		return GetExpireRemainTime(openPunishmentRaidInfo.ExpireTime) <= 0;
	}

	public static int GetExpireRemainTime(DateTime expireDateTime)
	{
		return Mathf.Max((int)(expireDateTime - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime()).TotalSeconds, 0);
	}

	public static int GetExpireRemainTime(string expireTime)
	{
		Vespa_String_Extension_Method2.TryParseNetworkTime2(expireTime, out var o_dateTime);
		return Mathf.Max((int)(o_dateTime - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime()).TotalSeconds, 0);
	}

	public static bool IsOpenPunishRaid(OpenPunishmentRaidInfo openPunishmentRaidInfo)
	{
		if (openPunishmentRaidInfo == null || openPunishmentRaidInfo.IsOpen == 0)
		{
			return false;
		}
		return !IsExpireRaid(openPunishmentRaidInfo);
	}
}
