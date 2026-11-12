using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_PartyDeckHeroCancel
{
	public static PartyDeckHeroCancel Parse(string s)
	{
		return Parse(s, new PartyDeckHeroCancel());
	}

	public static PartyDeckHeroCancel Parse(string s, PartyDeckHeroCancel o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PartyDeckHeroCancel Parse(IDictionary d)
	{
		return Parse(d, new PartyDeckHeroCancel());
	}

	public static PartyDeckHeroCancel Parse(IDictionary d, PartyDeckHeroCancel o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<PartyDeckHeroCancel.ResultType>(d, "Result");
		o.MemberId = BaseJsonMarshaler.ParseValueLong(d, "MemberId");
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.DeckIndex = BaseJsonMarshaler.ParseValueInt(d, "DeckIndex");
		return o;
	}

	public static string EncodeJsonString(PartyDeckHeroCancel o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PartyDeckHeroCancel o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Result", o.Result.ToString());
		dictionary.Add("MemberId", o.MemberId.ToString());
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("DeckIndex", o.DeckIndex.ToString());
		return dictionary;
	}
}
