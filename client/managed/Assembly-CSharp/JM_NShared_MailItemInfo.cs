using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_MailItemInfo
{
	public static MailItemInfo Parse(string s)
	{
		return Parse(s, new MailItemInfo());
	}

	public static MailItemInfo Parse(string s, MailItemInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MailItemInfo Parse(IDictionary d)
	{
		return Parse(d, new MailItemInfo());
	}

	public static MailItemInfo Parse(IDictionary d, MailItemInfo o)
	{
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.ItemCount = BaseJsonMarshaler.ParseValueInt(d, "ItemCount");
		return o;
	}

	public static string EncodeJsonString(MailItemInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MailItemInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("ItemCount", o.ItemCount.ToString());
		return dictionary;
	}
}
