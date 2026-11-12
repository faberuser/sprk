using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetMailListByPage;

public static class JM_NShared_GetMailListByPage_Request
{
	public static NShared.GetMailListByPage.Request Parse(string s)
	{
		return Parse(s, new NShared.GetMailListByPage.Request());
	}

	public static NShared.GetMailListByPage.Request Parse(string s, NShared.GetMailListByPage.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetMailListByPage.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetMailListByPage.Request());
	}

	public static NShared.GetMailListByPage.Request Parse(IDictionary d, NShared.GetMailListByPage.Request o)
	{
		o.PageNo = BaseJsonMarshaler.ParseValueInt(d, "PageNo");
		o.MaxCount = BaseJsonMarshaler.ParseValueInt(d, "MaxCount");
		o.ForceReload = BaseJsonMarshaler.ParseValueBool(d, "ForceReload");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetMailListByPage.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetMailListByPage.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("PageNo", o.PageNo.ToString());
		dictionary.Add("MaxCount", o.MaxCount.ToString());
		dictionary.Add("ForceReload", o.ForceReload.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
