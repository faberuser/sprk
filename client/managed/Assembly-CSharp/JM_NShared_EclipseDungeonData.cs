using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EclipseDungeonData
{
	public static EclipseDungeonData Parse(string s)
	{
		return Parse(s, new EclipseDungeonData());
	}

	public static EclipseDungeonData Parse(string s, EclipseDungeonData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EclipseDungeonData Parse(IDictionary d)
	{
		return Parse(d, new EclipseDungeonData());
	}

	public static EclipseDungeonData Parse(IDictionary d, EclipseDungeonData o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		return o;
	}

	public static string EncodeJsonString(EclipseDungeonData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EclipseDungeonData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		return dictionary;
	}
}
