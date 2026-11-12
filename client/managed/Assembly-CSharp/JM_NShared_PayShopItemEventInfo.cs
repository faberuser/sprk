using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PayShopItemEventInfo
{
	public static PayShopItemEventInfo Parse(string s)
	{
		return Parse(s, new PayShopItemEventInfo());
	}

	public static PayShopItemEventInfo Parse(string s, PayShopItemEventInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PayShopItemEventInfo Parse(IDictionary d)
	{
		return Parse(d, new PayShopItemEventInfo());
	}

	public static PayShopItemEventInfo Parse(IDictionary d, PayShopItemEventInfo o)
	{
		o.Id = BaseJsonMarshaler.ParseValueLong(d, "Id");
		o.PayShopCategory = BaseJsonMarshaler.ParseEnum<PayShopCategory>(d, "PayShopCategory");
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.Group = BaseJsonMarshaler.ParseValueString(d, "Group");
		o.DiscountPercentage = BaseJsonMarshaler.ParseValueInt(d, "DiscountPercentage");
		o.Bonus = BaseJsonMarshaler.ParseValueInt(d, "Bonus");
		o.BeginTime = BaseJsonMarshaler.ParseValueString(d, "BeginTime");
		o.EndTime = BaseJsonMarshaler.ParseValueString(d, "EndTime");
		return o;
	}

	public static string EncodeJsonString(PayShopItemEventInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PayShopItemEventInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Id", o.Id.ToString());
		dictionary.Add("PayShopCategory", o.PayShopCategory.ToString());
		dictionary.Add("Index", o.Index.ToString());
		if (o.Group != null)
		{
			dictionary.Add("Group", o.Group);
		}
		dictionary.Add("DiscountPercentage", o.DiscountPercentage.ToString());
		dictionary.Add("Bonus", o.Bonus.ToString());
		if (o.BeginTime != null)
		{
			dictionary.Add("BeginTime", o.BeginTime);
		}
		if (o.EndTime != null)
		{
			dictionary.Add("EndTime", o.EndTime);
		}
		return dictionary;
	}
}
