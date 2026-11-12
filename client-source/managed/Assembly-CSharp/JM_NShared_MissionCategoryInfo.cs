using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_MissionCategoryInfo
{
	public static MissionCategoryInfo Parse(string s)
	{
		return Parse(s, new MissionCategoryInfo());
	}

	public static MissionCategoryInfo Parse(string s, MissionCategoryInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MissionCategoryInfo Parse(IDictionary d)
	{
		return Parse(d, new MissionCategoryInfo());
	}

	public static MissionCategoryInfo Parse(IDictionary d, MissionCategoryInfo o)
	{
		o.MainCategoryIndex = BaseJsonMarshaler.ParseValueInt(d, "MainCategoryIndex");
		o.SubCategoryIndex = BaseJsonMarshaler.ParseValueInt(d, "SubCategoryIndex");
		return o;
	}

	public static string EncodeJsonString(MissionCategoryInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MissionCategoryInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MainCategoryIndex", o.MainCategoryIndex.ToString());
		dictionary.Add("SubCategoryIndex", o.SubCategoryIndex.ToString());
		return dictionary;
	}
}
