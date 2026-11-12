using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ChangePetAvatar;

public static class JM_NShared_ChangePetAvatar_Request
{
	public static NShared.ChangePetAvatar.Request Parse(string s)
	{
		return Parse(s, new NShared.ChangePetAvatar.Request());
	}

	public static NShared.ChangePetAvatar.Request Parse(string s, NShared.ChangePetAvatar.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ChangePetAvatar.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ChangePetAvatar.Request());
	}

	public static NShared.ChangePetAvatar.Request Parse(IDictionary d, NShared.ChangePetAvatar.Request o)
	{
		o.AvatarPetIndex = BaseJsonMarshaler.ParseValueInt(d, "AvatarPetIndex");
		o.PetIndex = BaseJsonMarshaler.ParseValueInt(d, "PetIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ChangePetAvatar.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ChangePetAvatar.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AvatarPetIndex", o.AvatarPetIndex.ToString());
		dictionary.Add("PetIndex", o.PetIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
