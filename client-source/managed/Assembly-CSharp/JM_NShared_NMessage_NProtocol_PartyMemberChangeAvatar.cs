using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_PartyMemberChangeAvatar
{
	public static PartyMemberChangeAvatar Parse(string s)
	{
		return Parse(s, new PartyMemberChangeAvatar());
	}

	public static PartyMemberChangeAvatar Parse(string s, PartyMemberChangeAvatar o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PartyMemberChangeAvatar Parse(IDictionary d)
	{
		return Parse(d, new PartyMemberChangeAvatar());
	}

	public static PartyMemberChangeAvatar Parse(IDictionary d, PartyMemberChangeAvatar o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		return o;
	}

	public static string EncodeJsonString(PartyMemberChangeAvatar o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PartyMemberChangeAvatar o)
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
