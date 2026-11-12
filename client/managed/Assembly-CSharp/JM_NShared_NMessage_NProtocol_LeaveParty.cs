using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_LeaveParty
{
	public static LeaveParty Parse(string s)
	{
		return Parse(s, new LeaveParty());
	}

	public static LeaveParty Parse(string s, LeaveParty o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static LeaveParty Parse(IDictionary d)
	{
		return Parse(d, new LeaveParty());
	}

	public static LeaveParty Parse(IDictionary d, LeaveParty o)
	{
		return o;
	}

	public static string EncodeJsonString(LeaveParty o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(LeaveParty o)
	{
		return new Dictionary<string, object>();
	}
}
