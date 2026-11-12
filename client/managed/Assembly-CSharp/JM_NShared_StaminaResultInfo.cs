using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_StaminaResultInfo
{
	public static StaminaResultInfo Parse(string s)
	{
		return Parse(s, new StaminaResultInfo());
	}

	public static StaminaResultInfo Parse(string s, StaminaResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static StaminaResultInfo Parse(IDictionary d)
	{
		return Parse(d, new StaminaResultInfo());
	}

	public static StaminaResultInfo Parse(IDictionary d, StaminaResultInfo o)
	{
		o.SprkRaiderExp = RestoredPortal.ParseStaminaExp(d);
		o.Type = BaseJsonMarshaler.ParseEnum<StaminaType>(d, "Type");
		o.AddValue = BaseJsonMarshaler.ParseValueLong(d, "AddValue");
		o.NewValue = BaseJsonMarshaler.ParseValueLong(d, "NewValue");
		o.StaminaRechargeTime = BaseJsonMarshaler.ParseValueString(d, "StaminaRechargeTime");
		o.NextRechargeRemainTime = BaseJsonMarshaler.ParseValueInt(d, "NextRechargeRemainTime");
		o.FullRechargeRemainTime = BaseJsonMarshaler.ParseValueInt(d, "FullRechargeRemainTime");
		o.RechargeCount = BaseJsonMarshaler.ParseValueInt(d, "RechargeCount");
		o.IsHide = BaseJsonMarshaler.ParseValueBool(d, "IsHide");
		return o;
	}

	public static string EncodeJsonString(StaminaResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(StaminaResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Type", o.Type.ToString());
		dictionary.Add("AddValue", o.AddValue.ToString());
		dictionary.Add("NewValue", o.NewValue.ToString());
		if (o.StaminaRechargeTime != null)
		{
			dictionary.Add("StaminaRechargeTime", o.StaminaRechargeTime);
		}
		int nextRechargeRemainTime = o.NextRechargeRemainTime;
		dictionary.Add("NextRechargeRemainTime", ((long)nextRechargeRemainTime).ToString());
		int fullRechargeRemainTime = o.FullRechargeRemainTime;
		dictionary.Add("FullRechargeRemainTime", ((long)fullRechargeRemainTime).ToString());
		int rechargeCount = o.RechargeCount;
		dictionary.Add("RechargeCount", ((long)rechargeCount).ToString());
		dictionary.Add("IsHide", o.IsHide.ToString());
		return dictionary;
	}
}
