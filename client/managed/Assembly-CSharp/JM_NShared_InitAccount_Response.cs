using System.Collections;
using MiniJSON;
using NShared;
using NShared.InitAccount;

public static class JM_NShared_InitAccount_Response
{
	public static NShared.InitAccount.Response Parse(string s)
	{
		return Parse(s, new NShared.InitAccount.Response());
	}

	public static NShared.InitAccount.Response Parse(string s, NShared.InitAccount.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.InitAccount.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.InitAccount.Response());
	}

	public static NShared.InitAccount.Response Parse(IDictionary d, NShared.InitAccount.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		return o;
	}
}
