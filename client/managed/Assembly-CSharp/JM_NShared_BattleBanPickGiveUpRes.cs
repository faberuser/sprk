using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleBanPickGiveUpRes
{
	public static BattleBanPickGiveUpRes Parse(string s)
	{
		return Parse(s, new BattleBanPickGiveUpRes());
	}

	public static BattleBanPickGiveUpRes Parse(string s, BattleBanPickGiveUpRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleBanPickGiveUpRes Parse(IDictionary d)
	{
		return Parse(d, new BattleBanPickGiveUpRes());
	}

	public static BattleBanPickGiveUpRes Parse(IDictionary d, BattleBanPickGiveUpRes o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<BattleBanPickGiveUpRes.ResultType>(d, "Result");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(BattleBanPickGiveUpRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleBanPickGiveUpRes o)
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
