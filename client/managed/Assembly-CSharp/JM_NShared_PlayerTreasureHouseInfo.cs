using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlayerTreasureHouseInfo
{
	public static PlayerTreasureHouseInfo Parse(string s)
	{
		return Parse(s, new PlayerTreasureHouseInfo());
	}

	public static PlayerTreasureHouseInfo Parse(string s, PlayerTreasureHouseInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayerTreasureHouseInfo Parse(IDictionary d)
	{
		return Parse(d, new PlayerTreasureHouseInfo());
	}

	public static PlayerTreasureHouseInfo Parse(IDictionary d, PlayerTreasureHouseInfo o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.ClearCount = BaseJsonMarshaler.ParseValueInt(d, "ClearCount");
		o.ClearedTime = BaseJsonMarshaler.ParseValueString(d, "ClearedTime");
		o.UpdatedTime = BaseJsonMarshaler.ParseValueString(d, "UpdatedTime");
		o.CreatedTime = BaseJsonMarshaler.ParseValueString(d, "CreatedTime");
		o.ResetTime = BaseJsonMarshaler.ParseValueString(d, "ResetTime");
		o.NextResetTime = BaseJsonMarshaler.ParseValueString(d, "NextResetTime");
		return o;
	}

	public static string EncodeJsonString(PlayerTreasureHouseInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayerTreasureHouseInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("ClearCount", o.ClearCount.ToString());
		if (o.ClearedTime != null)
		{
			dictionary.Add("ClearedTime", o.ClearedTime);
		}
		if (o.UpdatedTime != null)
		{
			dictionary.Add("UpdatedTime", o.UpdatedTime);
		}
		if (o.CreatedTime != null)
		{
			dictionary.Add("CreatedTime", o.CreatedTime);
		}
		if (o.ResetTime != null)
		{
			dictionary.Add("ResetTime", o.ResetTime);
		}
		if (o.NextResetTime != null)
		{
			dictionary.Add("NextResetTime", o.NextResetTime);
		}
		return dictionary;
	}
}
