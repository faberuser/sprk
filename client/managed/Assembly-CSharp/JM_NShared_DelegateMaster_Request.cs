using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.DelegateMaster;

public static class JM_NShared_DelegateMaster_Request
{
	public static NShared.DelegateMaster.Request Parse(string s)
	{
		return Parse(s, new NShared.DelegateMaster.Request());
	}

	public static NShared.DelegateMaster.Request Parse(string s, NShared.DelegateMaster.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.DelegateMaster.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.DelegateMaster.Request());
	}

	public static NShared.DelegateMaster.Request Parse(IDictionary d, NShared.DelegateMaster.Request o)
	{
		o.DelegateAccountId = BaseJsonMarshaler.ParseValueLong(d, "DelegateAccountId");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.DelegateMaster.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.DelegateMaster.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("DelegateAccountId", o.DelegateAccountId.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
