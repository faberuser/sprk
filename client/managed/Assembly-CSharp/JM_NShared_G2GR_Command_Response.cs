using System.Collections;
using MiniJSON;
using NShared;
using NShared.G2GR_Command;

public static class JM_NShared_G2GR_Command_Response
{
	public static NShared.G2GR_Command.Response Parse(string s)
	{
		return Parse(s, new NShared.G2GR_Command.Response());
	}

	public static NShared.G2GR_Command.Response Parse(string s, NShared.G2GR_Command.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2GR_Command.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2GR_Command.Response());
	}

	public static NShared.G2GR_Command.Response Parse(IDictionary d, NShared.G2GR_Command.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		return o;
	}
}
