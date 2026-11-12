using System;
using System.Globalization;

namespace NShared;

public interface ILocalizeProxy
{
	string GetString(string key);

	string GetString(string key, params object[] values);

	string GetEnumString(Enum enumKey, params object[] values);

	string ConvertByCultureInfo(string value);

	void AddCache(object keyObj, string keyName, object value);

	bool TryGetCache(object keyObj, string keyName, out object value);

	CultureInfo GetCultureInfo();
}
