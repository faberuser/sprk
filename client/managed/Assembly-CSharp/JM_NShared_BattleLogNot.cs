using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleLogNot
{
	public static BattleLogNot Parse(string s)
	{
		return Parse(s, new BattleLogNot());
	}

	public static BattleLogNot Parse(string s, BattleLogNot o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleLogNot Parse(IDictionary d)
	{
		return Parse(d, new BattleLogNot());
	}

	public static BattleLogNot Parse(IDictionary d, BattleLogNot o)
	{
		o.BattleLogs = BaseJsonMarshaler.ParseArray(d, "BattleLogs", JM_NShared_BaseBattleLog.Parse);
		return SprkConquest.ReadClock(o, d);
	}

	public static string EncodeJsonString(BattleLogNot o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleLogNot o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("BattleLogs", BaseJsonMarshaler.EncodeArray(o.BattleLogs, JM_NShared_BaseBattleLog.EncodeJsonObject));
		return dictionary;
	}
}
