using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CreatureReserveOffSkillBattleLog
{
	public static CreatureReserveOffSkillBattleLog Parse(string s)
	{
		return Parse(s, new CreatureReserveOffSkillBattleLog());
	}

	public static CreatureReserveOffSkillBattleLog Parse(string s, CreatureReserveOffSkillBattleLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CreatureReserveOffSkillBattleLog Parse(IDictionary d)
	{
		return Parse(d, new CreatureReserveOffSkillBattleLog());
	}

	public static CreatureReserveOffSkillBattleLog Parse(IDictionary d, CreatureReserveOffSkillBattleLog o)
	{
		o.CreatureUid = BaseJsonMarshaler.ParseValueInt(d, "CreatureUid");
		return o;
	}

	public static string EncodeJsonString(CreatureReserveOffSkillBattleLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CreatureReserveOffSkillBattleLog o)
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
