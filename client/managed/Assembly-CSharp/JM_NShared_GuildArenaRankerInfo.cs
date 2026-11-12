using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildArenaRankerInfo
{
	public static GuildArenaRankerInfo Parse(string s)
	{
		return Parse(s, new GuildArenaRankerInfo());
	}

	public static GuildArenaRankerInfo Parse(string s, GuildArenaRankerInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildArenaRankerInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildArenaRankerInfo());
	}

	public static GuildArenaRankerInfo Parse(IDictionary d, GuildArenaRankerInfo o)
	{
		o.TierIndex = BaseJsonMarshaler.ParseValueInt(d, "TierIndex");
		o.Rank = BaseJsonMarshaler.ParseValueInt(d, "Rank");
		o.SeasonScore = BaseJsonMarshaler.ParseValueInt(d, "SeasonScore");
		o.SeasonWin = BaseJsonMarshaler.ParseValueInt(d, "SeasonWin");
		o.SeasonLose = BaseJsonMarshaler.ParseValueInt(d, "SeasonLose");
		o.SeasonDraw = BaseJsonMarshaler.ParseValueInt(d, "SeasonDraw");
		o.SessionScore = BaseJsonMarshaler.ParseValueInt(d, "SessionScore");
		o.GuildMembers = BaseJsonMarshaler.ParseArray(d, "GuildMembers", JM_NShared_GuildMemberServerInfo.Parse);
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		o.GuildId = BaseJsonMarshaler.ParseValueLong(d, "GuildId");
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.Logo = BaseJsonMarshaler.ParseValueByte(d, "Logo");
		o.Back = BaseJsonMarshaler.ParseValueByte(d, "Back");
		o.Flag = BaseJsonMarshaler.ParseValueByte(d, "Flag");
		o.CountryCode = BaseJsonMarshaler.ParseValueString(d, "CountryCode");
		o.MemberCount = BaseJsonMarshaler.ParseValueByte(d, "MemberCount");
		return o;
	}

	public static string EncodeJsonString(GuildArenaRankerInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildArenaRankerInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("TierIndex", o.TierIndex.ToString());
		dictionary.Add("Rank", o.Rank.ToString());
		dictionary.Add("SeasonScore", o.SeasonScore.ToString());
		dictionary.Add("SeasonWin", o.SeasonWin.ToString());
		dictionary.Add("SeasonLose", o.SeasonLose.ToString());
		dictionary.Add("SeasonDraw", o.SeasonDraw.ToString());
		dictionary.Add("SessionScore", o.SessionScore.ToString());
		dictionary.Add("GuildMembers", BaseJsonMarshaler.EncodeArray(o.GuildMembers, JM_NShared_GuildMemberServerInfo.EncodeJsonObject));
		if (o.ServerGroup != null)
		{
			dictionary.Add("ServerGroup", o.ServerGroup);
		}
		dictionary.Add("GuildId", o.GuildId.ToString());
		if (o.Name != null)
		{
			dictionary.Add("Name", o.Name);
		}
		dictionary.Add("Logo", o.Logo.ToString());
		dictionary.Add("Back", o.Back.ToString());
		dictionary.Add("Flag", o.Flag.ToString());
		if (o.CountryCode != null)
		{
			dictionary.Add("CountryCode", o.CountryCode);
		}
		dictionary.Add("MemberCount", o.MemberCount.ToString());
		return dictionary;
	}
}
