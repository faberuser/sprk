using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildSuppressChapterData
{
	public static GuildSuppressChapterData Parse(string s)
	{
		return Parse(s, new GuildSuppressChapterData());
	}

	public static GuildSuppressChapterData Parse(string s, GuildSuppressChapterData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildSuppressChapterData Parse(IDictionary d)
	{
		return Parse(d, new GuildSuppressChapterData());
	}

	public static GuildSuppressChapterData Parse(IDictionary d, GuildSuppressChapterData o)
	{
		o.RaidIndex = BaseJsonMarshaler.ParseValueInt(d, "RaidIndex");
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.Description = BaseJsonMarshaler.ParseValueString(d, "Description");
		o.BossInfoImage = BaseJsonMarshaler.ParseValueString(d, "BossInfoImage");
		return o;
	}

	public static string EncodeJsonString(GuildSuppressChapterData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildSuppressChapterData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RaidIndex", o.RaidIndex.ToString());
		if (o.Name != null)
		{
			dictionary.Add("Name", o.Name);
		}
		if (o.Description != null)
		{
			dictionary.Add("Description", o.Description);
		}
		if (o.BossInfoImage != null)
		{
			dictionary.Add("BossInfoImage", o.BossInfoImage);
		}
		return dictionary;
	}
}
