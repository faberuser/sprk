using UnityEngine;

public static class Vespa_Unity_Asset_Extension_Method
{
	public static string GetName(this AssetBundle i_this)
	{
		if (!String_Extension_Method.IsNullOrEmpty(i_this.name))
		{
			return i_this.name;
		}
		if (i_this.mainAsset != null)
		{
			return i_this.mainAsset.name;
		}
		return string.Empty;
	}
}
