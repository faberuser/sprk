using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NEvent;

public static class JM_NShared_NEvent_OpenOrdealArenaTierUI
{
	public static OpenOrdealArenaTierUI Parse(string s)
	{
		return Parse(s, new OpenOrdealArenaTierUI());
	}

	public static OpenOrdealArenaTierUI Parse(string s, OpenOrdealArenaTierUI o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OpenOrdealArenaTierUI Parse(IDictionary d)
	{
		return Parse(d, new OpenOrdealArenaTierUI());
	}

	public static OpenOrdealArenaTierUI Parse(IDictionary d, OpenOrdealArenaTierUI o)
	{
		return o;
	}

	public static string EncodeJsonString(OpenOrdealArenaTierUI o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OpenOrdealArenaTierUI o)
	{
		return new Dictionary<string, object>();
	}
}
