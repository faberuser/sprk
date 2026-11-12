using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_PayShopItemEventAdded
{
	public static PayShopItemEventAdded Parse(string s)
	{
		return Parse(s, new PayShopItemEventAdded());
	}

	public static PayShopItemEventAdded Parse(string s, PayShopItemEventAdded o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PayShopItemEventAdded Parse(IDictionary d)
	{
		return Parse(d, new PayShopItemEventAdded());
	}

	public static PayShopItemEventAdded Parse(IDictionary d, PayShopItemEventAdded o)
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

	public static string EncodeJsonString(PayShopItemEventAdded o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PayShopItemEventAdded o)
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
