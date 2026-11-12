using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NEvent;

public static class JM_NShared_NEvent_OpenRaidSelect
{
	public static OpenRaidSelect Parse(string s)
	{
		return Parse(s, new OpenRaidSelect());
	}

	public static OpenRaidSelect Parse(string s, OpenRaidSelect o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OpenRaidSelect Parse(IDictionary d)
	{
		return Parse(d, new OpenRaidSelect());
	}

	public static OpenRaidSelect Parse(IDictionary d, OpenRaidSelect o)
	{
		return o;
	}

	public static string EncodeJsonString(OpenRaidSelect o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OpenRaidSelect o)
	{
		return new Dictionary<string, object>();
	}
}
