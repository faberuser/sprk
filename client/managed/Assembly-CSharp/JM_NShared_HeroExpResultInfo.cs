using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_HeroExpResultInfo
{
	public static HeroExpResultInfo Parse(string s)
	{
		return Parse(s, new HeroExpResultInfo());
	}

	public static HeroExpResultInfo Parse(string s, HeroExpResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static HeroExpResultInfo Parse(IDictionary d)
	{
		return Parse(d, new HeroExpResultInfo());
	}

	public static HeroExpResultInfo Parse(IDictionary d, HeroExpResultInfo o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.AddValue = BaseJsonMarshaler.ParseValueInt(d, "AddValue");
		o.AddBoosterValue = BaseJsonMarshaler.ParseValueInt(d, "AddBoosterValue");
		o.AddTeamLevelValue = BaseJsonMarshaler.ParseValueInt(d, "AddTeamLevelValue");
		o.NewValue = BaseJsonMarshaler.ParseValueInt(d, "NewValue");
		o.OldLevel = BaseJsonMarshaler.ParseValueShort(d, "OldLevel");
		o.NewLevel = BaseJsonMarshaler.ParseValueShort(d, "NewLevel");
		return o;
	}

	public static string EncodeJsonString(HeroExpResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(HeroExpResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("AddValue", o.AddValue.ToString());
		dictionary.Add("AddBoosterValue", o.AddBoosterValue.ToString());
		dictionary.Add("AddTeamLevelValue", o.AddTeamLevelValue.ToString());
		dictionary.Add("NewValue", o.NewValue.ToString());
		dictionary.Add("OldLevel", o.OldLevel.ToString());
		dictionary.Add("NewLevel", o.NewLevel.ToString());
		return dictionary;
	}
}
