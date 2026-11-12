using System;
using System.Globalization;
using NClientData;
using NClientData.NLocalization;
using NShared;

namespace NVespa.NGlobal;

public class LocalizeProxy_Client : ILocalizeProxy
{
	string ILocalizeProxy.GetString(string key)
	{
		return LocalizationManager.GetStringDirect(key);
	}

	string ILocalizeProxy.GetString(string key, params object[] values)
	{
		return LocalizationManager.GetStringDirect(key, values);
	}

	string ILocalizeProxy.GetEnumString(Enum enumKey, params object[] values)
	{
		return LocalizationManager.GetString(enumKey, values);
	}

	string ILocalizeProxy.ConvertByCultureInfo(string value)
	{
		if (float.TryParse(value, out var result))
		{
			CultureInfo cultureInfo = GameRoot.Instance.LocalizationValueDataContainer.GetCultureInfo();
			return Convert.ToString(result, cultureInfo);
		}
		return value;
	}

	void ILocalizeProxy.AddCache(object keyObj, string keyName, object value)
	{
		SingletonAuto<LocalizationLoader>.instance.GetLocalizeCache()?.AddCache(keyObj, keyName, value);
	}

	bool ILocalizeProxy.TryGetCache(object keyObj, string keyName, out object value)
	{
		LocalizeCache localizeCache = SingletonAuto<LocalizationLoader>.instance.GetLocalizeCache();
		if (localizeCache == null)
		{
			value = null;
			return false;
		}
		return localizeCache.TryGetCache(keyObj, keyName, out value);
	}

	CultureInfo ILocalizeProxy.GetCultureInfo()
	{
		return GameRoot.Instance.LocalizationValueDataContainer.GetCultureInfo();
	}
}
