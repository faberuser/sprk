using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NContentsDefine;

public class CurrencyMaxCap
{
	public static bool ExceedMaxCap(CurrencyType currencyType)
	{
		long vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(ContentsDefine.CurrencyMaxCap.MaxCap, currencyType, 0L);
		if (vALUE <= 0)
		{
			return false;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(currencyType.ToString()) < vALUE)
		{
			return false;
		}
		return true;
	}

	public static List<CurrencyType> SelectExceed(IEnumerable<CurrencyType> currencyTypes)
	{
		List<CurrencyType> list = null;
		if (currencyTypes != null)
		{
			foreach (CurrencyType currencyType in currencyTypes)
			{
				if (ExceedMaxCap(currencyType))
				{
					list = list ?? new List<CurrencyType>();
					list.Add(currencyType);
				}
			}
		}
		return list;
	}

	public static List<CurrencyType> GetExceedMaxCap()
	{
		return SelectExceed(ContentsDefine.CurrencyMaxCap.MaxCap.Keys);
	}

	public static bool ExceedMaxCap(CurrencyType currencyType, long value)
	{
		long vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(ContentsDefine.CurrencyMaxCap.MaxCap, currencyType, 0L);
		if (vALUE <= 0)
		{
			return false;
		}
		if (value < vALUE)
		{
			return false;
		}
		return true;
	}

	public static void OpenExceedWarningPopup(CurrencyType currencyType, Action onOK)
	{
		MaxCapWarningPopup.Open(new List<CurrencyType> { currencyType }, null, onOK, null);
	}

	public static void OpenExceedWarningPopup(List<CurrencyType> currencyList, Action onOK)
	{
		MaxCapWarningPopup.Open(currencyList, null, onOK, null);
	}

	public static void WarningOrExecuteForAll(Action onOK)
	{
		List<CurrencyType> exceedMaxCap = GetExceedMaxCap();
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(exceedMaxCap))
		{
			MaxCapWarningPopup.Open(exceedMaxCap, null, onOK, null);
		}
		else
		{
			Vespa_System_Action_Extension_Method.fire(onOK);
		}
	}

	public static void WarningOrExecute(CurrencyType currencyType, Action onOK)
	{
		if (ExceedMaxCap(currencyType))
		{
			OpenExceedWarningPopup(currencyType, onOK);
		}
		else
		{
			Vespa_System_Action_Extension_Method.fire(onOK);
		}
	}

	public static void WarningOrExecute(IEnumerable<CurrencyType> currencyTypes, Action onOK)
	{
		List<CurrencyType> list = SelectExceed(currencyTypes);
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			OpenExceedWarningPopup(list, onOK);
		}
		else
		{
			Vespa_System_Action_Extension_Method.fire(onOK);
		}
	}
}
