using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PunishmentRuneItemBreakData
{
	public static PunishmentRuneItemBreakData Parse(string s)
	{
		return Parse(s, new PunishmentRuneItemBreakData());
	}

	public static PunishmentRuneItemBreakData Parse(string s, PunishmentRuneItemBreakData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PunishmentRuneItemBreakData Parse(IDictionary d)
	{
		return Parse(d, new PunishmentRuneItemBreakData());
	}

	public static PunishmentRuneItemBreakData Parse(IDictionary d, PunishmentRuneItemBreakData o)
	{
		o.BreakIndex = BaseJsonMarshaler.ParseValueInt(d, "BreakIndex");
		o.ItemCode1 = BaseJsonMarshaler.ParseValueString(d, "ItemCode1");
		o.MinItemCount1 = BaseJsonMarshaler.ParseValueInt(d, "MinItemCount1");
		o.MaxItemCount1 = BaseJsonMarshaler.ParseValueInt(d, "MaxItemCount1");
		o.MinItemCount2 = BaseJsonMarshaler.ParseValueInt(d, "MinItemCount2");
		o.MaxItemCount2 = BaseJsonMarshaler.ParseValueInt(d, "MaxItemCount2");
		o.MinItemCount3 = BaseJsonMarshaler.ParseValueInt(d, "MinItemCount3");
		o.MaxItemCount3 = BaseJsonMarshaler.ParseValueInt(d, "MaxItemCount3");
		o.MinItemCount4 = BaseJsonMarshaler.ParseValueInt(d, "MinItemCount4");
		o.MaxItemCount4 = BaseJsonMarshaler.ParseValueInt(d, "MaxItemCount4");
		o.MinItemCount5 = BaseJsonMarshaler.ParseValueInt(d, "MinItemCount5");
		o.MaxItemCount5 = BaseJsonMarshaler.ParseValueInt(d, "MaxItemCount5");
		return o;
	}

	public static string EncodeJsonString(PunishmentRuneItemBreakData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PunishmentRuneItemBreakData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("BreakIndex", o.BreakIndex.ToString());
		if (o.ItemCode1 != null)
		{
			dictionary.Add("ItemCode1", o.ItemCode1);
		}
		dictionary.Add("MinItemCount1", o.MinItemCount1.ToString());
		dictionary.Add("MaxItemCount1", o.MaxItemCount1.ToString());
		dictionary.Add("MinItemCount2", o.MinItemCount2.ToString());
		dictionary.Add("MaxItemCount2", o.MaxItemCount2.ToString());
		dictionary.Add("MinItemCount3", o.MinItemCount3.ToString());
		dictionary.Add("MaxItemCount3", o.MaxItemCount3.ToString());
		dictionary.Add("MinItemCount4", o.MinItemCount4.ToString());
		dictionary.Add("MaxItemCount4", o.MaxItemCount4.ToString());
		dictionary.Add("MinItemCount5", o.MinItemCount5.ToString());
		dictionary.Add("MaxItemCount5", o.MaxItemCount5.ToString());
		return dictionary;
	}
}
