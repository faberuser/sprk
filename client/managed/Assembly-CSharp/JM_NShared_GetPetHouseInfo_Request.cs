using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetPetHouseInfo;

public static class JM_NShared_GetPetHouseInfo_Request
{
	public static NShared.GetPetHouseInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetPetHouseInfo.Request());
	}

	public static NShared.GetPetHouseInfo.Request Parse(string s, NShared.GetPetHouseInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetPetHouseInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetPetHouseInfo.Request());
	}

	public static NShared.GetPetHouseInfo.Request Parse(IDictionary d, NShared.GetPetHouseInfo.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetPetHouseInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetPetHouseInfo.Request o)
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
