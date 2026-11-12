using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleGiveUpRes
{
	public static BattleGiveUpRes Parse(string s)
	{
		return Parse(s, new BattleGiveUpRes());
	}

	public static BattleGiveUpRes Parse(string s, BattleGiveUpRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleGiveUpRes Parse(IDictionary d)
	{
		return Parse(d, new BattleGiveUpRes());
	}

	public static BattleGiveUpRes Parse(IDictionary d, BattleGiveUpRes o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<BattleGiveUpRes.ResultType>(d, "Result");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(BattleGiveUpRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleGiveUpRes o)
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
