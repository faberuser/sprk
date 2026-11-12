using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_ShopItemBooster
{
	public static ShopItemBooster Parse(string s)
	{
		return Parse(s, new ShopItemBooster());
	}

	public static ShopItemBooster Parse(string s, ShopItemBooster o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ShopItemBooster Parse(IDictionary d)
	{
		return Parse(d, new ShopItemBooster());
	}

	public static ShopItemBooster Parse(IDictionary d, ShopItemBooster o)
	{
		o.ShopIndex = BaseJsonMarshaler.ParseValueInt(d, "ShopIndex");
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.ExpireTime = BaseJsonMarshaler.ParseValueString(d, "ExpireTime");
		o.PlayerPurchaseTimeDuration = BaseJsonMarshaler.ParseObject(d, "PlayerPurchaseTimeDuration", JM_NShared_PlayerPurchaseTimeDurationInfo.Parse);
		o.PlayerFirstPurchase = BaseJsonMarshaler.ParseObject(d, "PlayerFirstPurchase", JM_NShared_PlayerFirstPurchaseInfo.Parse);
		o.ItemTimeDurationInfo = BaseJsonMarshaler.ParseObject(d, "ItemTimeDurationInfo", JM_NShared_PlayerItemTimeDurationInfo.Parse);
		o.Attendance = BaseJsonMarshaler.ParseObject(d, "Attendance", JM_NShared_AttendanceInfo.Parse);
		o.AttendanceDatas = BaseJsonMarshaler.ParseArray(d, "AttendanceDatas", JM_NShared_AttendanceData.Parse);
		o.AttendanceRewardableDays = BaseJsonMarshaler.ParseArrayCast(d, "AttendanceRewardableDays", BaseJsonMarshaler.CastToInt);
		return o;
	}

	public static string EncodeJsonString(ShopItemBooster o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ShopItemBooster o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ShopIndex", o.ShopIndex.ToString());
		dictionary.Add("Index", o.Index.ToString());
		if (o.ExpireTime != null)
		{
			dictionary.Add("ExpireTime", o.ExpireTime);
		}
		dictionary.Add("PlayerPurchaseTimeDuration", JM_NShared_PlayerPurchaseTimeDurationInfo.EncodeJsonObject(o.PlayerPurchaseTimeDuration));
		dictionary.Add("PlayerFirstPurchase", JM_NShared_PlayerFirstPurchaseInfo.EncodeJsonObject(o.PlayerFirstPurchase));
		dictionary.Add("ItemTimeDurationInfo", JM_NShared_PlayerItemTimeDurationInfo.EncodeJsonObject(o.ItemTimeDurationInfo));
		dictionary.Add("Attendance", JM_NShared_AttendanceInfo.EncodeJsonObject(o.Attendance));
		dictionary.Add("AttendanceDatas", BaseJsonMarshaler.EncodeArray(o.AttendanceDatas, JM_NShared_AttendanceData.EncodeJsonObject));
		dictionary.Add("AttendanceRewardableDays", BaseJsonMarshaler.EncodeArray(o.AttendanceRewardableDays));
		return dictionary;
	}
}
