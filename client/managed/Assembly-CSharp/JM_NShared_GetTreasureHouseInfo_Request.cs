using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetTreasureHouseInfo;

public static class JM_NShared_GetTreasureHouseInfo_Request
{
	public static NShared.GetTreasureHouseInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetTreasureHouseInfo.Request());
	}

	public static NShared.GetTreasureHouseInfo.Request Parse(string s, NShared.GetTreasureHouseInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetTreasureHouseInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetTreasureHouseInfo.Request());
	}

	public static NShared.GetTreasureHouseInfo.Request Parse(IDictionary d, NShared.GetTreasureHouseInfo.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetTreasureHouseInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetTreasureHouseInfo.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
