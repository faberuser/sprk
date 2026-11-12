using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_CanDuelRequest
{
	public static CanDuelRequest Parse(string s)
	{
		return Parse(s, new CanDuelRequest());
	}

	public static CanDuelRequest Parse(string s, CanDuelRequest o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CanDuelRequest Parse(IDictionary d)
	{
		return Parse(d, new CanDuelRequest());
	}

	public static CanDuelRequest Parse(IDictionary d, CanDuelRequest o)
	{
		return o;
	}

	public static string EncodeJsonString(CanDuelRequest o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CanDuelRequest o)
	{
		return new Dictionary<string, object>();
	}
}
