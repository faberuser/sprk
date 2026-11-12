using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EclipseContinuousBattleRes
{
	public static EclipseContinuousBattleRes Parse(string s)
	{
		return Parse(s, new EclipseContinuousBattleRes());
	}

	public static EclipseContinuousBattleRes Parse(string s, EclipseContinuousBattleRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EclipseContinuousBattleRes Parse(IDictionary d)
	{
		return Parse(d, new EclipseContinuousBattleRes());
	}

	public static EclipseContinuousBattleRes Parse(IDictionary d, EclipseContinuousBattleRes o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<EclipseContinuousBattleRes.ResultType>(d, "Result");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(EclipseContinuousBattleRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EclipseContinuousBattleRes o)
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
