using System;
using System.Globalization;

namespace NShared;

public class BypassLocalizeProxy : ILocalizeProxy
{
	string ILocalizeProxy.GetString(string key)
	{
		return key;
	}

	string ILocalizeProxy.GetString(string key, params object[] values)
	{
		return key;
	}

	string ILocalizeProxy.ConvertByCultureInfo(string value)
	{
		return value;
	}

	string ILocalizeProxy.GetEnumString(Enum enumKey, params object[] values)
	{
		return enumKey.ToString();
	}

	void ILocalizeProxy.AddCache(object keyObj, string keyName, object value)
	{
	}

	bool ILocalizeProxy.TryGetCache(object keyObj, string keyName, out object value)
	{
		value = null;
		return false;
	}

	CultureInfo ILocalizeProxy.GetCultureInfo()
	{
		return CultureInfo.CurrentCulture;
	}
}
