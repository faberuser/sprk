using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.PetInteractive;

public static class JM_NShared_PetInteractive_Request
{
	public static NShared.PetInteractive.Request Parse(string s)
	{
		return Parse(s, new NShared.PetInteractive.Request());
	}

	public static NShared.PetInteractive.Request Parse(string s, NShared.PetInteractive.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.PetInteractive.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.PetInteractive.Request());
	}

	public static NShared.PetInteractive.Request Parse(IDictionary d, NShared.PetInteractive.Request o)
	{
		o.PetIndex = BaseJsonMarshaler.ParseValueInt(d, "PetIndex");
		o.ActionType = BaseJsonMarshaler.ParseEnum<PetActionType>(d, "ActionType");
		o.ActionCount = BaseJsonMarshaler.ParseValueInt(d, "ActionCount");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.PetInteractive.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.PetInteractive.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("PetIndex", o.PetIndex.ToString());
		dictionary.Add("ActionType", o.ActionType.ToString());
		dictionary.Add("ActionCount", o.ActionCount.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
