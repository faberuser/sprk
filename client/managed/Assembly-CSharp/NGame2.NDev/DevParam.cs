using System;
using System.Collections.Generic;

namespace NGame2.NDev;

public class DevParam
{
	private static Dictionary<string, string> _devParams = new Dictionary<string, string>();

	public static void SetDevParam(string devParam)
	{
		_devParams.Clear();
		if (devParam == null)
		{
			return;
		}
		string[] array = devParam.Split(new char[1] { ',' });
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(array); i++)
		{
			string[] array2 = array[i].Split(new char[1] { '=' });
			if (array2.Length >= 2)
			{
				string key = array2[0];
				string value = array2[1];
				_devParams[key] = value;
			}
		}
	}

	public static string GetDevParam(string key)
	{
		string value = null;
		_devParams.TryGetValue(key, out value);
		return value;
	}

	public static bool GetDevParamBool(string key)
	{
		string devParam = GetDevParam(key);
		if (devParam == null)
		{
			return false;
		}
		return string.Equals(devParam, "true", StringComparison.InvariantCultureIgnoreCase);
	}

	public static int GetDevParamInt(string key)
	{
		string devParam = GetDevParam(key);
		if (devParam == null)
		{
			return 0;
		}
		int result = 0;
		if (int.TryParse(devParam, out result))
		{
			return result;
		}
		return 0;
	}
}
