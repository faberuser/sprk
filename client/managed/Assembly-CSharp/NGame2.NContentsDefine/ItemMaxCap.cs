using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NContentsDefine;

public class ItemMaxCap
{
	public static void OpenExceedWarningPopup(List<CurrencyType> currencyList, List<int> itemIndexList, Action onOK)
	{
		MaxCapWarningPopup.Open(currencyList, itemIndexList, onOK, null);
	}

	public static void WarningOrExecuteForAllMail(Action onOK)
	{
		List<CurrencyType> exceedMaxCap = CurrencyMaxCap.GetExceedMaxCap();
		List<int> itemIndices_of_ExceedMaxCap = GetItemIndices_of_ExceedMaxCap();
		if (__Extension_Method_ICollection__.COUNT(exceedMaxCap) > 0 || __Extension_Method_ICollection__.COUNT(itemIndices_of_ExceedMaxCap) > 0)
		{
			MaxCapWarningPopup.OpenForAllMail(exceedMaxCap, itemIndices_of_ExceedMaxCap, onOK, null);
		}
		else
		{
			Vespa_System_Action_Extension_Method.fire(onOK);
		}
	}

	public static List<int> GetItemIndices_of_ExceedMaxCap()
	{
		return SelectExceed(GameRoot.Instance.ItemDataContainer.GetMaxCapIndices());
	}

	public static List<int> SelectExceed(IEnumerable<int> itemIndices)
	{
		List<int> list = null;
		if (itemIndices == null)
		{
			return list;
		}
		foreach (int itemIndex in itemIndices)
		{
			if (ExceedMaxCap(itemIndex))
			{
				list = list ?? new List<int>();
				list.Add(itemIndex);
			}
		}
		return list;
	}

	public static bool ExceedMaxCap(int itemIndex)
	{
		int maxCapCount = GetMaxCapCount(itemIndex);
		if (maxCapCount <= 0)
		{
			return false;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(itemIndex) < maxCapCount)
		{
			return false;
		}
		return true;
	}

	public static int GetMaxCapCount(int itemIndex)
	{
		return GameRoot.Instance.ItemDataContainer.GetData(itemIndex)?.ItemMaxCap ?? 0;
	}

	public static void WarningOrExecute(IEnumerable<CurrencyType> currencyTypes, IEnumerable<int> itemIndices, Action onOK)
	{
		List<CurrencyType> list = CurrencyMaxCap.SelectExceed(currencyTypes);
		List<int> list2 = SelectExceed(itemIndices);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list) && __Extension_Method_ICollection__.IsNullOrEmpty(list2))
		{
			Vespa_System_Action_Extension_Method.fire(onOK);
		}
		else
		{
			OpenExceedWarningPopup(list, list2, onOK);
		}
	}
}
