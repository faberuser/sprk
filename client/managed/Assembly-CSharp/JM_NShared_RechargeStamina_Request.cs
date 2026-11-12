using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.RechargeStamina;

public static class JM_NShared_RechargeStamina_Request
{
	public static NShared.RechargeStamina.Request Parse(string s)
	{
		return Parse(s, new NShared.RechargeStamina.Request());
	}

	public static NShared.RechargeStamina.Request Parse(string s, NShared.RechargeStamina.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.RechargeStamina.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.RechargeStamina.Request());
	}

	public static NShared.RechargeStamina.Request Parse(IDictionary d, NShared.RechargeStamina.Request o)
	{
		o.StaminaType = BaseJsonMarshaler.ParseEnum<StaminaType>(d, "StaminaType");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.RechargeStamina.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.RechargeStamina.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("StaminaType", o.StaminaType.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
