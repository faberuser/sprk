using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NEvent;

public static class JM_NShared_NEvent_OpenValanceRestore
{
	public static OpenValanceRestore Parse(string s)
	{
		return Parse(s, new OpenValanceRestore());
	}

	public static OpenValanceRestore Parse(string s, OpenValanceRestore o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OpenValanceRestore Parse(IDictionary d)
	{
		return Parse(d, new OpenValanceRestore());
	}

	public static OpenValanceRestore Parse(IDictionary d, OpenValanceRestore o)
	{
		return o;
	}

	public static string EncodeJsonString(OpenValanceRestore o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OpenValanceRestore o)
	{
		return new Dictionary<string, object>();
	}
}
