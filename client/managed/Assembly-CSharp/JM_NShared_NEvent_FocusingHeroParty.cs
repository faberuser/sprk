using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NEvent;

public static class JM_NShared_NEvent_FocusingHeroParty
{
	public static FocusingHeroParty Parse(string s)
	{
		return Parse(s, new FocusingHeroParty());
	}

	public static FocusingHeroParty Parse(string s, FocusingHeroParty o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static FocusingHeroParty Parse(IDictionary d)
	{
		return Parse(d, new FocusingHeroParty());
	}

	public static FocusingHeroParty Parse(IDictionary d, FocusingHeroParty o)
	{
		return o;
	}

	public static string EncodeJsonString(FocusingHeroParty o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(FocusingHeroParty o)
	{
		return new Dictionary<string, object>();
	}
}
