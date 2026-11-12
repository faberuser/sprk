using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NEvent;

public static class JM_NShared_NEvent_OpenShakmehDungeonWindow
{
	public static OpenShakmehDungeonWindow Parse(string s)
	{
		return Parse(s, new OpenShakmehDungeonWindow());
	}

	public static OpenShakmehDungeonWindow Parse(string s, OpenShakmehDungeonWindow o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OpenShakmehDungeonWindow Parse(IDictionary d)
	{
		return Parse(d, new OpenShakmehDungeonWindow());
	}

	public static OpenShakmehDungeonWindow Parse(IDictionary d, OpenShakmehDungeonWindow o)
	{
		return o;
	}

	public static string EncodeJsonString(OpenShakmehDungeonWindow o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OpenShakmehDungeonWindow o)
	{
		return new Dictionary<string, object>();
	}
}
