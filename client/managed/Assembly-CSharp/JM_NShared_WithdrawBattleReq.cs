using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_WithdrawBattleReq
{
	public static WithdrawBattleReq Parse(string s)
	{
		return Parse(s, new WithdrawBattleReq());
	}

	public static WithdrawBattleReq Parse(string s, WithdrawBattleReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WithdrawBattleReq Parse(IDictionary d)
	{
		return Parse(d, new WithdrawBattleReq());
	}

	public static WithdrawBattleReq Parse(IDictionary d, WithdrawBattleReq o)
	{
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(WithdrawBattleReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WithdrawBattleReq o)
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
