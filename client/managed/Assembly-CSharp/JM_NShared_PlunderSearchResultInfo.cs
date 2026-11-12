using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlunderSearchResultInfo
{
	public static PlunderSearchResultInfo Parse(string s)
	{
		return Parse(s, new PlunderSearchResultInfo());
	}

	public static PlunderSearchResultInfo Parse(string s, PlunderSearchResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlunderSearchResultInfo Parse(IDictionary d)
	{
		return Parse(d, new PlunderSearchResultInfo());
	}

	public static PlunderSearchResultInfo Parse(IDictionary d, PlunderSearchResultInfo o)
	{
		o.AddValue = BaseJsonMarshaler.ParseValueInt(d, "AddValue");
		o.NewValue = BaseJsonMarshaler.ParseValueInt(d, "NewValue");
		return o;
	}

	public static string EncodeJsonString(PlunderSearchResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlunderSearchResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AddValue", o.AddValue.ToString());
		dictionary.Add("NewValue", o.NewValue.ToString());
		return dictionary;
	}
}
