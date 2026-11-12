using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_SetUserAvatarHero
{
	public static SetUserAvatarHero Parse(string s)
	{
		return Parse(s, new SetUserAvatarHero());
	}

	public static SetUserAvatarHero Parse(string s, SetUserAvatarHero o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static SetUserAvatarHero Parse(IDictionary d)
	{
		return Parse(d, new SetUserAvatarHero());
	}

	public static SetUserAvatarHero Parse(IDictionary d, SetUserAvatarHero o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.AvatarHeroIndex = BaseJsonMarshaler.ParseValueInt(d, "AvatarHeroIndex");
		return o;
	}

	public static string EncodeJsonString(SetUserAvatarHero o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(SetUserAvatarHero o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("AvatarHeroIndex", o.AvatarHeroIndex.ToString());
		return dictionary;
	}
}
