using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_InvalidPartyMember
{
	public static InvalidPartyMember Parse(string s)
	{
		return Parse(s, new InvalidPartyMember());
	}

	public static InvalidPartyMember Parse(string s, InvalidPartyMember o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static InvalidPartyMember Parse(IDictionary d)
	{
		return Parse(d, new InvalidPartyMember());
	}

	public static InvalidPartyMember Parse(IDictionary d, InvalidPartyMember o)
	{
		return o;
	}

	public static string EncodeJsonString(InvalidPartyMember o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(InvalidPartyMember o)
	{
		return new Dictionary<string, object>();
	}
}
