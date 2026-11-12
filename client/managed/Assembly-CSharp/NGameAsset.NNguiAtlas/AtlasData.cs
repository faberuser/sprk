using System.Collections.Generic;
using JsonBin;
using NGameAsset.NAsyncLoad;

namespace NGameAsset.NNguiAtlas;

public class AtlasData
{
	public class ReferenceSpriteData : ILoadJson
	{
		public string SpriteName = string.Empty;

		public string OrigName = string.Empty;

		bool ILoadJson.Load(JsonRead json)
		{
			SpriteName = json["SpriteName"].GETSTRING(SpriteName);
			OrigName = json["OrigName"].GETSTRING(OrigName);
			return true;
		}
	}

	public string Code = string.Empty;

	public string BundleFileName = string.Empty;

	public int Width;

	public int Height;

	public string Prefix = string.Empty;

	public List<string> SpriteList = new List<string>();

	public AssetDataAsync<UIAtlas, AtlasData> CreateAssetAsync()
	{
		AssetDataAsync<UIAtlas, AtlasData> assetDataAsync = new AssetDataAsync<UIAtlas, AtlasData>();
		assetDataAsync.OpenMainAsset(this, BundleFileName);
		return assetDataAsync;
	}

	public bool Open(string code, JsonRead json)
	{
		Code = code;
		if (!json.IsObject())
		{
			return false;
		}
		if (!LoadFromJson(json))
		{
			return false;
		}
		return true;
	}

	private bool LoadFromJson(JsonRead json)
	{
		string sTRING = json["BundleTypeName"].GETSTRING("(empty)");
		string sTRING2 = json["VersionCode"].GETSTRING("(empty)");
		if (!Vespa_Common_Class_Extension_Method.EQUALS(sTRING, "[NguiAtlas]"))
		{
			return false;
		}
		if (!Vespa_Common_Class_Extension_Method.EQUALS(sTRING2, "20160311"))
		{
			return false;
		}
		BundleFileName = json["BundleFileName"].GETSTRING(BundleFileName);
		Width = json["Width"].GETINT(Width);
		Height = json["Height"].GETINT(Height);
		Prefix = json["Prefix"].GETSTRING(Prefix);
		SpriteList.LoadListFromJson(json["SpriteList"]);
		return true;
	}
}
