using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;
using NShared.NParty;

public static class JM_NShared_NMessage_NProtocol_ChangePartyMemberStatus
{
	public static ChangePartyMemberStatus Parse(string s)
	{
		return Parse(s, new ChangePartyMemberStatus());
	}

	public static ChangePartyMemberStatus Parse(string s, ChangePartyMemberStatus o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ChangePartyMemberStatus Parse(IDictionary d)
	{
		return Parse(d, new ChangePartyMemberStatus());
	}

	public static ChangePartyMemberStatus Parse(IDictionary d, ChangePartyMemberStatus o)
	{
		o.Status = BaseJsonMarshaler.ParseEnum<MemberStatus>(d, "Status");
		return o;
	}

	public static string EncodeJsonString(ChangePartyMemberStatus o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ChangePartyMemberStatus o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Status", o.Status.ToString());
		return dictionary;
	}
}
