using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PetIncubatorInfo
{
	public static PetIncubatorInfo Parse(string s)
	{
		return Parse(s, new PetIncubatorInfo());
	}

	public static PetIncubatorInfo Parse(string s, PetIncubatorInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PetIncubatorInfo Parse(IDictionary d)
	{
		return Parse(d, new PetIncubatorInfo());
	}

	public static PetIncubatorInfo Parse(IDictionary d, PetIncubatorInfo o)
	{
		o.SlotIndex = BaseJsonMarshaler.ParseValueInt(d, "SlotIndex");
		o.IncubatorIndex = BaseJsonMarshaler.ParseValueInt(d, "IncubatorIndex");
		o.CreatedTime = BaseJsonMarshaler.ParseValueString(d, "CreatedTime");
		return o;
	}

	public static string EncodeJsonString(PetIncubatorInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PetIncubatorInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SlotIndex", o.SlotIndex.ToString());
		dictionary.Add("IncubatorIndex", o.IncubatorIndex.ToString());
		if (o.CreatedTime != null)
		{
			dictionary.Add("CreatedTime", o.CreatedTime);
		}
		return dictionary;
	}
}
