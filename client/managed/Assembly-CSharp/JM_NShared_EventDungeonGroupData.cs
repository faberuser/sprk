using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EventDungeonGroupData
{
	public static EventDungeonGroupData Parse(string s)
	{
		return Parse(s, new EventDungeonGroupData());
	}

	public static EventDungeonGroupData Parse(string s, EventDungeonGroupData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EventDungeonGroupData Parse(IDictionary d)
	{
		return Parse(d, new EventDungeonGroupData());
	}

	public static EventDungeonGroupData Parse(IDictionary d, EventDungeonGroupData o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.GroupIndex = BaseJsonMarshaler.ParseValueInt(d, "GroupIndex");
		o.GroupType = BaseJsonMarshaler.ParseEnum<EDungeonGroupType>(d, "GroupType");
		o.DungeonName = BaseJsonMarshaler.ParseValueString(d, "DungeonName");
		o.DungeonDesc = BaseJsonMarshaler.ParseValueString(d, "DungeonDesc");
		o.PortraitPath = BaseJsonMarshaler.ParseValueString(d, "PortraitPath");
		o.DungeonIndices = BaseJsonMarshaler.ParseArrayCast(d, "DungeonIndices", BaseJsonMarshaler.CastToInt);
		o.BackGroundPath = BaseJsonMarshaler.ParseValueString(d, "BackGroundPath");
		return o;
	}

	public static string EncodeJsonString(EventDungeonGroupData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EventDungeonGroupData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("GroupIndex", o.GroupIndex.ToString());
		dictionary.Add("GroupType", o.GroupType.ToString());
		if (o.DungeonName != null)
		{
			dictionary.Add("DungeonName", o.DungeonName);
		}
		if (o.DungeonDesc != null)
		{
			dictionary.Add("DungeonDesc", o.DungeonDesc);
		}
		if (o.PortraitPath != null)
		{
			dictionary.Add("PortraitPath", o.PortraitPath);
		}
		dictionary.Add("DungeonIndices", BaseJsonMarshaler.EncodeArray(o.DungeonIndices));
		if (o.BackGroundPath != null)
		{
			dictionary.Add("BackGroundPath", o.BackGroundPath);
		}
		return dictionary;
	}
}
