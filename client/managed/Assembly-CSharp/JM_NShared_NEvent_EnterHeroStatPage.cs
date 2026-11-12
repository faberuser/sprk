using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NEvent;

public static class JM_NShared_NEvent_EnterHeroStatPage
{
	public static EnterHeroStatPage Parse(string s)
	{
		return Parse(s, new EnterHeroStatPage());
	}

	public static EnterHeroStatPage Parse(string s, EnterHeroStatPage o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EnterHeroStatPage Parse(IDictionary d)
	{
		return Parse(d, new EnterHeroStatPage());
	}

	public static EnterHeroStatPage Parse(IDictionary d, EnterHeroStatPage o)
	{
		return o;
	}

	public static string EncodeJsonString(EnterHeroStatPage o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EnterHeroStatPage o)
	{
		return new Dictionary<string, object>();
	}
}
