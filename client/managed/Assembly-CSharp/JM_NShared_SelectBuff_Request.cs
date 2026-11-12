using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SelectBuff;

public static class JM_NShared_SelectBuff_Request
{
	public static NShared.SelectBuff.Request Parse(string s)
	{
		return Parse(s, new NShared.SelectBuff.Request());
	}

	public static NShared.SelectBuff.Request Parse(string s, NShared.SelectBuff.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SelectBuff.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SelectBuff.Request());
	}

	public static NShared.SelectBuff.Request Parse(IDictionary d, NShared.SelectBuff.Request o)
	{
		o.BuffIndex = BaseJsonMarshaler.ParseValueInt(d, "BuffIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SelectBuff.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SelectBuff.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("BuffIndex", o.BuffIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
