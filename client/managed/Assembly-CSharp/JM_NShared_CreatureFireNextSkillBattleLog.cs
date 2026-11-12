using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CreatureFireNextSkillBattleLog
{
	public static CreatureFireNextSkillBattleLog Parse(string s)
	{
		return Parse(s, new CreatureFireNextSkillBattleLog());
	}

	public static CreatureFireNextSkillBattleLog Parse(string s, CreatureFireNextSkillBattleLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CreatureFireNextSkillBattleLog Parse(IDictionary d)
	{
		return Parse(d, new CreatureFireNextSkillBattleLog());
	}

	public static CreatureFireNextSkillBattleLog Parse(IDictionary d, CreatureFireNextSkillBattleLog o)
	{
		o.CreatureUid = BaseJsonMarshaler.ParseValueInt(d, "CreatureUid");
		return o;
	}

	public static string EncodeJsonString(CreatureFireNextSkillBattleLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CreatureFireNextSkillBattleLog o)
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
