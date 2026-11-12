using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.InspectUser;

public static class JM_NShared_InspectUser_Request
{
	public static NShared.InspectUser.Request Parse(string s)
	{
		return Parse(s, new NShared.InspectUser.Request());
	}

	public static NShared.InspectUser.Request Parse(string s, NShared.InspectUser.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.InspectUser.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.InspectUser.Request());
	}

	public static NShared.InspectUser.Request Parse(IDictionary d, NShared.InspectUser.Request o)
	{
		o.TargetAccountId = BaseJsonMarshaler.ParseValueLong(d, "TargetAccountId");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.InspectUser.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.InspectUser.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("TargetAccountId", o.TargetAccountId.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
