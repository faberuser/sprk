using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_Logout
{
	public static Logout Parse(string s)
	{
		return Parse(s, new Logout());
	}

	public static Logout Parse(string s, Logout o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static Logout Parse(IDictionary d)
	{
		return Parse(d, new Logout());
	}

	public static Logout Parse(IDictionary d, Logout o)
	{
		return o;
	}

	public static string EncodeJsonString(Logout o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(Logout o)
	{
		return new Dictionary<string, object>();
	}
}
