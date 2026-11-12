using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_RightAwayProductInfo
{
	public static RightAwayProductInfo Parse(string s)
	{
		return Parse(s, new RightAwayProductInfo());
	}

	public static RightAwayProductInfo Parse(string s, RightAwayProductInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RightAwayProductInfo Parse(IDictionary d)
	{
		return Parse(d, new RightAwayProductInfo());
	}

	public static RightAwayProductInfo Parse(IDictionary d, RightAwayProductInfo o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.EndTime = BaseJsonMarshaler.ParseValueString(d, "EndTime");
		o.LimitedQuantity = BaseJsonMarshaler.ParseValueInt(d, "LimitedQuantity");
		o.BannerPath = BaseJsonMarshaler.ParseValueString(d, "BannerPath");
		o.ShowIndex = BaseJsonMarshaler.ParseValueInt(d, "ShowIndex");
		return o;
	}

	public static string EncodeJsonString(RightAwayProductInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RightAwayProductInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		if (o.EndTime != null)
		{
			dictionary.Add("EndTime", o.EndTime);
		}
		dictionary.Add("LimitedQuantity", o.LimitedQuantity.ToString());
		if (o.BannerPath != null)
		{
			dictionary.Add("BannerPath", o.BannerPath);
		}
		dictionary.Add("ShowIndex", o.ShowIndex.ToString());
		return dictionary;
	}
}
