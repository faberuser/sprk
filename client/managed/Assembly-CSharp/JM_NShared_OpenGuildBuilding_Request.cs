using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.OpenGuildBuilding;

public static class JM_NShared_OpenGuildBuilding_Request
{
	public static NShared.OpenGuildBuilding.Request Parse(string s)
	{
		return Parse(s, new NShared.OpenGuildBuilding.Request());
	}

	public static NShared.OpenGuildBuilding.Request Parse(string s, NShared.OpenGuildBuilding.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.OpenGuildBuilding.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.OpenGuildBuilding.Request());
	}

	public static NShared.OpenGuildBuilding.Request Parse(IDictionary d, NShared.OpenGuildBuilding.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.OpenGuildBuilding.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.OpenGuildBuilding.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
