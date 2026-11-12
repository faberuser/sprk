using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NEvent;

public static class JM_NShared_NEvent_Login
{
	public static Login Parse(string s)
	{
		return Parse(s, new Login());
	}

	public static Login Parse(string s, Login o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static Login Parse(IDictionary d)
	{
		return Parse(d, new Login());
	}

	public static Login Parse(IDictionary d, Login o)
	{
		return o;
	}

	public static string EncodeJsonString(Login o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(Login o)
	{
		return new Dictionary<string, object>();
	}
}
