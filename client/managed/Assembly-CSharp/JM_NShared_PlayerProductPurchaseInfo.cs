using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlayerProductPurchaseInfo
{
	public static PlayerProductPurchaseInfo Parse(string s)
	{
		return Parse(s, new PlayerProductPurchaseInfo());
	}

	public static PlayerProductPurchaseInfo Parse(string s, PlayerProductPurchaseInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayerProductPurchaseInfo Parse(IDictionary d)
	{
		return Parse(d, new PlayerProductPurchaseInfo());
	}

	public static PlayerProductPurchaseInfo Parse(IDictionary d, PlayerProductPurchaseInfo o)
	{
		o.ProductIndex = BaseJsonMarshaler.ParseValueInt(d, "ProductIndex");
		o.PurchasedCount = BaseJsonMarshaler.ParseValueInt(d, "PurchasedCount");
		o.PurchasedTime = BaseJsonMarshaler.ParseValueString(d, "PurchasedTime");
		o.PurchaseCountResetTime = BaseJsonMarshaler.ParseValueString(d, "PurchaseCountResetTime");
		o.StartTime = BaseJsonMarshaler.ParseValueString(d, "StartTime");
		o.EndTime = BaseJsonMarshaler.ParseValueString(d, "EndTime");
		o.MailSentTime = BaseJsonMarshaler.ParseValueString(d, "MailSentTime");
		o.ActionValue = BaseJsonMarshaler.ParseValueInt(d, "ActionValue");
		return o;
	}

	public static string EncodeJsonString(PlayerProductPurchaseInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayerProductPurchaseInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ProductIndex", o.ProductIndex.ToString());
		dictionary.Add("PurchasedCount", o.PurchasedCount.ToString());
		if (o.PurchasedTime != null)
		{
			dictionary.Add("PurchasedTime", o.PurchasedTime);
		}
		if (o.PurchaseCountResetTime != null)
		{
			dictionary.Add("PurchaseCountResetTime", o.PurchaseCountResetTime);
		}
		if (o.StartTime != null)
		{
			dictionary.Add("StartTime", o.StartTime);
		}
		if (o.EndTime != null)
		{
			dictionary.Add("EndTime", o.EndTime);
		}
		if (o.MailSentTime != null)
		{
			dictionary.Add("MailSentTime", o.MailSentTime);
		}
		dictionary.Add("ActionValue", o.ActionValue.ToString());
		return dictionary;
	}
}
