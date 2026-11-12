using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NWindow;
using NShared;
using NShared.BuyShopItem;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NHelper.NEventGrowWorldTree;

public static class EventGrowWorldTreeHelper
{
	public static void SetRewardStep(EventStepType type, int step)
	{
		NGame2.NAccount.UserEventGrowWorldTreeManager eventGrowWorldTreeManager = GlobalAutoBehaviour<AccountManager>.instance.EventGrowWorldTreeManager;
		if (eventGrowWorldTreeManager.PlayerEventStepInfo != null)
		{
			switch (type)
			{
			case EventStepType.General:
				eventGrowWorldTreeManager.PlayerEventStepInfo.MissionStep = step;
				break;
			case EventStepType.Group:
				eventGrowWorldTreeManager.PlayerEventStepInfo.GroupTotalStep = step;
				break;
			}
		}
	}

	public static EventStepInfo GetPlayerEventStepInfo()
	{
		NGame2.NAccount.UserEventGrowWorldTreeManager eventGrowWorldTreeManager = GlobalAutoBehaviour<AccountManager>.instance.EventGrowWorldTreeManager;
		if (eventGrowWorldTreeManager == null)
		{
			VespaDebug.LogError("[EventGrowWorldTreeHelper] GrowWorldTreeUserManager is Null!!");
			return null;
		}
		EventStepInfo playerEventStepInfo = eventGrowWorldTreeManager.PlayerEventStepInfo;
		if (playerEventStepInfo == null)
		{
			return null;
		}
		return playerEventStepInfo;
	}

	public static int GetCurrentStep(EventStepType eventStepType)
	{
		EventStepInfo playerEventStepInfo = GetPlayerEventStepInfo();
		if (playerEventStepInfo == null)
		{
			return 0;
		}
		return eventStepType switch
		{
			EventStepType.Daily => playerEventStepInfo.DailyStep, 
			EventStepType.General => playerEventStepInfo.MissionStep, 
			EventStepType.Group => playerEventStepInfo.GroupCurrentStep, 
			_ => -1, 
		};
	}

	public static long GetTotalPoint(EventStepType eventStepType)
	{
		EventStepInfo playerEventStepInfo = GetPlayerEventStepInfo();
		switch (eventStepType)
		{
		case EventStepType.Daily:
		{
			PlayerCurrencyInfo playerCurrencyInfo = GetPlayerCurrencyInfo(CurrencyType.GrowWorldTreePoint);
			if (playerCurrencyInfo != null)
			{
				return playerCurrencyInfo.Amount;
			}
			int num4 = 0;
			return num4;
		}
		case EventStepType.General:
		{
			if (playerEventStepInfo != null)
			{
				int missionCount = playerEventStepInfo.MissionCount;
				return missionCount;
			}
			int num3 = -1;
			return num3;
		}
		case EventStepType.Group:
		{
			if (playerEventStepInfo != null)
			{
				int groupTotalCount = playerEventStepInfo.GroupTotalCount;
				return groupTotalCount;
			}
			int num2 = -1;
			return num2;
		}
		default:
		{
			int num = -1;
			return num;
		}
		}
	}

	public static EventStepData GetCurrentStepData(EventStepType eventStepType)
	{
		int currentStep = GetCurrentStep(eventStepType);
		currentStep = ((eventStepType == EventStepType.Group) ? (currentStep + 1) : currentStep);
		return GetEventStepData(eventStepType, currentStep);
	}

	public static EventStepData GetEventStepData(EventStepType eventStepType, int step)
	{
		return GameRoot.Instance.EventStepDataContainer?.GetEventStepData(CurrencyType.GrowWorldTreePoint, eventStepType, step);
	}

	public static int GetTargetScore(EventStepType eventStepType, int step)
	{
		if (eventStepType == EventStepType.Group)
		{
			return GetPlayerEventStepInfo()?.GroupTargetScore ?? (-1);
		}
		return GetEventStepData(eventStepType, step)?.TargetScore ?? (-1);
	}

	public static PlayerCurrencyInfo GetPlayerCurrencyInfo(CurrencyType currencyType)
	{
		if (!AccountManager.IsLogin())
		{
			return null;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.Get(currencyType);
	}

	public static long GetGrowWorldTreeEventCoin()
	{
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		if (instance == null)
		{
			int num = -1;
			return num;
		}
		UserManager userManager = instance.UserManager;
		if (userManager == null)
		{
			int num2 = -1;
			return num2;
		}
		return userManager.EventDungeonPoint;
	}

	public static int GetEventStepTypeDataCount(EventStepType eventStepType)
	{
		return GameRoot.Instance.EventStepDataContainer?.GetMissionMaxStep(CurrencyType.GrowWorldTreePoint, eventStepType) ?? (-1);
	}

	public static DateTime GetTime(bool isUseLocalTime = true)
	{
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		if (instance == null)
		{
			return default(DateTime);
		}
		if (isUseLocalTime)
		{
			return instance.GetCurrentServerLocalTime();
		}
		return instance.GetCurrentServerUTCTime();
	}

	public static bool IsOpenContents()
	{
		if (!GlobalAutoBehaviour<DropBonusEventManager>.isValidInstance || GlobalAutoBehaviour<DropBonusEventManager>.instance == null)
		{
			return false;
		}
		List<DropBonusEvent> effectiveEvents = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvents(EventBonusType.GrowWorldTree);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(effectiveEvents))
		{
			return false;
		}
		foreach (DropBonusEvent item in effectiveEvents)
		{
			if (Vespa_String_Extension_Method2.TryParseNetworkTime2(item.EndTime, out var o_dateTime) && (int)(o_dateTime - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime()).TotalSeconds > 0)
			{
				return true;
			}
		}
		return false;
	}

	public static void OpenErrorPopup(string error = "")
	{
		string stringDirect = LocalizationManager.GetStringDirect("EVENT_GROW_WORLD_TREE_TITLE");
		string message = LocalizationManager.GetString(ResultType.CannotBuyNoEvent);
		MessagePopup.Open(stringDirect, message);
	}

	public static void ForceCloseEventWindow(string error = "", Action onClick = null)
	{
		string stringDirect = LocalizationManager.GetStringDirect("EVENT_GROW_WORLD_TREE_TITLE");
		string message = LocalizationManager.GetString(ResultType.CannotBuyNoEvent);
		GlobalMessagePopup.OpenGlobal(stringDirect, message, onClick, retryButton: false);
	}
}
