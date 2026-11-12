using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.UseCoupon;

public static class JM_NShared_UseCoupon_Request
{
	public static NShared.UseCoupon.Request Parse(string s)
	{
		return Parse(s, new NShared.UseCoupon.Request());
	}

	public static NShared.UseCoupon.Request Parse(string s, NShared.UseCoupon.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.UseCoupon.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.UseCoupon.Request());
	}

	public static NShared.UseCoupon.Request Parse(IDictionary d, NShared.UseCoupon.Request o)
	{
		o.CouponCode = BaseJsonMarshaler.ParseValueString(d, "CouponCode");
		o.CountryCode = BaseJsonMarshaler.ParseValueString(d, "CountryCode");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.UseCoupon.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.UseCoupon.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.CouponCode != null)
		{
			dictionary.Add("CouponCode", o.CouponCode);
		}
		if (o.CountryCode != null)
		{
			dictionary.Add("CountryCode", o.CountryCode);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
