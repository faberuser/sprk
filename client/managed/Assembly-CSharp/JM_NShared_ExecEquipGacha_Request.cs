using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ExecEquipGacha;

public static class JM_NShared_ExecEquipGacha_Request
{
	public static NShared.ExecEquipGacha.Request Parse(string s)
	{
		return Parse(s, new NShared.ExecEquipGacha.Request());
	}

	public static NShared.ExecEquipGacha.Request Parse(string s, NShared.ExecEquipGacha.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ExecEquipGacha.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ExecEquipGacha.Request());
	}

	public static NShared.ExecEquipGacha.Request Parse(IDictionary d, NShared.ExecEquipGacha.Request o)
	{
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.GachaIndex = BaseJsonMarshaler.ParseValueInt(d, "GachaIndex");
		o.HeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "HeroIndices", BaseJsonMarshaler.CastToInt);
		o.HighGachaCategory = BaseJsonMarshaler.ParseEnum<HighGachaCategoryType>(d, "HighGachaCategory");
		o.Free = BaseJsonMarshaler.ParseValueBool(d, "Free");
		o.Discount = BaseJsonMarshaler.ParseValueBool(d, "Discount");
		o.DiscountedCost = BaseJsonMarshaler.ParseValueInt(d, "DiscountedCost");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ExecEquipGacha.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ExecEquipGacha.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("GachaIndex", o.GachaIndex.ToString());
		dictionary.Add("HeroIndices", BaseJsonMarshaler.EncodeArray(o.HeroIndices));
		dictionary.Add("HighGachaCategory", o.HighGachaCategory.ToString());
		dictionary.Add("Free", o.Free.ToString());
		dictionary.Add("Discount", o.Discount.ToString());
		dictionary.Add("DiscountedCost", o.DiscountedCost.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
