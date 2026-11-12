using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_HouseObjectSkinResultInfo
{
	public static HouseObjectSkinResultInfo Parse(string s)
	{
		return Parse(s, new HouseObjectSkinResultInfo());
	}

	public static HouseObjectSkinResultInfo Parse(string s, HouseObjectSkinResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static HouseObjectSkinResultInfo Parse(IDictionary d)
	{
		return Parse(d, new HouseObjectSkinResultInfo());
	}

	public static HouseObjectSkinResultInfo Parse(IDictionary d, HouseObjectSkinResultInfo o)
	{
		o.SlotIndex = BaseJsonMarshaler.ParseValueInt(d, "SlotIndex");
		o.SkinIndex = BaseJsonMarshaler.ParseValueInt(d, "SkinIndex");
		return o;
	}

	public static string EncodeJsonString(HouseObjectSkinResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(HouseObjectSkinResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SlotIndex", o.SlotIndex.ToString());
		dictionary.Add("SkinIndex", o.SkinIndex.ToString());
		return dictionary;
	}
}
