using System;
using System.Collections.Generic;
using JsonBin;
using NVespa;

namespace NGameAsset.NSound;

public class BgmDataContainer : SingletonAuto<BgmDataContainer>
{
	private JsonRead _rootRead;

	private Dictionary<string, BgmData> _cache = new Dictionary<string, BgmData>(StringComparer.OrdinalIgnoreCase);

	public BgmData GetBgmData(string code)
	{
		if (String_Extension_Method.IsNullOrEmpty(code))
		{
			return null;
		}
		lock (_cache)
		{
			if (_cache.TryGetValue(code, out var value))
			{
				return value;
			}
			JsonRead json = _rootRead[code];
			if (!json.IsObject())
			{
				return null;
			}
			value = new BgmData();
			value.LOADFROMJSON(json);
			_cache.Add(code, value);
			return value;
		}
	}

	public void LoadFromJson(JsonRead json)
	{
		_rootRead = json;
	}
}
