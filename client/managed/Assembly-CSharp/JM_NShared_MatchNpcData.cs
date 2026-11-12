using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_MatchNpcData
{
	public static MatchNpcData Parse(string s)
	{
		return Parse(s, new MatchNpcData());
	}

	public static MatchNpcData Parse(string s, MatchNpcData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MatchNpcData Parse(IDictionary d)
	{
		return Parse(d, new MatchNpcData());
	}

	public static MatchNpcData Parse(IDictionary d, MatchNpcData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.HeroIndex1 = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex1");
		o.HeroInfo1 = BaseJsonMarshaler.ParseValueString(d, "HeroInfo1");
		o.HeroIndex2 = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex2");
		o.HeroInfo2 = BaseJsonMarshaler.ParseValueString(d, "HeroInfo2");
		o.HeroIndex3 = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex3");
		o.HeroInfo3 = BaseJsonMarshaler.ParseValueString(d, "HeroInfo3");
		o.HeroIndex4 = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex4");
		o.HeroInfo4 = BaseJsonMarshaler.ParseValueString(d, "HeroInfo4");
		o.TierIndex = BaseJsonMarshaler.ParseValueInt(d, "TierIndex");
		o.MinMatchScore = BaseJsonMarshaler.ParseValueInt(d, "MinMatchScore");
		o.MaxMatchScore = BaseJsonMarshaler.ParseValueInt(d, "MaxMatchScore");
		return o;
	}

	public static string EncodeJsonString(MatchNpcData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MatchNpcData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		if (o.Name != null)
		{
			dictionary.Add("Name", o.Name);
		}
		dictionary.Add("HeroIndex1", o.HeroIndex1.ToString());
		if (o.HeroInfo1 != null)
		{
			dictionary.Add("HeroInfo1", o.HeroInfo1);
		}
		dictionary.Add("HeroIndex2", o.HeroIndex2.ToString());
		if (o.HeroInfo2 != null)
		{
			dictionary.Add("HeroInfo2", o.HeroInfo2);
		}
		dictionary.Add("HeroIndex3", o.HeroIndex3.ToString());
		if (o.HeroInfo3 != null)
		{
			dictionary.Add("HeroInfo3", o.HeroInfo3);
		}
		dictionary.Add("HeroIndex4", o.HeroIndex4.ToString());
		if (o.HeroInfo4 != null)
		{
			dictionary.Add("HeroInfo4", o.HeroInfo4);
		}
		dictionary.Add("TierIndex", o.TierIndex.ToString());
		dictionary.Add("MinMatchScore", o.MinMatchScore.ToString());
		dictionary.Add("MaxMatchScore", o.MaxMatchScore.ToString());
		return dictionary;
	}
}
