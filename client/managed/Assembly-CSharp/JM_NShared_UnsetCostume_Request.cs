using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.UnsetCostume;

public static class JM_NShared_UnsetCostume_Request
{
	public static NShared.UnsetCostume.Request Parse(string s)
	{
		return Parse(s, new NShared.UnsetCostume.Request());
	}

	public static NShared.UnsetCostume.Request Parse(string s, NShared.UnsetCostume.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.UnsetCostume.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.UnsetCostume.Request());
	}

	public static NShared.UnsetCostume.Request Parse(IDictionary d, NShared.UnsetCostume.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.UnsetCostume.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.UnsetCostume.Request o)
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
