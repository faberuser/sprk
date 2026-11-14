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
				// SPRK: cameras the game never had, derived from another camera (see SprkDerivedCameras).
				value = SprkDerivedCameras.Build(code, _rootRead);
				if (value != null)
				{
					_cache.Add(code, value);
				}
				return value;
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
