using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CreatureCreateBattleLog
{
	public static CreatureCreateBattleLog Parse(string s)
	{
		return Parse(s, new CreatureCreateBattleLog());
	}

	public static CreatureCreateBattleLog Parse(string s, CreatureCreateBattleLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CreatureCreateBattleLog Parse(IDictionary d)
	{
		return Parse(d, new CreatureCreateBattleLog());
	}

	public static CreatureCreateBattleLog Parse(IDictionary d, CreatureCreateBattleLog o)
	{
		o.PlayerId = BaseJsonMarshaler.ParseValueLong(d, "PlayerId");
		o.CreateInfo = BaseJsonMarshaler.ParseObject(d, "CreateInfo", JM_NShared_BattleCreatureCreateInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(CreatureCreateBattleLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CreatureCreateBattleLog o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("PlayerId", o.PlayerId.ToString());
		dictionary.Add("CreateInfo", JM_NShared_BattleCreatureCreateInfo.EncodeJsonObject(o.CreateInfo));
		return dictionary;
	}
}
