using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SetPurchaseMarketing;

public static class JM_NShared_SetPurchaseMarketing_Request
{
	public static NShared.SetPurchaseMarketing.Request Parse(string s)
	{
		return Parse(s, new NShared.SetPurchaseMarketing.Request());
	}

	public static NShared.SetPurchaseMarketing.Request Parse(string s, NShared.SetPurchaseMarketing.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SetPurchaseMarketing.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SetPurchaseMarketing.Request());
	}

	public static NShared.SetPurchaseMarketing.Request Parse(IDictionary d, NShared.SetPurchaseMarketing.Request o)
	{
		o.PurchaseMarketingIndices = BaseJsonMarshaler.ParseArrayCast(d, "PurchaseMarketingIndices", BaseJsonMarshaler.CastToInt);
		o.IsActive = BaseJsonMarshaler.ParseValueBool(d, "IsActive");
		o.IsOpenPopup = BaseJsonMarshaler.ParseValueBool(d, "IsOpenPopup");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SetPurchaseMarketing.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SetPurchaseMarketing.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("PurchaseMarketingIndices", BaseJsonMarshaler.EncodeArray(o.PurchaseMarketingIndices));
		dictionary.Add("IsActive", o.IsActive.ToString());
		dictionary.Add("IsOpenPopup", o.IsOpenPopup.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
