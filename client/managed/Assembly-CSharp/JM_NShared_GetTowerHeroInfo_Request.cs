using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetTowerHeroInfo;

public static class JM_NShared_GetTowerHeroInfo_Request
{
	public static NShared.GetTowerHeroInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetTowerHeroInfo.Request());
	}

	public static NShared.GetTowerHeroInfo.Request Parse(string s, NShared.GetTowerHeroInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetTowerHeroInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetTowerHeroInfo.Request());
	}

	public static NShared.GetTowerHeroInfo.Request Parse(IDictionary d, NShared.GetTowerHeroInfo.Request o)
	{
		o.TowerIndex = BaseJsonMarshaler.ParseValueInt(d, "TowerIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetTowerHeroInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetTowerHeroInfo.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("TowerIndex", o.TowerIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
