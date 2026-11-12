using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_PartyMemberPing
{
	public static PartyMemberPing Parse(string s)
	{
		return Parse(s, new PartyMemberPing());
	}

	public static PartyMemberPing Parse(string s, PartyMemberPing o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PartyMemberPing Parse(IDictionary d)
	{
		return Parse(d, new PartyMemberPing());
	}

	public static PartyMemberPing Parse(IDictionary d, PartyMemberPing o)
	{
		return o;
	}

	public static string EncodeJsonString(PartyMemberPing o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PartyMemberPing o)
	{
		return new Dictionary<string, object>();
	}
}
