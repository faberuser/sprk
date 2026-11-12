using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlayerPurchaseTimeDurationInfo
{
	public static PlayerPurchaseTimeDurationInfo Parse(string s)
	{
		return Parse(s, new PlayerPurchaseTimeDurationInfo());
	}

	public static PlayerPurchaseTimeDurationInfo Parse(string s, PlayerPurchaseTimeDurationInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayerPurchaseTimeDurationInfo Parse(IDictionary d)
	{
		return Parse(d, new PlayerPurchaseTimeDurationInfo());
	}

	public static PlayerPurchaseTimeDurationInfo Parse(IDictionary d, PlayerPurchaseTimeDurationInfo o)
	{
		o.ShopIndex = BaseJsonMarshaler.ParseValueInt(d, "ShopIndex");
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.StartTime = BaseJsonMarshaler.ParseValueString(d, "StartTime");
		o.EndTime = BaseJsonMarshaler.ParseValueString(d, "EndTime");
		o.PurchasedCount = BaseJsonMarshaler.ParseValueInt(d, "PurchasedCount");
		o.MailSentTime = BaseJsonMarshaler.ParseValueString(d, "MailSentTime");
		o.AttendanceGroupIndex = BaseJsonMarshaler.ParseValueInt(d, "AttendanceGroupIndex");
		return o;
	}

	public static string EncodeJsonString(PlayerPurchaseTimeDurationInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayerPurchaseTimeDurationInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ShopIndex", o.ShopIndex.ToString());
		dictionary.Add("Index", o.Index.ToString());
		if (o.StartTime != null)
		{
			dictionary.Add("StartTime", o.StartTime);
		}
		if (o.EndTime != null)
		{
			dictionary.Add("EndTime", o.EndTime);
		}
		dictionary.Add("PurchasedCount", o.PurchasedCount.ToString());
		if (o.MailSentTime != null)
		{
			dictionary.Add("MailSentTime", o.MailSentTime);
		}
		dictionary.Add("AttendanceGroupIndex", o.AttendanceGroupIndex.ToString());
		return dictionary;
	}
}
