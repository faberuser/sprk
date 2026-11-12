using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleBanPickReq
{
	public static BattleBanPickReq Parse(string s)
	{
		return Parse(s, new BattleBanPickReq());
	}

	public static BattleBanPickReq Parse(string s, BattleBanPickReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleBanPickReq Parse(IDictionary d)
	{
		return Parse(d, new BattleBanPickReq());
	}

	public static BattleBanPickReq Parse(IDictionary d, BattleBanPickReq o)
	{
		o.BanPickState = BaseJsonMarshaler.ParseEnum<MatchBanPickState>(d, "BanPickState");
		o.HeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "HeroIndices", BaseJsonMarshaler.CastToInt);
		o.IsPenalty = BaseJsonMarshaler.ParseValueBool(d, "IsPenalty");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(BattleBanPickReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleBanPickReq o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("BanPickState", o.BanPickState.ToString());
		dictionary.Add("HeroIndices", BaseJsonMarshaler.EncodeArray(o.HeroIndices));
		dictionary.Add("IsPenalty", o.IsPenalty.ToString());
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}
