using System.Collections;
using MiniJSON;
using NShared;
using NShared.G2G_DropUser;

public static class JM_NShared_G2G_DropUser_Response
{
	public static NShared.G2G_DropUser.Response Parse(string s)
	{
		return Parse(s, new NShared.G2G_DropUser.Response());
	}

	public static NShared.G2G_DropUser.Response Parse(string s, NShared.G2G_DropUser.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2G_DropUser.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2G_DropUser.Response());
	}

	public static NShared.G2G_DropUser.Response Parse(IDictionary d, NShared.G2G_DropUser.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		return o;
	}
}
