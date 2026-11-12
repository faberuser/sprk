using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PetIncubatorSlotData
{
	public static PetIncubatorSlotData Parse(string s)
	{
		return Parse(s, new PetIncubatorSlotData());
	}

	public static PetIncubatorSlotData Parse(string s, PetIncubatorSlotData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PetIncubatorSlotData Parse(IDictionary d)
	{
		return Parse(d, new PetIncubatorSlotData());
	}

	public static PetIncubatorSlotData Parse(IDictionary d, PetIncubatorSlotData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.BuyCurrencyType = BaseJsonMarshaler.ParseEnum<CurrencyType>(d, "BuyCurrencyType");
		o.BuyCurrencyValue = BaseJsonMarshaler.ParseValueInt(d, "BuyCurrencyValue");
		o.OpenNum = BaseJsonMarshaler.ParseValueInt(d, "OpenNum");
		o.Position = BaseJsonMarshaler.ParseArrayCast(d, "Position", BaseJsonMarshaler.CastToFloat);
		o.Rotation = BaseJsonMarshaler.ParseArrayCast(d, "Rotation", BaseJsonMarshaler.CastToFloat);
		o.DefaultIndex = BaseJsonMarshaler.ParseValueInt(d, "DefaultIndex");
		o.Mileage = BaseJsonMarshaler.ParseValueInt(d, "Mileage");
		return o;
	}

	public static string EncodeJsonString(PetIncubatorSlotData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PetIncubatorSlotData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("BuyCurrencyType", o.BuyCurrencyType.ToString());
		dictionary.Add("BuyCurrencyValue", o.BuyCurrencyValue.ToString());
		dictionary.Add("OpenNum", o.OpenNum.ToString());
		dictionary.Add("Position", BaseJsonMarshaler.EncodeArray(o.Position));
		dictionary.Add("Rotation", BaseJsonMarshaler.EncodeArray(o.Rotation));
		dictionary.Add("DefaultIndex", o.DefaultIndex.ToString());
		dictionary.Add("Mileage", o.Mileage.ToString());
		return dictionary;
	}
}
