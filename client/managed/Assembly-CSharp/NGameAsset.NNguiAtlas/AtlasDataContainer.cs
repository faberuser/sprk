using System.Collections.Generic;
using JsonBin;
using NVespa;

namespace NGameAsset.NNguiAtlas;

public class AtlasDataContainer : SingletonAuto<AtlasDataContainer>
{
	public class AtlasLink : ILoadJson
	{
		public string AtlasCode = string.Empty;

		public string SpriteName = string.Empty;

		public static AtlasLink Null = new AtlasLink();

		bool ILoadJson.Load(JsonRead json)
		{
			AtlasCode = json["BundlePath"].GETSTRING(AtlasCode);
			SpriteName = json["Name"].GETSTRING(SpriteName);
			return true;
		}
	}

	public const string BundlePrefix = "NguiAtlas.";

	private JsonRead _atlasRootRead;

	private JsonRead _assetRootRead;

	private Dictionary<string, AtlasData> _cache = new Dictionary<string, AtlasData>();

	private IncreaseDic<string, AtlasLink> _spriteCache = new IncreaseDic<string, AtlasLink>();

	public AtlasLink GetAtlasLink(string spritePath)
	{
		if (String_Extension_Method.IsNullOrEmpty(spritePath))
		{
			return null;
		}
		lock (_spriteCache)
		{
			if (_spriteCache.TryGetValue(spritePath, out var o_value))
			{
				return o_value;
			}
			JsonRead json = _assetRootRead[spritePath];
			if (!json.IsObject())
			{
				return null;
			}
			o_value = new AtlasLink();
			o_value.LOADFROMJSON(json);
			_spriteCache.Add(spritePath, o_value);
			return o_value;
		}
	}

	public AtlasData Get(string requestCode)
	{
		if (String_Extension_Method.IsNullOrEmpty(requestCode))
		{
			return null;
		}
		string text = ((!requestCode.StartsWith("NguiAtlas.")) ? ("NguiAtlas." + requestCode) : requestCode);
		lock (_cache)
		{
			if (_cache.TryGetValue(text, out var value))
			{
				return value;
			}
			JsonRead json = _atlasRootRead[text];
			if (!json.IsObject())
			{
				return null;
			}
			value = new AtlasData();
			value.Open(text, json);
			_cache.Add(text, value);
			return value;
		}
	}

	public void Load(JsonRead read, JsonRead assetRead)
	{
		_atlasRootRead = read;
		_assetRootRead = assetRead;
	}
}
