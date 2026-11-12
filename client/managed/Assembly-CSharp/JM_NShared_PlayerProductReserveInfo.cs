using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlayerProductReserveInfo
{
	public static PlayerProductReserveInfo Parse(string s)
	{
		return Parse(s, new PlayerProductReserveInfo());
	}

	public static PlayerProductReserveInfo Parse(string s, PlayerProductReserveInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayerProductReserveInfo Parse(IDictionary d)
	{
		return Parse(d, new PlayerProductReserveInfo());
	}

	public static PlayerProductReserveInfo Parse(IDictionary d, PlayerProductReserveInfo o)
	{
		o.Platform = BaseJsonMarshaler.ParseValueString(d, "Platform");
		o.ProductId = BaseJsonMarshaler.ParseValueString(d, "ProductId");
		o.ProductIndex = BaseJsonMarshaler.ParseValueInt(d, "ProductIndex");
		o.ReservedTime = BaseJsonMarshaler.ParseValueString(d, "ReservedTime");
		o.ExpireTime = BaseJsonMarshaler.ParseValueString(d, "ExpireTime");
		o.ExtraInfo = BaseJsonMarshaler.ParseValueString(d, "ExtraInfo");
		return o;
	}

	public static string EncodeJsonString(PlayerProductReserveInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayerProductReserveInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Platform != null)
		{
			dictionary.Add("Platform", o.Platform);
		}
		if (o.ProductId != null)
		{
			dictionary.Add("ProductId", o.ProductId);
		}
		dictionary.Add("ProductIndex", o.ProductIndex.ToString());
		if (o.ReservedTime != null)
		{
			dictionary.Add("ReservedTime", o.ReservedTime);
		}
		if (o.ExpireTime != null)
		{
			dictionary.Add("ExpireTime", o.ExpireTime);
		}
		if (o.ExtraInfo != null)
		{
			dictionary.Add("ExtraInfo", o.ExtraInfo);
		}
		return dictionary;
	}
}
