using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_AssetBaseInfo_Parse
{
	public static AssetBaseInfo_Parse Parse(string s)
	{
		return Parse(s, new AssetBaseInfo_Parse());
	}

	public static AssetBaseInfo_Parse Parse(string s, AssetBaseInfo_Parse o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static AssetBaseInfo_Parse Parse(IDictionary d)
	{
		return Parse(d, new AssetBaseInfo_Parse());
	}

	public static AssetBaseInfo_Parse Parse(IDictionary d, AssetBaseInfo_Parse o)
	{
		o.AssetName = BaseJsonMarshaler.ParseValueString(d, "AssetName");
		o.Category = BaseJsonMarshaler.ParseValueString(d, "Category");
		o.PriorityValue = BaseJsonMarshaler.ParseValueInt(d, "PriorityValue");
		return o;
	}

	public static string EncodeJsonString(AssetBaseInfo_Parse o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(AssetBaseInfo_Parse o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.AssetName != null)
		{
			dictionary.Add("AssetName", o.AssetName);
		}
		if (o.Category != null)
		{
			dictionary.Add("Category", o.Category);
		}
		dictionary.Add("PriorityValue", o.PriorityValue.ToString());
		return dictionary;
	}
}
