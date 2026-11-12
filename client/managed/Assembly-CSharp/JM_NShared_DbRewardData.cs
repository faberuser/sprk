using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_DbRewardData
{
	public static DbRewardData Parse(string s)
	{
		return Parse(s, new DbRewardData());
	}

	public static DbRewardData Parse(string s, DbRewardData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static DbRewardData Parse(IDictionary d)
	{
		return Parse(d, new DbRewardData());
	}

	public static DbRewardData Parse(IDictionary d, DbRewardData o)
	{
		o.Type = BaseJsonMarshaler.ParseEnum<RewardType>(d, "Type");
		o.Value = BaseJsonMarshaler.ParseValueInt(d, "Value");
		o.Count = BaseJsonMarshaler.ParseValueInt(d, "Count");
		o.Star = BaseJsonMarshaler.ParseValueInt(d, "Star");
		return o;
	}

	public static string EncodeJsonString(DbRewardData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(DbRewardData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Type", o.Type.ToString());
		dictionary.Add("Value", o.Value.ToString());
		dictionary.Add("Count", o.Count.ToString());
		dictionary.Add("Star", o.Star.ToString());
		return dictionary;
	}
}
