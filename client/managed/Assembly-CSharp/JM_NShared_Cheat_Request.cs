using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.Cheat;

public static class JM_NShared_Cheat_Request
{
	public static NShared.Cheat.Request Parse(string s)
	{
		return Parse(s, new NShared.Cheat.Request());
	}

	public static NShared.Cheat.Request Parse(string s, NShared.Cheat.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.Cheat.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.Cheat.Request());
	}

	public static NShared.Cheat.Request Parse(IDictionary d, NShared.Cheat.Request o)
	{
		o.Command = BaseJsonMarshaler.ParseValueString(d, "Command");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.Cheat.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.Cheat.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Command != null)
		{
			dictionary.Add("Command", o.Command);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
