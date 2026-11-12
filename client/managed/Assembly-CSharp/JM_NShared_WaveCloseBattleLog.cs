using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_WaveCloseBattleLog
{
	public static WaveCloseBattleLog Parse(string s)
	{
		return Parse(s, new WaveCloseBattleLog());
	}

	public static WaveCloseBattleLog Parse(string s, WaveCloseBattleLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WaveCloseBattleLog Parse(IDictionary d)
	{
		return Parse(d, new WaveCloseBattleLog());
	}

	public static WaveCloseBattleLog Parse(IDictionary d, WaveCloseBattleLog o)
	{
		o.Timeouted = BaseJsonMarshaler.ParseValueBool(d, "Timeouted");
		return o;
	}

	public static string EncodeJsonString(WaveCloseBattleLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WaveCloseBattleLog o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Timeouted", o.Timeouted.ToString());
		return dictionary;
	}
}
