using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ContentsNoticePopupData
{
	public static ContentsNoticePopupData Parse(string s)
	{
		return Parse(s, new ContentsNoticePopupData());
	}

	public static ContentsNoticePopupData Parse(string s, ContentsNoticePopupData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ContentsNoticePopupData Parse(IDictionary d)
	{
		return Parse(d, new ContentsNoticePopupData());
	}

	public static ContentsNoticePopupData Parse(IDictionary d, ContentsNoticePopupData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.ContentsType = BaseJsonMarshaler.ParseEnum<ContentsNoticeType>(d, "ContentsType");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.PopupType = BaseJsonMarshaler.ParseEnum<ContentsNoticePopupType>(d, "PopupType");
		o.ContentsImagePath = BaseJsonMarshaler.ParseValueString(d, "ContentsImagePath");
		o.TitleDescKey = BaseJsonMarshaler.ParseValueString(d, "TitleDescKey");
		o.UseRank = BaseJsonMarshaler.ParseValueBool(d, "UseRank");
		o.UseHonoraryChamber = BaseJsonMarshaler.ParseValueBool(d, "UseHonoraryChamber");
		o.OpenCondition = BaseJsonMarshaler.ParseArrayCast(d, "OpenCondition", BaseJsonMarshaler.CastToString);
		o.TutorialIndex = BaseJsonMarshaler.ParseValueInt(d, "TutorialIndex");
		o.OpenAction = BaseJsonMarshaler.ParseArrayCast(d, "OpenAction", BaseJsonMarshaler.CastToString);
		o.ContentsStatusType = BaseJsonMarshaler.ParseEnum<ContentsStatusType>(d, "ContentsStatusType");
		o.ContentsStatusSubType = BaseJsonMarshaler.ParseEnum<ContentsStatusSubType>(d, "ContentsStatusSubType");
		return o;
	}

	public static string EncodeJsonString(ContentsNoticePopupData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ContentsNoticePopupData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("ContentsType", o.ContentsType.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("PopupType", o.PopupType.ToString());
		if (o.ContentsImagePath != null)
		{
			dictionary.Add("ContentsImagePath", o.ContentsImagePath);
		}
		if (o.TitleDescKey != null)
		{
			dictionary.Add("TitleDescKey", o.TitleDescKey);
		}
		dictionary.Add("UseRank", o.UseRank.ToString());
		dictionary.Add("UseHonoraryChamber", o.UseHonoraryChamber.ToString());
		dictionary.Add("OpenCondition", BaseJsonMarshaler.EncodeArray(o.OpenCondition));
		dictionary.Add("TutorialIndex", o.TutorialIndex.ToString());
		dictionary.Add("OpenAction", BaseJsonMarshaler.EncodeArray(o.OpenAction));
		dictionary.Add("ContentsStatusType", o.ContentsStatusType.ToString());
		dictionary.Add("ContentsStatusSubType", o.ContentsStatusSubType.ToString());
		return dictionary;
	}
}
