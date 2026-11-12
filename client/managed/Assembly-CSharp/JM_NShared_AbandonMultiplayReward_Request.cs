using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.AbandonMultiplayReward;

public static class JM_NShared_AbandonMultiplayReward_Request
{
	public static NShared.AbandonMultiplayReward.Request Parse(string s)
	{
		return Parse(s, new NShared.AbandonMultiplayReward.Request());
	}

	public static NShared.AbandonMultiplayReward.Request Parse(string s, NShared.AbandonMultiplayReward.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.AbandonMultiplayReward.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.AbandonMultiplayReward.Request());
	}

	public static NShared.AbandonMultiplayReward.Request Parse(IDictionary d, NShared.AbandonMultiplayReward.Request o)
	{
		o.MasterAccountId = BaseJsonMarshaler.ParseValueLong(d, "MasterAccountId");
		o.MasterServerIp = BaseJsonMarshaler.ParseValueString(d, "MasterServerIp");
		o.MasterServerPort = BaseJsonMarshaler.ParseValueInt(d, "MasterServerPort");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.AbandonMultiplayReward.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.AbandonMultiplayReward.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MasterAccountId", o.MasterAccountId.ToString());
		if (o.MasterServerIp != null)
		{
			dictionary.Add("MasterServerIp", o.MasterServerIp);
		}
		dictionary.Add("MasterServerPort", o.MasterServerPort.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
