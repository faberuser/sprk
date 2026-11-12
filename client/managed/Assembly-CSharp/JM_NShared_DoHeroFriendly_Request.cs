using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.DoHeroFriendly;

public static class JM_NShared_DoHeroFriendly_Request
{
	public static NShared.DoHeroFriendly.Request Parse(string s)
	{
		return Parse(s, new NShared.DoHeroFriendly.Request());
	}

	public static NShared.DoHeroFriendly.Request Parse(string s, NShared.DoHeroFriendly.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.DoHeroFriendly.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.DoHeroFriendly.Request());
	}

	public static NShared.DoHeroFriendly.Request Parse(IDictionary d, NShared.DoHeroFriendly.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.Action = BaseJsonMarshaler.ParseEnum<FriendlyActionType>(d, "Action");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.DoHeroFriendly.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.DoHeroFriendly.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("Action", o.Action.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
