using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildRaidChapterData
{
	public static GuildRaidChapterData Parse(string s)
	{
		return Parse(s, new GuildRaidChapterData());
	}

	public static GuildRaidChapterData Parse(string s, GuildRaidChapterData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildRaidChapterData Parse(IDictionary d)
	{
		return Parse(d, new GuildRaidChapterData());
	}

	public static GuildRaidChapterData Parse(IDictionary d, GuildRaidChapterData o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.Description = BaseJsonMarshaler.ParseValueString(d, "Description");
		o.BonusGroup = BaseJsonMarshaler.ParseValueInt(d, "BonusGroup");
		o.IsBonusDungeon = BaseJsonMarshaler.ParseValueBool(d, "IsBonusDungeon");
		o.IsDummyDungeon = BaseJsonMarshaler.ParseValueBool(d, "IsDummyDungeon");
		return o;
	}

	public static string EncodeJsonString(GuildRaidChapterData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildRaidChapterData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		if (o.Name != null)
		{
			dictionary.Add("Name", o.Name);
		}
		if (o.Description != null)
		{
			dictionary.Add("Description", o.Description);
		}
		dictionary.Add("BonusGroup", o.BonusGroup.ToString());
		dictionary.Add("IsBonusDungeon", o.IsBonusDungeon.ToString());
		dictionary.Add("IsDummyDungeon", o.IsDummyDungeon.ToString());
		return dictionary;
	}
}
