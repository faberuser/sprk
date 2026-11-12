using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_WaveWithdrawBattleLog
{
	public static WaveWithdrawBattleLog Parse(string s)
	{
		return Parse(s, new WaveWithdrawBattleLog());
	}

	public static WaveWithdrawBattleLog Parse(string s, WaveWithdrawBattleLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WaveWithdrawBattleLog Parse(IDictionary d)
	{
		return Parse(d, new WaveWithdrawBattleLog());
	}

	public static WaveWithdrawBattleLog Parse(IDictionary d, WaveWithdrawBattleLog o)
	{
		return o;
	}

	public static string EncodeJsonString(WaveWithdrawBattleLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WaveWithdrawBattleLog o)
	{
		return new Dictionary<string, object>();
	}
}
