using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ResetAllCustomizingCostumes;

public static class JM_NShared_ResetAllCustomizingCostumes_Request
{
	public static NShared.ResetAllCustomizingCostumes.Request Parse(string s)
	{
		return Parse(s, new NShared.ResetAllCustomizingCostumes.Request());
	}

	public static NShared.ResetAllCustomizingCostumes.Request Parse(string s, NShared.ResetAllCustomizingCostumes.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ResetAllCustomizingCostumes.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ResetAllCustomizingCostumes.Request());
	}

	public static NShared.ResetAllCustomizingCostumes.Request Parse(IDictionary d, NShared.ResetAllCustomizingCostumes.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ResetAllCustomizingCostumes.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ResetAllCustomizingCostumes.Request o)
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
