using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlayerStaminaInfo
{
	public static PlayerStaminaInfo Parse(string s)
	{
		return Parse(s, new PlayerStaminaInfo());
	}

	public static PlayerStaminaInfo Parse(string s, PlayerStaminaInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayerStaminaInfo Parse(IDictionary d)
	{
		return Parse(d, new PlayerStaminaInfo());
	}

	public static PlayerStaminaInfo Parse(IDictionary d, PlayerStaminaInfo o)
	{
		o.StaminaType = BaseJsonMarshaler.ParseEnum<StaminaType>(d, "StaminaType");
		o.Amount = BaseJsonMarshaler.ParseValueInt(d, "Amount");
		o.RechargeTime = BaseJsonMarshaler.ParseValueString(d, "RechargeTime");
		o.RechargeCount = BaseJsonMarshaler.ParseValueInt(d, "RechargeCount");
		return o;
	}

	public static string EncodeJsonString(PlayerStaminaInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayerStaminaInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("StaminaType", o.StaminaType.ToString());
		dictionary.Add("Amount", o.Amount.ToString());
		if (o.RechargeTime != null)
		{
			dictionary.Add("RechargeTime", o.RechargeTime);
		}
		dictionary.Add("RechargeCount", o.RechargeCount.ToString());
		return dictionary;
	}
}
