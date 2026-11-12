using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_MailEquipItemInfo
{
	public static MailEquipItemInfo Parse(string s)
	{
		return Parse(s, new MailEquipItemInfo());
	}

	public static MailEquipItemInfo Parse(string s, MailEquipItemInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MailEquipItemInfo Parse(IDictionary d)
	{
		return Parse(d, new MailEquipItemInfo());
	}

	public static MailEquipItemInfo Parse(IDictionary d, MailEquipItemInfo o)
	{
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.Star = BaseJsonMarshaler.ParseValueInt(d, "Star");
		o.Level = BaseJsonMarshaler.ParseValueInt(d, "Level");
		o.OptionIndex1 = BaseJsonMarshaler.ParseValueInt(d, "OptionIndex1");
		o.OptionIndex2 = BaseJsonMarshaler.ParseValueInt(d, "OptionIndex2");
		o.OptionIndex3 = BaseJsonMarshaler.ParseValueInt(d, "OptionIndex3");
		o.OptionIndex4 = BaseJsonMarshaler.ParseValueInt(d, "OptionIndex4");
		o.OptionIndex5 = BaseJsonMarshaler.ParseValueInt(d, "OptionIndex5");
		return o;
	}

	public static string EncodeJsonString(MailEquipItemInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MailEquipItemInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("Star", o.Star.ToString());
		dictionary.Add("Level", o.Level.ToString());
		dictionary.Add("OptionIndex1", o.OptionIndex1.ToString());
		dictionary.Add("OptionIndex2", o.OptionIndex2.ToString());
		dictionary.Add("OptionIndex3", o.OptionIndex3.ToString());
		dictionary.Add("OptionIndex4", o.OptionIndex4.ToString());
		dictionary.Add("OptionIndex5", o.OptionIndex5.ToString());
		return dictionary;
	}
}
