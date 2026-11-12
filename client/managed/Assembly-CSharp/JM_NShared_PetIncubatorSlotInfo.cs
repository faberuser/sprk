using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PetIncubatorSlotInfo
{
	public static PetIncubatorSlotInfo Parse(string s)
	{
		return Parse(s, new PetIncubatorSlotInfo());
	}

	public static PetIncubatorSlotInfo Parse(string s, PetIncubatorSlotInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PetIncubatorSlotInfo Parse(IDictionary d)
	{
		return Parse(d, new PetIncubatorSlotInfo());
	}

	public static PetIncubatorSlotInfo Parse(IDictionary d, PetIncubatorSlotInfo o)
	{
		o.SlotIndex = BaseJsonMarshaler.ParseValueInt(d, "SlotIndex");
		o.MountingIndex = BaseJsonMarshaler.ParseValueInt(d, "MountingIndex");
		o.SetItemIndex = BaseJsonMarshaler.ParseValueInt(d, "SetItemIndex");
		o.CompletedTime = BaseJsonMarshaler.ParseValueString(d, "CompletedTime");
		return o;
	}

	public static string EncodeJsonString(PetIncubatorSlotInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PetIncubatorSlotInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SlotIndex", o.SlotIndex.ToString());
		dictionary.Add("MountingIndex", o.MountingIndex.ToString());
		dictionary.Add("SetItemIndex", o.SetItemIndex.ToString());
		if (o.CompletedTime != null)
		{
			dictionary.Add("CompletedTime", o.CompletedTime);
		}
		return dictionary;
	}
}
