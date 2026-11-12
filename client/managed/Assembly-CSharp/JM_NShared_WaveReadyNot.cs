using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_WaveReadyNot
{
	public static WaveReadyNot Parse(string s)
	{
		return Parse(s, new WaveReadyNot());
	}

	public static WaveReadyNot Parse(string s, WaveReadyNot o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WaveReadyNot Parse(IDictionary d)
	{
		return Parse(d, new WaveReadyNot());
	}

	public static WaveReadyNot Parse(IDictionary d, WaveReadyNot o)
	{
		o.WaveIndex = BaseJsonMarshaler.ParseValueInt(d, "WaveIndex");
		o.TotalWaveIndex = BaseJsonMarshaler.ParseValueInt(d, "TotalWaveIndex");
		return o;
	}

	public static string EncodeJsonString(WaveReadyNot o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WaveReadyNot o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("WaveIndex", o.WaveIndex.ToString());
		dictionary.Add("TotalWaveIndex", o.TotalWaveIndex.ToString());
		return dictionary;
	}
}
