using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EquipOptionChangeOnUpgradeTierData
{
	public static EquipOptionChangeOnUpgradeTierData Parse(string s)
	{
		return Parse(s, new EquipOptionChangeOnUpgradeTierData());
	}

	public static EquipOptionChangeOnUpgradeTierData Parse(string s, EquipOptionChangeOnUpgradeTierData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EquipOptionChangeOnUpgradeTierData Parse(IDictionary d)
	{
		return Parse(d, new EquipOptionChangeOnUpgradeTierData());
	}

	public static EquipOptionChangeOnUpgradeTierData Parse(IDictionary d, EquipOptionChangeOnUpgradeTierData o)
	{
		o.OptionIndex = BaseJsonMarshaler.ParseValueInt(d, "OptionIndex");
		o.BeforeTier = BaseJsonMarshaler.ParseValueInt(d, "BeforeTier");
		o.AfterTier = BaseJsonMarshaler.ParseValueInt(d, "AfterTier");
		o.ResultOptionIndex = BaseJsonMarshaler.ParseValueInt(d, "ResultOptionIndex");
		return o;
	}

	public static string EncodeJsonString(EquipOptionChangeOnUpgradeTierData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EquipOptionChangeOnUpgradeTierData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("OptionIndex", o.OptionIndex.ToString());
		dictionary.Add("BeforeTier", o.BeforeTier.ToString());
		dictionary.Add("AfterTier", o.AfterTier.ToString());
		dictionary.Add("ResultOptionIndex", o.ResultOptionIndex.ToString());
		return dictionary;
	}
}
