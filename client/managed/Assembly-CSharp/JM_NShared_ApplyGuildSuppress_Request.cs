using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ApplyGuildSuppress;

public static class JM_NShared_ApplyGuildSuppress_Request
{
	public static NShared.ApplyGuildSuppress.Request Parse(string s)
	{
		return Parse(s, new NShared.ApplyGuildSuppress.Request());
	}

	public static NShared.ApplyGuildSuppress.Request Parse(string s, NShared.ApplyGuildSuppress.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ApplyGuildSuppress.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ApplyGuildSuppress.Request());
	}

	public static NShared.ApplyGuildSuppress.Request Parse(IDictionary d, NShared.ApplyGuildSuppress.Request o)
	{
		o.SessionIndex = BaseJsonMarshaler.ParseValueInt(d, "SessionIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ApplyGuildSuppress.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ApplyGuildSuppress.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SessionIndex", o.SessionIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
