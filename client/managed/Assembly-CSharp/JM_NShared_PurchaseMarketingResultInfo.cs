using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PurchaseMarketingResultInfo
{
	public static PurchaseMarketingResultInfo Parse(string s)
	{
		return Parse(s, new PurchaseMarketingResultInfo());
	}

	public static PurchaseMarketingResultInfo Parse(string s, PurchaseMarketingResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PurchaseMarketingResultInfo Parse(IDictionary d)
	{
		return Parse(d, new PurchaseMarketingResultInfo());
	}

	public static PurchaseMarketingResultInfo Parse(IDictionary d, PurchaseMarketingResultInfo o)
	{
		o.PurchaseIndex = BaseJsonMarshaler.ParseValueInt(d, "PurchaseIndex");
		o.BeginTime = BaseJsonMarshaler.ParseValueString(d, "BeginTime");
		o.EndTime = BaseJsonMarshaler.ParseValueString(d, "EndTime");
		o.PurchasedTime = BaseJsonMarshaler.ParseValueString(d, "PurchasedTime");
		o.Count = BaseJsonMarshaler.ParseValueInt(d, "Count");
		o.Active = BaseJsonMarshaler.ParseValueByte(d, "Active");
		o.IsOpenPopup = BaseJsonMarshaler.ParseValueByte(d, "IsOpenPopup");
		return o;
	}

	public static string EncodeJsonString(PurchaseMarketingResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PurchaseMarketingResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("PurchaseIndex", o.PurchaseIndex.ToString());
		if (o.BeginTime != null)
		{
			dictionary.Add("BeginTime", o.BeginTime);
		}
		if (o.EndTime != null)
		{
			dictionary.Add("EndTime", o.EndTime);
		}
		if (o.PurchasedTime != null)
		{
			dictionary.Add("PurchasedTime", o.PurchasedTime);
		}
		dictionary.Add("Count", o.Count.ToString());
		dictionary.Add("Active", o.Active.ToString());
		dictionary.Add("IsOpenPopup", o.IsOpenPopup.ToString());
		return dictionary;
	}
}
