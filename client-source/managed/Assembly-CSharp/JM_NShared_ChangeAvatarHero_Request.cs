using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ChangeAvatarHero;

public static class JM_NShared_ChangeAvatarHero_Request
{
	public static NShared.ChangeAvatarHero.Request Parse(string s)
	{
		return Parse(s, new NShared.ChangeAvatarHero.Request());
	}

	public static NShared.ChangeAvatarHero.Request Parse(string s, NShared.ChangeAvatarHero.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ChangeAvatarHero.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ChangeAvatarHero.Request());
	}

	public static NShared.ChangeAvatarHero.Request Parse(IDictionary d, NShared.ChangeAvatarHero.Request o)
	{
		o.AvatarHeroIndex = BaseJsonMarshaler.ParseValueInt(d, "AvatarHeroIndex");
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ChangeAvatarHero.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ChangeAvatarHero.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AvatarHeroIndex", o.AvatarHeroIndex.ToString());
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
