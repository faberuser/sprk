using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildSuppressStatusBoardInfo
{
	public static GuildSuppressStatusBoardInfo Parse(string s)
	{
		return Parse(s, new GuildSuppressStatusBoardInfo());
	}

	public static GuildSuppressStatusBoardInfo Parse(string s, GuildSuppressStatusBoardInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildSuppressStatusBoardInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildSuppressStatusBoardInfo());
	}

	public static GuildSuppressStatusBoardInfo Parse(IDictionary d, GuildSuppressStatusBoardInfo o)
	{
		o.GuildInfo = BaseJsonMarshaler.ParseObject(d, "GuildInfo", JM_NShared_GuildInfo.Parse);
		o.GuildSuppressPlayInfos = BaseJsonMarshaler.ParseArray(d, "GuildSuppressPlayInfos", JM_NShared_GuildSuppressPlayInfo.Parse);
		o.GuildSuppressMemberScores = BaseJsonMarshaler.ParseArray(d, "GuildSuppressMemberScores", JM_NShared_GuildSuppressMemberScoreInfo.Parse);
		o.GuildSuppressMemberPlayHistorys = BaseJsonMarshaler.ParseArray(d, "GuildSuppressMemberPlayHistorys", JM_NShared_GuildSuppressMemberPlayHistory.Parse);
		return o;
	}

	public static string EncodeJsonString(GuildSuppressStatusBoardInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildSuppressStatusBoardInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GuildInfo", JM_NShared_GuildInfo.EncodeJsonObject(o.GuildInfo));
		dictionary.Add("GuildSuppressPlayInfos", BaseJsonMarshaler.EncodeArray(o.GuildSuppressPlayInfos, JM_NShared_GuildSuppressPlayInfo.EncodeJsonObject));
		dictionary.Add("GuildSuppressMemberScores", BaseJsonMarshaler.EncodeArray(o.GuildSuppressMemberScores, JM_NShared_GuildSuppressMemberScoreInfo.EncodeJsonObject));
		dictionary.Add("GuildSuppressMemberPlayHistorys", BaseJsonMarshaler.EncodeArray(o.GuildSuppressMemberPlayHistorys, JM_NShared_GuildSuppressMemberPlayHistory.EncodeJsonObject));
		return dictionary;
	}
}
