using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetGuildSuppressStatusBoard;

public static class JM_NShared_GetGuildSuppressStatusBoard_Request
{
	public static NShared.GetGuildSuppressStatusBoard.Request Parse(string s)
	{
		return Parse(s, new NShared.GetGuildSuppressStatusBoard.Request());
	}

	public static NShared.GetGuildSuppressStatusBoard.Request Parse(string s, NShared.GetGuildSuppressStatusBoard.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetGuildSuppressStatusBoard.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetGuildSuppressStatusBoard.Request());
	}

	public static NShared.GetGuildSuppressStatusBoard.Request Parse(IDictionary d, NShared.GetGuildSuppressStatusBoard.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetGuildSuppressStatusBoard.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetGuildSuppressStatusBoard.Request o)
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
