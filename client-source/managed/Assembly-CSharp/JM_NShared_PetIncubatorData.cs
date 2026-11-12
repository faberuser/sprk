using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PetIncubatorData
{
	public static PetIncubatorData Parse(string s)
	{
		return Parse(s, new PetIncubatorData());
	}

	public static PetIncubatorData Parse(string s, PetIncubatorData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PetIncubatorData Parse(IDictionary d)
	{
		return Parse(d, new PetIncubatorData());
	}

	public static PetIncubatorData Parse(IDictionary d, PetIncubatorData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.Grade = BaseJsonMarshaler.ParseValueInt(d, "Grade");
		o.NameLocalKey = BaseJsonMarshaler.ParseValueString(d, "NameLocalKey");
		o.DescLocalKey = BaseJsonMarshaler.ParseValueString(d, "DescLocalKey");
		o.TimePer = BaseJsonMarshaler.ParseValueInt(d, "TimePer");
		o.BuyCurrencyType = BaseJsonMarshaler.ParseEnum<CurrencyType>(d, "BuyCurrencyType");
		o.BuyCurrencyValue = BaseJsonMarshaler.ParseValueInt(d, "BuyCurrencyValue");
		o.Mileage = BaseJsonMarshaler.ParseValueInt(d, "Mileage");
		o.IconPath = BaseJsonMarshaler.ParseValueString(d, "IconPath");
		o.CreatureCode = BaseJsonMarshaler.ParseValueString(d, "CreatureCode");
		o.CreaturePosition = BaseJsonMarshaler.ParseArrayCast(d, "CreaturePosition", BaseJsonMarshaler.CastToInt);
		o.CreatureRotation = BaseJsonMarshaler.ParseArrayCast(d, "CreatureRotation", BaseJsonMarshaler.CastToInt);
		o.CreatureScale = BaseJsonMarshaler.ParseValueInt(d, "CreatureScale");
		return o;
	}

	public static string EncodeJsonString(PetIncubatorData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PetIncubatorData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("Grade", o.Grade.ToString());
		if (o.NameLocalKey != null)
		{
			dictionary.Add("NameLocalKey", o.NameLocalKey);
		}
		if (o.DescLocalKey != null)
		{
			dictionary.Add("DescLocalKey", o.DescLocalKey);
		}
		dictionary.Add("TimePer", o.TimePer.ToString());
		dictionary.Add("BuyCurrencyType", o.BuyCurrencyType.ToString());
		dictionary.Add("BuyCurrencyValue", o.BuyCurrencyValue.ToString());
		dictionary.Add("Mileage", o.Mileage.ToString());
		if (o.IconPath != null)
		{
			dictionary.Add("IconPath", o.IconPath);
		}
		if (o.CreatureCode != null)
		{
			dictionary.Add("CreatureCode", o.CreatureCode);
		}
		dictionary.Add("CreaturePosition", BaseJsonMarshaler.EncodeArray(o.CreaturePosition));
		dictionary.Add("CreatureRotation", BaseJsonMarshaler.EncodeArray(o.CreatureRotation));
		dictionary.Add("CreatureScale", o.CreatureScale.ToString());
		return dictionary;
	}
}
