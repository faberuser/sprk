using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CreatureWakeUpBattleLog
{
	public static CreatureWakeUpBattleLog Parse(string s)
	{
		return Parse(s, new CreatureWakeUpBattleLog());
	}

	public static CreatureWakeUpBattleLog Parse(string s, CreatureWakeUpBattleLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CreatureWakeUpBattleLog Parse(IDictionary d)
	{
		return Parse(d, new CreatureWakeUpBattleLog());
	}

	public static CreatureWakeUpBattleLog Parse(IDictionary d, CreatureWakeUpBattleLog o)
	{
		o.CreatureUid = BaseJsonMarshaler.ParseValueInt(d, "CreatureUid");
		return o;
	}

	public static string EncodeJsonString(CreatureWakeUpBattleLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CreatureWakeUpBattleLog o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		return dictionary;
	}
}
