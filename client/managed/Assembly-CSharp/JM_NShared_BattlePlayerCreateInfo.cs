using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattlePlayerCreateInfo
{
	public static BattlePlayerCreateInfo Parse(string s)
	{
		return Parse(s, new BattlePlayerCreateInfo());
	}

	public static BattlePlayerCreateInfo Parse(string s, BattlePlayerCreateInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattlePlayerCreateInfo Parse(IDictionary d)
	{
		return Parse(d, new BattlePlayerCreateInfo());
	}

	public static BattlePlayerCreateInfo Parse(IDictionary d, BattlePlayerCreateInfo o)
	{
		o.Id = BaseJsonMarshaler.ParseValueLong(d, "Id");
		o.TeamType = BaseJsonMarshaler.ParseEnum<TeamType>(d, "TeamType");
		return o;
	}

	public static string EncodeJsonString(BattlePlayerCreateInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattlePlayerCreateInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Id", o.Id.ToString());
		dictionary.Add("TeamType", o.TeamType.ToString());
		return dictionary;
	}
}
