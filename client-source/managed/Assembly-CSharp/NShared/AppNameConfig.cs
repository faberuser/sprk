using System;
using System.Collections.Generic;

namespace NShared;

[JsonSerializable]
public class AppNameConfig
{
	public string Name { get; set; }

	public string[] BundleNames { get; set; }

	public string[] BundleLangs { get; set; }

	public string GetAppNameByLanguage(string lang)
	{
		string result = string.Empty;
		if (BundleNames.Length != 0 && BundleLangs.Length != 0)
		{
			int num = 0;
			for (int i = 0; i < BundleLangs.Length; i++)
			{
				if (BundleNames[i].Equals(lang, StringComparison.OrdinalIgnoreCase))
				{
					num = i;
					break;
				}
			}
			result = BundleNames[num];
		}
		return result;
	}

	public Dictionary<string, string> GetAppNameConfigDictionary()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (BundleNames.Length != 0 && BundleLangs.Length != 0)
		{
			for (int i = 0; i < BundleLangs.Length; i++)
			{
				if (BundleLangs.Length > i && BundleNames.Length > i)
				{
					dictionary.Add(BundleLangs[i], BundleNames[i]);
				}
			}
		}
		return dictionary;
	}
}
