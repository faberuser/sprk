using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_SelectShopNoticeInfo
{
	public static SelectShopNoticeInfo Parse(string s)
	{
		return Parse(s, new SelectShopNoticeInfo());
	}

	public static SelectShopNoticeInfo Parse(string s, SelectShopNoticeInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static SelectShopNoticeInfo Parse(IDictionary d)
	{
		return Parse(d, new SelectShopNoticeInfo());
	}

	public static SelectShopNoticeInfo Parse(IDictionary d, SelectShopNoticeInfo o)
	{
		o.EndTime = BaseJsonMarshaler.ParseValueString(d, "EndTime");
		o.OpenCategory = BaseJsonMarshaler.ParseValueString(d, "OpenCategory");
		o.CategoryGroups = BaseJsonMarshaler.ParseArrayCast(d, "CategoryGroups", BaseJsonMarshaler.CastToString);
		o.CategoryNameKeys = BaseJsonMarshaler.ParseArrayCast(d, "CategoryNameKeys", BaseJsonMarshaler.CastToString);
		return o;
	}

	public static string EncodeJsonString(SelectShopNoticeInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(SelectShopNoticeInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.EndTime != null)
		{
			dictionary.Add("EndTime", o.EndTime);
		}
		if (o.OpenCategory != null)
		{
			dictionary.Add("OpenCategory", o.OpenCategory);
		}
		dictionary.Add("CategoryGroups", BaseJsonMarshaler.EncodeArray(o.CategoryGroups));
		dictionary.Add("CategoryNameKeys", BaseJsonMarshaler.EncodeArray(o.CategoryNameKeys));
		return dictionary;
	}
}
