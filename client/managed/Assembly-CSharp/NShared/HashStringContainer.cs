using System.Collections.Generic;

namespace NShared;

public static class HashStringContainer
{
	private static Dictionary<string, string> _keyDic = new Dictionary<string, string>();

	public static int Count => _keyDic.Count;

	public static string GetHashString(string value)
	{
		if (value == null)
		{
			return string.Empty;
		}
		lock (_keyDic)
		{
			if (!_keyDic.TryGetValue(value, out var value2))
			{
				value2 = value;
				_keyDic.Add(value, value2);
			}
			return value2;
		}
	}
}
