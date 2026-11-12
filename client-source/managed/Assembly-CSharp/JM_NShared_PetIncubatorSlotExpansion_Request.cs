using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.PetIncubatorSlotExpansion;

public static class JM_NShared_PetIncubatorSlotExpansion_Request
{
	public static NShared.PetIncubatorSlotExpansion.Request Parse(string s)
	{
		return Parse(s, new NShared.PetIncubatorSlotExpansion.Request());
	}

	public static NShared.PetIncubatorSlotExpansion.Request Parse(string s, NShared.PetIncubatorSlotExpansion.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.PetIncubatorSlotExpansion.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.PetIncubatorSlotExpansion.Request());
	}

	public static NShared.PetIncubatorSlotExpansion.Request Parse(IDictionary d, NShared.PetIncubatorSlotExpansion.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.PetIncubatorSlotExpansion.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.PetIncubatorSlotExpansion.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
