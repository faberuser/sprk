using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CreatureManualTargetBattleLog
{
	public static CreatureManualTargetBattleLog Parse(string s)
	{
		return Parse(s, new CreatureManualTargetBattleLog());
	}

	public static CreatureManualTargetBattleLog Parse(string s, CreatureManualTargetBattleLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CreatureManualTargetBattleLog Parse(IDictionary d)
	{
		return Parse(d, new CreatureManualTargetBattleLog());
	}

	public static CreatureManualTargetBattleLog Parse(IDictionary d, CreatureManualTargetBattleLog o)
	{
		o.CreatureUid = BaseJsonMarshaler.ParseValueInt(d, "CreatureUid");
		o.TargetCreatureUid = BaseJsonMarshaler.ParseValueInt(d, "TargetCreatureUid");
		return o;
	}

	public static string EncodeJsonString(CreatureManualTargetBattleLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CreatureManualTargetBattleLog o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		dictionary.Add("TargetCreatureUid", o.TargetCreatureUid.ToString());
		return dictionary;
	}
}
