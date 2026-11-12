using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ContentsNoticePopupInfo
{
	public static ContentsNoticePopupInfo Parse(string s)
	{
		return Parse(s, new ContentsNoticePopupInfo());
	}

	public static ContentsNoticePopupInfo Parse(string s, ContentsNoticePopupInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ContentsNoticePopupInfo Parse(IDictionary d)
	{
		return Parse(d, new ContentsNoticePopupInfo());
	}

	public static ContentsNoticePopupInfo Parse(IDictionary d, ContentsNoticePopupInfo o)
	{
		o.ContentsType = BaseJsonMarshaler.ParseEnum<ContentsNoticeType>(d, "ContentsType");
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.SessionIndex = BaseJsonMarshaler.ParseValueInt(d, "SessionIndex");
		return o;
	}

	public static string EncodeJsonString(ContentsNoticePopupInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ContentsNoticePopupInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ContentsType", o.ContentsType.ToString());
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("SessionIndex", o.SessionIndex.ToString());
		return dictionary;
	}
}
