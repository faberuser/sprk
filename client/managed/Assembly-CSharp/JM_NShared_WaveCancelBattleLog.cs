using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_WaveCancelBattleLog
{
	public static WaveCancelBattleLog Parse(string s)
	{
		return Parse(s, new WaveCancelBattleLog());
	}

	public static WaveCancelBattleLog Parse(string s, WaveCancelBattleLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WaveCancelBattleLog Parse(IDictionary d)
	{
		return Parse(d, new WaveCancelBattleLog());
	}

	public static WaveCancelBattleLog Parse(IDictionary d, WaveCancelBattleLog o)
	{
		return o;
	}

	public static string EncodeJsonString(WaveCancelBattleLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WaveCancelBattleLog o)
	{
		return new Dictionary<string, object>();
	}
}
