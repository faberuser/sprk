using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_MailStaminaInfo
{
	public static MailStaminaInfo Parse(string s)
	{
		return Parse(s, new MailStaminaInfo());
	}

	public static MailStaminaInfo Parse(string s, MailStaminaInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MailStaminaInfo Parse(IDictionary d)
	{
		return Parse(d, new MailStaminaInfo());
	}

	public static MailStaminaInfo Parse(IDictionary d, MailStaminaInfo o)
	{
		o.StaminaType = BaseJsonMarshaler.ParseEnum<CurrencyType>(d, "StaminaType");
		o.Amount = BaseJsonMarshaler.ParseValueInt(d, "Amount");
		return o;
	}

	public static string EncodeJsonString(MailStaminaInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MailStaminaInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("StaminaType", o.StaminaType.ToString());
		dictionary.Add("Amount", o.Amount.ToString());
		return dictionary;
	}
}
