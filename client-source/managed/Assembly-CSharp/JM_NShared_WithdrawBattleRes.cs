using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_WithdrawBattleRes
{
	public static WithdrawBattleRes Parse(string s)
	{
		return Parse(s, new WithdrawBattleRes());
	}

	public static WithdrawBattleRes Parse(string s, WithdrawBattleRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WithdrawBattleRes Parse(IDictionary d)
	{
		return Parse(d, new WithdrawBattleRes());
	}

	public static WithdrawBattleRes Parse(IDictionary d, WithdrawBattleRes o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<WithdrawBattleRes.ResultType>(d, "Result");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(WithdrawBattleRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WithdrawBattleRes o)
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
