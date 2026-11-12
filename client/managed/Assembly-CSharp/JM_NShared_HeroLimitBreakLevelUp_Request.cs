using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.HeroLimitBreakLevelUp;

public static class JM_NShared_HeroLimitBreakLevelUp_Request
{
	public static NShared.HeroLimitBreakLevelUp.Request Parse(string s)
	{
		return Parse(s, new NShared.HeroLimitBreakLevelUp.Request());
	}

	public static NShared.HeroLimitBreakLevelUp.Request Parse(string s, NShared.HeroLimitBreakLevelUp.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.HeroLimitBreakLevelUp.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.HeroLimitBreakLevelUp.Request());
	}

	public static NShared.HeroLimitBreakLevelUp.Request Parse(IDictionary d, NShared.HeroLimitBreakLevelUp.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.HeroLimitBreakLevelUp.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.HeroLimitBreakLevelUp.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
