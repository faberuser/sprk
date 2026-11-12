using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ResetTreasureHouseInfo;

public static class JM_NShared_ResetTreasureHouseInfo_Request
{
	public static NShared.ResetTreasureHouseInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.ResetTreasureHouseInfo.Request());
	}

	public static NShared.ResetTreasureHouseInfo.Request Parse(string s, NShared.ResetTreasureHouseInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ResetTreasureHouseInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ResetTreasureHouseInfo.Request());
	}

	public static NShared.ResetTreasureHouseInfo.Request Parse(IDictionary d, NShared.ResetTreasureHouseInfo.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ResetTreasureHouseInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ResetTreasureHouseInfo.Request o)
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
