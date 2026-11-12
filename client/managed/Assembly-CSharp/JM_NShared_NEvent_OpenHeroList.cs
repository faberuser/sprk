using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NEvent;

public static class JM_NShared_NEvent_OpenHeroList
{
	public static OpenHeroList Parse(string s)
	{
		return Parse(s, new OpenHeroList());
	}

	public static OpenHeroList Parse(string s, OpenHeroList o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OpenHeroList Parse(IDictionary d)
	{
		return Parse(d, new OpenHeroList());
	}

	public static OpenHeroList Parse(IDictionary d, OpenHeroList o)
	{
		return o;
	}

	public static string EncodeJsonString(OpenHeroList o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OpenHeroList o)
	{
		return new Dictionary<string, object>();
	}
}
