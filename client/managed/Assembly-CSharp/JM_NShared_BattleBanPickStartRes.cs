using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleBanPickStartRes
{
	public static BattleBanPickStartRes Parse(string s)
	{
		return Parse(s, new BattleBanPickStartRes());
	}

	public static BattleBanPickStartRes Parse(string s, BattleBanPickStartRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleBanPickStartRes Parse(IDictionary d)
	{
		return Parse(d, new BattleBanPickStartRes());
	}

	public static BattleBanPickStartRes Parse(IDictionary d, BattleBanPickStartRes o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<BattleBanPickStartRes.ResultType>(d, "Result");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(BattleBanPickStartRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleBanPickStartRes o)
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
