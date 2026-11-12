using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlayerEnterBattleLog
{
	public static PlayerEnterBattleLog Parse(string s)
	{
		return Parse(s, new PlayerEnterBattleLog());
	}

	public static PlayerEnterBattleLog Parse(string s, PlayerEnterBattleLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayerEnterBattleLog Parse(IDictionary d)
	{
		return Parse(d, new PlayerEnterBattleLog());
	}

	public static PlayerEnterBattleLog Parse(IDictionary d, PlayerEnterBattleLog o)
	{
		o.PlayerCreateInfo = BaseJsonMarshaler.ParseObject(d, "PlayerCreateInfo", JM_NShared_BattlePlayerCreateInfo.Parse);
		o.CreatureCreateInfos = BaseJsonMarshaler.ParseArray(d, "CreatureCreateInfos", JM_NShared_BattleCreatureCreateInfo.Parse);
		o.InitialCreatureInfos = BaseJsonMarshaler.ParseArray(d, "InitialCreatureInfos", JM_NShared_ResultCreatureInfo.Parse);
		o.BattleCheckCreatureCreateInfos = BaseJsonMarshaler.ParseArray(d, "BattleCheckCreatureCreateInfos", JM_NShared_BattleCreatureCreateInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(PlayerEnterBattleLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayerEnterBattleLog o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("PlayerCreateInfo", JM_NShared_BattlePlayerCreateInfo.EncodeJsonObject(o.PlayerCreateInfo));
		dictionary.Add("CreatureCreateInfos", BaseJsonMarshaler.EncodeArray(o.CreatureCreateInfos, JM_NShared_BattleCreatureCreateInfo.EncodeJsonObject));
		dictionary.Add("InitialCreatureInfos", BaseJsonMarshaler.EncodeArray(o.InitialCreatureInfos, JM_NShared_ResultCreatureInfo.EncodeJsonObject));
		dictionary.Add("BattleCheckCreatureCreateInfos", BaseJsonMarshaler.EncodeArray(o.BattleCheckCreatureCreateInfos, JM_NShared_BattleCreatureCreateInfo.EncodeJsonObject));
		return dictionary;
	}
}
