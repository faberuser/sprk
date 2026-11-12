using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CreatureReserveOnSkillBattleLog
{
	public static CreatureReserveOnSkillBattleLog Parse(string s)
	{
		return Parse(s, new CreatureReserveOnSkillBattleLog());
	}

	public static CreatureReserveOnSkillBattleLog Parse(string s, CreatureReserveOnSkillBattleLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CreatureReserveOnSkillBattleLog Parse(IDictionary d)
	{
		return Parse(d, new CreatureReserveOnSkillBattleLog());
	}

	public static CreatureReserveOnSkillBattleLog Parse(IDictionary d, CreatureReserveOnSkillBattleLog o)
	{
		o.CreatureUid = BaseJsonMarshaler.ParseValueInt(d, "CreatureUid");
		o.SkillSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "SkillSlotIndex");
		o.ManualTargetUid = BaseJsonMarshaler.ParseValueInt(d, "ManualTargetUid");
		o.AutoReserved = BaseJsonMarshaler.ParseValueBool(d, "AutoReserved");
		return o;
	}

	public static string EncodeJsonString(CreatureReserveOnSkillBattleLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CreatureReserveOnSkillBattleLog o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		dictionary.Add("SkillSlotIndex", o.SkillSlotIndex.ToString());
		dictionary.Add("ManualTargetUid", o.ManualTargetUid.ToString());
		dictionary.Add("AutoReserved", o.AutoReserved.ToString());
		return dictionary;
	}
}
