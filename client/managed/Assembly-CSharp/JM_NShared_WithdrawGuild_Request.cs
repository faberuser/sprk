using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.WithdrawGuild;

public static class JM_NShared_WithdrawGuild_Request
{
	public static NShared.WithdrawGuild.Request Parse(string s)
	{
		return Parse(s, new NShared.WithdrawGuild.Request());
	}

	public static NShared.WithdrawGuild.Request Parse(string s, NShared.WithdrawGuild.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.WithdrawGuild.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.WithdrawGuild.Request());
	}

	public static NShared.WithdrawGuild.Request Parse(IDictionary d, NShared.WithdrawGuild.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.WithdrawGuild.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.WithdrawGuild.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
