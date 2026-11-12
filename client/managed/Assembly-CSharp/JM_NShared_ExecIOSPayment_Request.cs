using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ExecIOSPayment;

public static class JM_NShared_ExecIOSPayment_Request
{
	public static NShared.ExecIOSPayment.Request Parse(string s)
	{
		return Parse(s, new NShared.ExecIOSPayment.Request());
	}

	public static NShared.ExecIOSPayment.Request Parse(string s, NShared.ExecIOSPayment.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ExecIOSPayment.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ExecIOSPayment.Request());
	}

	public static NShared.ExecIOSPayment.Request Parse(IDictionary d, NShared.ExecIOSPayment.Request o)
	{
		o.ProductId = BaseJsonMarshaler.ParseValueString(d, "ProductId");
		o.ProductIndex = BaseJsonMarshaler.ParseValueInt(d, "ProductIndex");
		o.TransactionId = BaseJsonMarshaler.ParseValueString(d, "TransactionId");
		o.DeveloperPayload = BaseJsonMarshaler.ParseValueString(d, "DeveloperPayload");
		o.Receipt = BaseJsonMarshaler.ParseValueString(d, "Receipt");
		o.CurrencyCode = BaseJsonMarshaler.ParseValueString(d, "CurrencyCode");
		o.Price = BaseJsonMarshaler.ParseValueFloat(d, "Price");
		o.Resume = BaseJsonMarshaler.ParseValueByte(d, "Resume");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ExecIOSPayment.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ExecIOSPayment.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.ProductId != null)
		{
			dictionary.Add("ProductId", o.ProductId);
		}
		dictionary.Add("ProductIndex", o.ProductIndex.ToString());
		if (o.TransactionId != null)
		{
			dictionary.Add("TransactionId", o.TransactionId);
		}
		if (o.DeveloperPayload != null)
		{
			dictionary.Add("DeveloperPayload", o.DeveloperPayload);
		}
		if (o.Receipt != null)
		{
			dictionary.Add("Receipt", o.Receipt);
		}
		if (o.CurrencyCode != null)
		{
			dictionary.Add("CurrencyCode", o.CurrencyCode);
		}
		dictionary.Add("Price", o.Price.ToString());
		dictionary.Add("Resume", o.Resume.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
