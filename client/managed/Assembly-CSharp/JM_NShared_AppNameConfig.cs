using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_AppNameConfig
{
	public static AppNameConfig Parse(string s)
	{
		return Parse(s, new AppNameConfig());
	}

	public static AppNameConfig Parse(string s, AppNameConfig o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static AppNameConfig Parse(IDictionary d)
	{
		return Parse(d, new AppNameConfig());
	}

	public static AppNameConfig Parse(IDictionary d, AppNameConfig o)
	{
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.BundleNames = BaseJsonMarshaler.ParseArrayCast(d, "BundleNames", BaseJsonMarshaler.CastToString);
		o.BundleLangs = BaseJsonMarshaler.ParseArrayCast(d, "BundleLangs", BaseJsonMarshaler.CastToString);
		return o;
	}

	public static string EncodeJsonString(AppNameConfig o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(AppNameConfig o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Name != null)
		{
			dictionary.Add("Name", o.Name);
		}
		dictionary.Add("BundleNames", BaseJsonMarshaler.EncodeArray(o.BundleNames));
		dictionary.Add("BundleLangs", BaseJsonMarshaler.EncodeArray(o.BundleLangs));
		return dictionary;
	}
}
