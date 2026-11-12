using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetTowerNpcInfo;

public static class JM_NShared_GetTowerNpcInfo_Request
{
	public static NShared.GetTowerNpcInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetTowerNpcInfo.Request());
	}

	public static NShared.GetTowerNpcInfo.Request Parse(string s, NShared.GetTowerNpcInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetTowerNpcInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetTowerNpcInfo.Request());
	}

	public static NShared.GetTowerNpcInfo.Request Parse(IDictionary d, NShared.GetTowerNpcInfo.Request o)
	{
		o.TowerIndex = BaseJsonMarshaler.ParseValueInt(d, "TowerIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetTowerNpcInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetTowerNpcInfo.Request o)
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
