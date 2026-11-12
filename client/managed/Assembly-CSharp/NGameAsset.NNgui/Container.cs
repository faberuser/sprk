using System.Collections.Generic;
using JsonBin;
using NVespa;

namespace NGameAsset.NNgui;

public class Container : SingletonAuto<Container>
{
	public const string BundlePrefix = "Ngui.";

	private JsonRead _rootRead;

	private Dictionary<string, NguiData> _cache = new Dictionary<string, NguiData>();

	public INguiPrefab LoadPrefab(string code)
	{
		NguiData nguiData = Get(code);
		if (nguiData == null)
		{
			return null;
		}
		NguiPrefab nguiPrefab = new NguiPrefab();
		nguiPrefab.Open(nguiData);
		return nguiPrefab;
	}

	public NguiData Get(string requestCode)
	{
		if (String_Extension_Method.IsNullOrEmpty(requestCode))
		{
			return null;
		}
		lock (_cache)
		{
			string text = "Ngui." + requestCode;
			if (_cache.TryGetValue(text, out var value))
			{
				return value;
			}
			JsonRead json = _rootRead[text];
			if (!json.IsObject())
			{
				return null;
			}
			value = new NguiData();
			value.Open(text, json);
			_cache.Add(text, value);
			return value;
		}
	}

	public void Load(JsonRead read)
	{
		_rootRead = read;
	}
}
