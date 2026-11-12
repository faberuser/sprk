using System.Collections.Generic;
using JsonBin;
using NVespa;

namespace NGameAsset.NWorldMap;

public class Container : SingletonAuto<Container>
{
	public const string BundlePrefix = "WorldMap.";

	private JsonRead _rootRead;

	private Dictionary<string, WorldMapData> _cache = new Dictionary<string, WorldMapData>();

	public IWorldMapLoad CreateLoad(string code)
	{
		WorldMapData worldMapData = Get(code);
		if (worldMapData == null)
		{
			return null;
		}
		WorldMapLoad worldMapLoad = new WorldMapLoad();
		worldMapLoad.Open(worldMapData);
		return worldMapLoad;
	}

	public WorldMapData Get(string requestCode)
	{
		if (String_Extension_Method.IsNullOrEmpty(requestCode))
		{
			return null;
		}
		string text = "WorldMap." + requestCode;
		lock (_cache)
		{
			if (_cache.TryGetValue(text, out var value))
			{
				return value;
			}
			if (!_rootRead[text].IsObject())
			{
				return null;
			}
			value = new WorldMapData();
			value.Open(text, _rootRead[text]);
			_cache.Add(text, value);
			return value;
		}
	}

	public void Load(JsonRead read)
	{
		_rootRead = read;
	}
}
