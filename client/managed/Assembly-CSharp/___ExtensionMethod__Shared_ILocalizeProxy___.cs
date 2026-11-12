using System;
using NShared;

public static class ___ExtensionMethod__Shared_ILocalizeProxy___
{
	public static string[] ConvertArrayByCultureInfo(this ILocalizeProxy This, string[] paramArr)
	{
		if (This == null)
		{
			return paramArr;
		}
		if (paramArr == null || paramArr.Length == 0)
		{
			return paramArr;
		}
		string[] array = null;
		string[] array2 = paramArr;
		for (int i = 0; i < paramArr.Length; i++)
		{
			string text = array2[i];
			string text2 = This.ConvertByCultureInfo(text);
			if (!(text2 == text))
			{
				if (array == null)
				{
					array = new string[paramArr.Length];
					Array.Copy(paramArr, array, paramArr.Length);
					array2 = array;
				}
				array2[i] = text2;
			}
		}
		return array2;
	}

	public static string GetStringWithReplaceKey(this ILocalizeProxy This, string origKey, string replaceKey)
	{
		string key = (string.IsNullOrEmpty(replaceKey) ? origKey : replaceKey);
		return This.GetString(key);
	}

	public static string GetStringWithReplaceKey(this ILocalizeProxy This, string origKey, string replaceKey, params object[] values)
	{
		string key = (string.IsNullOrEmpty(replaceKey) ? origKey : replaceKey);
		return This.GetString(key, values);
	}

	public static T GetCache_or_Build<T>(this ILocalizeProxy This, object key, string keyName, Func<T> build) where T : class
	{
		T val = null;
		if (This.TryGetCache(key, keyName, out var value))
		{
			val = value as T;
			if (val != null)
			{
				return val;
			}
		}
		if (build != null)
		{
			val = build();
		}
		return val;
	}
}
