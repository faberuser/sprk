using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_WebPayShopPurchase
{
	public static WebPayShopPurchase Parse(string s)
	{
		return Parse(s, new WebPayShopPurchase());
	}

	public static WebPayShopPurchase Parse(string s, WebPayShopPurchase o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WebPayShopPurchase Parse(IDictionary d)
	{
		return Parse(d, new WebPayShopPurchase());
	}

	public static WebPayShopPurchase Parse(IDictionary d, WebPayShopPurchase o)
	{
		o.PlayerProductPurchase = BaseJsonMarshaler.ParseObject(d, "PlayerProductPurchase", JM_NShared_PlayerProductPurchaseInfo.Parse);
		o.ItemTimeDurationInfo = BaseJsonMarshaler.ParseObject(d, "ItemTimeDurationInfo", JM_NShared_PlayerItemTimeDurationInfo.Parse);
		o.Attendance = BaseJsonMarshaler.ParseObject(d, "Attendance", JM_NShared_AttendanceInfo.Parse);
		o.AttendanceDatas = BaseJsonMarshaler.ParseArray(d, "AttendanceDatas", JM_NShared_AttendanceData.Parse);
		o.AttendanceRewardableDays = BaseJsonMarshaler.ParseArrayCast(d, "AttendanceRewardableDays", BaseJsonMarshaler.CastToInt);
		return o;
	}

	public static string EncodeJsonString(WebPayShopPurchase o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WebPayShopPurchase o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("PlayerProductPurchase", JM_NShared_PlayerProductPurchaseInfo.EncodeJsonObject(o.PlayerProductPurchase));
		dictionary.Add("ItemTimeDurationInfo", JM_NShared_PlayerItemTimeDurationInfo.EncodeJsonObject(o.ItemTimeDurationInfo));
		dictionary.Add("Attendance", JM_NShared_AttendanceInfo.EncodeJsonObject(o.Attendance));
		dictionary.Add("AttendanceDatas", BaseJsonMarshaler.EncodeArray(o.AttendanceDatas, JM_NShared_AttendanceData.EncodeJsonObject));
		dictionary.Add("AttendanceRewardableDays", BaseJsonMarshaler.EncodeArray(o.AttendanceRewardableDays));
		return dictionary;
	}
}
