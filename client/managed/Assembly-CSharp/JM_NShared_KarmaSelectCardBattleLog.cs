using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_KarmaSelectCardBattleLog
{
	public static KarmaSelectCardBattleLog Parse(string s)
	{
		return Parse(s, new KarmaSelectCardBattleLog());
	}

	public static KarmaSelectCardBattleLog Parse(string s, KarmaSelectCardBattleLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static KarmaSelectCardBattleLog Parse(IDictionary d)
	{
		return Parse(d, new KarmaSelectCardBattleLog());
	}

	public static KarmaSelectCardBattleLog Parse(IDictionary d, KarmaSelectCardBattleLog o)
	{
		o.SelectSlot = BaseJsonMarshaler.ParseValueInt(d, "SelectSlot");
		return o;
	}

	public static string EncodeJsonString(KarmaSelectCardBattleLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(KarmaSelectCardBattleLog o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SelectSlot", o.SelectSlot.ToString());
		return dictionary;
	}
}
