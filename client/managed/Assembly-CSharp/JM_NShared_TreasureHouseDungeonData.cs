using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_TreasureHouseDungeonData
{
	public static TreasureHouseDungeonData Parse(string s)
	{
		return Parse(s, new TreasureHouseDungeonData());
	}

	public static TreasureHouseDungeonData Parse(string s, TreasureHouseDungeonData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static TreasureHouseDungeonData Parse(IDictionary d)
	{
		return Parse(d, new TreasureHouseDungeonData());
	}

	public static TreasureHouseDungeonData Parse(IDictionary d, TreasureHouseDungeonData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.InfoIndex = BaseJsonMarshaler.ParseValueInt(d, "InfoIndex");
		o.ShowIndex = BaseJsonMarshaler.ParseValueInt(d, "ShowIndex");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.CreatureIndex = BaseJsonMarshaler.ParseValueInt(d, "CreatureIndex");
		o.IconPortraitPath = BaseJsonMarshaler.ParseValueString(d, "IconPortraitPath");
		o.SelectIconPortraitPath = BaseJsonMarshaler.ParseValueString(d, "SelectIconPortraitPath");
		o.RewardIndex = BaseJsonMarshaler.ParseValueInt(d, "RewardIndex");
		o.Ratio = BaseJsonMarshaler.ParseValueInt(d, "Ratio");
		o.SequenceIndex = BaseJsonMarshaler.ParseValueInt(d, "SequenceIndex");
		o.CreaturePosition = BaseJsonMarshaler.ParseArrayCast(d, "CreaturePosition", BaseJsonMarshaler.CastToInt);
		o.CreatureRotation = BaseJsonMarshaler.ParseArrayCast(d, "CreatureRotation", BaseJsonMarshaler.CastToInt);
		o.CreatureScale = BaseJsonMarshaler.ParseValueInt(d, "CreatureScale");
		return o;
	}

	public static string EncodeJsonString(TreasureHouseDungeonData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(TreasureHouseDungeonData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("InfoIndex", o.InfoIndex.ToString());
		dictionary.Add("ShowIndex", o.ShowIndex.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("CreatureIndex", o.CreatureIndex.ToString());
		if (o.IconPortraitPath != null)
		{
			dictionary.Add("IconPortraitPath", o.IconPortraitPath);
		}
		if (o.SelectIconPortraitPath != null)
		{
			dictionary.Add("SelectIconPortraitPath", o.SelectIconPortraitPath);
		}
		dictionary.Add("RewardIndex", o.RewardIndex.ToString());
		dictionary.Add("Ratio", o.Ratio.ToString());
		dictionary.Add("SequenceIndex", o.SequenceIndex.ToString());
		dictionary.Add("CreaturePosition", BaseJsonMarshaler.EncodeArray(o.CreaturePosition));
		dictionary.Add("CreatureRotation", BaseJsonMarshaler.EncodeArray(o.CreatureRotation));
		dictionary.Add("CreatureScale", o.CreatureScale.ToString());
		return dictionary;
	}
}
