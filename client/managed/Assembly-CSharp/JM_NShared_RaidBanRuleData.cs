using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_RaidBanRuleData
{
	public static RaidBanRuleData Parse(string s)
	{
		return Parse(s, new RaidBanRuleData());
	}

	public static RaidBanRuleData Parse(string s, RaidBanRuleData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RaidBanRuleData Parse(IDictionary d)
	{
		return Parse(d, new RaidBanRuleData());
	}

	public static RaidBanRuleData Parse(IDictionary d, RaidBanRuleData o)
	{
		o.BanType = BaseJsonMarshaler.ParseEnum<RaidBanType>(d, "BanType");
		o.BanValue = BaseJsonMarshaler.ParseValueString(d, "BanValue");
		return o;
	}

	public static string EncodeJsonString(RaidBanRuleData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RaidBanRuleData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("BanType", o.BanType.ToString());
		if (o.BanValue != null)
		{
			dictionary.Add("BanValue", o.BanValue);
		}
		return dictionary;
	}
}
