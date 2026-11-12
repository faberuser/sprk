using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_KarmaInitBattleLog
{
	public static KarmaInitBattleLog Parse(string s)
	{
		return Parse(s, new KarmaInitBattleLog());
	}

	public static KarmaInitBattleLog Parse(string s, KarmaInitBattleLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static KarmaInitBattleLog Parse(IDictionary d)
	{
		return Parse(d, new KarmaInitBattleLog());
	}

	public static KarmaInitBattleLog Parse(IDictionary d, KarmaInitBattleLog o)
	{
		return o;
	}

	public static string EncodeJsonString(KarmaInitBattleLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(KarmaInitBattleLog o)
	{
		return new Dictionary<string, object>();
	}
}
