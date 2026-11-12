using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_WaveEndBattleLog
{
	public static WaveEndBattleLog Parse(string s)
	{
		return Parse(s, new WaveEndBattleLog());
	}

	public static WaveEndBattleLog Parse(string s, WaveEndBattleLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WaveEndBattleLog Parse(IDictionary d)
	{
		return Parse(d, new WaveEndBattleLog());
	}

	public static WaveEndBattleLog Parse(IDictionary d, WaveEndBattleLog o)
	{
		o.winner = BaseJsonMarshaler.ParseEnum<TeamType>(d, "winner");
		o._winDecider = BaseJsonMarshaler.ParseEnum<WinDeciderType>(d, "_winDecider");
		o._withdraw = BaseJsonMarshaler.ParseValueBool(d, "_withdraw");
		return o;
	}

	public static string EncodeJsonString(WaveEndBattleLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WaveEndBattleLog o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("winner", o.winner.ToString());
		dictionary.Add("_winDecider", o._winDecider.ToString());
		dictionary.Add("_withdraw", o._withdraw.ToString());
		return dictionary;
	}
}
