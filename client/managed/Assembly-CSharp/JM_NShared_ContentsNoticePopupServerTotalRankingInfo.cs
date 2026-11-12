using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ContentsNoticePopupServerTotalRankingInfo
{
	public static ContentsNoticePopupServerTotalRankingInfo Parse(string s)
	{
		return Parse(s, new ContentsNoticePopupServerTotalRankingInfo());
	}

	public static ContentsNoticePopupServerTotalRankingInfo Parse(string s, ContentsNoticePopupServerTotalRankingInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ContentsNoticePopupServerTotalRankingInfo Parse(IDictionary d)
	{
		return Parse(d, new ContentsNoticePopupServerTotalRankingInfo());
	}

	public static ContentsNoticePopupServerTotalRankingInfo Parse(IDictionary d, ContentsNoticePopupServerTotalRankingInfo o)
	{
		o.Rank = BaseJsonMarshaler.ParseValueInt(d, "Rank");
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		o.Score = BaseJsonMarshaler.ParseValueLong(d, "Score");
		return o;
	}

	public static string EncodeJsonString(ContentsNoticePopupServerTotalRankingInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ContentsNoticePopupServerTotalRankingInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Rank", o.Rank.ToString());
		if (o.ServerGroup != null)
		{
			dictionary.Add("ServerGroup", o.ServerGroup);
		}
		dictionary.Add("Score", o.Score.ToString());
		return dictionary;
	}
}
