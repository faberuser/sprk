using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_AvatarChangeNot
{
	public static AvatarChangeNot Parse(string s)
	{
		return Parse(s, new AvatarChangeNot());
	}

	public static AvatarChangeNot Parse(string s, AvatarChangeNot o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static AvatarChangeNot Parse(IDictionary d)
	{
		return Parse(d, new AvatarChangeNot());
	}

	public static AvatarChangeNot Parse(IDictionary d, AvatarChangeNot o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		return o;
	}

	public static string EncodeJsonString(AvatarChangeNot o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(AvatarChangeNot o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		return dictionary;
	}
}
