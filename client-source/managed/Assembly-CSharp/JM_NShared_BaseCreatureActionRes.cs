using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BaseCreatureActionRes
{
	public static BaseCreatureActionRes Parse(string s)
	{
		return Parse(s, new BaseCreatureActionRes());
	}

	public static BaseCreatureActionRes Parse(string s, BaseCreatureActionRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BaseCreatureActionRes Parse(IDictionary d)
	{
		return Parse(d, new BaseCreatureActionRes());
	}

	public static BaseCreatureActionRes Parse(IDictionary d, BaseCreatureActionRes o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<BaseCreatureActionRes.ResultType>(d, "Result");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(BaseCreatureActionRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BaseCreatureActionRes o)
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
