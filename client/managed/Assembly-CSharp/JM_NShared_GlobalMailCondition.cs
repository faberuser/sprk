using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GlobalMailCondition
{
	public static GlobalMailCondition Parse(string s)
	{
		return Parse(s, new GlobalMailCondition());
	}

	public static GlobalMailCondition Parse(string s, GlobalMailCondition o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GlobalMailCondition Parse(IDictionary d)
	{
		return Parse(d, new GlobalMailCondition());
	}

	public static GlobalMailCondition Parse(IDictionary d, GlobalMailCondition o)
	{
		o.CountryCode = BaseJsonMarshaler.ParseValueString(d, "CountryCode");
		o.Language = BaseJsonMarshaler.ParseValueString(d, "Language");
		o.DevicePlatform = BaseJsonMarshaler.ParseValueString(d, "DevicePlatform");
		o.CreatedTimeBegin = BaseJsonMarshaler.ParseValueString(d, "CreatedTimeBegin");
		o.CreatedTimeEnd = BaseJsonMarshaler.ParseValueString(d, "CreatedTimeEnd");
		o.GroupIndex = BaseJsonMarshaler.ParseValueInt(d, "GroupIndex");
		return o;
	}

	public static string EncodeJsonString(GlobalMailCondition o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GlobalMailCondition o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.CountryCode != null)
		{
			dictionary.Add("CountryCode", o.CountryCode);
		}
		if (o.Language != null)
		{
			dictionary.Add("Language", o.Language);
		}
		if (o.DevicePlatform != null)
		{
			dictionary.Add("DevicePlatform", o.DevicePlatform);
		}
		if (o.CreatedTimeBegin != null)
		{
			dictionary.Add("CreatedTimeBegin", o.CreatedTimeBegin);
		}
		if (o.CreatedTimeEnd != null)
		{
			dictionary.Add("CreatedTimeEnd", o.CreatedTimeEnd);
		}
		dictionary.Add("GroupIndex", o.GroupIndex.ToString());
		return dictionary;
	}
}
