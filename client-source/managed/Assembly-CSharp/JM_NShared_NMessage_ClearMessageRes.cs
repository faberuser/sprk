using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage;

public static class JM_NShared_NMessage_ClearMessageRes
{
	public static ClearMessageRes Parse(string s)
	{
		return Parse(s, new ClearMessageRes());
	}

	public static ClearMessageRes Parse(string s, ClearMessageRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ClearMessageRes Parse(IDictionary d)
	{
		return Parse(d, new ClearMessageRes());
	}

	public static ClearMessageRes Parse(IDictionary d, ClearMessageRes o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ClearMessageResType>(d, "Result");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(ClearMessageRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ClearMessageRes o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Result", o.Result.ToString());
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}
