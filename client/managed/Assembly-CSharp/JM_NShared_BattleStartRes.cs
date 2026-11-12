using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleStartRes
{
	public static BattleStartRes Parse(string s)
	{
		return Parse(s, new BattleStartRes());
	}

	public static BattleStartRes Parse(string s, BattleStartRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleStartRes Parse(IDictionary d)
	{
		return Parse(d, new BattleStartRes());
	}

	public static BattleStartRes Parse(IDictionary d, BattleStartRes o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<BattleStartRes.ResultType>(d, "Result");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(BattleStartRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleStartRes o)
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
