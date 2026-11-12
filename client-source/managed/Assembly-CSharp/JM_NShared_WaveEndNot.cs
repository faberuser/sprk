using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_WaveEndNot
{
	public static WaveEndNot Parse(string s)
	{
		return Parse(s, new WaveEndNot());
	}

	public static WaveEndNot Parse(string s, WaveEndNot o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WaveEndNot Parse(IDictionary d)
	{
		return Parse(d, new WaveEndNot());
	}

	public static WaveEndNot Parse(IDictionary d, WaveEndNot o)
	{
		o.Winner = BaseJsonMarshaler.ParseEnum<TeamType>(d, "Winner");
		return o;
	}

	public static string EncodeJsonString(WaveEndNot o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WaveEndNot o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Winner", o.Winner.ToString());
		return dictionary;
	}
}
