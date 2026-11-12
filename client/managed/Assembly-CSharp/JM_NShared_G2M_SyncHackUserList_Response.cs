using System.Collections;
using MiniJSON;
using NShared;
using NShared.G2M_SyncHackUserList;

public static class JM_NShared_G2M_SyncHackUserList_Response
{
	public static NShared.G2M_SyncHackUserList.Response Parse(string s)
	{
		return Parse(s, new NShared.G2M_SyncHackUserList.Response());
	}

	public static NShared.G2M_SyncHackUserList.Response Parse(string s, NShared.G2M_SyncHackUserList.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2M_SyncHackUserList.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2M_SyncHackUserList.Response());
	}

	public static NShared.G2M_SyncHackUserList.Response Parse(IDictionary d, NShared.G2M_SyncHackUserList.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		return o;
	}
}
