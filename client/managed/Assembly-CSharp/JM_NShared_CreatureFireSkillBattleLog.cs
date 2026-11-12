using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CreatureFireSkillBattleLog
{
	public static CreatureFireSkillBattleLog Parse(string s)
	{
		return Parse(s, new CreatureFireSkillBattleLog());
	}

	public static CreatureFireSkillBattleLog Parse(string s, CreatureFireSkillBattleLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CreatureFireSkillBattleLog Parse(IDictionary d)
	{
		return Parse(d, new CreatureFireSkillBattleLog());
	}

	public static CreatureFireSkillBattleLog Parse(IDictionary d, CreatureFireSkillBattleLog o)
	{
		o.CreatureUid = BaseJsonMarshaler.ParseValueInt(d, "CreatureUid");
		o.SkillSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "SkillSlotIndex");
		return o;
	}

	public static string EncodeJsonString(CreatureFireSkillBattleLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CreatureFireSkillBattleLog o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		dictionary.Add("SkillSlotIndex", o.SkillSlotIndex.ToString());
		return dictionary;
	}
}
