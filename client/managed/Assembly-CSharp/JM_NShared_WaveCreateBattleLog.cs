using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_WaveCreateBattleLog
{
	public static WaveCreateBattleLog Parse(string s)
	{
		return Parse(s, new WaveCreateBattleLog());
	}

	public static WaveCreateBattleLog Parse(string s, WaveCreateBattleLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WaveCreateBattleLog Parse(IDictionary d)
	{
		return Parse(d, new WaveCreateBattleLog());
	}

	public static WaveCreateBattleLog Parse(IDictionary d, WaveCreateBattleLog o)
	{
		o.RandomSeedByMonth = BaseJsonMarshaler.ParseValueInt(d, "RandomSeedByMonth");
		o.ProjectileGroupIndicesFromServer = BaseJsonMarshaler.ParseArrayCast(d, "ProjectileGroupIndicesFromServer", BaseJsonMarshaler.CastToInt);
		return o;
	}

	public static string EncodeJsonString(WaveCreateBattleLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WaveCreateBattleLog o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RandomSeedByMonth", o.RandomSeedByMonth.ToString());
		dictionary.Add("ProjectileGroupIndicesFromServer", BaseJsonMarshaler.EncodeArray(o.ProjectileGroupIndicesFromServer));
		return dictionary;
	}
}
