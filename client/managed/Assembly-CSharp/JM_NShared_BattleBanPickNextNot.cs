using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleBanPickNextNot
{
	public static BattleBanPickNextNot Parse(string s)
	{
		return Parse(s, new BattleBanPickNextNot());
	}

	public static BattleBanPickNextNot Parse(string s, BattleBanPickNextNot o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleBanPickNextNot Parse(IDictionary d)
	{
		return Parse(d, new BattleBanPickNextNot());
	}

	public static BattleBanPickNextNot Parse(IDictionary d, BattleBanPickNextNot o)
	{
		o.RemainTimeSec = BaseJsonMarshaler.ParseValueInt(d, "RemainTimeSec");
		o.BanPickState = BaseJsonMarshaler.ParseEnum<MatchBanPickState>(d, "BanPickState");
		o.HeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "HeroIndices", BaseJsonMarshaler.CastToInt);
		o.BanPickAccountInfo = BaseJsonMarshaler.ParseArray(d, "BanPickAccountInfo", JM_NShared_BanPickAccountInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(BattleBanPickNextNot o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleBanPickNextNot o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RemainTimeSec", o.RemainTimeSec.ToString());
		dictionary.Add("BanPickState", o.BanPickState.ToString());
		dictionary.Add("HeroIndices", BaseJsonMarshaler.EncodeArray(o.HeroIndices));
		dictionary.Add("BanPickAccountInfo", BaseJsonMarshaler.EncodeArray(o.BanPickAccountInfo, JM_NShared_BanPickAccountInfo.EncodeJsonObject));
		return dictionary;
	}
}
