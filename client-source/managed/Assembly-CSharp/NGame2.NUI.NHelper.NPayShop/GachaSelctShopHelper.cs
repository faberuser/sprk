using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NShared;
using NShared.GetSelectShopInfo;
using NShared.RestockSelectShopInfo;
using NVespa;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NHelper.NPayShop;

public static class GachaSelctShopHelper
{
	private static DateTime _customEndtime;

	private static string[] _customGroupArray;

	private static string _groupName;

	private static EGachaSelectShopCategory _eCategory;

	private static RightAwayProductInfo[] __cachedArray;

	public static bool UseEditor { get; set; }

	public static void RequestSelectShopinfo(string categoryName, Action<NShared.GetSelectShopInfo.Response> responseAction)
	{
		if (UseEditor)
		{
			Vespa_System_Action_Extension_Method.fire(responseAction, null);
			return;
		}
		NShared.GetSelectShopInfo.Request request = new NShared.GetSelectShopInfo.Request
		{
			CategoryGroup = categoryName
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetSelectShopInfo.Response response)
		{
			Vespa_System_Action_Extension_Method.fire(responseAction, response);
		});
	}

	public static void RequestStockShopInfo(string categoryName, Action<NShared.RestockSelectShopInfo.Response> responseAction)
	{
		if (UseEditor)
		{
			Vespa_System_Action_Extension_Method.fire(responseAction, null);
			return;
		}
		NShared.RestockSelectShopInfo.Request request = new NShared.RestockSelectShopInfo.Request
		{
			CategoryGroup = categoryName
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.RestockSelectShopInfo.Response response)
		{
			Vespa_System_Action_Extension_Method.fire(responseAction, response);
		});
	}

	public static int GetRemainTimeSec(SelectShopNoticeInfo selectShopNoticeInfo)
	{
		if (selectShopNoticeInfo == null)
		{
			return 0;
		}
		return GetRemainTimeSec(selectShopNoticeInfo.EndTime);
	}

	public static int GetRemainTimeSec(SelectShopInfo selectShopInfo)
	{
		if (selectShopInfo == null)
		{
			return 0;
		}
		return GetRemainTimeSec(selectShopInfo.RestockEndTime);
	}

	private static int GetRemainTimeSec(string endTimeStr)
	{
		if (String_Extension_Method.IsNullOrEmpty(endTimeStr))
		{
			return 0;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(endTimeStr, out var o_dateTime))
		{
			return 0;
		}
		DateTime currentServerUTCTime = GetCurrentServerUTCTime();
		if (o_dateTime.CompareTo(currentServerUTCTime) <= 0)
		{
			return 0;
		}
		return (int)(o_dateTime - currentServerUTCTime).TotalSeconds;
	}

	private static DateTime GetCurrentServerUTCTime()
	{
		if (!AccountManager.IsLogin())
		{
			return DateTime.MinValue;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
	}

	public static void SetRemainTimeLabel(UILabel label, int remainSec, string timeDescKey = "", bool isEndHide = false)
	{
		if (!(label == null))
		{
			bool flag = isEndHide && remainSec == 0;
			UIHelper.EnableGameObject(label, !flag);
			if (!flag)
			{
				string text = "";
				text = ((!String_Extension_Method.IsNullOrEmpty(timeDescKey)) ? LocalizationManager.GetStringDirect(timeDescKey, UIHelper.GetRemainTimeText(remainSec)) : UIHelper.GetRemainTimeText(remainSec));
				UIHelper.SetLabelText(label, text);
			}
		}
	}

	public static ObtainItemType GetObtainType(CurrencyType type, bool isFree)
	{
		switch (type)
		{
		case CurrencyType.Mileage:
		case CurrencyType.LuaPoint:
			return ObtainItemType.BonusPoint;
		case CurrencyType.ShopEventPoint:
			return ObtainItemType.BonusEventShopPoint;
		case CurrencyType.LimitedShopEventPoint:
			return ObtainItemType.BonusLimitedEventShopPoint;
		case CurrencyType.Gold:
			if (isFree)
			{
				return ObtainItemType.BonusGold;
			}
			return ObtainItemType.BaseItem;
		case CurrencyType.Gem:
			if (isFree)
			{
				return ObtainItemType.AddGem;
			}
			return ObtainItemType.BaseGem;
		default:
			return ObtainItemType.BaseItem;
		}
	}

	public static void OnDrawGUI()
	{
	}

	private static void OnDrawUseEditor()
	{
		if (GUILayout.Button(UseEditor ? "UnUseEditor" : "UseEditor"))
		{
			SetUseEdtior(!UseEditor);
		}
	}

	public static void OnDrawRightAway()
	{
		using (new BlockManager().Horizontal())
		{
			if (GUILayout.Button("Remove RightAway"))
			{
				__cachedArray = GlobalAutoBehaviour<AccountManager>.instance.RightAwayProductManager.GetValues();
				GlobalAutoBehaviour<AccountManager>.instance.RightAwayProductManager.Reset(null);
			}
			if (GUILayout.Button("Add RightAway"))
			{
				GlobalAutoBehaviour<AccountManager>.instance.RightAwayProductManager.Apply(__cachedArray, null);
			}
		}
	}

	public static void OnDrawSetTime()
	{
		GUILayout.Label($"Time : {_customEndtime.ToString()}");
		using (new BlockManager().Horizontal())
		{
			if (GUILayout.Button("SetCurrentServerUTCTime"))
			{
				_customEndtime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
			}
			if (GUILayout.Button("+Hours"))
			{
				_customEndtime = _customEndtime.AddHours(1.0);
			}
			if (GUILayout.Button("-Hours"))
			{
				_customEndtime = _customEndtime.AddHours(-1.0);
			}
			if (GUILayout.Button("+Minutes"))
			{
				_customEndtime = _customEndtime.AddMinutes(1.0);
			}
			if (GUILayout.Button("-Minutes"))
			{
				_customEndtime = _customEndtime.AddMinutes(-1.0);
			}
		}
	}

	private static void OnDrawSetGroup()
	{
		using (new BlockManager().Horizontal())
		{
			_eCategory = EnumTools.IngamePopup("Category", _eCategory);
			if (GUILayout.Button("Reset GroupArray"))
			{
				_customGroupArray = null;
			}
			if (GUILayout.Button("Add Array"))
			{
				List<string> list = null;
				list = ((_customGroupArray == null) ? new List<string>() : new List<string>(_customGroupArray));
				list.Add(_groupName);
				_customGroupArray = list.ToArray();
			}
		}
	}

	private static void SetUseEdtior(bool useEditor)
	{
		UseEditor = useEditor;
	}

	private static void OnDrawSetCustomNoticeInfo()
	{
		using (new BlockManager().Horizontal())
		{
			GUI.enabled = !__Extension_Method_ICollection__.IsNullOrEmpty(_customGroupArray);
			if (GUILayout.Button("UseCustomNoticeInfo"))
			{
				GlobalAutoBehaviour<AccountManager>.instance.GachaSelectShopManager.SetCustomNoticeInfo(_customEndtime, _customGroupArray);
			}
			GUI.enabled = true;
			if (GUILayout.Button("ClearCustomNoticeInfo"))
			{
				GlobalAutoBehaviour<AccountManager>.instance.GachaSelectShopManager.ClearSelectShopNoticeInfo();
			}
		}
	}

	private static void OnDrawSetCustomInfo()
	{
	}
}
