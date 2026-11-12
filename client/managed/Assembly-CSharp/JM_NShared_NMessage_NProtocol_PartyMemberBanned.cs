using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_PartyMemberBanned
{
	public static PartyMemberBanned Parse(string s)
	{
		return Parse(s, new PartyMemberBanned());
	}

	public static PartyMemberBanned Parse(string s, PartyMemberBanned o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PartyMemberBanned Parse(IDictionary d)
	{
		return Parse(d, new PartyMemberBanned());
	}

	public static PartyMemberBanned Parse(IDictionary d, PartyMemberBanned o)
	{
		o.BannedId = BaseJsonMarshaler.ParseValueLong(d, "BannedId");
		return o;
	}

	public static string EncodeJsonString(PartyMemberBanned o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PartyMemberBanned o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("BannedId", o.BannedId.ToString());
		return dictionary;
	}
}
