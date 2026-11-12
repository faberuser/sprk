using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_InviteRematchRes
{
	public static InviteRematchRes Parse(string s)
	{
		return Parse(s, new InviteRematchRes());
	}

	public static InviteRematchRes Parse(string s, InviteRematchRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static InviteRematchRes Parse(IDictionary d)
	{
		return Parse(d, new InviteRematchRes());
	}

	public static InviteRematchRes Parse(IDictionary d, InviteRematchRes o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<InviteRematchRes.ResultType>(d, "Result");
		o.MatchInfo = BaseJsonMarshaler.ParseObject(d, "MatchInfo", JM_NShared_MatchInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(InviteRematchRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(InviteRematchRes o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Result", o.Result.ToString());
		dictionary.Add("MatchInfo", JM_NShared_MatchInfo.EncodeJsonObject(o.MatchInfo));
		return dictionary;
	}
}
