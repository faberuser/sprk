using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ResetTower;

public static class JM_NShared_ResetTower_Request
{
	public static NShared.ResetTower.Request Parse(string s)
	{
		return Parse(s, new NShared.ResetTower.Request());
	}

	public static NShared.ResetTower.Request Parse(string s, NShared.ResetTower.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ResetTower.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ResetTower.Request());
	}

	public static NShared.ResetTower.Request Parse(IDictionary d, NShared.ResetTower.Request o)
	{
		o.TowerIndex = BaseJsonMarshaler.ParseValueInt(d, "TowerIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ResetTower.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ResetTower.Request o)
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
