using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CreatureReserveOnDungeonSkillBattleLog
{
	public static CreatureReserveOnDungeonSkillBattleLog Parse(string s)
	{
		return Parse(s, new CreatureReserveOnDungeonSkillBattleLog());
	}

	public static CreatureReserveOnDungeonSkillBattleLog Parse(string s, CreatureReserveOnDungeonSkillBattleLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CreatureReserveOnDungeonSkillBattleLog Parse(IDictionary d)
	{
		return Parse(d, new CreatureReserveOnDungeonSkillBattleLog());
	}

	public static CreatureReserveOnDungeonSkillBattleLog Parse(IDictionary d, CreatureReserveOnDungeonSkillBattleLog o)
	{
		o.TeamType = BaseJsonMarshaler.ParseEnum<TeamType>(d, "TeamType");
		o.SkillSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "SkillSlotIndex");
		return o;
	}

	public static string EncodeJsonString(CreatureReserveOnDungeonSkillBattleLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CreatureReserveOnDungeonSkillBattleLog o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("TeamType", o.TeamType.ToString());
		dictionary.Add("SkillSlotIndex", o.SkillSlotIndex.ToString());
		return dictionary;
	}
}
