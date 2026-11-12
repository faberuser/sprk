using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.PetHappyGift;

public static class JM_NShared_PetHappyGift_Request
{
	public static NShared.PetHappyGift.Request Parse(string s)
	{
		return Parse(s, new NShared.PetHappyGift.Request());
	}

	public static NShared.PetHappyGift.Request Parse(string s, NShared.PetHappyGift.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.PetHappyGift.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.PetHappyGift.Request());
	}

	public static NShared.PetHappyGift.Request Parse(IDictionary d, NShared.PetHappyGift.Request o)
	{
		o.PetIndex = BaseJsonMarshaler.ParseValueInt(d, "PetIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.PetHappyGift.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.PetHappyGift.Request o)
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
