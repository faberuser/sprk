using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NEvent;

public static class JM_NShared_NEvent_OpenSanctuaryOfGodKing
{
	public static OpenSanctuaryOfGodKing Parse(string s)
	{
		return Parse(s, new OpenSanctuaryOfGodKing());
	}

	public static OpenSanctuaryOfGodKing Parse(string s, OpenSanctuaryOfGodKing o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OpenSanctuaryOfGodKing Parse(IDictionary d)
	{
		return Parse(d, new OpenSanctuaryOfGodKing());
	}

	public static OpenSanctuaryOfGodKing Parse(IDictionary d, OpenSanctuaryOfGodKing o)
	{
		return o;
	}

	public static string EncodeJsonString(OpenSanctuaryOfGodKing o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OpenSanctuaryOfGodKing o)
	{
		return new Dictionary<string, object>();
	}
}
