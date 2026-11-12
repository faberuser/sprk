using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_WaveGiveUpBattleLog
{
	public static WaveGiveUpBattleLog Parse(string s)
	{
		return Parse(s, new WaveGiveUpBattleLog());
	}

	public static WaveGiveUpBattleLog Parse(string s, WaveGiveUpBattleLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WaveGiveUpBattleLog Parse(IDictionary d)
	{
		return Parse(d, new WaveGiveUpBattleLog());
	}

	public static WaveGiveUpBattleLog Parse(IDictionary d, WaveGiveUpBattleLog o)
	{
		o.GiveUpWinTeamType = BaseJsonMarshaler.ParseEnum<TeamType>(d, "GiveUpWinTeamType");
		return o;
	}

	public static string EncodeJsonString(WaveGiveUpBattleLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WaveGiveUpBattleLog o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GiveUpWinTeamType", o.GiveUpWinTeamType.ToString());
		return dictionary;
	}
}
