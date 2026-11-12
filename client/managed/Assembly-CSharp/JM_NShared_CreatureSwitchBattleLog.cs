using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CreatureSwitchBattleLog
{
	public static CreatureSwitchBattleLog Parse(string s)
	{
		return Parse(s, new CreatureSwitchBattleLog());
	}

	public static CreatureSwitchBattleLog Parse(string s, CreatureSwitchBattleLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CreatureSwitchBattleLog Parse(IDictionary d)
	{
		return Parse(d, new CreatureSwitchBattleLog());
	}

	public static CreatureSwitchBattleLog Parse(IDictionary d, CreatureSwitchBattleLog o)
	{
		o.OutCreatureUid = BaseJsonMarshaler.ParseValueInt(d, "OutCreatureUid");
		o.InCreatureUid = BaseJsonMarshaler.ParseValueInt(d, "InCreatureUid");
		return o;
	}

	public static string EncodeJsonString(CreatureSwitchBattleLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CreatureSwitchBattleLog o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("OutCreatureUid", o.OutCreatureUid.ToString());
		dictionary.Add("InCreatureUid", o.InCreatureUid.ToString());
		return dictionary;
	}
}
