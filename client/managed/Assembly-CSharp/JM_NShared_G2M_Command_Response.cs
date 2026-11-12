using System.Collections;
using MiniJSON;
using NShared;
using NShared.G2M_Command;

public static class JM_NShared_G2M_Command_Response
{
	public static NShared.G2M_Command.Response Parse(string s)
	{
		return Parse(s, new NShared.G2M_Command.Response());
	}

	public static NShared.G2M_Command.Response Parse(string s, NShared.G2M_Command.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2M_Command.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2M_Command.Response());
	}

	public static NShared.G2M_Command.Response Parse(IDictionary d, NShared.G2M_Command.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		return o;
	}
}
