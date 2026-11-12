using System.Collections.Generic;
using JsonBin;
using NVespa;

namespace NGameAsset;

public class CameraContainer : SingletonAuto<CameraContainer>
{
	private JsonRead _rootRead;

	private Dictionary<string, CameraData> _cache = new Dictionary<string, CameraData>();

	public CameraData Get(string code)
	{
		if (!_rootRead.IsValid)
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
			value = new CameraData();
			value.LOADFROMJSON(json);
			_cache.Add(code, value);
			return value;
		}
	}

	public void LoadFromJson(JsonRead json)
	{
		_rootRead = json;
		_cache.Clear();
	}
}
