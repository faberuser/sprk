using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EclipseInfo
{
	public static EclipseInfo Parse(string s)
	{
		return Parse(s, new EclipseInfo());
	}

	public static EclipseInfo Parse(string s, EclipseInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EclipseInfo Parse(IDictionary d)
	{
		return Parse(d, new EclipseInfo());
	}

	public static EclipseInfo Parse(IDictionary d, EclipseInfo o)
	{
		o.MatchIndex = BaseJsonMarshaler.ParseValueInt(d, "MatchIndex");
		o.IsDeckSave = BaseJsonMarshaler.ParseValueBool(d, "IsDeckSave");
		o.IsPlayEclipse = BaseJsonMarshaler.ParseValueBool(d, "IsPlayEclipse");
		o.MaxWaveIndex = BaseJsonMarshaler.ParseValueInt(d, "MaxWaveIndex");
		o.CurrentWaveIndex = BaseJsonMarshaler.ParseValueInt(d, "CurrentWaveIndex");
		o.DeckIndex = BaseJsonMarshaler.ParseValueInt(d, "DeckIndex");
		o.LastDeckIndex = BaseJsonMarshaler.ParseValueInt(d, "LastDeckIndex");
		o.ExpireTime = BaseJsonMarshaler.ParseValueString(d, "ExpireTime");
		o.EclipseStaminaResult = BaseJsonMarshaler.ParseObject(d, "EclipseStaminaResult", JM_NShared_StaminaResultInfo.Parse);
		o.EclipsePointResult = BaseJsonMarshaler.ParseObject(d, "EclipsePointResult", JM_NShared_CurrencyResultInfo3.Parse);
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.DeckResults = BaseJsonMarshaler.ParseArray(d, "DeckResults", JM_NShared_EclipseDeckResult.Parse);
		return o;
	}

	public static string EncodeJsonString(EclipseInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EclipseInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MatchIndex", o.MatchIndex.ToString());
		dictionary.Add("IsDeckSave", o.IsDeckSave.ToString());
		dictionary.Add("IsPlayEclipse", o.IsPlayEclipse.ToString());
		dictionary.Add("MaxWaveIndex", o.MaxWaveIndex.ToString());
		dictionary.Add("CurrentWaveIndex", o.CurrentWaveIndex.ToString());
		dictionary.Add("DeckIndex", o.DeckIndex.ToString());
		dictionary.Add("LastDeckIndex", o.LastDeckIndex.ToString());
		if (o.ExpireTime != null)
		{
			dictionary.Add("ExpireTime", o.ExpireTime);
		}
		dictionary.Add("EclipseStaminaResult", JM_NShared_StaminaResultInfo.EncodeJsonObject(o.EclipseStaminaResult));
		dictionary.Add("EclipsePointResult", JM_NShared_CurrencyResultInfo3.EncodeJsonObject(o.EclipsePointResult));
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("DeckResults", BaseJsonMarshaler.EncodeArray(o.DeckResults, JM_NShared_EclipseDeckResult.EncodeJsonObject));
		return dictionary;
	}
}
