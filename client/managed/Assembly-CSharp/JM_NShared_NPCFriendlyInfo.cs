using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_NPCFriendlyInfo
{
	public static NPCFriendlyInfo Parse(string s)
	{
		return Parse(s, new NPCFriendlyInfo());
	}

	public static NPCFriendlyInfo Parse(string s, NPCFriendlyInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NPCFriendlyInfo Parse(IDictionary d)
	{
		return Parse(d, new NPCFriendlyInfo());
	}

	public static NPCFriendlyInfo Parse(IDictionary d, NPCFriendlyInfo o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.Step = BaseJsonMarshaler.ParseValueInt(d, "Step");
		o.FriendlyPoint = BaseJsonMarshaler.ParseValueInt(d, "FriendlyPoint");
		o.EndRewardCount = BaseJsonMarshaler.ParseValueInt(d, "EndRewardCount");
		return o;
	}

	public static string EncodeJsonString(NPCFriendlyInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NPCFriendlyInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("Step", o.Step.ToString());
		dictionary.Add("FriendlyPoint", o.FriendlyPoint.ToString());
		dictionary.Add("EndRewardCount", o.EndRewardCount.ToString());
		return dictionary;
	}
}
