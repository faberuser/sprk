using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.DumpDbSchema;

public static class JM_NShared_DumpDbSchema_Request
{
	public static NShared.DumpDbSchema.Request Parse(string s)
	{
		return Parse(s, new NShared.DumpDbSchema.Request());
	}

	public static NShared.DumpDbSchema.Request Parse(string s, NShared.DumpDbSchema.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.DumpDbSchema.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.DumpDbSchema.Request());
	}

	public static NShared.DumpDbSchema.Request Parse(IDictionary d, NShared.DumpDbSchema.Request o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueInt(d, "AccountId");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.DumpDbSchema.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.DumpDbSchema.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
