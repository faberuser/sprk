using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.G2R_CheatGuildSuppressMatch;

public static class JM_NShared_G2R_CheatGuildSuppressMatch_Request
{
	public static Request Parse(string s)
	{
		return Parse(s, new Request());
	}

	public static Request Parse(string s, Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static Request Parse(IDictionary d)
	{
		return Parse(d, new Request());
	}

	public static Request Parse(IDictionary d, Request o)
	{
		return o;
	}

	public static string EncodeJsonString(Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(Request o)
	{
		return new Dictionary<string, object>();
	}
}
