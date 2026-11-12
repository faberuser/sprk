using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_RewardItemInfo
{
	public static RewardItemInfo Parse(string s)
	{
		return Parse(s, new RewardItemInfo());
	}

	public static RewardItemInfo Parse(string s, RewardItemInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RewardItemInfo Parse(IDictionary d)
	{
		return Parse(d, new RewardItemInfo());
	}

	public static RewardItemInfo Parse(IDictionary d, RewardItemInfo o)
	{
		o.Code = BaseJsonMarshaler.ParseValueString(d, "Code");
		o.Count = BaseJsonMarshaler.ParseValueInt(d, "Count");
		o.Star = BaseJsonMarshaler.ParseValueInt(d, "Star");
		o.IsBonus = BaseJsonMarshaler.ParseValueBool(d, "IsBonus");
		return o;
	}

	public static string EncodeJsonString(RewardItemInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RewardItemInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Code != null)
		{
			dictionary.Add("Code", o.Code);
		}
		dictionary.Add("Count", o.Count.ToString());
		dictionary.Add("Star", o.Star.ToString());
		dictionary.Add("IsBonus", o.IsBonus.ToString());
		return dictionary;
	}
}
