using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.G2M_SyncHackUserList;

public static class JM_NShared_G2M_SyncHackUserList_Request
{
	public static NShared.G2M_SyncHackUserList.Request Parse(string s)
	{
		return Parse(s, new NShared.G2M_SyncHackUserList.Request());
	}

	public static NShared.G2M_SyncHackUserList.Request Parse(string s, NShared.G2M_SyncHackUserList.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2M_SyncHackUserList.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2M_SyncHackUserList.Request());
	}

	public static NShared.G2M_SyncHackUserList.Request Parse(IDictionary d, NShared.G2M_SyncHackUserList.Request o)
	{
		o.Host = BaseJsonMarshaler.ParseValueString(d, "Host");
		o.HackUserAccountIds = BaseJsonMarshaler.ParseValueString(d, "HackUserAccountIds");
		return o;
	}

	public static string EncodeJsonString(NShared.G2M_SyncHackUserList.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.G2M_SyncHackUserList.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Host != null)
		{
			dictionary.Add("Host", o.Host);
		}
		if (o.HackUserAccountIds != null)
		{
			dictionary.Add("HackUserAccountIds", o.HackUserAccountIds);
		}
		return dictionary;
	}
}
