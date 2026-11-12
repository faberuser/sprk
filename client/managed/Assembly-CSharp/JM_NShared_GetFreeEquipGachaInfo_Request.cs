using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetFreeEquipGachaInfo;

public static class JM_NShared_GetFreeEquipGachaInfo_Request
{
	public static NShared.GetFreeEquipGachaInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetFreeEquipGachaInfo.Request());
	}

	public static NShared.GetFreeEquipGachaInfo.Request Parse(string s, NShared.GetFreeEquipGachaInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetFreeEquipGachaInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetFreeEquipGachaInfo.Request());
	}

	public static NShared.GetFreeEquipGachaInfo.Request Parse(IDictionary d, NShared.GetFreeEquipGachaInfo.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetFreeEquipGachaInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetFreeEquipGachaInfo.Request o)
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
