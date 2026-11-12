using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleMessageRes
{
	public static BattleMessageRes Parse(string s)
	{
		return Parse(s, new BattleMessageRes());
	}

	public static BattleMessageRes Parse(string s, BattleMessageRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleMessageRes Parse(IDictionary d)
	{
		return Parse(d, new BattleMessageRes());
	}

	public static BattleMessageRes Parse(IDictionary d, BattleMessageRes o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<BattleMessageRes.ResultType>(d, "Result");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(BattleMessageRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleMessageRes o)
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
