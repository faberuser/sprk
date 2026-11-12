using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_KickedRaidMemberRes
{
	public static KickedRaidMemberRes Parse(string s)
	{
		return Parse(s, new KickedRaidMemberRes());
	}

	public static KickedRaidMemberRes Parse(string s, KickedRaidMemberRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static KickedRaidMemberRes Parse(IDictionary d)
	{
		return Parse(d, new KickedRaidMemberRes());
	}

	public static KickedRaidMemberRes Parse(IDictionary d, KickedRaidMemberRes o)
	{
		o.KickedAccountIds = BaseJsonMarshaler.ParseArrayCast(d, "KickedAccountIds", BaseJsonMarshaler.CastToLong);
		o.ReplaceMasterAccountId = BaseJsonMarshaler.ParseValueLong(d, "ReplaceMasterAccountId");
		return o;
	}

	public static string EncodeJsonString(KickedRaidMemberRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(KickedRaidMemberRes o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("KickedAccountIds", BaseJsonMarshaler.EncodeArray(o.KickedAccountIds));
		dictionary.Add("ReplaceMasterAccountId", o.ReplaceMasterAccountId.ToString());
		return dictionary;
	}
}
