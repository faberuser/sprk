using System;
using NGame2.NAccount;
using NShared;
using NShared.NUtil;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class Count : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 4)
		{
			return false;
		}
		string text = "";
		int result = 0;
		CheckType checkType = CheckType.None;
		if (String_Extension_Method.IsNullOrEmpty(values[1]) || !int.TryParse(values[2], out result) || String_Extension_Method.IsNullOrEmpty(values[3]))
		{
			return false;
		}
		CurrencyType type = CurrencyType.None;
		text = values[1];
		bool flag = false;
		if (!CheckItemDataByCode(text))
		{
			if (!CheckCurrencyTypeByCode(text, out type))
			{
				return false;
			}
			flag = true;
		}
		if (!SingletonAuto<ConditionChecker>.instance.CheckGetCheckType(values[3], ref checkType))
		{
			return false;
		}
		if (flag)
		{
			long userValue = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(type.ToString());
			if (!SingletonAuto<ConditionChecker>.instance.CheckCheckType(checkType, result, userValue))
			{
				return false;
			}
		}
		else
		{
			int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(text);
			long userValue2 = itemCount;
			if (!SingletonAuto<ConditionChecker>.instance.CheckCheckType(checkType, result, userValue2))
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckItemDataByCode(string code)
	{
		return GameRoot.Instance.ItemDataContainer.GetDataByCode(code) != null;
	}

	private bool CheckCurrencyTypeByCode(string code, out CurrencyType type)
	{
		CurrencyType currencyType = CurrencyType.None;
		try
		{
			currencyType = (CurrencyType)Enum.Parse(typeof(CurrencyType), code);
		}
		catch
		{
		}
		type = currencyType;
		return type != CurrencyType.None;
	}
}
