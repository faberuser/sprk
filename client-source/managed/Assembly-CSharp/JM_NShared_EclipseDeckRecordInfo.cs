using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EclipseDeckRecordInfo
{
	public static EclipseDeckRecordInfo Parse(string s)
	{
		return Parse(s, new EclipseDeckRecordInfo());
	}

	public static EclipseDeckRecordInfo Parse(string s, EclipseDeckRecordInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EclipseDeckRecordInfo Parse(IDictionary d)
	{
		return Parse(d, new EclipseDeckRecordInfo());
	}

	public static EclipseDeckRecordInfo Parse(IDictionary d, EclipseDeckRecordInfo o)
	{
		o.DeckIndex = BaseJsonMarshaler.ParseValueInt(d, "DeckIndex");
		o.MaxWaveIndex = BaseJsonMarshaler.ParseValueInt(d, "MaxWaveIndex");
		o.UpdatedTime = BaseJsonMarshaler.ParseValueString(d, "UpdatedTime");
		return o;
	}

	public static string EncodeJsonString(EclipseDeckRecordInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EclipseDeckRecordInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("DeckIndex", o.DeckIndex.ToString());
		dictionary.Add("MaxWaveIndex", o.MaxWaveIndex.ToString());
		if (o.UpdatedTime != null)
		{
			dictionary.Add("UpdatedTime", o.UpdatedTime);
		}
		return dictionary;
	}
}
