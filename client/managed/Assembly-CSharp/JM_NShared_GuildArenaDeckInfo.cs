using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildArenaDeckInfo
{
	public static GuildArenaDeckInfo Parse(string s)
	{
		return Parse(s, new GuildArenaDeckInfo());
	}

	public static GuildArenaDeckInfo Parse(string s, GuildArenaDeckInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildArenaDeckInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildArenaDeckInfo());
	}

	public static GuildArenaDeckInfo Parse(IDictionary d, GuildArenaDeckInfo o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.DeckIndex = BaseJsonMarshaler.ParseValueInt(d, "DeckIndex");
		o.CachedHeroInfos = BaseJsonMarshaler.ParseArray(d, "CachedHeroInfos", JM_NShared_CachedHeroInfo.Parse);
		o.HeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "HeroIndices", BaseJsonMarshaler.CastToInt);
		o.SkillSlotIndices = BaseJsonMarshaler.ParseArrayCast(d, "SkillSlotIndices", BaseJsonMarshaler.CastToInt);
		o.IsBattle = BaseJsonMarshaler.ParseValueInt(d, "IsBattle");
		o.IsLose = BaseJsonMarshaler.ParseValueInt(d, "IsLose");
		o.IsShow = BaseJsonMarshaler.ParseValueInt(d, "IsShow");
		o.IsBlind = BaseJsonMarshaler.ParseValueInt(d, "IsBlind");
		o.IsNPC = BaseJsonMarshaler.ParseValueInt(d, "IsNPC");
		o.IsForceAttack = BaseJsonMarshaler.ParseValueInt(d, "IsForceAttack");
		return o;
	}

	public static string EncodeJsonString(GuildArenaDeckInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildArenaDeckInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("DeckIndex", o.DeckIndex.ToString());
		dictionary.Add("CachedHeroInfos", BaseJsonMarshaler.EncodeArray(o.CachedHeroInfos, JM_NShared_CachedHeroInfo.EncodeJsonObject));
		dictionary.Add("HeroIndices", BaseJsonMarshaler.EncodeArray(o.HeroIndices));
		dictionary.Add("SkillSlotIndices", BaseJsonMarshaler.EncodeArray(o.SkillSlotIndices));
		dictionary.Add("IsBattle", o.IsBattle.ToString());
		dictionary.Add("IsLose", o.IsLose.ToString());
		dictionary.Add("IsShow", o.IsShow.ToString());
		dictionary.Add("IsBlind", o.IsBlind.ToString());
		dictionary.Add("IsNPC", o.IsNPC.ToString());
		dictionary.Add("IsForceAttack", o.IsForceAttack.ToString());
		return dictionary;
	}
}
