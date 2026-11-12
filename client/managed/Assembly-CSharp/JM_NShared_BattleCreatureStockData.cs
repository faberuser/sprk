using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleCreatureStockData
{
	public static BattleCreatureStockData Parse(string s)
	{
		return Parse(s, new BattleCreatureStockData());
	}

	public static BattleCreatureStockData Parse(string s, BattleCreatureStockData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleCreatureStockData Parse(IDictionary d)
	{
		return Parse(d, new BattleCreatureStockData());
	}

	public static BattleCreatureStockData Parse(IDictionary d, BattleCreatureStockData o)
	{
		o.CreatureData = BaseJsonMarshaler.ParseObject(d, "CreatureData", JM_NShared_CreatureData.Parse);
		o.CreateInfo = BaseJsonMarshaler.ParseObject(d, "CreateInfo", JM_NShared_BattleCreatureCreateInfo.Parse);
		o.InitialInfo = BaseJsonMarshaler.ParseObject(d, "InitialInfo", JM_NShared_ResultCreatureInfo.Parse);
		o.PlayerId = BaseJsonMarshaler.ParseValueLong(d, "PlayerId");
		o.CreatedTime = BaseJsonMarshaler.ParseValueLong(d, "CreatedTime");
		return o;
	}

	public static string EncodeJsonString(BattleCreatureStockData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleCreatureStockData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CreatureData", JM_NShared_CreatureData.EncodeJsonObject(o.CreatureData));
		dictionary.Add("CreateInfo", JM_NShared_BattleCreatureCreateInfo.EncodeJsonObject(o.CreateInfo));
		dictionary.Add("InitialInfo", JM_NShared_ResultCreatureInfo.EncodeJsonObject(o.InitialInfo));
		dictionary.Add("PlayerId", o.PlayerId.ToString());
		dictionary.Add("CreatedTime", o.CreatedTime.ToString());
		return dictionary;
	}
}
