using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_TopClearDungeonInfo
{
	public static TopClearDungeonInfo Parse(string s)
	{
		return Parse(s, new TopClearDungeonInfo());
	}

	public static TopClearDungeonInfo Parse(string s, TopClearDungeonInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static TopClearDungeonInfo Parse(IDictionary d)
	{
		return Parse(d, new TopClearDungeonInfo());
	}

	public static TopClearDungeonInfo Parse(IDictionary d, TopClearDungeonInfo o)
	{
		o.SweepDungeonType = BaseJsonMarshaler.ParseEnum<SweepDungeonType>(d, "SweepDungeonType");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		return o;
	}

	public static string EncodeJsonString(TopClearDungeonInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(TopClearDungeonInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SweepDungeonType", o.SweepDungeonType.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		return dictionary;
	}
}
