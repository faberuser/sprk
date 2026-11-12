using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage;

public static class JM_NShared_NMessage_ClearMessageReq
{
	public static ClearMessageReq Parse(string s)
	{
		return Parse(s, new ClearMessageReq());
	}

	public static ClearMessageReq Parse(string s, ClearMessageReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ClearMessageReq Parse(IDictionary d)
	{
		return Parse(d, new ClearMessageReq());
	}

	public static ClearMessageReq Parse(IDictionary d, ClearMessageReq o)
	{
		o.MessageTypeName = BaseJsonMarshaler.ParseValueString(d, "MessageTypeName");
		o.RemoveAccountId = BaseJsonMarshaler.ParseValueLong(d, "RemoveAccountId");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(ClearMessageReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ClearMessageReq o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.MessageTypeName != null)
		{
			dictionary.Add("MessageTypeName", o.MessageTypeName);
		}
		dictionary.Add("RemoveAccountId", o.RemoveAccountId.ToString());
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}
