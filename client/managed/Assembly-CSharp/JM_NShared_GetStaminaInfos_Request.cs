using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetStaminaInfos;

public static class JM_NShared_GetStaminaInfos_Request
{
	public static NShared.GetStaminaInfos.Request Parse(string s)
	{
		return Parse(s, new NShared.GetStaminaInfos.Request());
	}

	public static NShared.GetStaminaInfos.Request Parse(string s, NShared.GetStaminaInfos.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetStaminaInfos.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetStaminaInfos.Request());
	}

	public static NShared.GetStaminaInfos.Request Parse(IDictionary d, NShared.GetStaminaInfos.Request o)
	{
		o.StaminaTypes = BaseJsonMarshaler.ParseArray<StaminaType>(d, "StaminaTypes");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetStaminaInfos.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetStaminaInfos.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("StaminaTypes", BaseJsonMarshaler.EncodeArray(o.StaminaTypes));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
