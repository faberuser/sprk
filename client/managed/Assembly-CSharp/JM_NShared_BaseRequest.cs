using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BaseRequest
{
	public static BaseRequest Parse(string s)
	{
		return Parse(s, new BaseRequest());
	}

	public static BaseRequest Parse(string s, BaseRequest o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BaseRequest Parse(IDictionary d)
	{
		return Parse(d, new BaseRequest());
	}

	public static BaseRequest Parse(IDictionary d, BaseRequest o)
	{
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(BaseRequest o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BaseRequest o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}
