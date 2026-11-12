using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NEvent;

public static class JM_NShared_NEvent_OpenEclipseDungeonUI
{
	public static OpenEclipseDungeonUI Parse(string s)
	{
		return Parse(s, new OpenEclipseDungeonUI());
	}

	public static OpenEclipseDungeonUI Parse(string s, OpenEclipseDungeonUI o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OpenEclipseDungeonUI Parse(IDictionary d)
	{
		return Parse(d, new OpenEclipseDungeonUI());
	}

	public static OpenEclipseDungeonUI Parse(IDictionary d, OpenEclipseDungeonUI o)
	{
		return o;
	}

	public static string EncodeJsonString(OpenEclipseDungeonUI o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OpenEclipseDungeonUI o)
	{
		return new Dictionary<string, object>();
	}
}
