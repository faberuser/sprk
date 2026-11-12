using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.InitAccount;

public static class JM_NShared_InitAccount_Request
{
	public static NShared.InitAccount.Request Parse(string s)
	{
		return Parse(s, new NShared.InitAccount.Request());
	}

	public static NShared.InitAccount.Request Parse(string s, NShared.InitAccount.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.InitAccount.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.InitAccount.Request());
	}

	public static NShared.InitAccount.Request Parse(IDictionary d, NShared.InitAccount.Request o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		return o;
	}

	public static string EncodeJsonString(NShared.InitAccount.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.InitAccount.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		return dictionary;
	}
}
