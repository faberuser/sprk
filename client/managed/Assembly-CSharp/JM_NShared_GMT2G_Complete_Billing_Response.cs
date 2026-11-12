using System.Collections;
using MiniJSON;
using NShared;
using NShared.GMT2G_Complete_Billing;

public static class JM_NShared_GMT2G_Complete_Billing_Response
{
	public static NShared.GMT2G_Complete_Billing.Response Parse(string s)
	{
		return Parse(s, new NShared.GMT2G_Complete_Billing.Response());
	}

	public static NShared.GMT2G_Complete_Billing.Response Parse(string s, NShared.GMT2G_Complete_Billing.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GMT2G_Complete_Billing.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.GMT2G_Complete_Billing.Response());
	}

	public static NShared.GMT2G_Complete_Billing.Response Parse(IDictionary d, NShared.GMT2G_Complete_Billing.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		return o;
	}
}
