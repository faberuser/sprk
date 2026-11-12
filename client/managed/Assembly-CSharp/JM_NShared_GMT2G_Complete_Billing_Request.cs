using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GMT2G_Complete_Billing;

public static class JM_NShared_GMT2G_Complete_Billing_Request
{
	public static NShared.GMT2G_Complete_Billing.Request Parse(string s)
	{
		return Parse(s, new NShared.GMT2G_Complete_Billing.Request());
	}

	public static NShared.GMT2G_Complete_Billing.Request Parse(string s, NShared.GMT2G_Complete_Billing.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GMT2G_Complete_Billing.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GMT2G_Complete_Billing.Request());
	}

	public static NShared.GMT2G_Complete_Billing.Request Parse(IDictionary d, NShared.GMT2G_Complete_Billing.Request o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.Uid = BaseJsonMarshaler.ParseValueString(d, "Uid");
		return o;
	}

	public static string EncodeJsonString(NShared.GMT2G_Complete_Billing.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GMT2G_Complete_Billing.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		if (o.Uid != null)
		{
			dictionary.Add("Uid", o.Uid);
		}
		return dictionary;
	}
}
