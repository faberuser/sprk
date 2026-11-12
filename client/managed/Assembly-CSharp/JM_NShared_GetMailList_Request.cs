using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetMailList;

public static class JM_NShared_GetMailList_Request
{
	public static NShared.GetMailList.Request Parse(string s)
	{
		return Parse(s, new NShared.GetMailList.Request());
	}

	public static NShared.GetMailList.Request Parse(string s, NShared.GetMailList.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetMailList.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetMailList.Request());
	}

	public static NShared.GetMailList.Request Parse(IDictionary d, NShared.GetMailList.Request o)
	{
		o.LastMailIndex = BaseJsonMarshaler.ParseValueLong(d, "LastMailIndex");
		o.MaxCount = BaseJsonMarshaler.ParseValueInt(d, "MaxCount");
		o.ForceReload = BaseJsonMarshaler.ParseValueBool(d, "ForceReload");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetMailList.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetMailList.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("LastMailIndex", o.LastMailIndex.ToString());
		dictionary.Add("MaxCount", o.MaxCount.ToString());
		dictionary.Add("ForceReload", o.ForceReload.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
