using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetGuildRankingBoardRankerList;

public static class JM_NShared_GetGuildRankingBoardRankerList_Request
{
	public static NShared.GetGuildRankingBoardRankerList.Request Parse(string s)
	{
		return Parse(s, new NShared.GetGuildRankingBoardRankerList.Request());
	}

	public static NShared.GetGuildRankingBoardRankerList.Request Parse(string s, NShared.GetGuildRankingBoardRankerList.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetGuildRankingBoardRankerList.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetGuildRankingBoardRankerList.Request());
	}

	public static NShared.GetGuildRankingBoardRankerList.Request Parse(IDictionary d, NShared.GetGuildRankingBoardRankerList.Request o)
	{
		o.BoardType = BaseJsonMarshaler.ParseEnum<GuildRankingBoardType>(d, "BoardType");
		o.RankingType = BaseJsonMarshaler.ParseEnum<GuildRankingType>(d, "RankingType");
		o.PageNo = BaseJsonMarshaler.ParseValueInt(d, "PageNo");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetGuildRankingBoardRankerList.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetGuildRankingBoardRankerList.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("BoardType", o.BoardType.ToString());
		dictionary.Add("RankingType", o.RankingType.ToString());
		dictionary.Add("PageNo", o.PageNo.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
