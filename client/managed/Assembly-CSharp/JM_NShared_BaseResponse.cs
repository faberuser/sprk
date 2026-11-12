using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BaseResponse
{
	public static BaseResponse Parse(string s)
	{
		return Parse(s, new BaseResponse());
	}

	public static BaseResponse Parse(string s, BaseResponse o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BaseResponse Parse(IDictionary d)
	{
		return Parse(d, new BaseResponse());
	}

	public static BaseResponse Parse(IDictionary d, BaseResponse o)
	{
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(BaseResponse o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BaseResponse o)
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
