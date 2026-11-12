using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CreatureManualMoveBattleLog
{
	public static CreatureManualMoveBattleLog Parse(string s)
	{
		return Parse(s, new CreatureManualMoveBattleLog());
	}

	public static CreatureManualMoveBattleLog Parse(string s, CreatureManualMoveBattleLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CreatureManualMoveBattleLog Parse(IDictionary d)
	{
		return Parse(d, new CreatureManualMoveBattleLog());
	}

	public static CreatureManualMoveBattleLog Parse(IDictionary d, CreatureManualMoveBattleLog o)
	{
		o.CreatureUid = BaseJsonMarshaler.ParseValueInt(d, "CreatureUid");
		o.Position = BaseJsonMarshaler.ParseObject(d, "Position", JM_NShared_NUtil_Vector3.Parse);
		return o;
	}

	public static string EncodeJsonString(CreatureManualMoveBattleLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CreatureManualMoveBattleLog o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		dictionary.Add("Position", JM_NShared_NUtil_Vector3.EncodeJsonObject(o.Position));
		return dictionary;
	}
}
