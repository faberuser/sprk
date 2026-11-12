using System.Collections.Generic;
using JsonBin;

namespace NGameAsset.NBundleManager;

public class BundleInfoContainer
{
	private Dictionary<string, BundleFileInfo> _cache = new Dictionary<string, BundleFileInfo>();

	public BundleFileInfo GetInfo(string filename)
	{
		return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_cache, filename.ToLower(), null);
	}

	public void LoadFromJson(JsonRead json)
	{
		LoadBundleFileInfo(json);
	}

	private void LoadBundleFileInfo(JsonRead json)
	{
		if (!json.IsArray())
		{
			return;
		}
		for (int i = 0; i < json.Count; i++)
		{
			JsonRead fromArray = json.GetFromArray(i);
			BundleFileInfo bundleFileInfo = new BundleFileInfo();
			bundleFileInfo.LOADFROMJSON(fromArray);
			if (!String_Extension_Method.IsNullOrEmpty(bundleFileInfo.Name))
			{
				string key = bundleFileInfo.Name.ToLower();
				if (!_cache.ContainsKey(key))
				{
					_cache.Add(key, bundleFileInfo);
				}
			}
		}
	}
}
