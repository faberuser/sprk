using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CreatureFireEventSkillBattleLog
{
	public static CreatureFireEventSkillBattleLog Parse(string s)
	{
		return Parse(s, new CreatureFireEventSkillBattleLog());
	}

	public static CreatureFireEventSkillBattleLog Parse(string s, CreatureFireEventSkillBattleLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CreatureFireEventSkillBattleLog Parse(IDictionary d)
	{
		return Parse(d, new CreatureFireEventSkillBattleLog());
	}

	public static CreatureFireEventSkillBattleLog Parse(IDictionary d, CreatureFireEventSkillBattleLog o)
	{
		o.CreatureUid = BaseJsonMarshaler.ParseValueInt(d, "CreatureUid");
		o.SkillIndex = BaseJsonMarshaler.ParseValueInt(d, "SkillIndex");
		o.ManualTargetUid = BaseJsonMarshaler.ParseValueInt(d, "ManualTargetUid");
		return o;
	}

	public static string EncodeJsonString(CreatureFireEventSkillBattleLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CreatureFireEventSkillBattleLog o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		dictionary.Add("SkillIndex", o.SkillIndex.ToString());
		dictionary.Add("ManualTargetUid", o.ManualTargetUid.ToString());
		return dictionary;
	}
}
