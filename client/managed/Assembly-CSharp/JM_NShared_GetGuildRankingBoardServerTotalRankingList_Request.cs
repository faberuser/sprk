using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetGuildRankingBoardServerTotalRankingList;

public static class JM_NShared_GetGuildRankingBoardServerTotalRankingList_Request
{
	public static NShared.GetGuildRankingBoardServerTotalRankingList.Request Parse(string s)
	{
		return Parse(s, new NShared.GetGuildRankingBoardServerTotalRankingList.Request());
	}

	public static NShared.GetGuildRankingBoardServerTotalRankingList.Request Parse(string s, NShared.GetGuildRankingBoardServerTotalRankingList.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetGuildRankingBoardServerTotalRankingList.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetGuildRankingBoardServerTotalRankingList.Request());
	}

	public static NShared.GetGuildRankingBoardServerTotalRankingList.Request Parse(IDictionary d, NShared.GetGuildRankingBoardServerTotalRankingList.Request o)
	{
		o.PageNo = BaseJsonMarshaler.ParseValueInt(d, "PageNo");
		o.ViewSeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "ViewSeasonIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetGuildRankingBoardServerTotalRankingList.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetGuildRankingBoardServerTotalRankingList.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("PageNo", o.PageNo.ToString());
		dictionary.Add("ViewSeasonIndex", o.ViewSeasonIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
