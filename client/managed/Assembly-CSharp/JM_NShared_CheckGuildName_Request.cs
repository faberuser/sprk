using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.CheckGuildName;

public static class JM_NShared_CheckGuildName_Request
{
	public static NShared.CheckGuildName.Request Parse(string s)
	{
		return Parse(s, new NShared.CheckGuildName.Request());
	}

	public static NShared.CheckGuildName.Request Parse(string s, NShared.CheckGuildName.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.CheckGuildName.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.CheckGuildName.Request());
	}

	public static NShared.CheckGuildName.Request Parse(IDictionary d, NShared.CheckGuildName.Request o)
	{
		o.name = BaseJsonMarshaler.ParseValueString(d, "name");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.CheckGuildName.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.CheckGuildName.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.name != null)
		{
			dictionary.Add("name", o.name);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
