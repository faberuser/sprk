using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PatchProfileData
{
	public static PatchProfileData Parse(string s)
	{
		return Parse(s, new PatchProfileData());
	}

	public static PatchProfileData Parse(string s, PatchProfileData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PatchProfileData Parse(IDictionary d)
	{
		return Parse(d, new PatchProfileData());
	}

	public static PatchProfileData Parse(IDictionary d, PatchProfileData o)
	{
		o.Profile = BaseJsonMarshaler.ParseValueString(d, "Profile");
		o.AssetRoots = BaseJsonMarshaler.ParseArray(d, "AssetRoots", JM_NShared_PatchAssetRootData.Parse);
		return o;
	}

	public static string EncodeJsonString(PatchProfileData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PatchProfileData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Profile != null)
		{
			dictionary.Add("Profile", o.Profile);
		}
		dictionary.Add("AssetRoots", BaseJsonMarshaler.EncodeArray(o.AssetRoots, JM_NShared_PatchAssetRootData.EncodeJsonObject));
		return dictionary;
	}
}
