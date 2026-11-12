using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NEvent;

public static class JM_NShared_NEvent_UsePortal
{
	public static UsePortal Parse(string s)
	{
		return Parse(s, new UsePortal());
	}

	public static UsePortal Parse(string s, UsePortal o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static UsePortal Parse(IDictionary d)
	{
		return Parse(d, new UsePortal());
	}

	public static UsePortal Parse(IDictionary d, UsePortal o)
	{
		return o;
	}

	public static string EncodeJsonString(UsePortal o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(UsePortal o)
	{
		return new Dictionary<string, object>();
	}
}
