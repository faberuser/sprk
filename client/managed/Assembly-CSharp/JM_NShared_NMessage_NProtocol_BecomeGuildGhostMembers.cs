using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_BecomeGuildGhostMembers
{
	public static BecomeGuildGhostMembers Parse(string s)
	{
		return Parse(s, new BecomeGuildGhostMembers());
	}

	public static BecomeGuildGhostMembers Parse(string s, BecomeGuildGhostMembers o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BecomeGuildGhostMembers Parse(IDictionary d)
	{
		return Parse(d, new BecomeGuildGhostMembers());
	}

	public static BecomeGuildGhostMembers Parse(IDictionary d, BecomeGuildGhostMembers o)
	{
		o.GhostMemberIds = BaseJsonMarshaler.ParseArrayCast(d, "GhostMemberIds", BaseJsonMarshaler.CastToLong);
		return o;
	}

	public static string EncodeJsonString(BecomeGuildGhostMembers o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BecomeGuildGhostMembers o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GhostMemberIds", BaseJsonMarshaler.EncodeArray(o.GhostMemberIds));
		return dictionary;
	}
}
