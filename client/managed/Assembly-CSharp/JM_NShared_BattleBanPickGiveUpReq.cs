using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleBanPickGiveUpReq
{
	public static BattleBanPickGiveUpReq Parse(string s)
	{
		return Parse(s, new BattleBanPickGiveUpReq());
	}

	public static BattleBanPickGiveUpReq Parse(string s, BattleBanPickGiveUpReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleBanPickGiveUpReq Parse(IDictionary d)
	{
		return Parse(d, new BattleBanPickGiveUpReq());
	}

	public static BattleBanPickGiveUpReq Parse(IDictionary d, BattleBanPickGiveUpReq o)
	{
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(BattleBanPickGiveUpReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleBanPickGiveUpReq o)
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
