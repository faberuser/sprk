using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BoosterDefineData
{
	public static BoosterDefineData Parse(string s)
	{
		return Parse(s, new BoosterDefineData());
	}

	public static BoosterDefineData Parse(string s, BoosterDefineData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BoosterDefineData Parse(IDictionary d)
	{
		return Parse(d, new BoosterDefineData());
	}

	public static BoosterDefineData Parse(IDictionary d, BoosterDefineData o)
	{
		o.BoosterCode = BaseJsonMarshaler.ParseValueString(d, "BoosterCode");
		o.BonusType = BaseJsonMarshaler.ParseEnum<EventBonusType>(d, "BonusType");
		o.CostumeBuffType = BaseJsonMarshaler.ParseEnum<ECostumeBuffType>(d, "CostumeBuffType");
		o.IconItemCode = BaseJsonMarshaler.ParseValueString(d, "IconItemCode");
		o.TooltipName = BaseJsonMarshaler.ParseValueString(d, "TooltipName");
		o.TooltipDesc = BaseJsonMarshaler.ParseValueString(d, "TooltipDesc");
		o.PopUpBuffCategory = BaseJsonMarshaler.ParseValueString(d, "PopUpBuffCategory");
		o.PopUpBuffDetail = BaseJsonMarshaler.ParseValueString(d, "PopUpBuffDetail");
		return o;
	}

	public static string EncodeJsonString(BoosterDefineData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BoosterDefineData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.BoosterCode != null)
		{
			dictionary.Add("BoosterCode", o.BoosterCode);
		}
		dictionary.Add("BonusType", o.BonusType.ToString());
		dictionary.Add("CostumeBuffType", o.CostumeBuffType.ToString());
		if (o.IconItemCode != null)
		{
			dictionary.Add("IconItemCode", o.IconItemCode);
		}
		if (o.TooltipName != null)
		{
			dictionary.Add("TooltipName", o.TooltipName);
		}
		if (o.TooltipDesc != null)
		{
			dictionary.Add("TooltipDesc", o.TooltipDesc);
		}
		if (o.PopUpBuffCategory != null)
		{
			dictionary.Add("PopUpBuffCategory", o.PopUpBuffCategory);
		}
		if (o.PopUpBuffDetail != null)
		{
			dictionary.Add("PopUpBuffDetail", o.PopUpBuffDetail);
		}
		return dictionary;
	}
}
