using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlayerHeroFriendlyInfo
{
	public static PlayerHeroFriendlyInfo Parse(string s)
	{
		return Parse(s, new PlayerHeroFriendlyInfo());
	}

	public static PlayerHeroFriendlyInfo Parse(string s, PlayerHeroFriendlyInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayerHeroFriendlyInfo Parse(IDictionary d)
	{
		return Parse(d, new PlayerHeroFriendlyInfo());
	}

	public static PlayerHeroFriendlyInfo Parse(IDictionary d, PlayerHeroFriendlyInfo o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.SelectedHeroIndex = BaseJsonMarshaler.ParseValueInt(d, "SelectedHeroIndex");
		o.FriendlyPoint = BaseJsonMarshaler.ParseValueInt(d, "FriendlyPoint");
		o.LastGreetingTime = BaseJsonMarshaler.ParseValueString(d, "LastGreetingTime");
		o.LastConversationTime = BaseJsonMarshaler.ParseValueString(d, "LastConversationTime");
		o.LastGiftTime = BaseJsonMarshaler.ParseValueString(d, "LastGiftTime");
		o.SelectedTime = BaseJsonMarshaler.ParseValueString(d, "SelectedTime");
		o.SelectedHeroIndice = BaseJsonMarshaler.ParseValueString(d, "SelectedHeroIndice");
		o.LastRouletteTime = BaseJsonMarshaler.ParseValueString(d, "LastRouletteTime");
		return o;
	}

	public static string EncodeJsonString(PlayerHeroFriendlyInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayerHeroFriendlyInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("SelectedHeroIndex", o.SelectedHeroIndex.ToString());
		dictionary.Add("FriendlyPoint", o.FriendlyPoint.ToString());
		if (o.LastGreetingTime != null)
		{
			dictionary.Add("LastGreetingTime", o.LastGreetingTime);
		}
		if (o.LastConversationTime != null)
		{
			dictionary.Add("LastConversationTime", o.LastConversationTime);
		}
		if (o.LastGiftTime != null)
		{
			dictionary.Add("LastGiftTime", o.LastGiftTime);
		}
		if (o.SelectedTime != null)
		{
			dictionary.Add("SelectedTime", o.SelectedTime);
		}
		if (o.SelectedHeroIndice != null)
		{
			dictionary.Add("SelectedHeroIndice", o.SelectedHeroIndice);
		}
		if (o.LastRouletteTime != null)
		{
			dictionary.Add("LastRouletteTime", o.LastRouletteTime);
		}
		return dictionary;
	}
}
