using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_WaveStartBattleLog
{
	public static WaveStartBattleLog Parse(string s)
	{
		return Parse(s, new WaveStartBattleLog());
	}

	public static WaveStartBattleLog Parse(string s, WaveStartBattleLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WaveStartBattleLog Parse(IDictionary d)
	{
		return Parse(d, new WaveStartBattleLog());
	}

	public static WaveStartBattleLog Parse(IDictionary d, WaveStartBattleLog o)
	{
		return o;
	}

	public static string EncodeJsonString(WaveStartBattleLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WaveStartBattleLog o)
	{
		return new Dictionary<string, object>();
	}
}
