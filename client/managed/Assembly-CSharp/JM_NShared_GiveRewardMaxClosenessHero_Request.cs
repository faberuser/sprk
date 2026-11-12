using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GiveRewardMaxClosenessHero;

public static class JM_NShared_GiveRewardMaxClosenessHero_Request
{
	public static NShared.GiveRewardMaxClosenessHero.Request Parse(string s)
	{
		return Parse(s, new NShared.GiveRewardMaxClosenessHero.Request());
	}

	public static NShared.GiveRewardMaxClosenessHero.Request Parse(string s, NShared.GiveRewardMaxClosenessHero.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GiveRewardMaxClosenessHero.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GiveRewardMaxClosenessHero.Request());
	}

	public static NShared.GiveRewardMaxClosenessHero.Request Parse(IDictionary d, NShared.GiveRewardMaxClosenessHero.Request o)
	{
		o.HeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "HeroIndices", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GiveRewardMaxClosenessHero.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GiveRewardMaxClosenessHero.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndices", BaseJsonMarshaler.EncodeArray(o.HeroIndices));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
