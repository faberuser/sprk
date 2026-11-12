using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_EnterPunishmentDungeonTypeSelect
{
	public static EnterPunishmentDungeonTypeSelect Parse(string s)
	{
		return Parse(s, new EnterPunishmentDungeonTypeSelect());
	}

	public static EnterPunishmentDungeonTypeSelect Parse(string s, EnterPunishmentDungeonTypeSelect o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EnterPunishmentDungeonTypeSelect Parse(IDictionary d)
	{
		return Parse(d, new EnterPunishmentDungeonTypeSelect());
	}

	public static EnterPunishmentDungeonTypeSelect Parse(IDictionary d, EnterPunishmentDungeonTypeSelect o)
	{
		o.GroupIndex = BaseJsonMarshaler.ParseValueInt(d, "GroupIndex");
		return o;
	}

	public static string EncodeJsonString(EnterPunishmentDungeonTypeSelect o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EnterPunishmentDungeonTypeSelect o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GroupIndex", o.GroupIndex.ToString());
		return dictionary;
	}
}
