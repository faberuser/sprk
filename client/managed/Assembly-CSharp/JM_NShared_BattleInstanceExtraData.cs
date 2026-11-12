using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleInstanceExtraData
{
	public static BattleInstanceExtraData Parse(string s)
	{
		return Parse(s, new BattleInstanceExtraData());
	}

	public static BattleInstanceExtraData Parse(string s, BattleInstanceExtraData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleInstanceExtraData Parse(IDictionary d)
	{
		return Parse(d, new BattleInstanceExtraData());
	}

	public static BattleInstanceExtraData Parse(IDictionary d, BattleInstanceExtraData o)
	{
		o.InfinityLevel = BaseJsonMarshaler.ParseValueInt(d, "InfinityLevel");
		o.AffixIndices = BaseJsonMarshaler.ParseArrayCast(d, "AffixIndices", BaseJsonMarshaler.CastToInt);
		o.FlaskItemIndex = BaseJsonMarshaler.ParseValueInt(d, "FlaskItemIndex");
		o.FlaskItemCount = BaseJsonMarshaler.ParseValueInt(d, "FlaskItemCount");
		return o;
	}

	public static string EncodeJsonString(BattleInstanceExtraData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleInstanceExtraData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("InfinityLevel", o.InfinityLevel.ToString());
		dictionary.Add("AffixIndices", BaseJsonMarshaler.EncodeArray(o.AffixIndices));
		dictionary.Add("FlaskItemIndex", o.FlaskItemIndex.ToString());
		dictionary.Add("FlaskItemCount", o.FlaskItemCount.ToString());
		return dictionary;
	}
}
