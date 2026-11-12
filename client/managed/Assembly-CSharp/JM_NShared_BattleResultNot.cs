using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleResultNot
{
	public static BattleResultNot Parse(string s)
	{
		return Parse(s, new BattleResultNot());
	}

	public static BattleResultNot Parse(string s, BattleResultNot o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleResultNot Parse(IDictionary d)
	{
		return Parse(d, new BattleResultNot());
	}

	public static BattleResultNot Parse(IDictionary d, BattleResultNot o)
	{
		o.Win = BaseJsonMarshaler.ParseValueBool(d, "Win");
		o.Star = BaseJsonMarshaler.ParseValueInt(d, "Star");
		o.Dropped = BaseJsonMarshaler.ParseValueBool(d, "Dropped");
		o.KillCount = BaseJsonMarshaler.ParseDictionary<int, int>(d, "KillCount");
		o.AliveHeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "AliveHeroIndices", BaseJsonMarshaler.CastToInt);
		o.MaxWaveIndex = BaseJsonMarshaler.ParseValueInt(d, "MaxWaveIndex");
		return o;
	}

	public static string EncodeJsonString(BattleResultNot o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleResultNot o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Win", o.Win.ToString());
		dictionary.Add("Star", o.Star.ToString());
		dictionary.Add("Dropped", o.Dropped.ToString());
		dictionary.Add("KillCount", BaseJsonMarshaler.EncodeDictionary(o.KillCount));
		dictionary.Add("AliveHeroIndices", BaseJsonMarshaler.EncodeArray(o.AliveHeroIndices));
		dictionary.Add("MaxWaveIndex", o.MaxWaveIndex.ToString());
		return dictionary;
	}
}
