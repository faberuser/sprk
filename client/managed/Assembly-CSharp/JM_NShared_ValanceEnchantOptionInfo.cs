using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ValanceEnchantOptionInfo
{
	public static ValanceEnchantOptionInfo Parse(string s)
	{
		return Parse(s, new ValanceEnchantOptionInfo());
	}

	public static ValanceEnchantOptionInfo Parse(string s, ValanceEnchantOptionInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ValanceEnchantOptionInfo Parse(IDictionary d)
	{
		return Parse(d, new ValanceEnchantOptionInfo());
	}

	public static ValanceEnchantOptionInfo Parse(IDictionary d, ValanceEnchantOptionInfo o)
	{
		o.ConsumeItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ConsumeItemIndex");
		o.EnchantEquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EnchantEquipItemSlotIndex");
		o.EnchantOptionIndex1 = BaseJsonMarshaler.ParseValueInt(d, "EnchantOptionIndex1");
		o.EnchantOptionStep1 = BaseJsonMarshaler.ParseValueInt(d, "EnchantOptionStep1");
		o.EnchantOptionIndex2 = BaseJsonMarshaler.ParseValueInt(d, "EnchantOptionIndex2");
		o.EnchantOptionStep2 = BaseJsonMarshaler.ParseValueInt(d, "EnchantOptionStep2");
		o.EnchantOptionIndex3 = BaseJsonMarshaler.ParseValueInt(d, "EnchantOptionIndex3");
		o.EnchantOptionStep3 = BaseJsonMarshaler.ParseValueInt(d, "EnchantOptionStep3");
		return o;
	}

	public static string EncodeJsonString(ValanceEnchantOptionInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ValanceEnchantOptionInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ConsumeItemIndex", o.ConsumeItemIndex.ToString());
		dictionary.Add("EnchantEquipItemSlotIndex", o.EnchantEquipItemSlotIndex.ToString());
		dictionary.Add("EnchantOptionIndex1", o.EnchantOptionIndex1.ToString());
		dictionary.Add("EnchantOptionStep1", o.EnchantOptionStep1.ToString());
		dictionary.Add("EnchantOptionIndex2", o.EnchantOptionIndex2.ToString());
		dictionary.Add("EnchantOptionStep2", o.EnchantOptionStep2.ToString());
		dictionary.Add("EnchantOptionIndex3", o.EnchantOptionIndex3.ToString());
		dictionary.Add("EnchantOptionStep3", o.EnchantOptionStep3.ToString());
		return dictionary;
	}
}
