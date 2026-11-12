using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NEvent;

public static class JM_NShared_NEvent_OnKarmaDungeonEnter
{
	public static OnKarmaDungeonEnter Parse(string s)
	{
		return Parse(s, new OnKarmaDungeonEnter());
	}

	public static OnKarmaDungeonEnter Parse(string s, OnKarmaDungeonEnter o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OnKarmaDungeonEnter Parse(IDictionary d)
	{
		return Parse(d, new OnKarmaDungeonEnter());
	}

	public static OnKarmaDungeonEnter Parse(IDictionary d, OnKarmaDungeonEnter o)
	{
		return o;
	}

	public static string EncodeJsonString(OnKarmaDungeonEnter o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OnKarmaDungeonEnter o)
	{
		return new Dictionary<string, object>();
	}
}
