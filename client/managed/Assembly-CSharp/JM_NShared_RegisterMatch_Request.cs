using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.RegisterMatch;

public static class JM_NShared_RegisterMatch_Request
{
	public static NShared.RegisterMatch.Request Parse(string s)
	{
		return Parse(s, new NShared.RegisterMatch.Request());
	}

	public static NShared.RegisterMatch.Request Parse(string s, NShared.RegisterMatch.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.RegisterMatch.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.RegisterMatch.Request());
	}

	public static NShared.RegisterMatch.Request Parse(IDictionary d, NShared.RegisterMatch.Request o)
	{
		o.ArenaType = BaseJsonMarshaler.ParseEnum<MatchArenaType>(d, "ArenaType");
		o.DuelType = BaseJsonMarshaler.ParseEnum<MatchDuelType>(d, "DuelType");
		o.HeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "HeroIndices", BaseJsonMarshaler.CastToInt);
		o.LeaderHeroIndex = BaseJsonMarshaler.ParseValueInt(d, "LeaderHeroIndex");
		o.Rank = BaseJsonMarshaler.ParseValueInt(d, "Rank");
		o.SingleTest = BaseJsonMarshaler.ParseValueBool(d, "SingleTest");
		o.GroupId = BaseJsonMarshaler.ParseValueLong(d, "GroupId");
		o.IsRematch = BaseJsonMarshaler.ParseValueBool(d, "IsRematch");
		o.OpponentIsNpc = BaseJsonMarshaler.ParseValueBool(d, "OpponentIsNpc");
		o.OpponentServerGroup = BaseJsonMarshaler.ParseValueString(d, "OpponentServerGroup");
		o.OpponentAccountId = BaseJsonMarshaler.ParseValueLong(d, "OpponentAccountId");
		o.OpponentTierIndex = BaseJsonMarshaler.ParseValueInt(d, "OpponentTierIndex");
		o.OpponentHeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "OpponentHeroIndices", BaseJsonMarshaler.CastToInt);
		o.OpponentLeaderHeroIndex = BaseJsonMarshaler.ParseValueInt(d, "OpponentLeaderHeroIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.RegisterMatch.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.RegisterMatch.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ArenaType", o.ArenaType.ToString());
		dictionary.Add("DuelType", o.DuelType.ToString());
		dictionary.Add("HeroIndices", BaseJsonMarshaler.EncodeArray(o.HeroIndices));
		dictionary.Add("LeaderHeroIndex", o.LeaderHeroIndex.ToString());
		dictionary.Add("Rank", o.Rank.ToString());
		dictionary.Add("SingleTest", o.SingleTest.ToString());
		dictionary.Add("GroupId", o.GroupId.ToString());
		dictionary.Add("IsRematch", o.IsRematch.ToString());
		dictionary.Add("OpponentIsNpc", o.OpponentIsNpc.ToString());
		if (o.OpponentServerGroup != null)
		{
			dictionary.Add("OpponentServerGroup", o.OpponentServerGroup);
		}
		dictionary.Add("OpponentAccountId", o.OpponentAccountId.ToString());
		dictionary.Add("OpponentTierIndex", o.OpponentTierIndex.ToString());
		dictionary.Add("OpponentHeroIndices", BaseJsonMarshaler.EncodeArray(o.OpponentHeroIndices));
		dictionary.Add("OpponentLeaderHeroIndex", o.OpponentLeaderHeroIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
