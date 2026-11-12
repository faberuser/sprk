using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetGuildRankingBoardViewSeasonInfo;

public static class JM_NShared_GetGuildRankingBoardViewSeasonInfo_Request
{
	public static NShared.GetGuildRankingBoardViewSeasonInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetGuildRankingBoardViewSeasonInfo.Request());
	}

	public static NShared.GetGuildRankingBoardViewSeasonInfo.Request Parse(string s, NShared.GetGuildRankingBoardViewSeasonInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetGuildRankingBoardViewSeasonInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetGuildRankingBoardViewSeasonInfo.Request());
	}

	public static NShared.GetGuildRankingBoardViewSeasonInfo.Request Parse(IDictionary d, NShared.GetGuildRankingBoardViewSeasonInfo.Request o)
	{
		o.BoardType = BaseJsonMarshaler.ParseEnum<GuildRankingBoardType>(d, "BoardType");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetGuildRankingBoardViewSeasonInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetGuildRankingBoardViewSeasonInfo.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("BoardType", o.BoardType.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
