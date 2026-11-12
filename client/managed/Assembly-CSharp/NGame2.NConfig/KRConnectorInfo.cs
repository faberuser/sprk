using System;
using System.Collections.Generic;
using NGame2.NDevice;

namespace NGame2.NConfig;

public class KRConnectorInfo
{
	private Dictionary<string, object> krConnectorJson = new Dictionary<string, object>();

	public KRConnectorInfo()
	{
		LoadConnectorPreference();
	}

	public bool IsInfoExist()
	{
		return krConnectorJson.Count != 0;
	}

	public string GetKRConnectorPreferenceStr(string key)
	{
		if (!krConnectorJson.TryGetValue(key, out var value))
		{
			return null;
		}
		return (string)value;
	}

	public int GetKRConnectorPreferenceInt(string key)
	{
		if (!krConnectorJson.TryGetValue(key, out var value))
		{
			return -1;
		}
		return Convert.ToInt32(value);
	}

	public bool GetKRConnectorPreferenceBool(string key)
	{
		if (!krConnectorJson.TryGetValue(key, out var value))
		{
			return false;
		}
		return Convert.ToBoolean(value);
	}

	public void LoadConnectorPreference()
	{
		string kRConnectorPreference = DeviceHelper.GetKRConnectorPreference();
		if (!string.IsNullOrEmpty(kRConnectorPreference))
		{
			krConnectorJson = MiniJson.Deserialize(kRConnectorPreference) as Dictionary<string, object>;
		}
	}
}
