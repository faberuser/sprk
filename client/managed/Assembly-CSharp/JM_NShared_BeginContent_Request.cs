using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.BeginContent;

public static class JM_NShared_BeginContent_Request
{
	public static NShared.BeginContent.Request Parse(string s)
	{
		return Parse(s, new NShared.BeginContent.Request());
	}

	public static NShared.BeginContent.Request Parse(string s, NShared.BeginContent.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.BeginContent.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.BeginContent.Request());
	}

	public static NShared.BeginContent.Request Parse(IDictionary d, NShared.BeginContent.Request o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.DungeonDifficulty = BaseJsonMarshaler.ParseValueInt(d, "DungeonDifficulty");
		o.HeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "HeroIndices", BaseJsonMarshaler.CastToInt);
		o.AiHeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "AiHeroIndices", BaseJsonMarshaler.CastToInt);
		o.GroupIndex = BaseJsonMarshaler.ParseValueInt(d, "GroupIndex");
		o.DungeonType = BaseJsonMarshaler.ParseEnum<EPunishmentDungeonType>(d, "DungeonType");
		o.FlaskItemIndex = BaseJsonMarshaler.ParseValueInt(d, "FlaskItemIndex");
		o.FlaskItemCount = BaseJsonMarshaler.ParseValueInt(d, "FlaskItemCount");
		o.AffixIndices = BaseJsonMarshaler.ParseArrayCast(d, "AffixIndices", BaseJsonMarshaler.CastToInt);
		o.Level = BaseJsonMarshaler.ParseValueInt(d, "Level");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.BeginContent.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.BeginContent.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("DungeonDifficulty", o.DungeonDifficulty.ToString());
		dictionary.Add("HeroIndices", BaseJsonMarshaler.EncodeArray(o.HeroIndices));
		dictionary.Add("AiHeroIndices", BaseJsonMarshaler.EncodeArray(o.AiHeroIndices));
		dictionary.Add("GroupIndex", o.GroupIndex.ToString());
		dictionary.Add("DungeonType", o.DungeonType.ToString());
		dictionary.Add("FlaskItemIndex", o.FlaskItemIndex.ToString());
		dictionary.Add("FlaskItemCount", o.FlaskItemCount.ToString());
		dictionary.Add("AffixIndices", BaseJsonMarshaler.EncodeArray(o.AffixIndices));
		dictionary.Add("Level", o.Level.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
