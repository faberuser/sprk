using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetSelectShopInfo;

public static class JM_NShared_GetSelectShopInfo_Request
{
	public static NShared.GetSelectShopInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetSelectShopInfo.Request());
	}

	public static NShared.GetSelectShopInfo.Request Parse(string s, NShared.GetSelectShopInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetSelectShopInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetSelectShopInfo.Request());
	}

	public static NShared.GetSelectShopInfo.Request Parse(IDictionary d, NShared.GetSelectShopInfo.Request o)
	{
		o.CategoryGroup = BaseJsonMarshaler.ParseValueString(d, "CategoryGroup");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetSelectShopInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetSelectShopInfo.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.CategoryGroup != null)
		{
			dictionary.Add("CategoryGroup", o.CategoryGroup);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
