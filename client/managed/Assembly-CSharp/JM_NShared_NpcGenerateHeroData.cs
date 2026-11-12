using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_NpcGenerateHeroData
{
	public static NpcGenerateHeroData Parse(string s)
	{
		return Parse(s, new NpcGenerateHeroData());
	}

	public static NpcGenerateHeroData Parse(string s, NpcGenerateHeroData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NpcGenerateHeroData Parse(IDictionary d)
	{
		return Parse(d, new NpcGenerateHeroData());
	}

	public static NpcGenerateHeroData Parse(IDictionary d, NpcGenerateHeroData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.Level = BaseJsonMarshaler.ParseValueInt(d, "Level");
		o.MinStar = BaseJsonMarshaler.ParseValueInt(d, "MinStar");
		o.MaxStar = BaseJsonMarshaler.ParseValueInt(d, "MaxStar");
		o.EquipTier = BaseJsonMarshaler.ParseValueInt(d, "EquipTier");
		o.MinEquipGrade = BaseJsonMarshaler.ParseValueInt(d, "MinEquipGrade");
		o.MaxEquipGrade = BaseJsonMarshaler.ParseValueInt(d, "MaxEquipGrade");
		o.MinEquipLevel = BaseJsonMarshaler.ParseValueInt(d, "MinEquipLevel");
		o.MaxEquipLevel = BaseJsonMarshaler.ParseValueInt(d, "MaxEquipLevel");
		o.MinEquipStar = BaseJsonMarshaler.ParseValueInt(d, "MinEquipStar");
		o.MaxEquipStar = BaseJsonMarshaler.ParseValueInt(d, "MaxEquipStar");
		o.TierIndex = BaseJsonMarshaler.ParseValueInt(d, "TierIndex");
		o.MinMatchScore = BaseJsonMarshaler.ParseValueInt(d, "MinMatchScore");
		o.MaxMatchScore = BaseJsonMarshaler.ParseValueInt(d, "MaxMatchScore");
		return o;
	}

	public static string EncodeJsonString(NpcGenerateHeroData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NpcGenerateHeroData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("Level", o.Level.ToString());
		dictionary.Add("MinStar", o.MinStar.ToString());
		dictionary.Add("MaxStar", o.MaxStar.ToString());
		dictionary.Add("EquipTier", o.EquipTier.ToString());
		dictionary.Add("MinEquipGrade", o.MinEquipGrade.ToString());
		dictionary.Add("MaxEquipGrade", o.MaxEquipGrade.ToString());
		dictionary.Add("MinEquipLevel", o.MinEquipLevel.ToString());
		dictionary.Add("MaxEquipLevel", o.MaxEquipLevel.ToString());
		dictionary.Add("MinEquipStar", o.MinEquipStar.ToString());
		dictionary.Add("MaxEquipStar", o.MaxEquipStar.ToString());
		dictionary.Add("TierIndex", o.TierIndex.ToString());
		dictionary.Add("MinMatchScore", o.MinMatchScore.ToString());
		dictionary.Add("MaxMatchScore", o.MaxMatchScore.ToString());
		return dictionary;
	}
}
