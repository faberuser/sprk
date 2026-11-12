using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_GuildBuildingInvest
{
	public static GuildBuildingInvest Parse(string s)
	{
		return Parse(s, new GuildBuildingInvest());
	}

	public static GuildBuildingInvest Parse(string s, GuildBuildingInvest o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildBuildingInvest Parse(IDictionary d)
	{
		return Parse(d, new GuildBuildingInvest());
	}

	public static GuildBuildingInvest Parse(IDictionary d, GuildBuildingInvest o)
	{
		o.GuildBuildingIndex = BaseJsonMarshaler.ParseValueInt(d, "GuildBuildingIndex");
		o.InvestWood = BaseJsonMarshaler.ParseValueInt(d, "InvestWood");
		o.InvestStone = BaseJsonMarshaler.ParseValueInt(d, "InvestStone");
		o.InvestMetal = BaseJsonMarshaler.ParseValueInt(d, "InvestMetal");
		o.Wood = BaseJsonMarshaler.ParseValueInt(d, "Wood");
		o.Stone = BaseJsonMarshaler.ParseValueInt(d, "Stone");
		o.Metal = BaseJsonMarshaler.ParseValueInt(d, "Metal");
		return o;
	}

	public static string EncodeJsonString(GuildBuildingInvest o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildBuildingInvest o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GuildBuildingIndex", o.GuildBuildingIndex.ToString());
		dictionary.Add("InvestWood", o.InvestWood.ToString());
		dictionary.Add("InvestStone", o.InvestStone.ToString());
		dictionary.Add("InvestMetal", o.InvestMetal.ToString());
		dictionary.Add("Wood", o.Wood.ToString());
		dictionary.Add("Stone", o.Stone.ToString());
		dictionary.Add("Metal", o.Metal.ToString());
		return dictionary;
	}
}
