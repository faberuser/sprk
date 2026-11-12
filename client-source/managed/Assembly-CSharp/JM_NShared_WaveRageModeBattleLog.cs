using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_WaveRageModeBattleLog
{
	public static WaveRageModeBattleLog Parse(string s)
	{
		return Parse(s, new WaveRageModeBattleLog());
	}

	public static WaveRageModeBattleLog Parse(string s, WaveRageModeBattleLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WaveRageModeBattleLog Parse(IDictionary d)
	{
		return Parse(d, new WaveRageModeBattleLog());
	}

	public static WaveRageModeBattleLog Parse(IDictionary d, WaveRageModeBattleLog o)
	{
		return o;
	}

	public static string EncodeJsonString(WaveRageModeBattleLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WaveRageModeBattleLog o)
	{
		return new Dictionary<string, object>();
	}
}
