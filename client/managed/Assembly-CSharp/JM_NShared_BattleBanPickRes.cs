using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleBanPickRes
{
	public static BattleBanPickRes Parse(string s)
	{
		return Parse(s, new BattleBanPickRes());
	}

	public static BattleBanPickRes Parse(string s, BattleBanPickRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleBanPickRes Parse(IDictionary d)
	{
		return Parse(d, new BattleBanPickRes());
	}

	public static BattleBanPickRes Parse(IDictionary d, BattleBanPickRes o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<BattleBanPickRes.ResultType>(d, "Result");
		o.RemainTimeSec = BaseJsonMarshaler.ParseValueInt(d, "RemainTimeSec");
		o.BanPickState = BaseJsonMarshaler.ParseEnum<MatchBanPickState>(d, "BanPickState");
		o.BanPickAccountInfo = BaseJsonMarshaler.ParseArray(d, "BanPickAccountInfo", JM_NShared_BanPickAccountInfo.Parse);
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(BattleBanPickRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleBanPickRes o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Result", o.Result.ToString());
		dictionary.Add("RemainTimeSec", o.RemainTimeSec.ToString());
		dictionary.Add("BanPickState", o.BanPickState.ToString());
		dictionary.Add("BanPickAccountInfo", BaseJsonMarshaler.EncodeArray(o.BanPickAccountInfo, JM_NShared_BanPickAccountInfo.EncodeJsonObject));
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}
