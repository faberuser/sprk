using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.BuyHeroTranscendSkillPage;

public static class JM_NShared_BuyHeroTranscendSkillPage_Request
{
	public static NShared.BuyHeroTranscendSkillPage.Request Parse(string s)
	{
		return Parse(s, new NShared.BuyHeroTranscendSkillPage.Request());
	}

	public static NShared.BuyHeroTranscendSkillPage.Request Parse(string s, NShared.BuyHeroTranscendSkillPage.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.BuyHeroTranscendSkillPage.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.BuyHeroTranscendSkillPage.Request());
	}

	public static NShared.BuyHeroTranscendSkillPage.Request Parse(IDictionary d, NShared.BuyHeroTranscendSkillPage.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.PageIndex = BaseJsonMarshaler.ParseValueInt(d, "PageIndex");
		o.BuyGem = BaseJsonMarshaler.ParseValueInt(d, "BuyGem");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.BuyHeroTranscendSkillPage.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.BuyHeroTranscendSkillPage.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("PageIndex", o.PageIndex.ToString());
		dictionary.Add("BuyGem", o.BuyGem.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
