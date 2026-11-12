using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.PetAwaken;

public static class JM_NShared_PetAwaken_Request
{
	public static NShared.PetAwaken.Request Parse(string s)
	{
		return Parse(s, new NShared.PetAwaken.Request());
	}

	public static NShared.PetAwaken.Request Parse(string s, NShared.PetAwaken.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.PetAwaken.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.PetAwaken.Request());
	}

	public static NShared.PetAwaken.Request Parse(IDictionary d, NShared.PetAwaken.Request o)
	{
		o.PetIndex = BaseJsonMarshaler.ParseValueInt(d, "PetIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.PetAwaken.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.PetAwaken.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("PetIndex", o.PetIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
